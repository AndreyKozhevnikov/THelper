

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Windows.Forms;

namespace THelper {
#if DEBUGTEST
    [TestFixture]
    public class THelperTest {
        [Test]
        public void GetVersionFromContainingStringTest() {

            string st = @"Include=""DevExpress.Data.v15.1сргу, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
            Version v = new Version(st, true);
            Assert.AreEqual(v.Major, 151, "major");
            Assert.AreEqual(v.Minor, 5, "minor");


            string st2 = @"Include=""DevExpress.Data.v15.1""";
            Version v2 = new Version(st2, true);
            Assert.AreEqual( 151, v2.Major, "major2");
            Assert.AreEqual(0, v2.Minor, "minor2");
        }


        [Test]
        public void Message_MainMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            proc.TestSetCurrentVersion("15.2.2");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(3,proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[0]);
            Assert.AreEqual( ConverterMessages.ExactConversion,proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder,proc.TestMessageList[2] );
        }
        [Test]
        public void Message_MainMajorLastMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            proc.TestSetCurrentVersion("15.2.9");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(2,proc.TestMessageList.Count);
            Assert.AreEqual( ConverterMessages.OpenSolution, proc.TestMessageList[0]);
            Assert.AreEqual( ConverterMessages.OpenFolder, proc.TestMessageList[1]);
        }
        [Test]
        public void Message_MainMajorZeroMinor() { 
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            proc.TestSetCurrentVersion("15.2");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(2, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[1]);
        }
        [Test]
        public void Message_InstalledMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            proc.TestSetCurrentVersion("15.1.5");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(4, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.LastMinor, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.TestMessageList[2]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[3]);
        }
        [Test]
        public void Message_InstalledMajorLastMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            proc.TestSetCurrentVersion("15.1.13");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(3, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[2]);
        }
        [Test]
        public void Message_InstalledMajorZeroMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            proc.TestSetCurrentVersion("15.1");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(3, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.LastMinor, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[2]);
        }
    }
#endif
}


