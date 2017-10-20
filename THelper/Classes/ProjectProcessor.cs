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
using System.Windows.Forms;
using System.Collections;

namespace THelper {
    public class ProjectProcessor {
        protected internal int LastReleasedVersion;
        string archiveFilePath;
        List<string> csPaths;
        string slnPath;
        Version currentInstalledMajor;
        Version currentProjectVersion;
        public List<Version> installedVersions;
        bool isCurrentVersionMajorInstalled;
        bool isExample;

        bool isMainMajor;
        protected internal Version mainMajorLastVersion;
        List<ConverterMessages> MessagesList;
        Version LastMinorOfCurrentMajor;

        DirectoryInfo solutionFolderInfo;
        string solutionFolderName;

        public IFileWorker MyFileWorker;
        public IMessageProcessor MyMessageProcessor;
        public ICSProjProcessor csProjProcessor;

        public ProjectProcessor(string _filePath) {
            this.archiveFilePath = _filePath;
        }
      protected internal  static string fileWithVersionsPath = @"c:\Dropbox\Deploy\DXConverterDeploy\versions.xml";
        internal void GetSettings() {
            string pathToSettingsFile = @"C:\MSSQLSettings.ini";
            StreamReader sr = new StreamReader(pathToSettingsFile);
            string st = sr.ReadToEnd();
            sr.Close();

            XElement xl = XElement.Parse(st);
            var lastVersion = xl.Element("LastDXVersion").Value;
            LastReleasedVersion = int.Parse(lastVersion);
        }
        internal void ProcessArchive() { //0
            SetIsExample();
            ExtractFiles();
            ProcessFolder();
        }
        void SetIsExample() {//1.1 //tt
            isExample = archiveFilePath.EndsWith(".dxsample");
        }
        void ExtractFiles() { //1.2
            string winRarPath = Properties.Settings.Default.WinRarPath;
            string argsFullWinRar = GetArgsForWinRar();
            MyFileWorker.ProcessStart(winRarPath, argsFullWinRar);
        }

        string GetArgsForWinRar() {//1.2.1 /tt
            string argumentsFilePath = " x \"" + archiveFilePath + "\"";
            var archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            solutionFolderName = Directory.GetParent(archiveFilePath) + "\\" + archiveFileName.Replace(" ", "_");
            solutionFolderInfo = MyFileWorker.CreateDirectory(solutionFolderName);
            var argsFullWinRar = argumentsFilePath + " " + @"""" + solutionFolderName + @"""";
            return argsFullWinRar;
        }



        private void ProcessFolder() { //2
           // csPaths = string.Empty;
            //  bool isSoluiton = TryGetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);
            string[] solutionFiles = TryGetSolutionFiles(solutionFolderInfo);
            var solutionCount = solutionFiles.Count();
            if (solutionCount > 0) {
                slnPath = solutionFiles[0];
            }
            if (solutionCount > 1) {
                MyMessageProcessor.ConsoleWrite("There are many sln files!", ConsoleColor.Red);
                MyMessageProcessor.ConsoleWriteLine();
            }

            string[] projectFiles = TryGetProjectFiles(solutionFolderInfo);
            int projectsCount = projectFiles.Count();
            switch (projectsCount) {
                case 1:
                    csPaths =new List<string>() { projectFiles[0] };
                    WorkWithSolution();
                    break;
                case 0:
                    OpenFolder();
                    break;
                default:
                    var dxPaths = FindDXCsprojs(projectFiles);
                    if (dxPaths.Count>0) {
                        csPaths = dxPaths;
                        WorkWithSolution();
                    }
                    else {
                        OpenFolder();
                    }
                    break;
            }
        }

        private List<string> FindDXCsprojs(string[] solutionsFiles) {
            List<string> list = new List<string>();
            foreach (string st in solutionsFiles) {
                var tx = MyFileWorker.StreamReaderReadToEnd(st);
                if (tx.Contains(@"Reference Include=""DevExpress.")) {
                    list.Add(st);
                }
            }
            return list;
        }

        private void WorkWithSolution() {
            GetMessageInfo();
            var result = PrintMessage();
            ProcessProject(result);
        }

        string[] TryGetProjectFiles(DirectoryInfo dirInfo) { //3 td
            var st = MyFileWorker.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).ToArray();
            if (st.Count() == 0) {
                st = MyFileWorker.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).ToArray();
            }
            return st;
        }
        string[] TryGetSolutionFiles(DirectoryInfo dirInfo) {
            var st = MyFileWorker.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).ToArray();
            return st;
        }
        void GetMessageInfo() {//4 td
            MessagesList = new List<ConverterMessages>();
            csProjProcessor = CreateCSProjProcessor();
            GetInstalledVersions();
            GetProjectVersion();
            isMainMajor = currentProjectVersion.Major == mainMajorLastVersion.Major;
            if (isExample)
                MessagesList.Add(ConverterMessages.OpenSolution);
            else {
                if (currentProjectVersion.CompareTo(Version.Zero) == 0) {
                    MessagesList.Add(ConverterMessages.OpenSolution);
                }
                else {
                    //#endif
                    currentInstalledMajor = installedVersions.Where(x => x.Major == currentProjectVersion.Major).FirstOrDefault();
                    isCurrentVersionMajorInstalled = currentInstalledMajor != null;
                    if (isCurrentVersionMajorInstalled) {
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
                            if (currentProjectVersion.CompareTo(currentInstalledMajor) == 0) {
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
                        LastMinorOfCurrentMajor = FindLastVersionOfMajor(currentProjectVersion.Major);
                        if (currentProjectVersion.Minor == 0 || currentProjectVersion.Minor == LastMinorOfCurrentMajor.Minor) {
                            MessagesList.Add(ConverterMessages.LastMinor);
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                        }
                        else {
                            MessagesList.Add(ConverterMessages.ExactConversion);
                            MessagesList.Add(ConverterMessages.LastMinor);
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                        }
                    }
                }
            }
            MessagesList.Add(ConverterMessages.OpenFolder);

        }

        private ICSProjProcessor CreateCSProjProcessor() { //how to get rid off?
            if (csProjProcessor == null)
                csProjProcessor = new CSProjProcessor(csPaths, MyFileWorker);
            return csProjProcessor;
        }
        List<XElement> AllVersionsList;
        protected internal void GetInstalledVersions() {//5 td
            var xDoc = MyFileWorker.LoadXDocument(fileWithVersionsPath);
            var allVersionElement = xDoc.Element("Versions").Element("AllVersions");
            AllVersionsList = allVersionElement.Elements().ToList();
            var installVersionsElement = xDoc.Element("Versions").Element("InstalledVersions");
            installedVersions = installVersionsElement.Elements().Select(x => new Version(x.FirstAttribute.Value)).ToList();
            mainMajorLastVersion = installedVersions.Where(x => x.Major <= LastReleasedVersion).Max();
            mainMajorLastVersionConverterPath = installVersionsElement.Elements().Where(x => x.FirstAttribute.Value == mainMajorLastVersion.ToString(true)).First().Attribute("Path").Value;
        }
        string mainMajorLastVersionConverterPath;
        Version GetVersionFromFile(string projectUpgradeToolPath) {//5.1 td
            string assemblyFullName = MyFileWorker.AssemblyLoadFileFullName(projectUpgradeToolPath);
            return new Version(assemblyFullName, true);
        }

        private void GetProjectVersion() {//6 td
            currentProjectVersion = csProjProcessor.GetCurrentVersion();
        }
        private ConverterMessages PrintMessage() { //7 tt
            if (isExample) {
                MyMessageProcessor.ConsoleWrite("The current project version is an ");
                MyMessageProcessor.ConsoleWrite("example", ConsoleColor.Red);
            }
            else {
                MyMessageProcessor.ConsoleWrite("The current project version is ");
                MyMessageProcessor.ConsoleWrite(currentProjectVersion.ToString(), ConsoleColor.Red);
            }
            MyMessageProcessor.ConsoleWriteLine();
            int k = 1;
            foreach (ConverterMessages msg in MessagesList) {
                PrintConverterMessage(msg, k++.ToString());
            }
            int index = -1;

            while (index == -1 || (index != 9 && index > MessagesList.Count - 1)) {
                ConsoleKey enterKey = MyMessageProcessor.ConsoleReadKey(false);
                index = GetValueFromConsoleKey(enterKey);
            }
            if (index == 9)
                return ConverterMessages.OpenFolder;

            return MessagesList[index - 1];
        }

        private void PrintConverterMessage(ConverterMessages msg, string key) {//8tt
            if (msg == ConverterMessages.OpenSolution) {
                MyMessageProcessor.ConsoleWrite("To open solution press: ");
                MyMessageProcessor.ConsoleWrite(key, ConsoleColor.Red);
                MyMessageProcessor.ConsoleWriteLine();
                return;
            }
            if (msg == ConverterMessages.OpenFolder) {
                MyMessageProcessor.ConsoleWrite("To open folder press: ");
                MyMessageProcessor.ConsoleWrite("9", ConsoleColor.Red);
                MyMessageProcessor.ConsoleWriteLine();
                return;
            }
            string vers = GetMessageVersion(msg);
            MyMessageProcessor.ConsoleWrite("To convert to : ");
            MyMessageProcessor.ConsoleWrite(vers, ConsoleColor.Red);
            MyMessageProcessor.ConsoleWrite(" press ");
            MyMessageProcessor.ConsoleWrite(key, ConsoleColor.Red);
            MyMessageProcessor.ConsoleWriteLine();
        }
        string GetMessageVersion(ConverterMessages msg) { //8.1 tt
            switch (msg) {
                case ConverterMessages.ExactConversion:
                    return currentProjectVersion.ToString();
                case ConverterMessages.LastMinor:
                    if (currentInstalledMajor == null) {
                        return LastMinorOfCurrentMajor.ToString();
                    }
                    return currentInstalledMajor.ToString();
                case ConverterMessages.MainMajorLastVersion:
                    return mainMajorLastVersion.ToString();
                default:
                    return null;
            }
        }

        int GetValueFromConsoleKey(ConsoleKey key) {//9 tt
            int value = -1;
            if (key > (ConsoleKey.NumPad0) && key <= (ConsoleKey.NumPad9)) { // numpad
                value = (int)key - ((int)ConsoleKey.NumPad0);
            }
            else if ((int)key > ((int)ConsoleKey.D0) && (int)key <= ((int)ConsoleKey.D9)) { // regular numbers
                value = (int)key - ((int)ConsoleKey.D0);
            }
            return value;
        }

        private void ProcessProject(ConverterMessages message) {//12
            if (message == ConverterMessages.OpenFolder) {
                OpenFolder();
                return;
            }

            bool isXafSolution = GetIsXafSolution();

            csProjProcessor.DisableUseVSHostingProcess();


            if (isExample) {
                if (isMainMajor) {
                    csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                    csProjProcessor.SaveNewCsProj();
                }
                else {
                    csProjProcessor.SaveNewCsProj();
                    ConvertToMainMajorLastVersion();
                }
            }
            else { //check how to avoid csProjProcessor.SaveNewCsProj();
                if (!(currentProjectVersion.CompareTo(Version.Zero) == 0)) { //there are dx libs
                    csProjProcessor.RemoveLicense();
                    switch (message) {
                        case ConverterMessages.MainMajorLastVersion:
                            if (isMainMajor) {
                                csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                                csProjProcessor.SaveNewCsProj();
                            }
                            else {
                                csProjProcessor.SaveNewCsProj();
                                ConvertToMainMajorLastVersion();
                            }
                            break;
                        case ConverterMessages.LastMinor:
                            if (isCurrentVersionMajorInstalled) {
                                csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                                csProjProcessor.SaveNewCsProj();
                            }
                            else {
                                csProjProcessor.SaveNewCsProj();

                                if (GetIfLibrariesPersist()) {
                                    break;
                                }
                                ConvertProjectWithDxConverter(LastMinorOfCurrentMajor);
                            }
                            break;
                        case ConverterMessages.ExactConversion:
                            csProjProcessor.SaveNewCsProj();

                            if (GetIfLibrariesPersist()) {
                                break;
                            }
                            ConvertProjectWithDxConverter(currentProjectVersion);
                            break;
                        default:
                            csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                            csProjProcessor.SaveNewCsProj();
                            break;
                    }
                }

                else {
                    csProjProcessor.SaveNewCsProj();
                }
            }

            OpenSolution();

        }
        bool GetIsXafSolution() {
            return false;
        }

        private void ConvertToMainMajorLastVersion() {//13 tt 
            ConvertProjectWithDxConverter(mainMajorLastVersion,mainMajorLastVersionConverterPath);
        }
        private Version FindLastVersionOfMajor(int major) {//15tt
            var res = AllVersionsList.Where(x => x.FirstAttribute.Value.Split('.')[0] + x.FirstAttribute.Value.Split('.')[1] == major.ToString()).First().FirstAttribute.Value;
            return new Version(res);
        }

        private bool GetIfLibrariesPersist() {//14
            DirectoryInfo dirInfo = new DirectoryInfo(solutionFolderName);
            var v = MyFileWorker.DirectoryEnumerateFiles(dirInfo.FullName, "DevExpress*.dll", SearchOption.AllDirectories).ToList();
            return v.Count == csProjProcessor.DXLibrariesCount;
        }

        private void ConvertProjectWithDxConverter(Version v,string converterPath=null) {//16
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Properties.Settings.Default.DXConverterPath;
            string versionConverterFormat = v.ToString(true);
            psi.Arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"0.0.0\" \"{2}\"", solutionFolderName, versionConverterFormat,converterPath);
            MyFileWorker.ProcessStart(psi.FileName, psi.Arguments);
        }














        private void OpenFolder() { //tt
            MyFileWorker.OpenFolder(solutionFolderName);
        }
        private void OpenSolution() {//tt
            if (slnPath != null)
                MyFileWorker.ProcessStart(slnPath);
            else
                MyFileWorker.ProcessStart(csPaths[0]);
        }















    }

    public class VersionComparer : IComparer<string> {

        public int Compare(string x, string y) {
            int counter = 0, res = 0;
            while (counter < 3 && res == 0) {
                int versionX = Convert.ToInt32(x.Split('.')[counter].Replace("_new", ""));
                int versionY = Convert.ToInt32(y.Split('.')[counter]);
                res = Comparer.Default.Compare(versionX, versionY);
                counter++;
            }
            return -res;
        }
    }


}
