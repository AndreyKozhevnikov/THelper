using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace THelper {
    public class ProjectProcessor {
        protected internal int lastReleasedVersion;
        protected internal string filesToDetect;
        protected internal string namesToExclude;
        string archiveFilePath;
        List<string> csPaths;
        string slnPath;
        string gitBatchFile = null;
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
        internal void GetSettings() {
            lastReleasedVersion = Properties.Settings.Default.LastReleasedVersion;
            filesToDetect = Properties.Settings.Default.FilesToDetect;
            namesToExclude = Properties.Settings.Default.NamesToExclude;
        }
        internal void ProcessArchive() { //0
                                         // SetIsExample();
            SetIsExample();
            ExtractFiles();
            ProcessFolder();
        }
        void SetIsExample() {//1.1 //tt
            isExample = GetIsExample(archiveFilePath);
        }

        protected internal bool GetIsExample(string st) {
            Regex exampleRX = new Regex(@"\d.\d.\d-.zip");
            var res = exampleRX.Match(st);
            var isExample = res.Success;
            return isExample;
        }

        void ExtractFiles() { //1.2
            string winRarPath = Properties.Settings.Default.WinRarPath;
            string argsFullWinRar = GetArgsForWinRar();
            MyFileWorker.ProcessStart(winRarPath, argsFullWinRar);
            if(isExample) {
                var oldSolutionFolderName = Path.Combine(solutionFolderName, archiveFileName);
                var newFolderName = Path.Combine(solutionFolderName, "dxExampl-" + archiveFileName.Substring(0, 10));
                while(true) {
                    var allProcesses = Process.GetProcesses();
                    var wRarProc = allProcesses.Where(x => x.ProcessName == "WinRAR");
                    if(wRarProc.Count() == 0) {
                        break;
                    }
                    Thread.Sleep(500);
                }
                MyFileWorker.DirectoryMove(oldSolutionFolderName, newFolderName);

                solutionFolderName = Path.Combine(newFolderName, "CS");
                solutionFolderInfo = MyFileWorker.CreateDirectory(solutionFolderName);
            }
            var vsDirectories = MyFileWorker.DirectoryGetDirectories(solutionFolderInfo, ".vs", SearchOption.AllDirectories);
            if(vsDirectories.Count() > 0) {
                MyFileWorker.DirectoryDelete(vsDirectories[0].FullName, true);
                Thread.Sleep(100);
            }
        }
        string archiveFileName;
        string GetArgsForWinRar() {//1.2.1 /tt
            string argumentsFilePath = " x \"" + archiveFilePath + "\"";
            archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            if(!isExample) {
                solutionFolderName = Directory.GetParent(archiveFilePath) + "\\" + archiveFileName.Replace(" ", "_");
                solutionFolderInfo = MyFileWorker.CreateDirectory(solutionFolderName);
            } else {
                var fi = new FileInfo(archiveFilePath);
                var folderName = fi.DirectoryName;
                solutionFolderName = folderName;
            }
            var argsFullWinRar = argumentsFilePath + " " + @"""" + solutionFolderName + @"""";
            return argsFullWinRar;
        }



        private void ProcessFolder() { //2
                                       // csPaths = string.Empty;
                                       //  bool isSoluiton = TryGetSolutionFiles(solutionFolderInfo, out slnPath, out cspath);
            MyMessageProcessor.Setup();

            List<string> unexpectedFiles = GetUnexpectedFiles(solutionFolderInfo);
            if(unexpectedFiles.Count > 0) {
                MyMessageProcessor.ConsoleWrite("There are unexpected files!", ConsoleColor.Red, true);
                foreach(var fl in unexpectedFiles) {
                    MyMessageProcessor.ConsoleWrite(fl, ConsoleColor.Red, true);
                }
            }
            string[] solutionFiles = TryGetSolutionFiles(solutionFolderInfo);
            var solutionCount = solutionFiles.Count();
            if(solutionCount > 0) {
                slnPath = solutionFiles[0];
            }
            if(solutionCount > 1) {
                MyMessageProcessor.ConsoleWrite("There are many sln files!", ConsoleColor.Red);
                MyMessageProcessor.ConsoleWriteLine();
            }

            string[] projectFiles = TryGetProjectFiles(solutionFolderInfo);
            int projectsCount = projectFiles.Count();
            switch(projectsCount) {
                case 1:
                    csPaths = new List<string>() { projectFiles[0] };
                    WorkWithSolution();
                    break;
                case 0:
                    OpenFolder();
                    break;
                default:
                    var dxPaths = FindDXCsprojs(projectFiles);
                    if(dxPaths.Count > 0) {
                        csPaths = dxPaths;
                        WorkWithSolution();
                    } else {
                        OpenFolder();
                    }
                    break;
            }
        }

        bool isXafSolution = false;
        bool hasWebProject = false;
        private List<string> FindDXCsprojs(string[] solutionsFiles) {
            List<string> list = new List<string>();
            foreach(string st in solutionsFiles) {
                var tx = MyFileWorker.StreamReaderReadToEnd(st);
                if(tx.Contains(@"Reference Include=""DevExpress.")) {
                    list.Add(st);
                }
                if(!isXafSolution && tx.Contains(@"Reference Include=""DevExpress.ExpressApp")) {
                    isXafSolution = true;
                }
                if(st.Contains(".Web")) {
                    hasWebProject = true;
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
            if(st.Count() == 0) {
                st = MyFileWorker.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).ToArray();
            }
            return st;
        }
        protected internal List<string> GetUnexpectedFiles(DirectoryInfo dirInfo) {
            List<string> result = new List<string>();
            var filesArray = filesToDetect.Split(';');
            var namesToEx = namesToExclude.Split(';');
            foreach(var pattern in filesArray) {
                var tmpResult = MyFileWorker.EnumerateFiles(dirInfo.FullName, "*." + pattern, SearchOption.AllDirectories).ToArray();
                foreach(var candidate in tmpResult) {
                    var fi = new FileInfo(candidate);
                    bool flag = false;
                    foreach(var nameEx in namesToEx) {
                        if(fi.Name.Contains(nameEx)) {
                            flag = true;
                            break;
                        }
                    }
                    if(flag) {
                        continue;
                    }
                    result.Add(candidate);
                }

            }
            return result;
        }
        string[] TryGetSolutionFiles(DirectoryInfo dirInfo) {
            var st = MyFileWorker.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).ToArray();
            return st;
        }
        void GetMessageInfo() {//4 td
            MessagesList = new List<ConverterMessages>();
            csProjProcessor = CreateCSProjProcessor();
            GetAllVersions();
            GetInstalledVersions();
            GetProjectVersion();
            isMainMajor = currentProjectVersion.Major == mainMajorLastVersion.Major;
            if(isExample) {
                MessagesList.Add(ConverterMessages.OpenSolution);
            } else {
                if(currentProjectVersion.CompareTo(Version.Zero) == 0) {
                    MessagesList.Add(ConverterMessages.OpenSolution);
                } else {
                    //endif
                    currentInstalledMajor = installedVersions.Where(x => x.Major == currentProjectVersion.Major).FirstOrDefault();
                    isCurrentVersionMajorInstalled = currentInstalledMajor != null;
                    if(isCurrentVersionMajorInstalled) {
                        if(isMainMajor) {
                            if(currentProjectVersion.CompareTo(mainMajorLastVersion) == 0) {
                                MessagesList.Add(ConverterMessages.OpenSolution);
                            } else {
                                if(currentProjectVersion.Minor == 0) {
                                    MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                                } else {
                                    MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                                    MessagesList.Add(ConverterMessages.ExactConversion);
                                }
                            }
                        } else {
                            if(currentProjectVersion.CompareTo(currentInstalledMajor) == 0) {
                                MessagesList.Add(ConverterMessages.OpenSolution);
                                MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                            } else {
                                if(currentProjectVersion.Minor == 0) {
                                    MessagesList.Add(ConverterMessages.LastMinor);
                                    MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                                } else {
                                    MessagesList.Add(ConverterMessages.LastMinor);
                                    MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                                    MessagesList.Add(ConverterMessages.ExactConversion);
                                }
                            }
                        }
                    } else {
                        LastMinorOfCurrentMajor = FindLastVersionOfMajor(currentProjectVersion.Major);
                        if(currentProjectVersion.Minor == 0 || currentProjectVersion.Minor == LastMinorOfCurrentMajor.Minor) {
                            MessagesList.Add(ConverterMessages.LastMinor);
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                        } else if(LastMinorOfCurrentMajor.Major == 0) {
                            MessagesList.Add(ConverterMessages.MainMajorLastVersion);
                        } else {
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
            if(csProjProcessor == null)
                csProjProcessor = new CSProjProcessor(csPaths, MyFileWorker);
            return csProjProcessor;
        }
        public List<XElement> AllVersionsList;
        protected internal void GetAllVersions() {
            var xDoc = MyFileWorker.LoadXDocument(Properties.Settings.Default.FileWithVersionsPath);
            var allVersionElement = xDoc.Element("Versions").Element("AllVersions");
            AllVersionsList = allVersionElement.Elements().ToList();
        }


        protected internal void GetInstalledVersions() {//5 td
            installedVersions = new List<Version>();
            List<string> versions = MyFileWorker.GetRegistryVersions("SOFTWARE\\DevExpress\\Components\\");
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter-console.exe";
            foreach(string rootPath in versions) {
                var rootPath2 = Path.Combine(rootPath, projectUpgradeToolRelativePath);
                string libVersion = GetProjectUpgradeVersion(rootPath2);
                var vers = new Version(libVersion);
                vers.Path = rootPath2;
                installedVersions.Add(vers);
            }
            mainMajorLastVersion = installedVersions.Where(y => y.Major <= lastReleasedVersion).Max();
        }
        string GetProjectUpgradeVersion(string projectUpgradeToolPath) {//5.1 td
            string assemblyFullName = MyFileWorker.AssemblyLoadFileFullName(projectUpgradeToolPath);
            string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatch = regexVersion.Match(assemblyFullName);
            string versValue = versionMatch.Groups["Version"].Value;
            return versValue;
        }

        private void GetProjectVersion() {//6 td
            currentProjectVersion = csProjProcessor.GetCurrentVersion();
        }
        private ConverterMessages PrintMessage() { //7 tt
            if(isExample) {
                MyMessageProcessor.ConsoleWrite("The current project version is an ");
                MyMessageProcessor.ConsoleWrite("example", ConsoleColor.Red);
            } else {
                MyMessageProcessor.ConsoleWrite("The current project version is ");
                MyMessageProcessor.ConsoleWrite(currentProjectVersion.ToString(), ConsoleColor.Red);
            }
            MyMessageProcessor.ConsoleWriteLine();
            int k = 1;
            foreach(ConverterMessages msg in MessagesList) {
                PrintConverterMessage(msg, k++.ToString());
            }
            int index = -1;

            while(index == -1 || (index != 9 && index > MessagesList.Count - 1)) {
                ConsoleKey enterKey = MyMessageProcessor.ConsoleReadKey(false);
                index = GetValueFromConsoleKey(enterKey);
            }
            if(index == 9)
                return ConverterMessages.OpenFolder;

            return MessagesList[index - 1];
        }

        private void PrintConverterMessage(ConverterMessages msg, string key) {//8tt
            if(msg == ConverterMessages.OpenSolution) {
                MyMessageProcessor.ConsoleWrite("To open solution press: ");
                MyMessageProcessor.ConsoleWrite(key, ConsoleColor.Red);
                MyMessageProcessor.ConsoleWriteLine();
                return;
            }
            if(msg == ConverterMessages.OpenFolder) {
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
            switch(msg) {
                case ConverterMessages.ExactConversion:
                    return currentProjectVersion.ToString();
                case ConverterMessages.LastMinor:
                    if(currentInstalledMajor == null) {
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
            if(key > (ConsoleKey.NumPad0) && key <= (ConsoleKey.NumPad9)) { // numpad
                value = (int)key - ((int)ConsoleKey.NumPad0);
            } else if((int)key > ((int)ConsoleKey.D0) && (int)key <= ((int)ConsoleKey.D9)) { // regular numbers
                value = (int)key - ((int)ConsoleKey.D0);
            }
            return value;
        }

        void CopyBatchFiles() {
            var dropBoxPath = Properties.Settings.Default.DropboxPath;
            var slnFolder = Path.GetDirectoryName(slnPath);
            if(slnFolder != null) {
                var delFileName = Path.Combine(slnFolder, "delbinobj.bat");
                if(MyFileWorker.FileExist(delFileName))
                    return;
                MyFileWorker.FileCopy(Path.Combine(dropBoxPath, @"work\templates\MainSolution\delbinobj.bat"), delFileName);
                gitBatchFile = Path.Combine(slnFolder, "createGit.bat");
                MyFileWorker.FileCopy(Path.Combine(dropBoxPath, @"work\templates\MainSolution\createGit.bat"), gitBatchFile);
                var gitIgnoreDestination = Path.Combine(slnFolder, ".gitignore");
                if(File.Exists(gitIgnoreDestination)) {
                    File.Delete(gitIgnoreDestination);
                }
                MyFileWorker.FileCopy(Path.Combine(dropBoxPath, @"work\templates\MainSolution\.gitignoreToCopy"), gitIgnoreDestination);
            }
        }
        private void ProcessProject(ConverterMessages message) {//12
            if(message == ConverterMessages.OpenFolder) {
                OpenFolder();
                return;
            }
            CopyBatchFiles();
            if(isXafSolution) {
                MakeApplicationProjectFirst();
                CorrectConnectionStringsInConfigFiles();
            }

            csProjProcessor.DisableUseVSHostingProcess();
            csProjProcessor.AddImagesLibraries();
            csProjProcessor.CorrectFrameworkVersionIfNeeded();
            if(isExample) {
                if(isMainMajor) {
                    csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                    csProjProcessor.SaveNewCsProj();
                } else {

                    csProjProcessor.SaveNewCsProj();
                    ConvertProjectWithDxConverter(mainMajorLastVersion);
                }
            } else { //check how to avoid csProjProcessor.SaveNewCsProj();
                if(!(currentProjectVersion.CompareTo(Version.Zero) == 0)) { //there are dx libs
                    csProjProcessor.RemoveLicense();
                    switch(message) {
                        case ConverterMessages.MainMajorLastVersion:
                            if(isMainMajor && !hasWebProject) {
                                csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                                csProjProcessor.SaveNewCsProj();
                            } else {
                                csProjProcessor.SaveNewCsProj();
                                ConvertProjectWithDxConverter(mainMajorLastVersion);
                            }
                            break;
                        case ConverterMessages.LastMinor:
                            if(isCurrentVersionMajorInstalled) {
                                if(!hasWebProject) {
                                    csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                                    csProjProcessor.SaveNewCsProj();
                                } else {
                                    csProjProcessor.SaveNewCsProj();
                                    ConvertProjectWithDxConverter(currentInstalledMajor);
                                }
                            } else {
                                csProjProcessor.SaveNewCsProj();

                                if(GetIfLibrariesPersist()) {
                                    break;
                                }
                                ConvertProjectWithDxConverter(LastMinorOfCurrentMajor);
                            }
                            break;
                        case ConverterMessages.ExactConversion:
                            csProjProcessor.SaveNewCsProj();

                            if(GetIfLibrariesPersist()) {
                                break;
                            }
                            ConvertProjectWithDxConverter(currentProjectVersion);
                            break;
                        default:
                            csProjProcessor.SetSpecificVersionFalseAndRemoveHintPath();
                            csProjProcessor.SaveNewCsProj();
                            break;
                    }
                } else {
                    csProjProcessor.SaveNewCsProj();
                }
            }

            OpenSolution();

        }
        public void CorrectConnectionString(JObject jsonObject, string dbName) {

            var connStrings = jsonObject.SelectToken("ConnectionStrings") as JObject;
            //if(connStrings == null) {
            //    return;
            //}
            var oldConnectionString = connStrings["ConnectionString"];
           // if(oldConnectionString != null) {
                var oldConnValue = ((JValue)oldConnectionString).Value;
                connStrings.Property("ConnectionString").Remove();
                connStrings.Add(new JProperty("xOldConnectionString", oldConnValue));
         //   }
            string newConnectionString = string.Format(@"Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog={0}usr", dbName);
            connStrings.Add(new JProperty("ConnectionString", newConnectionString));
        }

        public void CorrectConnectionString(XDocument xDocument, string dbName) {
            var el = xDocument.Root;
            var el2 = xDocument.Root.Elements();
            var configNode = xDocument.Root.Elements().Where(x => x.Name.LocalName == "connectionStrings").FirstOrDefault();
            if(configNode == null) {
                return;
            }
            var configs = configNode.Elements();
            // var nameXName = XName.Get("name", configNode.Name.Namespace.NamespaceName); // problem config file here e244
            var nameXName = "name";
            //  var oldConfig = configs.Where(x => x.Attribute(nameXName)!=null && x.Attribute(nameXName).Value == "ConnectionString").FirstOrDefault();
            var oldConfig = configs.Where(x => x.Attribute(nameXName).Value == "ConnectionString").FirstOrDefault();
            if(oldConfig != null) {
                oldConfig.Attribute(nameXName).Value = "xOldConnectionString";
            }

            XName addXName = XName.Get("add", configNode.Name.Namespace.NamespaceName);
            XElement newConfigElement = new XElement(addXName);
            XAttribute connNameAttr = new XAttribute(nameXName, "ConnectionString");
            newConfigElement.Add(connNameAttr);
            XName connStringXName = XName.Get("connectionString", configNode.Name.Namespace.NamespaceName);
            string newConnectionString = string.Format(@"Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog={0}usr", dbName);
            XAttribute connAttr = new XAttribute(connStringXName, newConnectionString);
            newConfigElement.Add(connAttr);
            configNode.Add(newConfigElement);
        }
        public string GetDBNameFromSlnPath(string slnPath) {
            Regex ticketRegex = new Regex(@"T\d{6,7}");
            Match ticketMatch = ticketRegex.Match(slnPath);
            string folderNumber;
            if(ticketMatch.Success) {
                folderNumber = "dx" + ticketMatch.Value;
            } else {
                folderNumber = "dxSolution" + new Random().Next(12, 1234);
            }
            var dbName = string.Format("d{0}-{1}", DateTime.Today.DayOfYear, folderNumber);
            return dbName;
        }
        private void CorrectConnectionStringsInConfigFiles() {
            List<string> configFiles = new List<string>();
            configFiles.AddRange(MyFileWorker.DirectoryGetFiles(solutionFolderName, "app.config"));
            configFiles.AddRange(MyFileWorker.DirectoryGetFiles(solutionFolderName, "web.config"));
            string dbName = GetDBNameFromSlnPath(slnPath);
            foreach(string configFile in configFiles) {
                var configXML = MyFileWorker.LoadXDocument(configFile);
                CorrectConnectionString(configXML, dbName);
                MyFileWorker.SaveXDocument(configXML, configFile);
            }

            var jsonConfigFiles = MyFileWorker.DirectoryGetFiles(solutionFolderName, "appsettings.json");
            foreach(var jsonFile in jsonConfigFiles) {
                var jsonObject = MyFileWorker.LoadJObject(jsonFile);
                CorrectConnectionString(jsonObject, dbName);

                MyFileWorker.SaveJObject(jsonFile, jsonObject);
            }

        }

        public void MakeApplicationProjectFirst() {

            /*
            Regex appsProjectsRegex = new Regex(@"Project((.|\n)(?!Module))+?EndProject");//++ app projects
            Regex winProejctRegex= new Regex(@"Project(.(?!Module))+?Win\.(.|\n)+?EndProject");//++ win project
            Regex webProjectRegex = new Regex(@"Project(.(?!Module))+?Web\.(.|\n)+?EndProject");//++ web project
            Regex modulesProjectsRegex = new Regex(@"Project.+Module(.|\n)*?EndProject");//++ modules
            Regex allProjectsRegex = new Regex(@"Project(.|\n)*?EndProject");//++ all projects modules
            Regex allProjectsInOneStringRegex = new Regex(@"Project(.|\n)*EndProject");//++ all projects modules in one string
            http://regexstorm.net/tester
             */
            var slnText = MyFileWorker.StreamReaderReadToEnd(slnPath);
            Regex allProjectsInOneStringRegex = new Regex(@"\nProject(.|\n)*EndProject");//++ all projects modules in one string
            Regex modulesProjectsRegex = new Regex(@"\nProject.+Module(.|\n)*?EndProject");//++ modules
            Regex appsProjectsRegex = new Regex(@"\nProject((.|\n)(?!Module))+?EndProject");//++ app projects

            var allModulesString = allProjectsInOneStringRegex.Match(slnText).Value;
            var appProjects = appsProjectsRegex.Matches(slnText);
            var modulesProjects = modulesProjectsRegex.Matches(slnText);

            var appsArray = appProjects.Cast<Match>().Select(x => x.Value).ToArray();
            var modulesArray = modulesProjects.Cast<Match>().Select(x => x.Value).ToArray();
            var newModulesString = string.Join("\r", appsArray) + "\r" + string.Join("\r", modulesArray);
            slnText = slnText.Replace(allModulesString, newModulesString);
            MyFileWorker.StreamWriterWriteLine(slnPath, slnText);
        }

        public Version FindLastVersionOfMajor(int major) {//15tt
            var tmpRes = AllVersionsList.Where(x => x.FirstAttribute.Value.Split('.')[0] + x.FirstAttribute.Value.Split('.')[1] == major.ToString() && x.FirstAttribute.Value.Split('.')[2].Length < 3).FirstOrDefault();
            string res = "";
            if(tmpRes != null)
                res = tmpRes.FirstAttribute.Value;
            return new Version(res);
        }

        private bool GetIfLibrariesPersist() {//14
            DirectoryInfo dirInfo = new DirectoryInfo(solutionFolderName);
            var v = MyFileWorker.DirectoryEnumerateFiles(dirInfo.FullName, "DevExpress*.dll", SearchOption.AllDirectories).ToList();
            return v.Count == csProjProcessor.DXLibrariesCount;
        }

        private void ConvertProjectWithDxConverter(Version v) {//16
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Properties.Settings.Default.DXConverterPath;
            string versionConverterFormat = v.ToString(true);
            psi.Arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\" \"true\"", solutionFolderName, versionConverterFormat, v.Path);
            MyFileWorker.ProcessStart(psi.FileName, psi.Arguments);
        }
        private void OpenFolder() { //tt
            MyFileWorker.OpenFolder(solutionFolderName);
        }
        private void OpenSolution() {//tt
            if(slnPath != null) {
                MyFileWorker.ProcessStart(slnPath);
            } else {
                MyFileWorker.ProcessStart(csPaths[0]);
            }
            if(gitBatchFile != null) {
                MyFileWorker.ProcessStart(gitBatchFile);
            }
        }
    }

    public class VersionComparer : IComparer<string> {

        public int Compare(string x, string y) {
            int counter = 0, res = 0;
            while(counter < 3 && res == 0) {
                int versionX = Convert.ToInt32(x.Split('.')[counter].Replace("_new", ""));
                int versionY = Convert.ToInt32(y.Split('.')[counter]);
                res = Comparer.Default.Compare(versionX, versionY);
                counter++;
            }
            return -res;
        }
    }


}
