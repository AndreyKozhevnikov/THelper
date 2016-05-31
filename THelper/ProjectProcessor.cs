using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;

namespace THelper {
    public enum ConverterMessages { OpenSolution, MainMajorLastVersion, LastMinor, ExactConversion, OpenFolder }

    public class ProjectProcessor {
        private string archiveFilePath;
        string cspath;
        CSProjProcessor csProjProccessor;
        Version currentProjectVersion;
        List<Version> installedVersions;
        bool isCurrentVersionMajorInstalled;
        bool isExample;
        Version mainMajorLastVersion;
        List<ConverterMessages> MessagesList;
        string mmlvConverterPath;
        DirectoryInfo solutionFolderInfo;
        string solutionFolderName;

        public ProjectProcessor(string _filePath) {
            this.archiveFilePath = _filePath;
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
        private void GetCurrentVersion() {
            csProjProccessor = new CSProjProcessor(cspath);
            currentProjectVersion = csProjProccessor.GetCurrentVersion();
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
                if (string.IsNullOrEmpty(projectUpgradeToolPath))
                    continue;
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
        Version currentInstalled;
        private void GetMessageInfo() {
            MessagesList = new List<ConverterMessages>();
            if (isExample)
                MessagesList.Add(ConverterMessages.OpenSolution);
            else {
                GetInstalledVersions();
                GetCurrentVersion();
                currentInstalled = installedVersions.Where(x => x.Major == currentProjectVersion.Major).FirstOrDefault();
                isCurrentVersionMajorInstalled = currentInstalled != null;
                if (isCurrentVersionMajorInstalled) {
                    if (currentProjectVersion.Major == mainMajorLastVersion.Major) {
                        if (currentProjectVersion.CompareTo(mainMajorLastVersion) == 0) {
                            MessagesList.Add(ConverterMessages.OpenSolution);
                        }
                        else {
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                            MessagesList.Add(ConverterMessages.ExactConversion);
                        }
                    }
                    else {
                        if (currentProjectVersion.CompareTo(currentInstalled) == 0) {
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                            MessagesList.Add(ConverterMessages.OpenSolution);
                        }
                        else {
                            MessagesList.Add(ConverterMessages.ExactConversion);
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                            MessagesList.Add(ConverterMessages.LastMinor);
                        }
                    }
                }
                else {
                    MessagesList.Add(ConverterMessages.ExactConversion);
                    MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                }
            }
            MessagesList.Add(ConverterMessages.OpenFolder);

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
            if (licGroup != null)
                licGroup.Remove();
            var UseVSHostingProcess = elements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
            if (UseVSHostingProcess != null)
                UseVSHostingProcess.SetValue("false");
            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);

            string _dxLibraryString = null;
            if (dxlibraries.Count() > 0)
                _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();


            foreach (XElement dxlib in dxlibraries) {
                var specificVersionNode = dxlib.Element(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName));
                if (specificVersionNode != null)
                    dxlib.SetElementValue(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName), false);
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
        private void ProcessSolution() {
            string slnPath = string.Empty;
            cspath = string.Empty;
            bool isSoluiton = GetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);
            if (isSoluiton)
                GetMessageInfo();
            else
                OpenFolder();
        }


        private bool UpdgradeProject(string _projFolderPath, string _dxLibraryString, bool _isDxSample) {
            ProjectUpgrader upgrader = new ProjectUpgrader(_projFolderPath, _dxLibraryString, _isDxSample);
            return upgrader.Start();

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



        public bool GetSolutionFiles(DirectoryInfo dirInfo, out string _slnPath, out string _csprojPath) {
            _slnPath = Directory.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
            _csprojPath = Directory.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (_csprojPath == null)
                _csprojPath = Directory.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(_slnPath))
                _slnPath = _csprojPath;
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
                    string versValueShort = versionMatchShort.Groups[nameof(Version)].Value;
                    return new Version(versValueShort);
                }
                return Version.Zero;
            }
            string versValue = versionMatch.Groups[nameof(Version)].Value;
            return new Version(versValue);
        }
    }


}
