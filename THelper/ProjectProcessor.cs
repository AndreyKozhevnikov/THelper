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
        private string filePath;

        public ProjectProcessor(string _filePath) {
            this.filePath = _filePath;
        }

        internal void ProcessProject() {
            string value = Properties.Settings.Default.WinRarPath;
            string argumentsFilePath = " x \"" + filePath + "\"";
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            string destFolder = Directory.GetParent(filePath) + "\\" + fileName.Replace(" ", "_");
            bool isDxSample = filePath.EndsWith(".dxsample");
            var dirInfo = Directory.CreateDirectory(destFolder);
            var argsFullWinRar = argumentsFilePath + " " + @"""" + destFolder + @"""";
            var winrarProc = Process.Start(value, argsFullWinRar);
            winrarProc.WaitForExit();
            string slnPath = string.Empty;
            string cspath = string.Empty;
            bool isSoluiton = GetSolutionFiles(dirInfo, out slnPath, out cspath);
            if (isSoluiton) {

                string dxLibraryString = ProcessCsprojFile(cspath);
                if (dxLibraryString != null)
                    UpdgradeProject(destFolder, dxLibraryString, isDxSample);
                Process.Start(slnPath);
            }
            else {
                Process.Start(destFolder);
            }
        }
        private void UpdgradeProject(string _projFolderPath, string _dxLibraryString, bool _isDxSample) {
            ProjectUpgrader upgrader = new ProjectUpgrader(_projFolderPath, _dxLibraryString, _isDxSample);
            upgrader.Start();

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

        private string ProcessCsprojFile(string _csPath) {
            XmlTextReader reader = new XmlTextReader(_csPath);
            XElement xlroot = XElement.Load(reader);
            reader.Close();

            var elements = xlroot.Elements();

            var licGroup = elements.Where(x => x.Elements().Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).Count() > 0).FirstOrDefault();
            if (licGroup != null) {
                var lic = licGroup.Elements().Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).First();
                lic.Remove();
            }

            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.Contains("DevExpress"));

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
    }
}
