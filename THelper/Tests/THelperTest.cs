using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace THelper {
    [TestFixture]
    public class THelperTest {
        [Test]
        public void GetVersionFromContainingStringTest() {
            ProjectUpgrader upgr = new ProjectUpgrader(null, null, false);
           string  st=@"Include=""DevExpress.Data.v15.1, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
           Version v = upgr.GetVersionFromContainingString(st);
           Assert.AreEqual(v.Major, 151,"major");
           Assert.AreEqual(v.Minor, 5,"minor");
        

            string st2 =@"Include=""DevExpress.Data.v15.1""";
            Version v2 = upgr.GetVersionFromContainingString(st2);
            Assert.AreEqual(v2.Major, 151, "major2");
            Assert.AreEqual(v2.Minor, 0, "minor2");
        }
    }
}
