

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

namespace THelper {

    [TestFixture]
    public class CSProjProcessor_Tests {
        [Test]
        public void CSProjProcessor() {
            //arrange
            string st2 = @"c:\test\testproject\test.csproj";
            var moqFile = new Mock<IWorkWithFile>();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));

            // Act
            var proc = new CSProjProcessor(st2, moqFile.Object);
            //assert
            Assert.AreEqual(st2, proc.csProjFileName);
            Assert.AreNotEqual(null, proc.RootDocument);
            Assert.AreEqual(1, proc.RootElements.Count());
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
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);
            //proc.Test_SetRootElements(st);
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
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);

            //act
            Version v = proc.GetCurrentVersion();

            //assert
            Assert.AreEqual(152, v.Major);
            Assert.AreEqual(5, v.Minor);
        }
        [Test]
        public void GetCurrentversion_Zero() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);

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
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);
            //act
            proc.DisableUseVSHostingProcess();
            //assert
            var el = proc.RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
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
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);
            //act
            proc.DisableUseVSHostingProcess();
            //assert
            var el = proc.RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
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
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);
            //act
            proc.RemoveLicense();
            //assert
            var lic = proc.RootElements.SelectMany(x => x.Elements()).Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).FirstOrDefault();
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
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(null, moqFile.Object);
            //act
            proc.SetSpecificVersionFalse();
            //assert
            var libs = proc.RootElements.SelectMany(x => x.Elements()).Where(x => x.Name == "Reference").ToList();
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
        public void SaveNewCsProj() {
            //arrange
            string st = @"c:\test\testproject\test.csproj";
            var moqFile = new Mock<IWorkWithFile>();
            moqFile.Setup(x => x.LoadXDocument(st)).Returns(new XDocument());
            CSProjProcessor proc = new CSProjProcessor(st, moqFile.Object);
            //act
            proc.SaveNewCsProj();
            //assert
            moqFile.Verify(x => x.SaveXDocument(It.IsAny<XDocument>(), st), Times.AtLeastOnce);
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
    public class ProjectProcessor_Tests {
        [Test]
        public void ProjectProcessor() {
            //arrange
            string st = @"c:\test\test.zip";
            //act
            ProjectProcessor proc = new ProjectProcessor(st);
            //assert
            Assert.AreEqual(st, proc.archiveFilePath);
        }
        [Test]
        public void SetIsExample() {
            //arrange
            string st = @"c:\test\test.dxsample";
            //act
            ProjectProcessor proc = new ProjectProcessor(st);
            proc.SetIsExample();
            //assert
            Assert.AreEqual(true, proc.isExample);
        }
        [Test]
        public void GetArgsForWinRar() {
            //arrange
            string st = @"c:\test\test.dxsample";
            ProjectProcessor proc = new ProjectProcessor(st);
            var wrkFile = new Mock<IWorkWithFile>();
            wrkFile.Setup(x => x.CreateDirectory(@"c:\test\test")).Returns(new DirectoryInfo(@"c:\test\test"));
            proc.MyWorkWithFile = wrkFile.Object;
            //act
            var res = proc.GetArgsForWinRar();
            //assert
            wrkFile.Verify(x => x.CreateDirectory(@"c:\test\test"), Times.Once);
            Assert.AreEqual(@" x ""c:\test\test.dxsample"" ""c:\test\test""", res);
            Assert.AreEqual(proc.solutionFolderInfo.FullName, @"c:\test\test");
        }
        [Test]
        public void TryGetSolutionFiles_SLN() {
            //arrange
            string solutionPath = @"c:\test\testsolution";
            var wrkFile = new Mock<IWorkWithFile>();
            wrkFile.Setup(x => x.EnumerateFiles(solutionPath, "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testsolution\testsolution.sln" });
            wrkFile.Setup(x => x.EnumerateFiles(solutionPath, "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testsolution\testsolution\testsolution.csproj" });
            ProjectProcessor proc = new ProjectProcessor(solutionPath);
            proc.MyWorkWithFile = wrkFile.Object;
            DirectoryInfo di = new DirectoryInfo(solutionPath);
            string slnPath = null;
            string csPath = null;
            //act
            var b = proc.TryGetSolutionFiles(di, out slnPath, out csPath);

            //assert
            Assert.AreEqual(true, b);
            Assert.AreEqual(@"c:\test\testsolution\testsolution.sln", slnPath);
            Assert.AreEqual(@"c:\test\testsolution\testsolution\testsolution.csproj", csPath);
        }
        [Test]
        public void TryGetSolutionFiles_CSProj() {
            //arrange
            string solutionPath = @"c:\test\testsolution";
            var wrkFile = new Mock<IWorkWithFile>();
            wrkFile.Setup(x => x.EnumerateFiles(solutionPath, "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testsolution\testsolution\testsolution.csproj" });
            ProjectProcessor proc = new ProjectProcessor(solutionPath);
            proc.MyWorkWithFile = wrkFile.Object;
            DirectoryInfo di = new DirectoryInfo(solutionPath);
            string slnPath = null;
            string csPath = null;
            //act
            var b = proc.TryGetSolutionFiles(di, out slnPath, out csPath);

            //assert
            Assert.AreEqual(true, b);
            Assert.AreEqual(@"c:\test\testsolution\testsolution\testsolution.csproj", slnPath);
            Assert.AreEqual(@"c:\test\testsolution\testsolution\testsolution.csproj", csPath);
        }
        [Test]
        public void TryGetSolutionFiles_VBProj() {
            //arrange
            string solutionPath = @"c:\test\testsolution";
            var wrkFile = new Mock<IWorkWithFile>();
            wrkFile.Setup(x => x.EnumerateFiles(solutionPath, "*.vbproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testsolution\testsolution\testsolution.vbproj" });
            ProjectProcessor proc = new ProjectProcessor(solutionPath);
            proc.MyWorkWithFile = wrkFile.Object;
            DirectoryInfo di = new DirectoryInfo(solutionPath);
            string slnPath = null;
            string csPath = null;
            //act
            var b = proc.TryGetSolutionFiles(di, out slnPath, out csPath);

            //assert
            Assert.AreEqual(true, b);
            Assert.AreEqual(@"c:\test\testsolution\testsolution\testsolution.vbproj", slnPath);
            Assert.AreEqual(@"c:\test\testsolution\testsolution\testsolution.vbproj", csPath);
        }
        [Test]
        public void TryGetSolutionFiles_None() {
            //arrange
            string solutionPath = @"c:\test\testsolution";
            var wrkFile = new Mock<IWorkWithFile>();
            ProjectProcessor proc = new ProjectProcessor(solutionPath);
            proc.MyWorkWithFile = wrkFile.Object;
            DirectoryInfo di = new DirectoryInfo(solutionPath);
            string slnPath = null;
            string csPath = null;
            //act
            var b = proc.TryGetSolutionFiles(di, out slnPath, out csPath);

            //assert
            Assert.AreEqual(false, b);
        }
#if DEBUGTEST
        [Test]
        public void Message_IsExample() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testsolution.dxsample");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            proc.SetIsExample();
            proc.TestSetCurrentVersion("15.2.2");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(2, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[1]);
        }

        [Test]
        public void Message_isCurrentVersionMajorInstalled_false() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testsolution.csproj");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            
            proc.TestSetCurrentVersion("14.2.2");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(3, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[2]);
        }
        [Test]
        public void Message_MainMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            proc.TestSetCurrentVersion("15.2.2");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(3, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[2]);
        }
        [Test]
        public void Message_MainMajorLastMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            proc.TestSetCurrentVersion("15.2.9");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(2, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[1]);
        }
        [Test]
        public void Message_MainMajorZeroMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
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
            //     ProjectProcessor proc = new ProjectProcessor(null);

            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            // proc.Test_Csprojprocessor.Test_SetRootElements(st);
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
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
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
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
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
        [Test]
        public void Message_IsNotDxSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            proc.TestSetCurrentVersion("0.0.0");

            proc.TestAddToInstalledVersions("15.1.13");
            proc.TestAddToInstalledVersions("15.2.9");

            proc.TestSetMainMajorLastVersion("15.2.9");

            //act
            proc.TestGetMessageInfo();

            //assert
            Assert.AreEqual(2, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[1]);
        }
        [Test]
        public void SetIsExample_False() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.rar");

            //act
            proc.Test_SetIsExample();

            //assert
            Assert.AreEqual(false, proc.Test_IsExample);
        }
        [Test]
        public void SetIsExample_True() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.dxsample");

            //act
            proc.Test_SetIsExample();

            //assert
            Assert.AreEqual(true, proc.Test_IsExample);
        }

#endif
        [Test]
        public void GetProjectUpgradeVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            string stPath = @"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe";
            var moqWrk = new Mock<IWorkWithFile>();
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(stPath)).Returns("ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
        
            //act
            var vers = proc.GetProjectUpgradeVersion(stPath);
            //assert
            Assert.AreEqual(142, vers.Major);
        }
        [Test]
        public void GetProjectUpgradeVersion_zero() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            string stPath = @"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe";
            var moqWrk = new Mock<IWorkWithFile>();
            proc.MyWorkWithFile = moqWrk.Object;

            //act
            var vers = proc.GetProjectUpgradeVersion(stPath);
            //assert
            Assert.AreEqual(true, vers.IsZero);
        }

        [Test]
        public void GetInstalledVersions() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            //act
            proc.GetInstalledVersions();
            //assert
            Assert.AreEqual(2, proc.installedVersions.Count);
            Assert.AreEqual(151, proc.mainMajorLastVersion.Major);

        }
    }
}



