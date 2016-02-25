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
        string fullLibraryString;
        List<Version> installedVersions;
        Version dxGreatestVersion;
        Dictionary<int, string> installedSupportedMajorsAndPCPaths = new Dictionary<int, string>();
        bool isDxSample;

        public ProjectUpgrader(string _projPath, string _fullLibraryString, bool _isDxSample) {
            projPath = _projPath;
            fullLibraryString = _fullLibraryString;
            isDxSample = _isDxSample;
        }

        internal void Start() {
            Version projectDXReferencesVersion = GetVersionFromContainingString(fullLibraryString);
            PopulateInstalledVersions();

            Version currentProjectVersionInstalled = Version.Zero;
            var f = installedVersions.Where(x => x.Major == projectDXReferencesVersion.Major).FirstOrDefault();
            if (f != null) {
                currentProjectVersionInstalled = f;
            }

            Version versionForUpdate;
            if (currentProjectVersionInstalled.IsZero || isDxSample) {
                versionForUpdate = dxGreatestVersion;
            }
            else {
                versionForUpdate = currentProjectVersionInstalled;
            }

            bool isVersionForUpdateGreatest;

            isVersionForUpdateGreatest = versionForUpdate.CompareTo(projectDXReferencesVersion) > 0;
            if (versionForUpdate.Major < minSupportedMajorVersion || projectDXReferencesVersion.IsZero || !isVersionForUpdateGreatest) {
                return;
            }
            if (isDxSample) {
                DXProjectUpgrade(versionForUpdate.Major, projPath);
                return;
            }


            PrintMessage(projectDXReferencesVersion, versionForUpdate);
            var v = Console.ReadKey(false);
            switch (v.Key) {
                case ConsoleKey.NumPad1:
                case ConsoleKey.D1:
                    DXProjectUpgrade(versionForUpdate.Major, projPath);
                    break;
                case ConsoleKey.NumPad2:
                case ConsoleKey.D2:
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "converter";
                    string versionConverterFormat = projectDXReferencesVersion.ToString(true);
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

        void DXProjectUpgrade(int major, string projPath) {
            string toolPath = installedSupportedMajorsAndPCPaths[major];
            projPath = "\"" + projPath + "\"";
            Process updgrade = Process.Start(toolPath, projPath);
            updgrade.WaitForExit();
        }

        void PrintMessage(Version projectVersion, Version versionForUpdate) {
            Console.Write("The current project version is ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(projectVersion);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            PrintConvertTheProject(versionForUpdate, 1);

            if (projectVersion.Minor > 0)
                PrintConvertTheProject(projectVersion, 2);

            if (versionForUpdate != dxGreatestVersion)
                PrintConvertTheProject(dxGreatestVersion, 3);
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
            return new Version(versionMatch.Groups["Version"].Value);
        }

        private void PopulateInstalledVersions() {
            installedVersions = new List<Version>();
            RegistryKey dxpKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DevExpress\\Components\\");
            string[] versions = dxpKey.GetSubKeyNames();
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter.exe";
            foreach (string strVersion in versions) {
                RegistryKey dxVersionKey = dxpKey.OpenSubKey(strVersion);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                if (string.IsNullOrEmpty(projectUpgradeToolPath)) {
                    continue;
                }
                int version = GetIntMajorVersion(strVersion);
                projectUpgradeToolPath = Path.Combine(projectUpgradeToolPath, projectUpgradeToolRelativePath);
                Version projectUpgradeVersion = GetProjectUpgradeVersion(projectUpgradeToolPath);
                installedVersions.Add(projectUpgradeVersion);
                projectUpgradeToolPath = projectUpgradeToolPath.Replace("ProjectConverter", "ProjectConverter-console");
                installedSupportedMajorsAndPCPaths[projectUpgradeVersion.Major] = projectUpgradeToolPath;
                if (dxGreatestVersion == null || dxGreatestVersion.CompareTo(projectUpgradeVersion) == -1) {
                    dxGreatestVersion = projectUpgradeVersion;
                }
            }
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
        public int GetIntMajorVersion(string strVersion) {
            if (string.IsNullOrEmpty(strVersion) || !strVersion.Contains(".")) {
                return 0;
            }
            string preparedVersion = strVersion.Replace(".", string.Empty).Replace("v", string.Empty);
            int intVersion;
            if (int.TryParse(preparedVersion, out intVersion)) {
                return intVersion;
            }
            return 0;
        }

    }


}
