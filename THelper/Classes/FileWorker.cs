using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace THelper {
    public interface IFileWorker {
        JObject LoadJObject(string jsonPath);
        void SaveJObject(string path, JObject jObj);
        XDocument LoadXDocument(string projectPath);
        void SaveXDocument(XDocument projDocument, string projectPath);
        DirectoryInfo CreateDirectory(string _path);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
        string AssemblyLoadFileFullName(string path);
        List<string> GetRegistryVersions(string path);
        void ProcessStart(string path);
        void ProcessStart(string fileName, string arguments, bool _wait);
        void ProcessStart(ProcessStartInfo startInfo);
        void ProcessStart(string fileName, string arguments);
        void DirectoryMove(string sourceDirName, string destDirName);
        void OpenFolder(string path);
        IEnumerable<string> DirectoryEnumerateFiles(string path, string searchPattern, SearchOption searchOption);
        string[] DirectoryGetDirectories(string path);
        string StreamReaderReadToEnd(string path);
        void StreamWriterWriteLine(string path, string text);
        string[] DirectoryGetFiles(string path, string pattern);
        DirectoryInfo[] DirectoryGetDirectories(DirectoryInfo solutionFolderInfo, string v, SearchOption allDirectories);
        void DirectoryDelete(string fullName, bool v);
        void FileCopy(string source, string destination);
        bool FileExist(string path);
    }

    public class FileWorker : IFileWorker {
        static string totalCmdPath = @"C:\Program Files\totalcmd\TOTALCMD64.EXE";

        public void DirectoryMove(string sourceDirName, string destDirName) {
            Directory.Move(sourceDirName, destDirName);
        }

        public string AssemblyLoadFileFullName(string path) {
            try {
                var assembly = Assembly.LoadFile(path);
                return assembly.FullName;
            }
            catch {
                return null;
            }
        }
        public string[] DirectoryGetFiles(string path, string pattern) {
            return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
        }
        public DirectoryInfo CreateDirectory(string _path) {
            return Directory.CreateDirectory(_path);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public List<string> GetRegistryVersions(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var lst = regKey.GetSubKeyNames();
            List<string> resList = new List<string>();
            foreach(string st in lst) {
                RegistryKey dxVersionKey = regKey.OpenSubKey(st);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                resList.Add(projectUpgradeToolPath);
            }
            return resList;
        }

        public string[] GetSubKeyNames(string key) {
            var regKey = Registry.LocalMachine.OpenSubKey(key);
            return regKey.GetSubKeyNames();
        }

        public XDocument LoadXDocument(string projectPath) {
            return XDocument.Load(projectPath);
        }

        public void ProcessStart(string path) {
            Process.Start(path);
        }

        public void SaveXDocument(XDocument projDocument, string projectPath) {
            var fileInfo = new FileInfo(projectPath);
            if(fileInfo.IsReadOnly)
                fileInfo.IsReadOnly = false;
            projDocument.Save(projectPath);
        }

        public void ProcessStart(string fileName, string arguments, bool wait) { //???
            var p = Process.Start(fileName, arguments);
            if(wait) {
                p.WaitForExit();
            }
        }

        public IEnumerable<string> DirectoryEnumerateFiles(string path, string searchPattern, SearchOption searchOption) {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public string[] DirectoryGetDirectories(string path) {
            return Directory.GetDirectories(path);
        }

        public void ProcessStart(ProcessStartInfo startInfo) {
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }



        public void ProcessStart(string fileName, string arguments) {
            var p = Process.Start(fileName, arguments);
            p.WaitForExit();
        }


        public string StreamReaderReadToEnd(string path) {
            var sr = new StreamReader(path);
            string st = sr.ReadToEnd();
            sr.Close();
            return st;

        }

        public void OpenFolder(string path) {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = totalCmdPath;
            startInfo.Arguments = string.Format("/O /T /R=\"{0}\"", path);
            Process.Start(startInfo);
        }

        public void StreamWriterWriteLine(string path, string text) {
            using(StreamWriter writer = new StreamWriter(path)) {
                writer.Write(text);
                writer.Flush();
            }
        }

        public DirectoryInfo[] DirectoryGetDirectories(DirectoryInfo solutionFolderInfo, string v, SearchOption allDirectories) {
            return solutionFolderInfo.GetDirectories(v, allDirectories);
        }

        public void DirectoryDelete(string fullName, bool v) {
            try {
                Directory.Delete(fullName, v);
            }
            catch(IOException) {
                Directory.Delete(fullName, v);
            }
            catch(UnauthorizedAccessException) {
                Directory.Delete(fullName, v);
            }
        }

        public void FileCopy(string source, string destination) {
            File.Copy(source, destination);
        }

        public bool FileExist(string path) {
            return File.Exists(path);
        }

        public JObject LoadJObject(string jsonPath) {
            var jsonString = File.ReadAllText(jsonPath);
            var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
            return (JObject)jsonObject;
        }

        public void SaveJObject( string path,JObject jObj) {
            File.WriteAllText(path, jObj.ToString());
        }
    }
}
