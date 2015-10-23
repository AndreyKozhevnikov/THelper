using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    if (GetFile(dirInfo, out path)) {
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

        public static bool GetFile(DirectoryInfo dirInfo, out string path) {
            path = Directory.EnumerateFiles(dirInfo.FullName, "*.sln", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrEmpty(path)) {
                path = Directory.EnumerateFiles(dirInfo.FullName, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
            }

            return !string.IsNullOrEmpty(path);
        }
    }
}
