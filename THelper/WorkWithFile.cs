using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace THelper {
    public interface IWorkWithFile {
        XDocument LoadXDocument(string projectPath);
        void SaveXDocument(XDocument projDocument, string projectPath);
        DirectoryInfo CreateDirectory(string _path);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    }

    public class CustomWorkWithFile : IWorkWithFile {
        public DirectoryInfo CreateDirectory(string _path) {
         return   Directory.CreateDirectory(_path);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public XDocument LoadXDocument(string projectPath) {
            return XDocument.Load(projectPath);
        }
        public void SaveXDocument(XDocument projDocument, string projectPath) {
            projDocument.Save(projectPath);
        }
    }
}
