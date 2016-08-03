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
    public enum ConverterMessages { OpenSolution, MainMajorLastVersion, LastMinor, ExactConversion, OpenFolder }



    public class ProjectProcessor {
        public string archiveFilePath;
        string cspath;
        CSProjProcessor csProjProccessor;
        Version currentInstalled;
        Version currentProjectVersion;
        List<Version> installedVersions;
        bool isCurrentVersionMajorInstalled;
        public bool isExample;
        bool isLibrariesPersist;
        bool isMainMajor;
        Version mainMajorLastVersion;
        public List<ConverterMessages> MessagesList;
        string mmlvConverterPath;
        string slnPath;
        public DirectoryInfo solutionFolderInfo;
        string solutionFolderName;

        public IWorkWithFile MyWorkWithFile;


        public ProjectProcessor(string _filePath) {
            this.archiveFilePath = _filePath;
        }

        internal void ProcessArchive() { //0
            SetIsExample();
            ExtractFiles();
            ProcessFolder();
        }
        public void SetIsExample() {//1.1 //tt
            isExample = archiveFilePath.EndsWith(".dxsample");
        }
        void ExtractFiles() { //1.2
            string winRarPath = Properties.Settings.Default.WinRarPath;
            string argsFullWinRar = GetArgsForWinRar();
            var winrarProc = Process.Start(winRarPath, argsFullWinRar);
            winrarProc.WaitForExit();
        }

        public string GetArgsForWinRar() {//1.2.1 /tt
            string argumentsFilePath = " x \"" + archiveFilePath + "\"";
            var archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            solutionFolderName = Directory.GetParent(archiveFilePath) + "\\" + archiveFileName.Replace(" ", "_");
            solutionFolderInfo = MyWorkWithFile.CreateDirectory(solutionFolderName);
            var argsFullWinRar = argumentsFilePath + " " + @"""" + solutionFolderName + @"""";
            return argsFullWinRar;
        }



        private void ProcessFolder() { //2
            slnPath = string.Empty;
            cspath = string.Empty;
            bool isSoluiton = TryGetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);

            if (isSoluiton) {
                GetMessageInfo();
                var result = PrintMessage();
                ProcessProject(result);
            }
            else
                OpenFolder();
        }
        public bool TryGetSolutionFiles(DirectoryInfo dirInfo, out string _slnPath, out string _csprojPath) { //3 td
            _slnPath = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
            _csprojPath = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (_csprojPath == null)
                _csprojPath = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(_slnPath))
                _slnPath = _csprojPath;
            return !string.IsNullOrEmpty(_slnPath);
        }
        public void GetMessageInfo() {//4
            MessagesList = new List<ConverterMessages>();
            csProjProccessor = new CSProjProcessor(cspath, MyWorkWithFile);
            GetInstalledVersions();
            if (isExample)
                MessagesList.Add(ConverterMessages.OpenSolution);
            else {
#if !DEBUGTEST
                GetCurrentVersion();
#endif
                if (currentProjectVersion.CompareTo(Version.Zero) == 0) {
                    MessagesList.Add(ConverterMessages.OpenSolution);
                }
                else {
                    //#endif
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
            }
            MessagesList.Add(ConverterMessages.OpenFolder);

        }
        private void GetInstalledVersions() {//5
            installedVersions = new List<Version>();
            mainMajorLastVersion = Version.Zero;
            List<string> versions = MyWorkWithFile.GetRegistryVersions("SOFTWARE\\DevExpress\\Components\\");
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter.exe";
            foreach (string rootPath in versions) {
                var rootPath2 = Path.Combine(rootPath, projectUpgradeToolRelativePath);
                Version projectUpgradeVersion = GetProjectUpgradeVersion(rootPath2);
                installedVersions.Add(projectUpgradeVersion);
                if (mainMajorLastVersion.CompareTo(projectUpgradeVersion) == -1 && projectUpgradeVersion.Major != 162) {
                    mainMajorLastVersion = projectUpgradeVersion;
                    mmlvConverterPath = rootPath2.Replace("ProjectConverter", "ProjectConverter-console");
                }
            }
        }
        public Version GetProjectUpgradeVersion(string projectUpgradeToolPath) {//5.1
            string assemblyFullName = MyWorkWithFile.AssemblyLoadFileFullName(projectUpgradeToolPath);
            if (assemblyFullName != null)
                return new Version(assemblyFullName, true);
            else
                return Version.Zero;
        }

        private void GetCurrentVersion() {//6
            currentProjectVersion = csProjProccessor.GetCurrentVersion();
        }
        private ConverterMessages PrintMessage() { //7
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
            if ((index - 1) > MessagesList.Count) {
                return ConverterMessages.OpenSolution;
            }
            return MessagesList[index - 1];
        }

        private void PrintConverterMessage(ConverterMessages msg, string key) {//8
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
        string GetMessageVersion(ConverterMessages msg) { //8.1
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

        int GetValueFromConsoleKey(ConsoleKey key) {//9
            int value = -1;
            if (key >= (ConsoleKey.NumPad0) && key <= (ConsoleKey.NumPad9)) { // numpad
                value = (int)key - ((int)ConsoleKey.NumPad0);
            }
            else if ((int)key >= ((int)ConsoleKey.D0) && (int)key <= ((int)ConsoleKey.D9)) { // regular numbers
                value = (int)key - ((int)ConsoleKey.D0);
            }
            return value;
        }
        void ConsoleWrite(object _message) { //10
            Console.Write(_message);
        }
        void ConsoleWrite(object _message, ConsoleColor color) {//11

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(_message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        private void ProcessProject(ConverterMessages message) {//12
            if (message == ConverterMessages.OpenFolder) {
                OpenFolder();
                return;
            }
            csProjProccessor.DisableUseVSHostingProcess();
            if (isExample) {
                UpgradeToMainMajorLastVersion();
            }
            else { //check how to avoid csProjProccessor.SaveNewCsProj();
                if (!(currentProjectVersion.CompareTo(Version.Zero) == 0)) {
                    csProjProccessor.RemoveLicense();
                    switch (message) {
                        case ConverterMessages.MainMajorLastVersion:
                            if (isMainMajor) {
                                csProjProccessor.SetSpecificVersionFalse();
                                csProjProccessor.SaveNewCsProj();
                            }
                            else {
                                csProjProccessor.SaveNewCsProj();
                                UpgradeToMainMajorLastVersion();
                            }
                            break;
                        case ConverterMessages.LastMinor:
                            if (isCurrentVersionMajorInstalled) {
                                csProjProccessor.SetSpecificVersionFalse();
                                csProjProccessor.SaveNewCsProj();
                            }
                            else {
                                csProjProccessor.SaveNewCsProj();
                                FindIfLibrariesPersist();
                                if (isLibrariesPersist) {
                                    break;
                                }
                                Version LastMinorOfCurrentMajor = FindLastVersionOfMajor();

                                ConvertProjectWithSvetaConverter(LastMinorOfCurrentMajor);
                            }
                            break;
                        case ConverterMessages.ExactConversion:
                            csProjProccessor.SaveNewCsProj();
                            FindIfLibrariesPersist();
                            if (isLibrariesPersist) {
                                break;
                            }
                            ConvertProjectWithSvetaConverter(currentProjectVersion);
                            break;
                        default:
                            csProjProccessor.SaveNewCsProj();
                            break;
                    }
                }
                else {
                    csProjProccessor.SaveNewCsProj();
                }
            }

            OpenSolution();

        }

        private void UpgradeToMainMajorLastVersion() {//13
            string _projPath = "\"" + solutionFolderName + "\"";
            Process updgrade = Process.Start(mmlvConverterPath, _projPath);
            updgrade.WaitForExit();
        }
        private void FindIfLibrariesPersist() {//14
            DirectoryInfo dirInfo = new DirectoryInfo(solutionFolderName);
            var v = Directory.EnumerateFiles(dirInfo.FullName, "DevExpress*.dll", SearchOption.AllDirectories).ToList();
            isLibrariesPersist = v.Count > 0;
        }
        private Version FindLastVersionOfMajor() {//15
            var maj = currentProjectVersion.Major;
            List<string> directories = new List<string>();
            foreach (string directory in Directory.GetDirectories(@"\\CORP\builds\release\DXDlls\"))
                directories.Add(Path.GetFileName(directory));
            directories.Sort(new VersionComparer());
            var res = directories.Where(x => x.Split('.')[0] + x.Split('.')[1] == maj.ToString()).First();
            return new Version(res);
        }
        private void ConvertProjectWithSvetaConverter(Version v) {//16
            ProcessStartInfo psi = new ProcessStartInfo();
            //    psi.FileName = @"\\corp\internal\common\4Nikishina\Converter\EXE\Converter.exe";
            psi.FileName = @"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe";
            string versionConverterFormat = v.ToString(true);
            //psi.Arguments = string.Format("{0} \\\"{1}\\\"", versionConverterFormat, solutionFolderName);
            psi.Arguments = string.Format("{0} {1}", solutionFolderName, versionConverterFormat);
            var proc = System.Diagnostics.Process.Start(psi);
            proc.WaitForExit();
        }














        private void OpenFolder() {
            Process.Start(solutionFolderName);
        }
        private void OpenSolution() {
            Process.Start(slnPath);
        }













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
        public void Test_SetIsExample() {
            this.SetIsExample();
        }
        public bool Test_IsExample {
            get { return isExample; }
        }
        public string Test_GetArgsForWinRar() {
            return GetArgsForWinRar();
        }
        //public CSProjProcessor Test_Csprojprocessor {
        //    get {
        //        return csProjProccessor;
        //    }
        //}
#endif
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

    //public class RegistryVersion {
    //    public string Version;
    //    public string RootDirectory;
    //}
}
