

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Windows.Forms;

using System.IO;
using Moq;
using System.Xml.Linq;
using System.Diagnostics;
using THelper.Classes;

namespace THelper {

    [TestFixture]
    public class CSProjProcessor_Tests {
        [Test]
        public void GetIsFirstVersionGreater_1_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual("v4.5.2", "v2.0");
            //assert
            Assert.AreEqual(true, res);
        }
        [Test]
        public void GetIsFirstVersionGreater_2_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual("v4.5.2", "v4.5.3");
            //assert
            Assert.AreEqual(false, res);
        }
        [Test]
        public void GetIsFirstVersionGreater_3_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual("v4.5.2", "v4.6.1");
            //assert
            Assert.AreEqual(false, res);
        }
        [Test]
        public void GetIsFirstVersionGreater_4_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual( "v2.0", "v4.5.2");
            //assert
            Assert.AreEqual(false, res);
        }
        [Test]
        public void GetIsFirstVersionGreater_5_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual("v4.5.2", "v4.5.2");
            //assert
            Assert.AreEqual(true, res);
        }
        [Test]
        public void GetIsFirstVersionGreater_6_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual("v4.5", "v4.5.2");
            //assert
            Assert.AreEqual(false, res);
        }
        [Test]
        public void GetIsFirstVersionGreater_7_Test() {
            //arrange
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);

            //act
            var res = proc.GetIsFirstVersionGreaterOrEqual("v4.5.2", "v4.5");
            //assert
            Assert.AreEqual(true, res);
        }
        [Test]
public void FindTargetFramework_Test() {
            //arrange
            var doc =XDocument.Parse(Properties.Resources.OldFramework);
            var proc = new CSProjProcessor(new List<string>(), null);
            var dxDoc = new DXProjDocument(doc, null);
            //act
            var res = proc.FindTargetFramework(dxDoc);
            //assert
            
            Assert.AreEqual("v2.0", res.Value);
        }
        [Test]
        public void CSProjProcessor() {
            //arrange
            string st2 = @"c:\test\testproject\test.csproj";
            var moqFile = new Mock<IFileWorker>();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));

            // Act
            var proc = new CSProjProcessor(new List<string>() { st2 }, moqFile.Object);
            //assert
            Assert.AreEqual(st2, proc.csProjFileNames[0]);
            Assert.AreNotEqual(null, proc.RootDocuments[0]);
            Assert.AreEqual(1, proc.RootDocuments.Count());
        }
        [Test]
        public void GetCurrentversion_woMinor() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "     <Reference Include=\"DevExpress.Mvvm.v15.2\" />";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            Version v = proc.GetCurrentVersion();

            //assert
            Assert.AreEqual(152, v.Major);
            Assert.AreEqual(0, v.Minor);
        }
        [Test]
        public void GetCurrentversion() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);

            //act
            Version v = proc.GetCurrentVersion();

            //assert
            Assert.AreEqual(152, v.Major);
            Assert.AreEqual(5, v.Minor);
        }
        [Test]
        public void GetCurrentversion_notFirstLibrary() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "     <Reference Include = \"DevExpress.Xpf.Core.v15.2\"/>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);

            //act
            Version v = proc.GetCurrentVersion();

            //assert
            Assert.AreEqual(152, v.Major);
            Assert.AreEqual(5, v.Minor);
        }
        [Test]
        public void DxLibrariesCount() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "   <Reference Include=\"DevExpress.Grid.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);

            //act
            Version v = proc.GetCurrentVersion();
            int cnt = proc.DXLibrariesCount;

            //assert
            Assert.AreEqual(2, cnt);
        }
        [Test]
        public void GetCurrentversion_Zero() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);

            //act
            Version v = proc.GetCurrentVersion();

            //assert
            Assert.AreEqual(0, v.Major);
            Assert.AreEqual(0, v.Minor);
        }

        [Test]
        public void DisableUseVSHostingProcess_Exist() {
            //arrange
            string st = "<Project>";
            st = st + " <PropertyGroup Condition = \"'$(Configuration)|$(Platform)' == 'Debug|x86'\">";
            st = st + " <UseVSHostingProcess> True </UseVSHostingProcess>";
            st = st + " </PropertyGroup>";
            st = st + "</Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.DisableUseVSHostingProcess();
            //assert
            var el = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
            var val = el.Value;
            Assert.AreEqual("False", val);
        }

        [Test]
        public void DisableUseVSHostingProcess_NotExist() {
            //arrange
            string st = "<Project>";
            st = st + " <PropertyGroup Condition = \"'$(Configuration)|$(Platform)' == 'Debug|x86'\">";
            st = st + " </PropertyGroup>";
            st = st + "</Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.DisableUseVSHostingProcess();
            //assert
            var el = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
            Assert.AreNotEqual(null, el);
            var val = el.Value;
            Assert.AreEqual("False", val);
        }
        [Test]
        public void RemoveLicense() {
            //arrange
            string st = "<Project>";
            st = st + " <ItemGroup>";
            st = st + " <EmbeddedResource Include=\"Properties\\Licenses.licx\" />";
            st = st + " </ItemGroup>";
            st = st + "</Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.RemoveLicense();
            //assert
            var lic = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).FirstOrDefault();
            Assert.AreEqual(null, lic);
        }
        [Test]
        public void SetSpecificVersionFalse() {
            //arrange
            string st = "<Project>";
            st = st + " <ItemGroup>";
            st = st + " <Reference Include=\"DevExpress.Data.v16.1, Version = 16.1.4.0, Culture = neutral, PublicKeyToken = b88d1754d700e49a, processorArchitecture = MSIL\"/>";
            st = st + "  <Reference Include=\"DevExpress.Xpf.Core.v16.1, Version = 16.1.4.0, Culture = neutral, PublicKeyToken = b88d1754d700e49a, processorArchitecture = MSIL\">";
            st = st + "      <SpecificVersion>True</SpecificVersion>";
            st = st + "    </Reference>";
            st = st + " </ItemGroup>";
            st = st + "</Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.SetSpecificVersionFalseAndRemoveHintPath();
            //assert
            var libs = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).Where(x => x.Name == "Reference").ToList();
            Assert.AreEqual(true, libs[0].HasElements);
            var spec = libs[0].Elements().First();
            Assert.AreEqual("SpecificVersion", spec.Name.LocalName);
            Assert.AreEqual("False", spec.Value);

            Assert.AreEqual(true, libs[1].HasElements);
            var spec1 = libs[1].Elements().First();
            Assert.AreEqual("SpecificVersion", spec1.Name.LocalName);
            Assert.AreEqual("false", spec1.Value);
        }
        [Test]
        public void RemoveHintPathIfExist() {
            //arrange
            string st = "<Project>";
            st = st + " <ItemGroup>";
            st = st + "  <Reference Include=\"DevExpress.Xpf.Core.v16.1, Version = 16.1.4.0, Culture = neutral, PublicKeyToken = b88d1754d700e49a, processorArchitecture = MSIL\">";
            st = st + "      <SpecificVersion>True</SpecificVersion>";
            st = st + @"   <HintPath>..\DevExpressRef\DevExpress.Mvvm.v16.1.dll </HintPath >";
            st = st + "    </Reference>";
            st = st + " </ItemGroup>";
            st = st + "</Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.SetSpecificVersionFalseAndRemoveHintPath();
            //assert
            var libs = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).Where(x => x.Name == "Reference").ToList();
            Assert.AreEqual(true, libs[0].HasElements);
            var elCount = libs[0].Elements().Count();
            Assert.AreEqual(1, elCount);
        }

        [Test]
        public void SaveNewCsProj() {
            //arrange
            string st = @"c:\test\testproject\test.csproj";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(st)).Returns(new XDocument());
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { st }, moqFile.Object);
            //act
            proc.SaveNewCsProj();
            //assert
            moqFile.Verify(x => x.SaveXDocument(It.IsAny<XDocument>(), st), Times.AtLeastOnce);
        }
    }

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
        public void CorrectConnectionStringTest() {
            //arrange
            XDocument doc =XDocument.Parse(Properties.Resources.AppWrong);
            var proc = new ProjectProcessor(null);
            //act
            proc.CorrectConnectionString(doc, "dxT598706");
            var stringBuild = new StringBuilder();
            var writer = new StringWriter(stringBuild);
            doc.Save(writer);
            var st = stringBuild.ToString();
            var st2 = doc.Declaration.ToString() +Environment.NewLine+ doc.ToString();
            //assert
            Assert.AreEqual(Properties.Resources.AppRight, st2);
        }


        [Test]
        public void GetTicketNameFromSlnPathTest() {
            //arrange
            string st = @"c:\!Tickets\T598825 How to use state code table as a\SupportTickets\StateCodesPropertyEditor.vb";
            string stWithoutNumber = @"c:\!Tickets\SupportTickets\StateCodesPropertyEditor.vb";
            var proc = new ProjectProcessor(null);
            //act
            var newSt = proc.GetTicketNameFromSlnPath(st);
            var newStWithout = proc.GetTicketNameFromSlnPath(stWithoutNumber);
            //assert
            Assert.AreEqual("dxT598825", newSt);
            Assert.IsTrue(newStWithout.Contains("dx"));
        }
    }

    [TestFixture]
    public class Versions_Test {
        [Test]
        public void GetVersionFromFullString() {
            //arrange
            string st = @"Include=""DevExpress.Data.v15.1сргу, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
            //act
            Version v = new Version(st, true);
            //assert
            Assert.AreEqual(v.Major, 151, "major");
            Assert.AreEqual(v.Minor, 5, "minor");



        }
        [Test]
        public void GetVersionFromShortString() {
            //arrange
            string st2 = @"Include=""DevExpress.Data.v15.1""";
            //act
            Version v2 = new Version(st2, true);
            //assert
            Assert.AreEqual(151, v2.Major, "major2");
            Assert.AreEqual(0, v2.Minor, "minor2");
        }

        [Test]
        public void Zero() {
            //arrange
            var v = Version.Zero;
            //assert
            Assert.AreEqual(0, v.Major);
            Assert.AreEqual(0, v.Minor);
        }
        [Test]
        public void Constructor() {
            //arrange
            var v = new Version(152, 2, 10);
            //assert
            Assert.AreEqual(152, v.Major);
            Assert.AreEqual(2, v.Minor);
            Assert.AreEqual(10, v.Build);
            Assert.AreEqual("15.2.2", v.ToString(true));
        }
        [Test]
        public void Constructor_Short() {
            //arrange
            var v = new Version(91, 2, 10);
            //assert
            Assert.AreEqual(91, v.Major);
            Assert.AreEqual(2, v.Minor);
            Assert.AreEqual(10, v.Build);
            Assert.AreEqual("9.1.2", v.ToString(true));
        }
        [Test]
        public void IsZero() {
            //arrange
            var v = new Version(0, 0, 0);
            //Assert
            Assert.AreEqual(true, v.IsZero);
        }
        [Test]
        public void CompareTo() {
            //arrange
            var etalon = new Version(152, 15, 265);

            //act
            var majorLess = new Version(151, 15, 265);
            var majorMax = new Version(161, 15, 265);

            var minorLess = new Version(152, 14, 265);
            var minorMax = new Version(152, 16, 265);

            var buildLess = new Version(152, 15, 264);
            var buildMax = new Version(152, 15, 266);

            var majLres = etalon.CompareTo(majorLess);
            var majMres = etalon.CompareTo(majorMax);

            var minLres = etalon.CompareTo(minorLess);
            var minMres = etalon.CompareTo(minorMax);

            var bldLres = etalon.CompareTo(buildLess);
            var bldMres = etalon.CompareTo(buildMax);

            var nullRes = etalon.CompareTo(null);

            //assert
            Assert.AreEqual(1, majLres);
            Assert.AreEqual(-1, majMres);

            Assert.AreEqual(1, minLres);
            Assert.AreEqual(-1, minMres);

            Assert.AreEqual(1, bldLres);
            Assert.AreEqual(-1, bldMres);

            Assert.AreEqual(1, nullRes);

        }
        [Test]
        public void ToStringTest() {
            //arrange
            string stver = @"Include=""DevExpress.Data.v15.1, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
            var v = new Version(stver, true);
            //act
            string st = v.ToString(false);
            string st2 = v.ToString(true);

            //assert
            Assert.AreEqual("151.5.0", st);
            Assert.AreEqual("15.1.5", st2);
        }

        [Test]
        public void ParseSimpleString() {
            //arrange
            string stVer = "15.1.5";
            //act
            var v = new Version(stVer);
            //assert
            Assert.AreEqual(151, v.Major);
            Assert.AreEqual(5, v.Minor);
            Assert.AreEqual("15.1.5", v.ToString(true));
        }
        [Test]
        public void ParseComplexString() {
            //arrange
            string stver = @"Include=""DevExpress.Data.v15.1, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
            //act
            var v = new Version(stver, true);
            //assert
            Assert.AreEqual(151, v.Major);
            Assert.AreEqual(5, v.Minor);

        }
        [Test]
        public void ParseComplexString_wrong1() {
            //arrange
            string stver = @"Include=""DevExpress.Data.151, Version=1d5.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
            //act
            var v = new Version(stver, true);
            //assert
            Assert.AreEqual(0, v.Major);
            Assert.AreEqual(0, v.Minor);

        }
        [Test]
        public void ParseComplexString_wrong() {
            //arrange
            string stver = "15";
            //act
            var v = new Version(stver, false);
            //assert
            Assert.AreEqual(0, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(0, v.Build);

        }
        [Test]
        public void ParseComplexString_Build() {
            //arrange
            string stver = "15.1.15.296";
            //act
            var v = new Version(stver, false);
            //assert
            Assert.AreEqual(151, v.Major);
            Assert.AreEqual(15, v.Minor);
            Assert.AreEqual(296, v.Build);

        }
    }
    [TestFixture]
    public class VersionComparer_Tests {
        [Test]
        public void Compare_min() {
            //arrange
            string stEtalon = "15.2.9";

            string stMajorMin = "14.2.9";
            string stMajorMax = "16.2.9";

            string stMinorMin = "15.1.9";
            string stMinorMax = "16.1.9";

            string stBuildMin = "15.2.8";
            string stBuildMax = "15.2.10";

            VersionComparer comp = new VersionComparer();

            //act
            int majMinRes = comp.Compare(stEtalon, stMajorMin);
            int majMaxRes = comp.Compare(stEtalon, stMajorMax);

            int minMinRes = comp.Compare(stEtalon, stMinorMin);
            int mimMaxRes = comp.Compare(stEtalon, stMinorMax);

            int buildMinRes = comp.Compare(stEtalon, stBuildMin);
            int buildMaxRes = comp.Compare(stBuildMin, stBuildMax);

            //assert
            Assert.AreEqual(-1, majMinRes);
            Assert.AreEqual(1, majMaxRes);
            Assert.AreEqual(-1, minMinRes);
            Assert.AreEqual(1, mimMaxRes);
            Assert.AreEqual(-1, buildMinRes);
            Assert.AreEqual(1, buildMaxRes);
        }
    }


    [TestFixture]
    public class HeavyTests {
        private string ReturnNameDelete(object x3) {
            var st = x3.GetType();
            var st2 = x3.ToString();
            var ind = st2.IndexOf("=> x.") + 5;
            var st3 = st2.Substring(ind, st2.Length - ind);
            var ind2 = st3.IndexOf("(");
            var st4 = st3.Substring(0, ind2);
            return st4;
        }
        public Mock<IFileWorker> SetupMoqFileSlnFile(Mock<IFileWorker> moqFile) {
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\archinveWithImages", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3) + "sln"] = orderCounter++; }).Callback(() => Test(callOrderDictionary["EnumerateFilessln"]));
            return moqFile;
        }

        public void Test(int targetValue) {
            Assert.That(callBackCounter++, Is.EqualTo(targetValue));
        }
        int callBackCounter = 0;
        int orderCounter = 0;
        Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

        [Test]
        public void SimpleFolder() {
            //arrange
            InitializeProcessor(@"c:\test\archinveWithImages.zip");

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\archinveWithImages")).Returns(new DirectoryInfo(@"c:\test\archinveWithImages")).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Test(callOrderDictionary["CreateDirectory"]));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\archinveWithImages.zip"" ""c:\test\archinveWithImages""")).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3) + "zip"] = orderCounter++; }).Callback(() => Test(callOrderDictionary["ProcessStartzip"]));
            SetupMoqFileSlnFile(moqFile);
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\archinveWithImages", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3) + "csproj"] = orderCounter++; }).Callback(() => Test(callOrderDictionary["EnumerateFilescsproj"]));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\archinveWithImages", "*.vbproj", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Test(callOrderDictionary["EnumerateFiles"]));
            moqFile.Setup(x => x.OpenFolder(@"c:\test\archinveWithImages")).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Test(callOrderDictionary["OpenFolder"]));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(6, callBackCounter);
        }
        XDocument xDoc;
        void SetupVersionsXDocument(string vers) {
            var installedVersions = new XElement("InstalledVersions");
            var verString = vers.Split(',');
            var versList = new List<String>();
            foreach(string s in verString) {
                var xL = new XElement("Version");
                xL.Add(new XAttribute("Version", s));
                var stringVers = string.Format(@"C:\Program Files (x86)\DevExpress {0}\Components\Tools\Components\ProjectConverter-console.exe", s.Substring(0, 4));
                var stringVersShort = string.Format(@"C:\Program Files (x86)\DevExpress {0}\Components\", s.Substring(0, 4));
                versList.Add(stringVersShort);
                var converterFullName = string.Format(@"ProjectConverter-console, Version={0}.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a", s);
                moqFile.Setup(x => x.AssemblyLoadFileFullName(stringVers)).Returns(converterFullName);
                xL.Add(new XAttribute("Path", stringVers));
                installedVersions.Add(xL);
            }


            XElement xlRoot = new XElement("Versions");
            XElement allVersions = new XElement("AllVersions");
            xlRoot.Add(installedVersions);
            xlRoot.Add(allVersions);
            xDoc = new XDocument();
            xDoc.Add(xlRoot);
            moqFile.Setup(x => x.LoadXDocument(Properties.Settings.Default.FileWithVersionsPath)).Returns(xDoc);
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(versList);
        }
        void SetupAllVersionsList(string vers) {
            var xl = xDoc.Element("Versions").Element("AllVersions");
            var verString = vers.Split(',');
            foreach(string s in verString) {
                var xL = new XElement("Version");
                xL.Add(new XAttribute("Version", s));
                xl.Add(xL);
            }

        }

        [Test]
        public void GetInstalledVersions() {
            //arrange
            InitializeProcessor(null);
            proc.lastReleasedVersion = 162;
            SetupVersionsXDocument("17.1.2,16.2.6,15.2.13");
            //act
            proc.GetInstalledVersions();
            //assert
            Assert.AreEqual(proc.mainMajorLastVersion.ToString(), "162.6.0");
        }
        void InitializeProcessor(string st) {
            proc = new ProjectProcessor(st);
            proc.lastReleasedVersion = 162;
            proc.filesToDetect = Properties.Settings.Default.FilesToDetect;
            proc.namesToExclude = Properties.Settings.Default.NamesToExclude;
            moqFile = new Mock<IFileWorker>();
            proc.MyFileWorker = moqFile.Object;
            SetupVersionsXDocument("15.2.7,16.1.4");
            moqMessage = new Mock<IMessageProcessor>();
            proc.MyMessageProcessor = moqMessage.Object;
        }
        ProjectProcessor proc;
        Mock<IFileWorker> moqFile;
        Mock<IMessageProcessor> moqMessage;

        [Test]
        public void Example_MMLVinstalled_OpenSolution() {
            //arrange 
            var exZipName = @"c:\test\Test how-to-access-a-nested-listview-from-the-parent-detailviews-controller-and-vice-versa-e3977-12.1.4-.zip";
            var winRarArgs = @" x """ + exZipName + @""" ""c:\test""";
            InitializeProcessor(exZipName);

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), winRarArgs)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExampl-Test how-t\CS")).Returns(new DirectoryInfo(@"c:\test\dxExampl-Test how-t\CS")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExampl-Test how-t\CS", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExampl-Test how-t\CS", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));

            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));

            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalse"] = orderCounter++; }).Callback(() => { Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalse"])); });
            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act 
            proc.ProcessArchive();
            //assert 
            moqFile.Verify(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln"), Times.Once);
        }
    
        [Test]
        public void Example_MMLVNOTinstalled_OpenSolution() {
            //arrange  
            var exZipName = @"c:\test\Test how-to-access-a-nested-listview-from-the-parent-detailviews-controller-and-vice-versa-e3977-12.1.4-.zip";
            var winRarArgs = @" x """ + exZipName + @""" ""c:\test""";
            InitializeProcessor(exZipName);
            int callBackCounter = 0;

            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), winRarArgs)).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExampl-Test how-t\CS")).Returns(new DirectoryInfo(@"c:\test\dxExampl-Test how-t\CS")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExampl-Test how-t\CS", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExampl-Test how-t\CS\dxExample.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExampl-Test how-t\CS", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExampl-Test how-t\CS\dxExample\dxExample.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.CorrectFrameworkVersionIfNeeded()).Do((x3) => { callOrderDictionary["CorrectFrameworkVersionIfNeeded"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CorrectFrameworkVersionIfNeeded"])));
            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj1"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\dxExample", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\dxExampl-Test how-t\CS\dxExample.sln"), Times.Once);
        }

        [Test]
        public void Example_OpenFolder() {
            //arrange  
            var exZipName = @"c:\test\Test how-to-access-a-nested-listview-from-the-parent-detailviews-controller-and-vice-versa-e3977-12.1.4-.zip";
            var winRarArgs = @" x """ + exZipName + @""" ""c:\test""";
            var finalSolutionFolder = @"c:\test\dxExampl-Test how-t\CS";
            InitializeProcessor(exZipName);

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();


            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), winRarArgs)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.CreateDirectory(finalSolutionFolder)).Returns(new DirectoryInfo(finalSolutionFolder)).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.EnumerateFiles(finalSolutionFolder, "*.sln", SearchOption.AllDirectories)).Returns(new string[] { finalSolutionFolder + @"\dxExample.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(finalSolutionFolder, "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { finalSolutionFolder + @"\dxExample.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqFile.Setup(x => x.OpenFolder(finalSolutionFolder)).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(finalSolutionFolder), Times.Once);
        }
        [Test]
        public void WrongKeyBoardInput() {
            //arrange  
            InitializeProcessor(@"c:\test\dxExample.dxsample");

            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample"));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.dxsample"" ""c:\test\dxExample"""));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" });
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" });
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2"));


            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an "));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: "));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red));


            moqMessage.SetupSequence(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D0).Returns(ConsoleKey.D8).Returns(ConsoleKey.D9);

            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\dxExample"), Times.Once);
        }


        [Test]
        public void Solution_without_dxlibs_opensolution() {
            //arrange  
            InitializeProcessor(@"c:\test\dxExample.zip");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();
            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.zip"" ""c:\test\dxExample""")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(Version.Zero).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));


            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("0.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample\dxExample.csproj")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\dxExample\dxExample\dxExample.csproj"), Times.Once);
        }
        [Test]
        public void Solution_with_several_projects_open_solution() {
            //arrange  
            InitializeProcessor(@"c:\test\dxExample.zip");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();
            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.zip"" ""c:\test\dxExample""")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln", @"c:\test\test.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqMessage.Setup(x => x.ConsoleWrite("There are many sln files!", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["manyslnfiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["manyslnfiles"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(Version.Zero).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));


            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("0.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln"), Times.Once);
        }


        [Test]
        public void Solution_without_dxlibs_openFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\dxExample.zip");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();


            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.zip"" ""c:\test\dxExample""")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(Version.Zero).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));


            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("0.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqFile.Setup(x => x.OpenFolder(@"c:\test\dxExample")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\dxExample"), Times.Once);
        }


        [Test]
        public void MainMajorLastMinor_OpenSolution() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.4")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalseAndRemoveHintPath"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalseAndRemoveHintPath"])));
            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution\testSolution.csproj")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution\testSolution.csproj"), Times.Once);
        }

        [Test]
        public void MainMajorLastMinor_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.4")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }


        [Test]
        public void MainMajorMinor0_MainMajorLastVersion() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalse"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalse"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MainMajorMinor0_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }


        [Test]
        public void MainMajorNoLastMinor_MainMajorLastVersion() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite8"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite8"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalse"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalse"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MainMajorNoLastMinor_MainMajorLastVersion_hasWeb() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj", @"c:\test\testSolution\testSolution\solution.Web\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            moqFile.Setup(x => x.StreamReaderReadToEnd(It.IsAny<string>())).Returns(@"Reference Include=""DevExpress.");


            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite8"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite8"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MainMajorNotLastMinor_ExactConversion_NotLibraries() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite8"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite8"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.NumPad2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"\"", @"c:\test\testSolution", "16.1.2");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MainMajorNotLastMinor_ExactConversion_LibrariesExist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: "));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.NumPad2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" });
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MainMajorNoLastMinor_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }



        [Test]
        public void MajorLastMinor_OpenSolution() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.7")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalseAndRemoveHintPath"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalseAndRemoveHintPath"])));
            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorZeroMinor_LastMinor() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { "c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite8"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite8"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalse"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalse"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart("c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart("c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorZeroMinor_MainMajor() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { "c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite8"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite8"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));


            moqFile.Setup(x => x.ProcessStart("c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart("c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorZeroMinor_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite8"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite8"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }

        [Test]
        public void MajorNotLastMinor_LastMinor() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SetSpecificVersionFalseAndRemoveHintPath()).Do((x3) => { callOrderDictionary["SetSpecificVersionFalse"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SetSpecificVersionFalse"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorNotLastMinor_LastMinor_hasWeb() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj", @"c:\test\testSolution\testSolution\solution.Web\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            moqFile.Setup(x => x.StreamReaderReadToEnd(It.IsAny<string>())).Returns(@"Reference Include=""DevExpress.");
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));


            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "15.2.7", @"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorNotLastMinor_MainMajor() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorNotLastMinor_ExactConversion_LibrariesPersist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D3).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorNotLastMinor_ExactConversion_LibrariesNotPersist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D3).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"\"", @"c:\test\testSolution", "15.2.5");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorNotLastMinor_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }

        [Test]
        public void MajorLastMinor_MainMajorLastVersion() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.7")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void MajorLastMinor_Openfolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.7")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }



        [Test]
        public void NotIstalledMajorNotZeroMinor_LastMinor_LibrariesExist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotIstalledMajorNotZeroMinor_LastMinor_LibrariesNotExist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"\"", @"c:\test\testSolution", "14.2.13");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotIstalledMajorNotZeroMinor_ExactConversion_LibrariesPersist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotIstalledMajorNotZeroMinor_ExactConversion_LibrariesNotPersist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));


            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"\"", @"c:\test\testSolution", "14.2.5");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotIstalledMajorNotZeroMinor_MainMajorLastVersion() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart4"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));


            //  string lst = "15.1.16\r,14.2.3\r,16.1.1\r,14.2.13\r,9.8.1";

            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D3).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));

            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));


            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotIstalledMajorNotZeroMinor_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart4"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite9"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite9"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.NumPad9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }


        [Test]
        public void NotIstalledMajorZeroMinor_LastMinor_LibrariesExist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }


        [Test]
        public void NotIstalledMajorZeroMinor_LastMinor_LibrariesNotExist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            string lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);


            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));

            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"\"", @"c:\test\testSolution", "14.2.13");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }


        [Test]
        public void NotIstalledMajorLastMinor_LastMinor_LibrariesExist() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart2"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.13")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }


        [Test]
        public void NotIstalledMajorZeroMinor_MainMajor() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);
            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));


            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotExistedVersion_MainMajor() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("12.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));

            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3";
            SetupAllVersionsList(lst);
            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("122.5.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));
            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));
            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));
            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"{2}\"", @"c:\test\testSolution", "16.1.4", @"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln"), Times.Once);
        }

        [Test]
        public void NotIstalledMajorZeroMinor_OpenFolder() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;



            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite("142.0.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("142.13.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite3"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["OpenFolder"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }


        [Test]
        public void NotIstalledMajorZeroMinor_OpenFolder_WrongKey() {
            //arrange  
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution"));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution"""));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" });
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" });
            var moqCSProj = new Mock<ICSProjProcessor>();
            proc.csProjProcessor = moqCSProj.Object;


            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.0")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));


            moqMessage.SetupSequence(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.Home).Returns(ConsoleKey.D9);

            var lst = "16.1.1,15.1.16,14.2.13,14.2.8,14.2.3,9.8.1";
            SetupAllVersionsList(lst);
            //act  
            proc.ProcessArchive();

            //assert  
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);
        }



        [Test]
        public void MoreThanOneSolutions_HasDX() {
            //arrange
            InitializeProcessor(@"c:\test\testSolution.rar");
            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\solution1\solution1.csproj", @"c:\test\testSolution\testSolution\solution2\solution2.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            moqFile.Setup(x => x.StreamReaderReadToEnd(@"c:\test\testSolution\testSolution\solution1\solution1.csproj")).Returns("test string").Do((x3) => { callOrderDictionary["StreamReaderReadToEnd3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["StreamReaderReadToEnd3"])));
            moqFile.Setup(x => x.StreamReaderReadToEnd(@"c:\test\testSolution\testSolution\solution2\solution2.csproj")).Returns(@"test string Reference Include=""DevExpress.Printing.v15.2.Core").Do((x3) => { callOrderDictionary["StreamReaderReadToEnd2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["StreamReaderReadToEnd2"])));

            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProcessor = moqCSProj.Object;

            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.5")).Do((x3) => { callOrderDictionary["GetCurrentVersion"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["GetCurrentVersion"])));



            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Do((x3) => { callOrderDictionary["ConsoleWrite11"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite11"])));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite7"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite7"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite6"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite6"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite5"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite5"])));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite4"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite4"])));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : "));
            moqMessage.Setup(x => x.ConsoleWrite("152.5.0", ConsoleColor.Red));
            moqMessage.Setup(x => x.ConsoleWrite(" press "));
            moqMessage.Setup(x => x.ConsoleWrite("3", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite2"])));

            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Do((x3) => { callOrderDictionary["ConsoleWrite1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite1"])));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Do((x3) => { callOrderDictionary["ConsoleWrite"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleWrite"])));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D3).Do((x3) => { callOrderDictionary["ConsoleReadKey"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ConsoleReadKey"])));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Do((x3) => { callOrderDictionary["DisableUseVSHostingProcess"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DisableUseVSHostingProcess"])));
            moqCSProj.Setup(x => x.RemoveLicense()).Do((x3) => { callOrderDictionary["RemoveLicense"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["RemoveLicense"])));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Do((x3) => { callOrderDictionary["SaveNewCsProj"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["SaveNewCsProj"])));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["DirectoryEnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DirectoryEnumerateFiles"])));
            moqCSProj.Setup(x => x.DXLibrariesCount).Returns(1).Do((x3) => { callOrderDictionary["DXLibrariesCount"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["DXLibrariesCount"])));
            var arguments = string.Format("\"{0}\" \"{1}\" \"false\" \"\"", @"c:\test\testSolution", "15.2.5");
            moqFile.Setup(x => x.ProcessStart(Properties.Settings.Default.DXConverterPath, arguments)).Do((x3) => { callOrderDictionary["ProcessStart1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart1"])));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution\solution2\solution2.csproj")).Do((x3) => { callOrderDictionary["ProcessStart"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart"])));
            //act  
            proc.ProcessArchive();
            //assert  
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testSolution\testSolution\solution2\solution2.csproj"), Times.Once);

        }
        [Test]
        public void MoreThanOneSolutions_NotDX() {
            //arrange
            InitializeProcessor(@"c:\test\testSolution.rar");

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary["CreateDirectory"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["CreateDirectory"])));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Do((x3) => { callOrderDictionary["ProcessStart3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["ProcessStart3"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { }).Do((x3) => { callOrderDictionary["EnumerateFiles1"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles1"])));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\solution1\solution1.csproj", @"c:\test\testSolution\testSolution\solution2\solution2.csproj" }).Do((x3) => { callOrderDictionary["EnumerateFiles"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["EnumerateFiles"])));
            moqFile.Setup(x => x.StreamReaderReadToEnd(@"c:\test\testSolution\testSolution\solution1\solution1.csproj")).Returns("test string").Do((x3) => { callOrderDictionary["StreamReaderReadToEnd3"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["StreamReaderReadToEnd3"])));
            moqFile.Setup(x => x.StreamReaderReadToEnd(@"c:\test\testSolution\testSolution\solution2\solution2.csproj")).Returns("test string Xpf.Grid TestAppDevExpress").Do((x3) => { callOrderDictionary["StreamReaderReadToEnd2"] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["StreamReaderReadToEnd2"])));

            moqFile.Setup(x => x.OpenFolder(@"c:\test\testSolution")).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["OpenFolder"])));

            //act
            proc.ProcessArchive();
            //assert
            moqFile.Verify(x => x.OpenFolder(@"c:\test\testSolution"), Times.Once);

        }
        [Test]
        public void MakeApplicationProjectFirst() {
            //arrange
            string slnText = Properties.Resources.WrongSln;
            ProjectProcessor proc = new ProjectProcessor(null);
            moqFile = new Mock<IFileWorker>();
            proc.MyFileWorker = moqFile.Object;
            moqFile.Setup(x => x.StreamReaderReadToEnd(It.IsAny<string>())).Returns(slnText);
            string tmp = "";
            moqFile.Setup(x => x.StreamWriterWriteLine(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((x, y) => tmp = y);

            //act
            proc.MakeApplicationProjectFirst();
            //assert
            var requiredString = Properties.Resources.RightSln;
            moqFile.Verify(x => x.StreamWriterWriteLine(It.IsAny<string>(), requiredString), Times.Once);
        }

    }
}


