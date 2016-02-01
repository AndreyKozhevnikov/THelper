using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace THelper {
    class Program {
        static void Main(string[] args) {
            //Console.WriteLine("---");
            //Console.WriteLine(args[0]);
            //Console.ReadLine();
            try {
                if (args.Count() > 0 && args[0] is string) {
                    string value = Properties.Settings.Default.WinRarPath;
                    string arguments = " x \"" + args[0].ToString() + "\"";
                    string fileName = args[0].ToString().Split('\\').LastOrDefault();

                    string tmp = Directory.GetParent(args[0].ToString()) + "\\" + fileName.Replace(" ", "_");
                    int dotIndex = tmp.LastIndexOf('.');
                    string destFolder = tmp.Remove(dotIndex);

                    var dirInfo = Directory.CreateDirectory(destFolder);
                    var argsforWR = arguments + " " + @"""" + destFolder + @"""";
                    var proc = Process.Start(value, argsforWR);
                    proc.WaitForExit();
                    string path = string.Empty;
                    string cspath = string.Empty;
                    if (GetFile(dirInfo, out path, out cspath)) {
                        FixCsprojSpecificVersion(cspath);
                        Process.Start(path);
                    }
                    else {
                        Process.Start("Explorer.exe", destFolder);
                    }
                }
            }
            catch (Exception exp) {

                Console.WriteLine(args[0]);
                Console.WriteLine("---");
                Console.WriteLine(exp);
                Console.ReadLine();
            }

        }

        public static bool GetFile(DirectoryInfo dirInfo, out string path, out string csprojpath) {
            path = Directory.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
            csprojpath = Directory.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(path)) {
                path = csprojpath;
            }
            return !string.IsNullOrEmpty(path);
        }

        private static void FixCsprojSpecificVersion(string cspath) {
            var flPath = cspath;
            StreamReader sr = new StreamReader(flPath);
            XmlTextReader reader = new XmlTextReader(flPath);
            sr.Close();
            // XStreamingElement xsr=new XStreamingElement()
            XElement xlroot = XElement.Load(reader);
            sr.Close();
            reader.Close();

            var elements = xlroot.Elements();
            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.Contains("DevExpress"));

            foreach (XElement dxlib in dxlibraries) {

                var v = dxlib.Value;
                var v1 = dxlib.NodeType;
                var v2 = dxlib.Name;
                var v3 = dxlib.Elements();
                var v4 = dxlib.Element(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName));
                dxlib.SetElementValue(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName), false);
            }

            var currentVersionFull = dxlibraries.First().Attribute("Include").Value;
            //var vIndex = currentVersionFull.IndexOf("Version=");
            //var currentVersion = currentVersionFull.Substring(vIndex + 8, 9);

            var st = currentVersionFull.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)[1];
            var currentVersion = st.Replace("Version=", "");

            string resultString = xlroot.ToString();
            StreamWriter sw = new StreamWriter(flPath, false);
            sw.Write(resultString);
            sw.Close();
        }
    }
}
