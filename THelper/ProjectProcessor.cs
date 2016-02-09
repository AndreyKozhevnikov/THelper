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

namespace THelper {
    public class ProjectProcessor {
        private string filePath;

        public ProjectProcessor(string filePath) {
            // TODO: Complete member initialization
            this.filePath = filePath;
        }


        internal void ProcessProject() {
            try {
                string flPath = filePath;
                string value = Properties.Settings.Default.WinRarPath;
                string arguments = " x \"" + flPath + "\"";
                string fileName = flPath.Split('\\').LastOrDefault();

                string tmp = Directory.GetParent(flPath) + "\\" + fileName.Replace(" ", "_");
                int dotIndex = tmp.LastIndexOf('.');
                string destFolder = tmp.Remove(dotIndex);

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
                    // UpdgradeProject(stringVersion);
                    Process.Start(path);
                }
                else {
                    Process.Start("Explorer.exe", destFolder);
                }

            }
            catch (Exception exp) {

                Console.WriteLine(filePath);
                Console.WriteLine("---");
                Console.WriteLine(exp);
                Console.ReadLine();
            }
        }
        private void UpdgradeProject(string stringWithVersion) {
            GetCurrentProject(stringWithVersion);
        }

        private Project GetCurrentProject(string stringWithVersion) {
            string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatch = regexVersion.Match(stringWithVersion);
            if (versionMatch == null || !versionMatch.Success) {
                return new Project(null) { Major = 0 };
            }
            return new Project(versionMatch.Groups["Version"].Value);
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
            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.Contains("DevExpress"));
            fullSolString = null;
            if (dxlibraries.Count() > 0)
                fullSolString = dxlibraries.First().Attribute("Include").ToString();

            foreach (XElement dxlib in dxlibraries) {

                var v = dxlib.Value;
                var v1 = dxlib.NodeType;
                var v2 = dxlib.Name;
                var v3 = dxlib.Elements();
                var v4 = dxlib.Element(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName));
                dxlib.SetElementValue(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName), false);
            }

            string resultString = xlroot.ToString();
            StreamWriter sw = new StreamWriter(flPath, false);
            sw.Write(resultString);
            sw.Close();
        }
    }
}
