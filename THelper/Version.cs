using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THelper {
  public  class Version {
      int major;
      string stringMajor;
      int minor;
      int build;

      public static Version Zero { get { return new Version(0, 0, 0); } }
      Version(int major, int minor, int build) {
          this.major = major;
          this.minor = minor;
          this.build = build;
      }
      public Version(string version) {
          string[] versionParts = version.Split('.');
          if (versionParts.Length < 2) {
              throw new Exception();
          }
          major = int.Parse(versionParts[0] + versionParts[1]);
          stringMajor = string.Format("{0}.{1}", versionParts[0], versionParts[1]);
          if (versionParts.Length >= 3) {
              minor = int.Parse(versionParts[2]);
          }
          else {
              minor = 0;
          }
          if (versionParts.Length > 3) {
              build = int.Parse(versionParts[3]);
          }
          else {
              build = 0;
          }
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
      public bool IsZero { get { return major == 0 && minor == 0 && build == 0; } }
      public int CompareTo(Version other) {
          if (other == null) {
              return 1;
          }
          if (this.major == other.major) {
              if (this.minor == other.minor) {
                  return this.build.CompareTo(other.build);
              }
              else {
                  return this.minor.CompareTo(other.minor);
              }
          }
          else {
              return this.major.CompareTo(other.major);
          }
      }
      public override string ToString() {
          return String.Format("{0}.{1}.{2}", major, minor, build);
      }
      public string ToString(bool isSplittedMajor) {
          if (isSplittedMajor) {
              var majorString = major.ToString();
              var resultString = String.Format("{0}.{1}.{2}", majorString.Substring(0, 2), majorString.Substring(2, 1), minor);
              return resultString;
          }
          else {
              return this.ToString();
          }
      }

    
  }
}
