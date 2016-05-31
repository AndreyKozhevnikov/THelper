using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Reflection;

namespace THelper {
    public class ProjectProcessor {
        private string archiveFilePath;
        List<Version> installedVersions;
        bool isExample;
        Version mainMajorLastVersion;
        List<string> MessagesList;
        string mmlvConverterPath;
        DirectoryInfo solutionFolderInfo;
        string solutionFolderName;

        public ProjectProcessor(string _filePath) {
            this.archiveFilePath = _filePath;
        }
        internal void ProcessProject() {
            ExtractFiles();
            ProcessSolution();


            //bool isNeedToOpenSolution = false;
            //if (isSoluiton) {

            //    string dxLibraryString = ProcessCsprojFile(cspath);
            //    if (dxLibraryString != null)
            //        isNeedToOpenSolution = UpdgradeProject(solutionFolderName, dxLibraryString, isExample);

            //}
            //if (isNeedToOpenSolution)
            //    Process.Start(slnPath);
            //else
            //    Process.Start(solutionFolderName);

        }
        private void ProcessSolution() {
            string slnPath = string.Empty;
            cspath = string.Empty;
            bool isSoluiton = GetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);
            if (isSoluiton) {
                GetMessageInfo();
            }
            else {
                OpenFolder();
            }
        }
        private void AddOpenSolutionMessage() {
            MessagesList.Add("Open solution");
        }
        void ExtractFiles() {
            string winRarPath = Properties.Settings.Default.WinRarPath;
            string argumentsFilePath = " x \"" + archiveFilePath + "\"";
            var archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            solutionFolderName = Directory.GetParent(archiveFilePath) + "\\" + archiveFileName.Replace(" ", "_");
            isExample = archiveFilePath.EndsWith(".dxsample");
            solutionFolderInfo = Directory.CreateDirectory(solutionFolderName);
            var argsFullWinRar = argumentsFilePath + " " + @"""" + solutionFolderName + @"""";
            //var winrarProc = Process.Start(winRarPath, argsFullWinRar);
            //winrarProc.WaitForExit();
        }
        private void GetInstalledVersions() {
            installedVersions = new List<Version>();
            mainMajorLastVersion = Version.Zero;

            RegistryKey dxpKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DevExpress\\Components\\");
            string[] versions = dxpKey.GetSubKeyNames();
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter.exe";
            foreach (string strVersion in versions) {
                RegistryKey dxVersionKey = dxpKey.OpenSubKey(strVersion);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                if (string.IsNullOrEmpty(projectUpgradeToolPath)) {
                    continue;
                }
                projectUpgradeToolPath = Path.Combine(projectUpgradeToolPath, projectUpgradeToolRelativePath);
                Version projectUpgradeVersion = GetProjectUpgradeVersion(projectUpgradeToolPath);
                installedVersions.Add(projectUpgradeVersion);
                if (mainMajorLastVersion.CompareTo(projectUpgradeVersion) == -1) {
                    mainMajorLastVersion = projectUpgradeVersion;
                    mmlvConverterPath = projectUpgradeToolPath.Replace("ProjectConverter", "ProjectConverter-console");
                }
                // projectUpgradeToolPath = projectUpgradeToolPath.Replace("ProjectConverter", "ProjectConverter-console");
                // installedSupportedMajorsAndPCPaths[projectUpgradeVersion.Major] = projectUpgradeToolPath;
            }
        }

        private void GetMessageInfo() {
            MessagesList = new List<string>();
            if (isExample) {
                AddOpenSolutionMessage();
            }
            else {
                GetInstalledVersions();
                GetCurrentVersion();
            }

        }
        Version currentVersion;
        CSProjProcessor csProjProccessor;
        private void GetCurrentVersion() {
            csProjProccessor = new CSProjProcessor(cspath);
            currentVersion = csProjProccessor.GetCurrentVersion();
        }

        Version GetProjectUpgradeVersion(string projectUpgradeToolPath) {
            Assembly assembly;
            try {
                assembly = Assembly.LoadFile(projectUpgradeToolPath);
            }
            catch {
                return Version.Zero;
            }
            Version result = GetVersionFromContainingString(assembly.FullName);

            return result;
        }

        private void OpenFolder() {
            Process.Start(solutionFolderName);
        }

        private string ProcessCsprojFile(string _csPath) {
            XmlTextReader reader = new XmlTextReader(_csPath);
            XElement xlroot = XElement.Load(reader);
            reader.Close();

            var elements = xlroot.Elements();

            var licGroup = elements.SelectMany(x => x.Elements()).Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).FirstOrDefault();
            if (licGroup != null) {
                licGroup.Remove();
            }
            var UseVSHostingProcess = elements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
            if (UseVSHostingProcess != null) {
                UseVSHostingProcess.SetValue("false");
            }
            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);

            string _dxLibraryString = null;
            if (dxlibraries.Count() > 0)
                _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();


            foreach (XElement dxlib in dxlibraries) {
                var specificVersionNode = dxlib.Element(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName));
                if (specificVersionNode != null) {
                    dxlib.SetElementValue(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName), false);
                }
                else {
                    XName xName = XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName);
                    XElement xatr = new XElement(xName, "False");
                    dxlib.Add(xatr);
                }
            }

            string resultString = xlroot.ToString();
            StreamWriter sw = new StreamWriter(_csPath, false);
            sw.Write(resultString);
            sw.Close();
            return _dxLibraryString;
        }
        string cspath;
  

        private bool UpdgradeProject(string _projFolderPath, string _dxLibraryString, bool _isDxSample) {
            ProjectUpgrader upgrader = new ProjectUpgrader(_projFolderPath, _dxLibraryString, _isDxSample);
            return upgrader.Start();

        }

 

        public bool GetSolutionFiles(DirectoryInfo dirInfo, out string _slnPath, out string _csprojPath) {
            _slnPath = Directory.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
            _csprojPath = Directory.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (_csprojPath == null)
                _csprojPath = Directory.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(_slnPath)) {
                _slnPath = _csprojPath;
            }
            return !string.IsNullOrEmpty(_slnPath);
        }
        public Version GetVersionFromContainingString(string stringWithVersion) {
            string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatch = regexVersion.Match(stringWithVersion);
            if (versionMatch == null || !versionMatch.Success) {
                string versionAssemblypatternShort = @".*DevExpress.*(?<Version>\d{2}\.\d)";
                Regex regexVersionShort = new Regex(versionAssemblypatternShort, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                Match versionMatchShort = regexVersionShort.Match(stringWithVersion);
                if (versionMatchShort != null && versionMatchShort.Success) {
                    string versValueShort = versionMatchShort.Groups["Version"].Value;
                    return new Version(versValueShort);
                }
                return Version.Zero;
            }
            string versValue = versionMatch.Groups["Version"].Value;
            return new Version(versValue);
        }
    }

    
}
