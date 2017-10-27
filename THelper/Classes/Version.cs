using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace THelper {
    public class Version : IComparable {
        int major;
        string stringMajor;
        int minor;
        int build;

        public static Version Zero { get { return new Version(0, 0, 0); } }
        public Version(int major, int minor, int build) {
            this.major = major;
            this.minor = minor;
            this.build = build;
            var st = major.ToString();
            var ind = st.Length - 1;
            stringMajor = st.Substring(0, ind) + "." + st.Substring(ind, 1);
        }
        public Version(string version) {
            ParseString(version, false);
        }
        public int Major {
            get {
                return major;
            }
        }
        public int Minor {
            get {
                return minor;
            }
        }
        public int Build {
            get {
                return build;
            }
        }
        public bool IsZero { get { return major == 0 && minor == 0 && build == 0; } }

        public int CompareTo(Version other) {
            if(other == null) {
                return 1;
            }
            if(this.major == other.major) {
                if(this.minor == other.minor) {
                    return this.build.CompareTo(other.build);
                } else {
                    return this.minor.CompareTo(other.minor);
                }
            } else {
                return this.major.CompareTo(other.major);
            }
        }
        public override string ToString() {
            return String.Format("{0}.{1}.{2}", major, minor, build);
        }
        public string ToString(bool isSplittedMajor) {
            if(isSplittedMajor) {
                var resultString = String.Format("{0}.{1}", stringMajor, minor);
                return resultString;
            } else {
                return this.ToString();
            }
        }

        public Version(string _complexString, bool _isComplex) {
            ParseString(_complexString, _isComplex);
        }

        void ParseString(string _stringWithVersion, bool _isComplex) {
            if(_isComplex) {
                string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
                Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                Match versionMatch = regexVersion.Match(_stringWithVersion);
                if(versionMatch == null || !versionMatch.Success) {
                    string versionAssemblypatternShort = @".*DevExpress.*(?<Version>\d{2}\.\d)";
                    Regex regexVersionShort = new Regex(versionAssemblypatternShort, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    Match versionMatchShort = regexVersionShort.Match(_stringWithVersion);
                    if(versionMatchShort != null && versionMatchShort.Success) {
                        string versValueShort = versionMatchShort.Groups["Version"].Value;
                        this.ParseString(versValueShort, false);
                    } else {
                        this.major = 0;
                        this.minor = 0;
                        this.build = 0;
                    }
                } else {
                    string versValue = versionMatch.Groups["Version"].Value;
                    this.ParseString(versValue, false);
                }
            } else {
                string[] versionParts = _stringWithVersion.Split('.');
                if(versionParts.Length < 2) {
                    this.major = 0;
                    this.minor = 0;
                    this.build = 0;
                    return;
                }
                major = int.Parse(versionParts[0] + versionParts[1]);
                stringMajor = string.Format("{0}.{1}", versionParts[0], versionParts[1]);
                if(versionParts.Length >= 3) {
                    minor = int.Parse(versionParts[2]);
                } else {
                    minor = 0;
                }
                if(versionParts.Length > 3) {
                    build = int.Parse(versionParts[3]);
                } else {
                    build = 0;
                }
            }
        }

        public int CompareTo(object obj) {
            return this.CompareTo((Version)obj);
        }
    }
}
