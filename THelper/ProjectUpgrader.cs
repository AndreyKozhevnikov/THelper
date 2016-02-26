using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace THelper {
    public class ProjectUpgrader {
        const int minSupportedMajorVersion = 122;
        string projPath;
        string dxLibraryString;


        Dictionary<int, string> installedSupportedMajorsAndPCPaths = new Dictionary<int, string>();
        bool isDxSample;

        public ProjectUpgrader(string _projPath, string _dxLibraryString, bool _isDxSample) {
            projPath = _projPath;
            dxLibraryString = _dxLibraryString;
            isDxSample = _isDxSample;
        }

        internal void Start() {
            Version projectVersion = GetVersionFromContainingString(dxLibraryString);
            List<Version> installedVersions = PopulateInstalledDxVersions();
            int maxMajor = installedVersions.Max(x => x.Major);
            Version dxGreatestVersion = installedVersions.Where(x => x.Major == maxMajor).First();
         
            if (isDxSample) {
                DXProjectUpgrade(dxGreatestVersion.Major, projPath);
                return;
            }

            Version currentProjectVersionInstalled = installedVersions.Where(x => x.Major == projectVersion.Major).FirstOrDefault();
            if (currentProjectVersionInstalled == null) {
                currentProjectVersionInstalled = Version.Zero;
            }

            Version versionForUpdate;
            if (currentProjectVersionInstalled.IsZero)
                versionForUpdate = dxGreatestVersion;
            else
                versionForUpdate = currentProjectVersionInstalled;

            bool isVersionForUpdateGreatest;

            isVersionForUpdateGreatest = versionForUpdate.CompareTo(projectVersion) > 0;
            if (versionForUpdate.Major < minSupportedMajorVersion || projectVersion.IsZero || !isVersionForUpdateGreatest) {
                return;
            }
      


            PrintMessage(projectVersion, versionForUpdate, dxGreatestVersion);
            var enterKey = Console.ReadKey(false);
            switch (enterKey.Key) {
                case ConsoleKey.NumPad1:
                case ConsoleKey.D1:
                    DXProjectUpgrade(versionForUpdate.Major, projPath);
                    break;
                case ConsoleKey.NumPad2:
                case ConsoleKey.D2:
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName=@"\\corp\internal\common\4Nikishina\Converter\EXE\Converter.exe";
                    string versionConverterFormat = projectVersion.ToString(true);
                    psi.Arguments = string.Format("{0} \\\"{1}\\\"", versionConverterFormat, projPath);
                    var proc = System.Diagnostics.Process.Start(psi);
                    proc.WaitForExit();
                    break;
                case ConsoleKey.NumPad3:
                case ConsoleKey.D3:
                    DXProjectUpgrade(dxGreatestVersion.Major, projPath);
                    break;
            }
        }

        void DXProjectUpgrade(int _major, string _projPath) {
            string toolPath = installedSupportedMajorsAndPCPaths[_major];
            _projPath = "\"" + _projPath + "\"";
            Process updgrade = Process.Start(toolPath, _projPath);
            updgrade.WaitForExit();
        }

        void PrintMessage(Version _projectVersion, Version _versionForUpdate, Version _dxGreatestVersion) {
            Console.Write("The current project version is ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_projectVersion);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            PrintConvertTheProject(_versionForUpdate, 1);

            if (_projectVersion.Minor > 0)
                PrintConvertTheProject(_projectVersion, 2);

            if (_versionForUpdate != _dxGreatestVersion)
                PrintConvertTheProject(_dxGreatestVersion, 3);
        }

        public void PrintConvertTheProject(Version v, int key) {
            Console.Write("To convert the project to the ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(v);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(", press ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(key);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public Version GetVersionFromContainingString(string stringWithVersion) {
            string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatch = regexVersion.Match(stringWithVersion);
            if (versionMatch == null || !versionMatch.Success) {
                return Version.Zero;
            }
            string versValue = versionMatch.Groups["Version"].Value;
            return new Version(versValue);
        }

        private List<Version> PopulateInstalledDxVersions() {
            var installedVersions = new List<Version>();
            RegistryKey dxpKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DevExpress\\Components\\");
            string[] versions = dxpKey.GetSubKeyNames();
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter.exe";
            foreach (string strVersion in versions) {
                RegistryKey dxVersionKey = dxpKey.OpenSubKey(strVersion);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                if (string.IsNullOrEmpty(projectUpgradeToolPath)) {
                    continue;
                }
                projectUpgradeToolPath = Path.Combine(projectUpgradeToolPath, projectUpgradeToolRelativePath);
                Version projectUpgradeVersion = GetProjectUpgradeVersion(projectUpgradeToolPath);
                installedVersions.Add(projectUpgradeVersion);
                projectUpgradeToolPath = projectUpgradeToolPath.Replace("ProjectConverter", "ProjectConverter-console");
                installedSupportedMajorsAndPCPaths[projectUpgradeVersion.Major] = projectUpgradeToolPath;
            }
            return installedVersions;
        }

        Version GetProjectUpgradeVersion(string projectUpgradeToolPath) {
            Assembly assembly;
            try {
                assembly = Assembly.LoadFile(projectUpgradeToolPath);
            }
            catch {
                return Version.Zero;
            }
            Version result = GetVersionFromContainingString(assembly.FullName);

            return result;
        }

    }


}
