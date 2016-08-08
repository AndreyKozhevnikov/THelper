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
        string archiveFilePath;
        string cspath;
        Version currentInstalledMajor;
        Version currentProjectVersion;
        public List<Version> installedVersions;
        bool isCurrentVersionMajorInstalled;
        bool isExample;

        bool isMainMajor;
        Version mainMajorLastVersion;
        List<ConverterMessages> MessagesList;
        Version LastMinorOfCurrentMajor;
        string mmlvConverterPath;
        
        DirectoryInfo solutionFolderInfo;
        string solutionFolderName;

        public IWorkWithFile MyWorkWithFile;
        public IMessenger MyMessenger;
        public ICSProjProcessor csProjProccessor;

        public ProjectProcessor(string _filePath) {
            this.archiveFilePath = _filePath;
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
            MyWorkWithFile.ProcessStart(winRarPath, argsFullWinRar);
        }

        string GetArgsForWinRar() {//1.2.1 /tt
            string argumentsFilePath = " x \"" + archiveFilePath + "\"";
            var archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            solutionFolderName = Directory.GetParent(archiveFilePath) + "\\" + archiveFileName.Replace(" ", "_");
            solutionFolderInfo = MyWorkWithFile.CreateDirectory(solutionFolderName);
            var argsFullWinRar = argumentsFilePath + " " + @"""" + solutionFolderName + @"""";
            return argsFullWinRar;
        }



        private void ProcessFolder() { //2
            cspath = string.Empty;
            //  bool isSoluiton = TryGetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);
            string[] solutionsFiles = TryGetSolutionFiles(solutionFolderInfo);
            if (solutionsFiles.Count()==1) {
                cspath = solutionsFiles[0];
                GetMessageInfo();
                var result = PrintMessage();
                ProcessProject(result);
            }
            else
                OpenFolder();
        }
        string[] TryGetSolutionFiles(DirectoryInfo dirInfo) { //3 td
            var st = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).ToArray();
            if (st.Count() == 0) {
                st = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).ToArray();
            }
            return st;
            //_slnPath = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
            //_csprojPath = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            //if (_csprojPath == null)
            //    _csprojPath = MyWorkWithFile.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).FirstOrDefault();
            //if (string.IsNullOrEmpty(_slnPath))
            //    _slnPath = _csprojPath;
            //return !string.IsNullOrEmpty(_slnPath);
        }
        void GetMessageInfo() {//4 td
            MessagesList = new List<ConverterMessages>();
            csProjProccessor = CreateCSProjProcessor();
            GetInstalledVersions();
            GetCurrentVersion();
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
            if (csProjProccessor == null)
                csProjProccessor = new CSProjProcessor(cspath, MyWorkWithFile);
            return csProjProccessor;
        }

        void GetInstalledVersions() {//5 td
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
        Version GetProjectUpgradeVersion(string projectUpgradeToolPath) {//5.1 td
            string assemblyFullName = MyWorkWithFile.AssemblyLoadFileFullName(projectUpgradeToolPath);
            if (assemblyFullName != null)
                return new Version(assemblyFullName, true);
            else
                return Version.Zero;
        }

        private void GetCurrentVersion() {//6 td
            currentProjectVersion = csProjProccessor.GetCurrentVersion();
        }
        private ConverterMessages PrintMessage() { //7 tt
            if (isExample) {
                MyMessenger.ConsoleWrite("The current project version is an ");
                MyMessenger.ConsoleWrite("example", ConsoleColor.Red);
            }
            else {
                MyMessenger.ConsoleWrite("The current project version is ");
                MyMessenger.ConsoleWrite(currentProjectVersion.ToString(), ConsoleColor.Red);
            }
            MyMessenger.ConsoleWriteLine();
            int k = 1;
            foreach (ConverterMessages msg in MessagesList) {
                PrintConverterMessage(msg, k++.ToString());
            }
            ConsoleKey enterKey = MyMessenger.ConsoleReadKey(false);

            int index = GetValueFromConsoleKey(enterKey);
            if (index == 9)
                return ConverterMessages.OpenFolder;
            if ((index - 1) > MessagesList.Count) {
                return ConverterMessages.OpenSolution;
            }
            return MessagesList[index - 1];
        }

        private void PrintConverterMessage(ConverterMessages msg, string key) {//8tt
            if (msg == ConverterMessages.OpenSolution) {
                MyMessenger.ConsoleWrite("To open solution press: ");
                MyMessenger.ConsoleWrite(key, ConsoleColor.Red);
                MyMessenger.ConsoleWriteLine();
                return;
            }
            if (msg == ConverterMessages.OpenFolder) {
                MyMessenger.ConsoleWrite("To open folder press: ");
                MyMessenger.ConsoleWrite("9", ConsoleColor.Red);
                MyMessenger.ConsoleWriteLine();
                return;
            }
            string vers = GetMessageVersion(msg);
            MyMessenger.ConsoleWrite("To convert to : ");
            MyMessenger.ConsoleWrite(vers, ConsoleColor.Red);
            MyMessenger.ConsoleWrite(" press ");
            MyMessenger.ConsoleWrite(key, ConsoleColor.Red);
            MyMessenger.ConsoleWriteLine();
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
            if (key >= (ConsoleKey.NumPad0) && key <= (ConsoleKey.NumPad9)) { // numpad
                value = (int)key - ((int)ConsoleKey.NumPad0);
            }
            else if ((int)key >= ((int)ConsoleKey.D0) && (int)key <= ((int)ConsoleKey.D9)) { // regular numbers
                value = (int)key - ((int)ConsoleKey.D0);
            }
            return value;
        }

        private void ProcessProject(ConverterMessages message) {//12
            if (message == ConverterMessages.OpenFolder) {
                OpenFolder();
                return;
            }
            csProjProccessor.DisableUseVSHostingProcess();


            if (isExample) {
                if (isMainMajor) {
                    csProjProccessor.SetSpecificVersionFalse();
                    csProjProccessor.SaveNewCsProj();
                }
                else {
                    csProjProccessor.SaveNewCsProj();
                    UpgradeToMainMajorLastVersion();
                }
            }
            else { //check how to avoid csProjProccessor.SaveNewCsProj();
                if (!(currentProjectVersion.CompareTo(Version.Zero) == 0)) { //there are dx libs
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

                                if (GetIfLibrariesPersist()) {
                                    break;
                                }
                                ConvertProjectWithDxConverter(LastMinorOfCurrentMajor);
                            }
                            break;
                        case ConverterMessages.ExactConversion:
                            csProjProccessor.SaveNewCsProj();

                            if (GetIfLibrariesPersist()) {
                                break;
                            }
                            ConvertProjectWithDxConverter(currentProjectVersion);
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

        private void UpgradeToMainMajorLastVersion() {//13 tt 
            string _projPath = "\"" + solutionFolderName + "\"";
            MyWorkWithFile.ProcessStart(mmlvConverterPath, _projPath, true);
        }
        private Version FindLastVersionOfMajor(int major) {//15tt
            var maj = major;
            List<string> directories = new List<string>();
            string filePath = @"c:\Dropbox\Deploy\DXConverterDeploy\versions.txt";
            var stringLst = MyWorkWithFile.StreamReaderReadToEnd(filePath);
            var dxDirectories = stringLst.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string directory in dxDirectories)
                directories.Add(Path.GetFileName(directory));
            directories.Sort(new VersionComparer());
            var res = directories.Where(x => x.Split('.')[0] + x.Split('.')[1] == maj.ToString()).First();
            return new Version(res);
        }

        private bool GetIfLibrariesPersist() {//14
            DirectoryInfo dirInfo = new DirectoryInfo(solutionFolderName);
            var v = MyWorkWithFile.DirectoryEnumerateFiles(dirInfo.FullName, "DevExpress*.dll", SearchOption.AllDirectories).ToList();
            return v.Count > 0;
        }

        private void ConvertProjectWithDxConverter(Version v) {//16
            ProcessStartInfo psi = new ProcessStartInfo();
            //    psi.FileName = @"\\corp\internal\common\4Nikishina\Converter\EXE\Converter.exe";
            psi.FileName = @"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe";
            string versionConverterFormat = v.ToString(true);
            //psi.Arguments = string.Format("{0} \\\"{1}\\\"", versionConverterFormat, solutionFolderName);
            psi.Arguments = string.Format("{0} {1}", solutionFolderName, versionConverterFormat);
            MyWorkWithFile.ProcessStart(psi.FileName, psi.Arguments);
            //var proc = System.Diagnostics.Process.Start(psi);
            // proc.WaitForExit();
        }














        private void OpenFolder() { //tt
            MyWorkWithFile.ProcessStart(solutionFolderName);
        }
        private void OpenSolution() {//tt
            MyWorkWithFile.ProcessStart(cspath);
        }














#if DEBUGTEST

        public void UpgradeToMainMajorLastVersion_t() {
            UpgradeToMainMajorLastVersion();
        }
        public void ProcessProject_t(ConverterMessages message) {
            ProcessProject(message);
        }
        public string slnPath_t {
            get { return slnPath; }
            set { slnPath = value; }
        }
        public void OpenSolution_t() {
            OpenSolution();
        }
        public void OpenFolder_t() {
            OpenFolder();
        }
        public DirectoryInfo solutionFolderInfo_t {
            get {
                return solutionFolderInfo;
            }

        }
        public ConverterMessages PrintMessage_t() {
            return PrintMessage();
        }
        public List<ConverterMessages> MessagesList_t {
            get {
                return MessagesList;
            }
            set {
                MessagesList = value;
            }
        }
        public string GetArgsForWinRar_t() {
            return GetArgsForWinRar();
        }

        public void GetInstalledVersions_t() {
            GetInstalledVersions();
        }
        public Version mainMajorLastVersion_t {
            get {
                return mainMajorLastVersion;
            }
            set {
                mainMajorLastVersion = value;
            }
        }
        public void PrintConverterMessage_t(ConverterMessages msg, string key) {
            PrintConverterMessage(msg, key);
        }

        public int GetValueFromConsoleKey_t(ConsoleKey key) {
            return GetValueFromConsoleKey(key);
        }
        public void GetMessageInfo_t() {
            GetMessageInfo();
        }
        public string archiveFilePath_t {
            get {
                return archiveFilePath;
            }
        }
        //public List<ConverterMessages> MessagesList_t { get { return MessagesList; } }
        //public void TestSetMainMajorLastVersion(string st) {
        //    mainMajorLastVersion = new Version(st);
        //}

        public bool isExample_t {
            get { return isExample; }
        }


        public bool TryGetSolutionFiles_T(DirectoryInfo dirInfo, out string _slnPath, out string _csprojPath) {
            return TryGetSolutionFiles(dirInfo, out _slnPath, out _csprojPath);
        }
        public void SetIsExample_t() {
            SetIsExample();
        }
        public Version GetProjectUpgradeVersion_t(string projectUpgradeToolPath) {
            return GetProjectUpgradeVersion(projectUpgradeToolPath);
        }
        public Version currentProjectVersion_t {
            get { return currentProjectVersion; }
            set { currentProjectVersion = value; }
        }
        public string GetMessageVersion_t(ConverterMessages msg) {
            return GetMessageVersion(msg);
        }
        public Version currentInstalledMajor_t {
            get { return currentInstalledMajor; }
            set { currentInstalledMajor = value; }
        }
    
        public void GetCurrentVersion_t() {
            GetCurrentVersion();
        }
        public void FindIfLibrariesPersist_t() {
            GetIfLibrariesPersist();
        }

        public Version FindLastVersionOfMajor_t(int m) {
            return FindLastVersionOfMajor(m);
        }
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
