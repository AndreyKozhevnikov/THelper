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
        string projPath;
        string fullLibraryString;
        Version projectVersion;
        List<Version> installedVersions;
        Version dxGreatestVersion;
        Dictionary<int, string> installedSupportedMajorsAndPCPaths = new Dictionary<int, string>();
      //  Dictionary<int, Assembly> projectConverterAsseblies = new Dictionary<int, Assembly>();



        public ProjectUpgrader(string _projPath, string _fullLibraryString) {
            projPath = _projPath;
            fullLibraryString = _fullLibraryString;
        }

        internal void Start() {
            projectVersion = GetVersionFromContainingString(fullLibraryString);
            PopulateInstalledVersions();

            int compareResult=projectVersion.CompareTo( dxGreatestVersion);
            switch (compareResult) {
                case 0:
                    return;
                case 1:
                    string toolPath = installedSupportedMajorsAndPCPaths[dxGreatestVersion.Major];
                    string projectPath = projPath;
                    Console.WriteLine(string.Format("Project version {0}, converted to {1}", projectVersion.ToString(), dxGreatestVersion));
                    //Console.ReadLine();
                    Process.Start(toolPath, projPath);
                    return;
            }
        }

        public  Version GetVersionFromContainingString(string stringWithVersion) {
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
           // List<Version> installedVersions = new List<Version>();
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
     //    installedVersions=_installedVersions;

        }
        Version GetProjectUpgradeVersion(string projectUpgradeToolPath) {
            Assembly assembly;
            try {
                assembly = Assembly.LoadFile(projectUpgradeToolPath);
            }
            catch (Exception exc) {
                return Version.Zero;
            }
            Version result = GetVersionFromContainingString(assembly.FullName);

         //   projectConverterAsseblies[result.Major] = assembly;
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
