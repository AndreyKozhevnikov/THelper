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

        public ProjectProcessor(string filePath) {
            this.filePath = filePath;
        }

        internal void ProcessProject() {
            string flPath = filePath;
            string value = Properties.Settings.Default.WinRarPath;
            string arguments = " x \"" + flPath + "\"";
            string fileName = flPath.Split('\\').LastOrDefault();

            string tmp = Directory.GetParent(flPath) + "\\" + fileName.Replace(" ", "_");
            int dotIndex = tmp.LastIndexOf('.');
            string destFolder = tmp.Remove(dotIndex);
            bool isDxSample = flPath.EndsWith(".dxsample");
            var dirInfo = Directory.CreateDirectory(destFolder);
            var argsforWR = arguments + " " + @"""" + destFolder + @"""";
            var proc = Process.Start(value, argsforWR);
            proc.WaitForExit();
            string path = string.Empty;
            string cspath = string.Empty;
            bool ifGetFileSuccess = GetSolutionFile(dirInfo, out path, out cspath);
            if (ifGetFileSuccess) {
                string stringVersion;
                FixCsprojSpecificVersion(cspath, out stringVersion);
                if (stringVersion != null)
                    UpdgradeProject(destFolder, stringVersion, isDxSample);
                Process.Start(path);
            }
            else {
                Process.Start("Explorer.exe", destFolder);
            }
        }
        private void UpdgradeProject(string projFolderPath, string stringWithVersion, bool isDxSmpl) {
            ProjectUpgrader upgrader = new ProjectUpgrader(projFolderPath, stringWithVersion, isDxSmpl);
            upgrader.Start();

        }

        public bool GetSolutionFile(DirectoryInfo dirInfo, out string path, out string csprojpath) {
            path = Directory.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
            csprojpath = Directory.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (csprojpath == null)
                csprojpath = Directory.EnumerateFiles(dirInfo.FullName, "*.vbproj", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(path)) {
                path = csprojpath;
            }
            return !string.IsNullOrEmpty(path);
        }

        private void FixCsprojSpecificVersion(string cspath, out string fullSolString) {
            var flPath = cspath;
            StreamReader sr = new StreamReader(flPath);
            XmlTextReader reader = new XmlTextReader(flPath);
            sr.Close();
            XElement xlroot = XElement.Load(reader);
            sr.Close();
            reader.Close();

            var elements = xlroot.Elements();

            var licGroup = elements.Where(x => x.Elements().Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).Count() > 0).FirstOrDefault();
            if (licGroup != null) {
                var lic = licGroup.Elements().Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).First();
                lic.Remove();
            }

            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.Contains("DevExpress"));
            fullSolString = null;
            if (dxlibraries.Count() > 0)
                fullSolString = dxlibraries.First().Attribute("Include").ToString();

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
            StreamWriter sw = new StreamWriter(flPath, false);
            sw.Write(resultString);
            sw.Close();
        }
    }
}
