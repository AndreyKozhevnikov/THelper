﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Windows.Forms;

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
        string slnPath;

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
            var winrarProc = Process.Start(winRarPath, argsFullWinRar);
            winrarProc.WaitForExit();
        }

        private void GetCurrentVersion() {

           
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
                if (mainMajorLastVersion.CompareTo(projectUpgradeVersion) == -1 && projectUpgradeVersion.Major != 161) {
                    mainMajorLastVersion = projectUpgradeVersion;
                    mmlvConverterPath = projectUpgradeToolPath.Replace("ProjectConverter", "ProjectConverter-console");
                }
                // projectUpgradeToolPath = projectUpgradeToolPath.Replace("ProjectConverter", "ProjectConverter-console");
                // installedSupportedMajorsAndPCPaths[projectUpgradeVersion.Major] = projectUpgradeToolPath;
            }
        }
        Version currentInstalled;
        bool isMainMajor;
        private void GetMessageInfo() {
            MessagesList = new List<ConverterMessages>();
            csProjProccessor = new CSProjProcessor(cspath);
            GetInstalledVersions();
            if (isExample)
                MessagesList.Add(ConverterMessages.OpenSolution);
            else {
#if !DEBUGTEST
             
                GetCurrentVersion();
#endif
                currentInstalled = installedVersions.Where(x => x.Major == currentProjectVersion.Major).FirstOrDefault();
                isCurrentVersionMajorInstalled = currentInstalled != null;
                if (isCurrentVersionMajorInstalled) {
                    isMainMajor = currentProjectVersion.Major == mainMajorLastVersion.Major;
                    if (isMainMajor) {
                        if (currentProjectVersion.CompareTo(mainMajorLastVersion) == 0) {
                            MessagesList.Add(ConverterMessages.OpenSolution);
                        }
                        else {
                            if (currentProjectVersion.Minor == 0) {
                                MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                            }
                            else {
                                MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                                MessagesList.Add(ConverterMessages.ExactConversion);
                            }
                        }
                    }
                    else {
                        if (currentProjectVersion.CompareTo(currentInstalled) == 0) {
                            MessagesList.Add(ConverterMessages.OpenSolution);
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                        }
                        else {
                            if (currentProjectVersion.Minor == 0) {
                                MessagesList.Add(ConverterMessages.LastMinor);
                                MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                            }
                            else {
                                MessagesList.Add(ConverterMessages.LastMinor);
                                MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                                MessagesList.Add(ConverterMessages.ExactConversion);
                            }
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

      
        private void ProcessFolder() {
             slnPath = string.Empty;
            cspath = string.Empty;
            bool isSoluiton = GetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);
            if (isSoluiton) {
                GetMessageInfo();
                var result = PrintMessage();
                ProcessProject(result);
            }
            else
                OpenFolder();
        }

        private void ProcessProject(ConverterMessages message) {
            if (message == ConverterMessages.OpenFolder) {
                OpenFolder();
                return;
            }
            csProjProccessor.DisableUseVSHostingProcess();
            if (isExample) {
                UpgradeToMainMajorLastVersion();
            }
            else {
                csProjProccessor.RemoveLicense();
                switch (message) {
                    case ConverterMessages.MainMajorLastVersion:
                        if (isMainMajor) {
                            csProjProccessor.SetSpecificVersionFalse();
                        }
                        else {
                            UpgradeToMainMajorLastVersion();
                        }
                        break;
                    case ConverterMessages.LastMinor:
                        if (isCurrentVersionMajorInstalled) {
                            csProjProccessor.SetSpecificVersionFalse();
                        }
                        else {
                            FindIfLibrariesPersist();
                            if (isLibrariesPersist) {
                                break;
                            }
                            else {
                                FindLastVersionOfMajor();
                            }
                        }
                        break;

                }
            }
            csProjProccessor.SaveNewCsProj();
            OpenSolution();

        }

        private void FindLastVersionOfMajor() {
            throw new NotImplementedException();
        }

        private void FindIfLibrariesPersist() {
            DirectoryInfo dirInfo = new DirectoryInfo(solutionFolderName);
            var v = Directory.EnumerateFiles(dirInfo.FullName, "DevExpress*.dll", SearchOption.AllDirectories).ToList();
            isLibrariesPersist= v.Count > 0;
        }

        bool isLibrariesPersist;
        private void OpenSolution() {
            Process.Start(slnPath);
        }

        private void UpgradeToMainMajorLastVersion() {
           // string toolPath = installedSupportedMajorsAndPCPaths[_major];
           string _projPath = "\"" + solutionFolderName + "\"";
            Process updgrade = Process.Start(mmlvConverterPath, _projPath);
            updgrade.WaitForExit();
        }

        private ConverterMessages PrintMessage() {
            if (isExample) {
                ConsoleWrite("The current project version is an ");
                ConsoleWrite("example", ConsoleColor.Red);
            }
            else {
                ConsoleWrite("The current project version is ");
                ConsoleWrite(currentProjectVersion, ConsoleColor.Red);
            }
            Console.WriteLine();
            int k = 1;
            foreach (ConverterMessages msg in MessagesList) {
                PrintConverterMessage(msg, k++.ToString());
            }
            ConsoleKeyInfo enterKey = Console.ReadKey(false);
            var v = enterKey.Key;
            int index = GetValueFromConsoleKey(v);
            if (index == 9)
                return ConverterMessages.OpenFolder;
            return MessagesList[index-1];
        }

        int GetValueFromConsoleKey(ConsoleKey key) {
            int value = -1;
            if (key >= (ConsoleKey.NumPad0) && key <= (ConsoleKey.NumPad9)) { // numpad
                value = (int)key - ((int)ConsoleKey.NumPad0);
            }
            else if ((int)key >= ((int)ConsoleKey.D0) && (int)key <= ((int)ConsoleKey.D9)) { // regular numbers
                value = (int)key - ((int)ConsoleKey.D0);
            }
            return value;
        }
        private void PrintConverterMessage(ConverterMessages msg, string key) {
            if (msg == ConverterMessages.OpenSolution) {
                ConsoleWrite("To open solution press: ");
                ConsoleWrite(key, ConsoleColor.Red);
                Console.WriteLine();
                return;
            }
            if (msg == ConverterMessages.OpenFolder) {
                ConsoleWrite("To open folder press: ");
                ConsoleWrite("9", ConsoleColor.Red);
                Console.WriteLine();
                return;
            }
            string vers = GetMessageVersion(msg);
            ConsoleWrite("To convert to : ");
            ConsoleWrite(vers, ConsoleColor.Red);
            ConsoleWrite(" press ");
            ConsoleWrite(key, ConsoleColor.Red);
            Console.WriteLine();
        }

        string GetMessageVersion(ConverterMessages msg) {
            switch (msg) {
                case ConverterMessages.ExactConversion:
                    return currentProjectVersion.ToString();
                case ConverterMessages.LastMinor:
                    if (currentInstalled == null)
                        return "0.0.0";
                    return currentInstalled.ToString();
                case ConverterMessages.MainMajorLastVersion:
                    return mainMajorLastVersion.ToString();
                default:
                    return null;
            }

        }


      

        private bool UpdgradeProject(string _projFolderPath, string _dxLibraryString, bool _isDxSample) {
            ProjectUpgrader upgrader = new ProjectUpgrader(_projFolderPath, _dxLibraryString, _isDxSample);
            return upgrader.Start();

        }

        internal void ProcessArchive() {
            ExtractFiles();
            ProcessFolder();
        }
        void ConsoleWrite(object _message, ConsoleColor color) {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(_message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        void ConsoleWrite(object _message) {
            Console.Write(_message);
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
        #region old
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
        #endregion

#if DEBUGTEST
        public void TestSetCurrentVersion(string st) {
            currentProjectVersion = new Version(st);
        }

        public void TestAddToInstalledVersions(string st) {
            if (installedVersions == null)
                installedVersions = new List<Version>();
            installedVersions.Add(new Version(st));
        }
        public void TestGetMessageInfo() {
            GetMessageInfo();
        }
        public List<ConverterMessages> TestMessageList { get { return MessagesList; } }
        public void TestSetMainMajorLastVersion(string st) {
            mainMajorLastVersion = new Version(st);
        }
#endif
    }
}
