

using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace THelper {
    [TestFixture]
    public class ProjectProcessor_Test {

        [Test]
        public void GetIsExample_Test() {
            //arrange
            var proc = new ProjectProcessor(null);
            //act

            var res0 = proc.GetIsExample("how-to-use-the-entity-framework-model-first-in-xaf-e4374-14.2.3-.zip");
            var res1 = proc.GetIsExample("how-to-access-a-tab-control-in-a-detail-view-layout-e372-12.2.4-.zip");
            var res2 = proc.GetIsExample("how-to-access-a-nested-listview-from-the-parent-detailviews-controller-and-vice-versa-e3977-12.1.4-.zip");
            var res3 = proc.GetIsExample("how-to-implement-a-custom-xpo-connection-provider-for-adonetcore-aseclient-18.1.2-.zip");
            var result = res0 && res1 && res2 && res3;
            //assert
            Assert.AreEqual(true, result);

        }

        [Test]
        public void GetUnexpectedFiles_Test() {
            //arrange
            var proc = new ProjectProcessor(null);
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.bak", It.IsAny<SearchOption>())).Returns(new string[] { @"C:\mybak.bak" });
            moqFile.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.jpg", It.IsAny<SearchOption>())).Returns(new string[] { @"C:\picture.jpg" });
            moqFile.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.png", It.IsAny<SearchOption>())).Returns(new string[] { @"C:\watch.png", @"C:\Logo.png", @"C:\test.png" });
            proc.MyFileWorker = moqFile.Object;
            proc.filesToDetect = Properties.Settings.Default.FilesToDetect;
            proc.namesToExclude = Properties.Settings.Default.NamesToExclude;
            //act
            var di = new DirectoryInfo(@"C:\");
            var lst = proc.GetUnexpectedFiles(di);
            //assert
            Assert.AreEqual(4, lst.Count);
        }

        [Test]
        public void GetUnexpectedFiles_2_Test() {
            //arrange
            var proc = new ProjectProcessor(null);
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.bak", It.IsAny<SearchOption>())).Returns(new string[] { @"C:\mybak.bak", @"C:\mybak.csproj.bak" });
            moqFile.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.png", It.IsAny<SearchOption>())).Returns(new string[] { @"C:\watch.png", @"C:\Logo.png", @"C:\test.png", @"C:\ExpressAppLogo.png" });
            proc.MyFileWorker = moqFile.Object;
            proc.filesToDetect = Properties.Settings.Default.FilesToDetect;
            proc.namesToExclude = Properties.Settings.Default.NamesToExclude;
            //act
            var di = new DirectoryInfo(@"C:\");
            var lst = proc.GetUnexpectedFiles(di);
            //assert
            Assert.AreEqual(3, lst.Count);
        }

        [Test]
        public void CorrectConnectionStringTestAppConfig() {
            //arrange
            XDocument doc = XDocument.Parse(Properties.Resources.AppWrong);
            var proc = new ProjectProcessor(null);
            //act
            proc.CorrectConnectionString(doc, "dxT598706usr");
            var stringBuild = new StringBuilder();
            var writer = new StringWriter(stringBuild);
            doc.Save(writer);
            var st = stringBuild.ToString();
            var st2 = doc.Declaration.ToString() + Environment.NewLine + doc.ToString();
            //assert
            Assert.AreEqual(Properties.Resources.AppRight, st2);
        }

        [Test]
        public void CorrectConnectionStringTestAppConfig_2() {
            //arrange
            XDocument doc = XDocument.Parse(Properties.Resources.AppConfigWrong2);
            var proc = new ProjectProcessor(null);
            //act
            Assert.DoesNotThrow(() =>
               proc.CorrectConnectionString(doc, "dxT598706")
             );
        }

        [Test]
        public void CorrectConnectionStringTestAppConfig_3() {
            //arrange
            XDocument doc = XDocument.Parse(Properties.Resources.AppConfigWrong3);
            var proc = new ProjectProcessor(null);
            //act
            proc.CorrectConnectionString(doc, "dxT598706usr");
            var stringBuild = new StringBuilder();
            var writer = new StringWriter(stringBuild);
            doc.Save(writer);
            var st = stringBuild.ToString();
            var st2 = doc.Declaration.ToString() + Environment.NewLine + doc.ToString();
            //assert
            Assert.AreEqual(Properties.Resources.AppConfigWrong3Correct, st2);
        }

        [Test]
        public void CorrectConnectionStringJsonTest() {
            //arrange
            //var jsonString = File.ReadAllText(Properties.Resources.appWrong1);
            var doc = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(Properties.Resources.appWrong1);
            //  XDocument doc = XDocument.Parse(Properties.Resources.AppWrong);
            var proc = new ProjectProcessor(null);
            //act
            proc.CorrectConnectionString(doc, "dxT598706usr");
            // var stringBuild = new StringBuilder();
            //var writer = new StringWriter(stringBuild);
            //doc.Save(writer);
            //var st = stringBuild.ToString();
            var st2 = doc.ToString();
            //assert
            Assert.AreEqual(Properties.Resources.appRight1, st2);
        }

        [Test]
        public void CorrectConnectionStringJsonTest_1() {
            //arrange
            //var jsonString = File.ReadAllText(Properties.Resources.appWrong1);
            var doc = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(Properties.Resources.AppSettingsWrong2);
            //  XDocument doc = XDocument.Parse(Properties.Resources.AppWrong);
            var proc = new ProjectProcessor(null);
            //act
            //assert
            Assert.DoesNotThrow(() =>
              proc.CorrectConnectionString(doc, "dxT598706")
            );

        }

        [Test]
        public void CorrectConnectionStringJsonTest_2() {
            //arrange
            //var jsonString = File.ReadAllText(Properties.Resources.appWrong1);
            var doc = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(Properties.Resources.AppSettingsWrong3);
            //  XDocument doc = XDocument.Parse(Properties.Resources.AppWrong);
            var proc = new ProjectProcessor(null);
            //act
            proc.CorrectConnectionString(doc, "dxT598706usr");
            // var stringBuild = new StringBuilder();
            //var writer = new StringWriter(stringBuild);
            //doc.Save(writer);
            //var st = stringBuild.ToString();
            var st2 = doc.ToString();
            //assert
            Assert.AreEqual(Properties.Resources.AppSettingsWrong3Correct, st2);
        }

        [Test]
        public void GetTicketNameFromSlnPathTest() {
            //arrange
            string st = @"c:\!Tickets\T598825 How to use state code table as a\SupportTickets\StateCodesPropertyEditor.vb";
            string stWithoutNumber = @"c:\!Tickets\SupportTickets\StateCodesPropertyEditor.vb";
            var proc = new ProjectProcessor(null);
            //act
            var newSt = proc.GetDBNameFromSlnPath(st);
            var newStWithout = proc.GetDBNameFromSlnPath(stWithoutNumber);
            //assert
            Assert.IsTrue(newSt.Contains("dxT598825"));
            Assert.IsTrue(newStWithout.Contains("dx"));
        }
        [Test]
        public void GetTicketNameFromSlnPathTest_2() {
            //arrange
            string st = @"c:\!Tickets\T1109690 - SQLException 'Constant expression\SupportTickets\StateCodesPropertyEditor.vb";
            string stWithoutNumber = @"c:\!Tickets\SupportTickets\StateCodesPropertyEditor.vb";
            var proc = new ProjectProcessor(null);
            //act
            var newSt = proc.GetDBNameFromSlnPath(st);
            var newStWithout = proc.GetDBNameFromSlnPath(stWithoutNumber);
            //assert
            Assert.IsTrue(newSt.Contains("dxT1109690"));
            Assert.IsTrue(newStWithout.Contains("dx"));
        }

        [Test]
        public void FindLastVersionOfMajor() {
            //arrange
            var proc = new ProjectProcessor(null);
            var xDoc = XDocument.Parse(Properties.Resources.versions);
            var allVersionElement = xDoc.Element("Versions").Element("AllVersions");
            proc.AllVersionsList = allVersionElement.Elements().ToList();
            //act
            var vers = proc.FindLastVersionOfMajor(191);
            //assert
            Assert.AreEqual("191.12.0", vers.ToString());
        }

        [Test]
        public void FindLastVersionOfMajor_1() {
            //arrange
            var proc = new ProjectProcessor(null);
            var xDoc = XDocument.Parse(Properties.Resources.versions);
            var allVersionElement = xDoc.Element("Versions").Element("AllVersions");
            proc.AllVersionsList = allVersionElement.Elements().ToList();
            //act
            var vers = proc.FindLastVersionOfMajor(202);

            //assert
            Assert.AreEqual("202.5.0", vers.ToString());

        }
    }
}


