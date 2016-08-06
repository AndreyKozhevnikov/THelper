using Microsoft.Win32;
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
    public interface IWorkWithFile {
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
        IEnumerable<string> DirectoryEnumerateFiles(string path, string searchPattern, SearchOption searchOption);
        string[] DirectoryGetDirectories(string path);
    }

    public class CustomWorkWithFile : IWorkWithFile {
        public string AssemblyLoadFileFullName(string path) {
            try {
                var assembly = Assembly.LoadFile(path);
                return assembly.FullName;
            }
            catch {
                return null;
            }
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
            foreach (string st in lst) {
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
            projDocument.Save(projectPath);
        }

        public void ProcessStart(string fileName, string arguments, bool wait) {
            var p = Process.Start(fileName, arguments);
            p.WaitForExit();
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
            var p = Process.Start(fileName,arguments);
            p.WaitForExit();
        }
    }
}
