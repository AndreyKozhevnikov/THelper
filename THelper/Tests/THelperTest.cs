#define DEBUGTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Windows.Forms;

namespace THelper {
    [TestFixture]
    public class THelperTest {
        [Test]
        public void GetVersionFromContainingStringTest() {
            
           string  st=@"Include=""DevExpress.Data.v15.1, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
           Version v = new Version(st,true);
           Assert.AreEqual(v.Major, 151,"major");
           Assert.AreEqual(v.Minor, 5,"minor");
        

            string st2 =@"Include=""DevExpress.Data.v15.1""";
            Version v2 = new Version(st2,true);
            Assert.AreEqual(v2.Major, 151, "major2");
            Assert.AreEqual(v2.Minor, 0, "minor2");
        }

        [Test]
        public void GetMessageInfoTest() {
            ProjectProcessor proc = new ProjectProcessor(@"c:\!Tickets\!Test\DXSample.zip");
            proc.ProcessProject();

        }
      //  [Test]
        //public void KeyboardTests() {
        //    string st = @"Include=""DevExpress.Data.v15.1, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
        //    ProjectUpgrader upgr = new ProjectUpgrader(@"f:\temp\THelper\dxSampleGrid1517", st, false);
        //    var b = upgr.Start();
        //   // SendKeys.Send("9");
        //    Assert.AreEqual(b, false);
        //}
    }
}
