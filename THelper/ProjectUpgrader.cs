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
        Version projectDXReferencesVersion;
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
            projectDXReferencesVersion = GetVersionFromContainingString(fullLibraryString);
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
            PrintMessage(projectDXReferencesVersion, versionForUpdate);
            var v = Console.ReadKey(false);
            if (!(v.Key == ConsoleKey.NumPad1 || v.Key == ConsoleKey.D1))
                return;
            string toolPath = installedSupportedMajorsAndPCPaths[versionForUpdate.Major];
            projPath = "\"" + projPath + "\"";
            Process updgrade = Process.Start(toolPath, projPath);
            updgrade.WaitForExit();
        }

        void PrintMessage(Version version1, Version version2) {
            Console.Write("Press 1 to convert the project from the ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(version1);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" version to ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(version2);
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
