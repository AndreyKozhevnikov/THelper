﻿

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
#if DEBUGTEST
    [TestFixture]
    public class ProjectProcessor_Tests {
        [Test]
        public void ProjectProcessor() {
            //arrange
            string st = @"c:\test\test.zip";
            //act
            ProjectProcessor proc = new ProjectProcessor(st);
            //assert
            Assert.AreEqual(st, proc.archiveFilePath_t);
        }
        [Test]
        public void SetIsExample() {
            //arrange
            string st = @"c:\test\test.dxsample";
            //act
            ProjectProcessor proc = new ProjectProcessor(st);
            proc.SetIsExample_t();
            //assert
            Assert.AreEqual(true, proc.isExample_t);
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
            var res = proc.GetArgsForWinRar_t();
            //assert
            wrkFile.Verify(x => x.CreateDirectory(@"c:\test\test"), Times.Once);
            Assert.AreEqual(@" x ""c:\test\test.dxsample"" ""c:\test\test""", res);
            Assert.AreEqual(proc.solutionFolderInfo_t.FullName, @"c:\test\test");
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
            var b = proc.TryGetSolutionFiles_T(di, out slnPath, out csPath);

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
            var b = proc.TryGetSolutionFiles_T(di, out slnPath, out csPath);

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
            var b = proc.TryGetSolutionFiles_T(di, out slnPath, out csPath);

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
            var b = proc.TryGetSolutionFiles_T(di, out slnPath, out csPath);

            //assert
            Assert.AreEqual(false, b);
        }

        [Test]
        public void Message_IsExample() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testsolution.dxsample");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            proc.SetIsExample_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(2, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[1]);
        }

        [Test]
        public void Message_isCurrentVersionMajorInstalled_false() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(3, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.MessagesList_t[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[2]);
        }
        [Test]
        public void Message_MainMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(3, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.MessagesList_t[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[2]);
        }
        [Test]
        public void Message_MainMajorLastMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2, Version=15.2.9.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");


            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(2, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[1]);
        }
        [Test]
        public void Message_MainMajorZeroMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.2,  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(2, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[1]);
        }
        [Test]
        public void Message_InstalledMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.1, Version=15.1.5.0,  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(4, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.LastMinor, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.MessagesList_t[1]);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.MessagesList_t[2]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[3]);
        }
        [Test]
        public void Message_InstalledMajorLastMinor() {

            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.1, Version=15.1.13.0,  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(3, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.MessagesList_t[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[2]);
        }
        [Test]
        public void Message_InstalledMajorZeroMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v15.1,  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");

            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(3, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.LastMinor, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.MessagesList_t[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[2]);
        }
        [Test]
        public void Message_IsNotDxSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test");
            var mock = new Mock<IWorkWithFile>();
            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(new XDocument());
            proc.MyWorkWithFile = mock.Object;
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            mock.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            mock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            mock.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.13.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            //act
            proc.GetMessageInfo_t();

            //assert
            Assert.AreEqual(2, proc.MessagesList_t.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.MessagesList_t[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.MessagesList_t[1]);
        }
        [Test]
        public void SetIsExample_False() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.rar");

            //act
            proc.SetIsExample_t();

            //assert
            Assert.AreEqual(false, proc.isExample_t);
        }
        [Test]
        public void SetIsExample_True() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.dxsample");

            //act
            proc.SetIsExample_t();

            //assert
            Assert.AreEqual(true, proc.isExample_t);
        }


        [Test]
        public void GetProjectUpgradeVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            string stPath = @"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe";
            var moqWrk = new Mock<IWorkWithFile>();
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(stPath)).Returns("ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;

            //act
            var vers = proc.GetProjectUpgradeVersion_t(stPath);
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
            var vers = proc.GetProjectUpgradeVersion_t(stPath);
            //assert
            Assert.AreEqual(true, vers.IsZero);
        }

        [Test]
        public void GetInstalledVersions() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            //act
            proc.GetInstalledVersions_t();
            //assert
            Assert.AreEqual(2, proc.installedVersions.Count);
            Assert.AreEqual(151, proc.mainMajorLastVersion_t.Major);

        }
        //[Category("TODO")]
        [Test]
        public void PrintMessage_isExample() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.dxsample");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            proc.SetIsExample_t();
            List<String> lst = new List<string>();

            moqPrint.Setup(x => x.ConsoleWrite(It.IsAny<string>(), It.IsAny<ConsoleColor>())).Callback<string, ConsoleColor>((x, y) => lst.Add(x));
            moqPrint.Setup(x => x.ConsoleReadKey(It.IsAny<bool>())).Returns(ConsoleKey.D1);
            proc.MessagesList_t = new List<ConverterMessages>();
            proc.MessagesList_t.Add(ConverterMessages.OpenFolder);
            //act
            proc.PrintMessage_t();
            //assert
            var res = lst.Where(x => x.StartsWith("example")).Count();
            Assert.AreEqual(1, res);

        }
        [Test]
        public void PrintMessagee() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            proc.SetIsExample_t();
            proc.currentProjectVersion_t = Version.Zero;
            List<String> lst = new List<string>();

            moqPrint.Setup(x => x.ConsoleWrite(It.IsAny<string>(), It.IsAny<ConsoleColor>())).Callback<string, ConsoleColor>((x, y) => lst.Add(x));
            moqPrint.Setup(x => x.ConsoleReadKey(It.IsAny<bool>())).Returns(ConsoleKey.D9);
            proc.MessagesList_t = new List<ConverterMessages>();
            //act
            var v = proc.PrintMessage_t();
            //assert
            Assert.AreEqual(ConverterMessages.OpenFolder, v);
        }
        [Test]
        public void PrintMessagee_wrongnumber() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            proc.SetIsExample_t();
            proc.currentProjectVersion_t = Version.Zero;
            List<String> lst = new List<string>();

            moqPrint.Setup(x => x.ConsoleWrite(It.IsAny<string>(), It.IsAny<ConsoleColor>())).Callback<string, ConsoleColor>((x, y) => lst.Add(x));
            moqPrint.Setup(x => x.ConsoleReadKey(It.IsAny<bool>())).Returns(ConsoleKey.D8);
            proc.MessagesList_t = new List<ConverterMessages>();
            //act
            var v = proc.PrintMessage_t();
            //assert
            Assert.AreEqual(ConverterMessages.OpenSolution, v);
        }
        [Test]
        public void PrintMessage_Common() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            proc.MessagesList_t = new List<ConverterMessages>();
            proc.MessagesList_t.Add(ConverterMessages.ExactConversion);
            proc.MessagesList_t.Add(ConverterMessages.MainMajorLastVersion);
            proc.MessagesList_t.Add(ConverterMessages.OpenFolder);
            proc.mainMajorLastVersion_t = new Version("16.1.5");
            proc.currentProjectVersion_t = new Version("15.2.14");
            moqPrint.Setup(x => x.ConsoleReadKey(It.IsAny<bool>())).Returns(ConsoleKey.D2);
            //act
            proc.PrintMessage_t();
            //assert
            moqPrint.Verify(x => x.ConsoleWrite("152.14.0", ConsoleColor.Red), Times.Exactly(2));
            moqPrint.Verify(x => x.ConsoleWrite("161.5.0", ConsoleColor.Red), Times.Once);
            moqPrint.Verify(x => x.ConsoleWrite("To open folder press: "), Times.Once);

        }
        [Test]
        public void PrintConverterMessage_OpenSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            var lst = new List<string>();
            //act
            proc.PrintConverterMessage_t(ConverterMessages.OpenSolution, "1");
            //assert
            moqPrint.Verify(x => x.ConsoleWrite("To open solution press: "), Times.Once);
            moqPrint.Verify(x => x.ConsoleWrite("1", ConsoleColor.Red), Times.Once);
        }
        [Test]
        public void PrintConverterMessage_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            var lst = new List<string>();
            //act
            proc.PrintConverterMessage_t(ConverterMessages.OpenFolder, "2");
            //assert
            moqPrint.Verify(x => x.ConsoleWrite("To open folder press: "), Times.Once);
            moqPrint.Verify(x => x.ConsoleWrite("9", ConsoleColor.Red), Times.Once);
        }
        [Test]
        public void PrintConverterMessage_Plain() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            var moqPrint = new Mock<IMessenger>();
            proc.MyMessenger = moqPrint.Object;
            proc.currentProjectVersion_t = new Version("15.2.14");
            var lst = new List<string>();
            //act
            proc.PrintConverterMessage_t(ConverterMessages.ExactConversion, "2");
            //assert
            //moqPrint.Verify(x => x.ConsoleWrite("To open folder press: "), Times.Once);
            moqPrint.Verify(x => x.ConsoleWrite("152.14.0", ConsoleColor.Red), Times.Once);
        }
        [Test]
        public void GetMessageVersion_exact() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            proc.currentProjectVersion_t = new Version("15.2.14");
            //act
            var st = proc.GetMessageVersion_t(ConverterMessages.ExactConversion);
            //Assert
            Assert.AreEqual("152.14.0", st);
        }
        [Test]
        public void GetMessageVersion_LastMinor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            proc.currentInstalledMajor_t = new Version("15.1.11");
            //act
            var st = proc.GetMessageVersion_t(ConverterMessages.LastMinor);
            //Assert
            Assert.AreEqual("151.11.0", st);
        }
        [Test]
        public void GetMessageVersion_MainMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            proc.mainMajorLastVersion_t = new Version("15.1.11");
            //act
            var st = proc.GetMessageVersion_t(ConverterMessages.MainMajorLastVersion);
            //Assert
            Assert.AreEqual("151.11.0", st);
        }
        [Test]
        public void GetMessageVersion_Null() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            proc.mainMajorLastVersion_t = new Version("15.1.11");
            //act
            var st = proc.GetMessageVersion_t(ConverterMessages.OpenFolder);
            //Assert
            Assert.AreEqual(null, st);
        }
        [Test]
        public void GetValueFromConsoleKey_Key() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            //act
            var res = proc.GetValueFromConsoleKey_t(ConsoleKey.D4);
            //assert
            Assert.AreEqual(4, res);
        }
        [Test]
        public void GetValueFromConsoleKey_Num() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor("test.csproj");
            //act
            var res = proc.GetValueFromConsoleKey_t(ConsoleKey.NumPad3);
            //assert
            Assert.AreEqual(3, res);
        }
        [Test]
        public void OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqFile = new Mock<IWorkWithFile>();
            proc.MyWorkWithFile = moqFile.Object;
            proc.GetArgsForWinRar_t();
            //act
            proc.OpenFolder_t();
            //assert
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testproject"), Times.Once);
        }
        [Test]
        public void OpenSolution() {
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
            var b = proc.TryGetSolutionFiles_T(di, out slnPath, out csPath);
            proc.slnPath_t = slnPath;
            //act
            proc.OpenSolution_t();
            //assert
            wrkFile.Verify(x => x.ProcessStart(@"c:\test\testsolution\testsolution.sln"), Times.Once);
        }
        [Test]
        public void ProcessProject_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqFile = new Mock<IWorkWithFile>();
            proc.MyWorkWithFile = moqFile.Object;
            proc.GetArgsForWinRar_t();
            //act
            proc.ProcessProject_t(ConverterMessages.OpenFolder);
            //arrange
            moqFile.Verify(x => x.ProcessStart(@"c:\test\testproject"), Times.Once);
        }
        [Test]
        public void UpgradeToMainMajorLastVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            //act
            proc.UpgradeToMainMajorLastVersion_t();
            //assert
            moqWrk.Verify(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testproject""", true), Times.Once);
        }
        [Test]
        public void ProcessProject_IsExample() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.dxsample");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            var csMoq = new Mock<ICSProjProcessor>();
            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            //act
            proc.ProcessProject_t(ConverterMessages.MainMajorLastVersion);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            moqWrk.Verify(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testproject""", true), Times.Once);
        }
        [Test]
        public void ProcessProject_DXProj_MainMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.1.8"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D1);
            proc.GetMessageInfo_t();
            //act
            proc.ProcessProject_t(ConverterMessages.MainMajorLastVersion);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SetSpecificVersionFalse(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj());
            //moqWrk.Verify(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testproject""", true), Times.Once);
        }


        [Test]
        public void ProcessProject_DXProj_NotMainMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.6"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D1);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            //act
            proc.ProcessProject_t(ConverterMessages.MainMajorLastVersion);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            moqWrk.Verify(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testproject""", true), Times.Once);
        }

        [Test]
        public void ProcessProject_DXProj_LastMinor_Installed() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.7"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            //csMoq.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            //act
            proc.ProcessProject_t(ConverterMessages.LastMinor);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SetSpecificVersionFalse(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
        }

        [Test]
        public void ProcessProject_DXProj_LastMinor_NoInstalled() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("13.2.6"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            moqWrk.Setup(x => x.ProcessStart(It.IsAny<string>(), It.IsAny<string>())).Callback(() => Assert.That(i++, Is.EqualTo(3)));
            var lst2 = new List<string>();
            lst2.Add(@"C:\temp\15.1.8");
            lst2.Add(@"C:\temp\15.1.9");
            lst2.Add(@"C:\temp\15.1.15");
            lst2.Add(@"C:\temp\15.2.13");
            lst2.Add(@"C:\temp\13.2.7");
            lst2.Add(@"C:\temp\13.2.13");
            lst2.Add(@"C:\temp\14.1.3");
            moqWrk.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst2.ToArray());
            //act
            proc.ProcessProject_t(ConverterMessages.LastMinor);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            moqWrk.Verify(x => x.ProcessStart(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ProcessProject_DXProj_LastMinor_NoInstalled_LibrariesExist() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("13.2.6"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            moqWrk.Setup(x => x.ProcessStart(It.IsAny<ProcessStartInfo>())).Callback(() => Assert.That(i++, Is.EqualTo(3)));
            var lst2 = new List<string>();
            lst2.Add(@"C:\temp\15.1.8");
            lst2.Add(@"C:\temp\15.1.9");
            lst2.Add(@"C:\temp\15.1.15");
            lst2.Add(@"C:\temp\15.2.13");
            lst2.Add(@"C:\temp\13.2.7");
            lst2.Add(@"C:\temp\13.2.13");
            lst2.Add(@"C:\temp\14.1.3");
            moqWrk.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst2.ToArray());
            moqWrk.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new List<string>() { "test" });

            //act
            proc.ProcessProject_t(ConverterMessages.LastMinor);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            //   moqWrk.Verify(x => x.ProcessStart(It.IsAny<ProcessStartInfo>()), Times.Once);
        }

        [Test]
        public void ProcessProject_DXProj_LastMinor_ExactConversion_LibraryExist() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("13.2.6"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));

            var lst2 = new List<string>();
            lst2.Add(@"C:\temp\15.1.8");
            lst2.Add(@"C:\temp\15.1.9");
            lst2.Add(@"C:\temp\15.1.15");
            lst2.Add(@"C:\temp\15.2.13");
            lst2.Add(@"C:\temp\13.2.7");
            lst2.Add(@"C:\temp\13.2.13");
            lst2.Add(@"C:\temp\14.1.3");
            moqWrk.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst2.ToArray());
            moqWrk.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new List<string>() { "test" });

            //act
            proc.ProcessProject_t(ConverterMessages.ExactConversion);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            //   moqWrk.Verify(x => x.ProcessStart(It.IsAny<ProcessStartInfo>()), Times.Once);
        }

        [Test]
        public void ProcessProject_DXProj_LastMinor_ExactConversion_LibraryNOtExist() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("13.2.6"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            moqWrk.Setup(x => x.ProcessStart(It.IsAny<string>(), It.IsAny<string>())).Callback(() => Assert.That(i++, Is.EqualTo(3)));
            var lst2 = new List<string>();
            lst2.Add(@"C:\temp\15.1.8");
            lst2.Add(@"C:\temp\15.1.9");
            lst2.Add(@"C:\temp\15.1.15");
            lst2.Add(@"C:\temp\15.2.13");
            lst2.Add(@"C:\temp\13.2.7");
            lst2.Add(@"C:\temp\13.2.13");
            lst2.Add(@"C:\temp\14.1.3");
            moqWrk.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst2.ToArray());

            //act
            proc.ProcessProject_t(ConverterMessages.ExactConversion);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            moqWrk.Verify(x => x.ProcessStart(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ProcessProject_DXProj_LastMinor_DefaultValue() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(new Version("13.2.6"));

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(2)));
            moqWrk.Setup(x => x.ProcessStart(It.IsAny<ProcessStartInfo>())).Callback(() => Assert.That(i++, Is.EqualTo(3)));
            var lst2 = new List<string>();
            lst2.Add(@"C:\temp\15.1.8");
            lst2.Add(@"C:\temp\15.1.9");
            lst2.Add(@"C:\temp\15.1.15");
            lst2.Add(@"C:\temp\15.2.13");
            lst2.Add(@"C:\temp\13.2.7");
            lst2.Add(@"C:\temp\13.2.13");
            lst2.Add(@"C:\temp\14.1.3");
            moqWrk.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst2.ToArray());

            //act
            proc.ProcessProject_t(ConverterMessages.OpenSolution);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);
            csMoq.Verify(x => x.RemoveLicense(), Times.Once);
            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            //moqWrk.Verify(x => x.ProcessStart(It.IsAny<ProcessStartInfo>()), Times.Once);
        }
        [Test]
        public void ProcessProject_NOtDXProj() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testsolution.zip");
            var moqWrk = new Mock<IWorkWithFile>();
            var lst = new List<string>();
            lst.Add(@"C:\Program Files (x86)\DevExpress 15.1\Components\");
            lst.Add(@"C:\Program Files (x86)\DevExpress 14.2\Components\");
            moqWrk.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lst);
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 14.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=14.2.12.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            moqWrk.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            proc.MyWorkWithFile = moqWrk.Object;
            proc.GetInstalledVersions_t();
            proc.GetArgsForWinRar_t();
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            // st = st + "   <Reference Include=\"DevExpress.Data.v14.2, Version=14.2.2.0  Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\"><SpecificVersion>False</SpecificVersion></Reference>";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";

            moqWrk.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            var csMoq = new Mock<ICSProjProcessor>();
            csMoq.Setup(x => x.GetCurrentVersion()).Returns(Version.Zero);

            proc.csProjProccessor = csMoq.Object;
            proc.SetIsExample_t();

            var mesMoq = new Mock<IMessenger>();
            proc.MyMessenger = mesMoq.Object;
            mesMoq.Setup(x => x.ConsoleReadKey(true)).Returns(ConsoleKey.D2);
            proc.GetMessageInfo_t();
            int i = 0;
            // csMoq.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            // csMoq.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            csMoq.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(i++, Is.EqualTo(0)));
            moqWrk.Setup(x => x.ProcessStart(@"c:\test\testsolution\testsolution.sln")).Callback(() => Assert.That(i++, Is.EqualTo(1)));
            //  moqWrk.Setup(x => x.ProcessStart(It.IsAny<ProcessStartInfo>())).Callback(() => Assert.That(i++, Is.EqualTo(3)));
            var lst2 = new List<string>();
            lst2.Add(@"C:\temp\15.1.8");
            lst2.Add(@"C:\temp\15.1.9");
            lst2.Add(@"C:\temp\15.1.15");
            lst2.Add(@"C:\temp\15.2.13");
            lst2.Add(@"C:\temp\13.2.7");
            lst2.Add(@"C:\temp\13.2.13");
            lst2.Add(@"C:\temp\14.1.3");
            moqWrk.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst2.ToArray());

            //act
            proc.ProcessProject_t(ConverterMessages.OpenSolution);
            //assert
            csMoq.Verify(x => x.DisableUseVSHostingProcess(), Times.Once);

            csMoq.Verify(x => x.SaveNewCsProj(), Times.Once);
            moqWrk.Verify(x => x.ProcessStart(It.IsAny<string>()), Times.Once);
            //moqWrk.Verify(x => x.ProcessStart(It.IsAny<ProcessStartInfo>()), Times.Once);
        }
        [Test]
        public void FindLastVersionOfMajor() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testproject.zip");
            var wrkMock = new Mock<IWorkWithFile>();
            proc.MyWorkWithFile = wrkMock.Object;
            var lst = new List<string>();
            lst.Add(@"C:\temp\15.1.8");
            lst.Add(@"C:\temp\15.1.9");
            lst.Add(@"C:\temp\15.1.15");
            lst.Add(@"C:\temp\15.2.13");
            lst.Add(@"C:\temp\14.1.3");
            wrkMock.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(lst.ToArray());
            //act
            var v = proc.FindLastVersionOfMajor_t(151);
            //assert
            Assert.AreEqual(15, v.Minor);
        }



    }
    [TestFixture]
    public class Test_TEst {
        [Test]
        public void TestClass() {
            //arrange
            TestClass t = new TestClass();
            var moq = new Mock<ITestInterFace>();
            t.MyProcessor = moq.Object;


            int i = -1;
            int tmpi = -1;
            int tmpK = 0;
            //    moq.Setup(x => x.Test1()).Callback(() => Assert.That(++i, Is.EqualTo(tmpK++)));
            //moq.Setup(x => x.Test1()).Callback(() => Assert.That(++i, Is.EqualTo(tmpK++)));
            Dictionary<string, int> tmpDict = new Dictionary<string, int>();
            moq.Do((x2) => { tmpDict["Test1"] = tmpK++; }).Setup(x => x.Test1()).Callback(() => Assert.That(++i, Is.EqualTo(tmpDict["Test1"])));
            moq.Do((x2) => { tmpDict["Test2"] = tmpK++; }).Setup(x => x.Test2()).Callback(() => Assert.That(++i, Is.EqualTo(tmpDict["Test2"])));
            moq.Do((x2) => { tmpDict["Test3"] = tmpK++; }).Setup(x => x.Test3()).Callback(() => Assert.That(++i, Is.EqualTo(tmpDict["Test3"])));
            //  moq.Setup(x => x.Test1()).Callback(() => Assert.That(++i, Is.EqualTo(tmpK++)));

            //moq.Setup(x => x.Test2()).Callback(() => Assert.That(++i, Is.EqualTo(tmpK++)));
            //moq.Setup(x => x.Test3()).Callback(() => Assert.That(++i, Is.EqualTo(tmpK++)));

            t.MyMethod();

            Assert.AreEqual(3, tmpK);
        }
    }
    public static class MyExtensions {
        public static TI Do<TI>(this TI input, Action<TI> action) where TI : class {
            if (input == null)
                return null;
            action(input);
            return input;
        }
    }
    public interface ITestInterFace {
        void Test1();

        void Test3();
        void Test2();
    }

    public class TestClass {
        public ITestInterFace MyProcessor;
        public void MyMethod() {

            MyProcessor.Test1();
            MyProcessor.Test2();

            MyProcessor.Test3();


        }
    }

#endif

    [TestFixture]
    public class HeavyTests {

        [Test]
        public void SimpleFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\archinveWithImages.zip");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;
            moqFile.Setup(x => x.CreateDirectory(@"c:\test\archinveWithImages")).Returns(new DirectoryInfo(@"c:\test\archinveWithImages")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\archinveWithImages.zip"" ""c:\test\archinveWithImages""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\archinveWithImages", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\archinveWithImages", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\archinveWithImages", "*.vbproj", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\archinveWithImages")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(5, callConsequenceCount);
        }
        [Test]
        public void Example_MMLVinstalled_OpenSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\dxExample.dxsample");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;
            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.dxsample"" ""c:\test\dxExample""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));

            moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));
            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(18, callConsequenceCount);
            //moqFile.Verify(x => x.CreateDirectory(@"c:\test\archinveWithImages"), Times.Once);
            //moqFile.Verify(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\archinveWithImages.zip"" ""c:\test\archinveWithImages"""), Times.Once);
        }

        [Test] //++
        public void Example_MMLVNOTinstalled_OpenSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\dxExample.dxsample");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.dxsample"" ""c:\test\dxExample""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));
            moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(18, callConsequenceCount);
        }

        [Test]
        public void Example_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\dxExample.dxsample");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.dxsample"" ""c:\test\dxExample""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is an ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("example", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            //moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(15, callConsequenceCount);
            //moqFile.Verify(x => x.CreateDirectory(@"c:\test\archinveWithImages"), Times.Once);
            //moqFile.Verify(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\archinveWithImages.zip"" ""c:\test\archinveWithImages"""), Times.Once);
        }


        [Test]
        public void Solution_without_dxlibs_opensolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\dxExample.zip");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.zip"" ""c:\test\dxExample""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(Version.Zero).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("0.0.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample\dxExample.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(17, callConsequenceCount);
        }
        [Test]
        public void Solution_without_dxlibs_openFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\dxExample.zip");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\dxExample")).Returns(new DirectoryInfo(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\dxExample.zip"" ""c:\test\dxExample""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\dxExample", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\dxExample\dxExample\dxExample.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(Version.Zero).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("0.0.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            //moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            //moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\dxExample")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(15, callConsequenceCount);
        }

        [Test]
        public void MainMajorLastMinor_OpenSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.4")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(18, callConsequenceCount);
        }
        [Test]
        public void MainMajorLastMinor_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.4")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));

            //moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            //moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(15, callConsequenceCount);
        }

        [Test]
        public void MainMajorMinor0_MainMajorLastVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.0")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.0.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(21, callConsequenceCount);
        }
        [Test]
        public void MainMajorMinor0_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.0")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.0.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));

            //moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            //moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(17, callConsequenceCount);
        }

        [Test]
        public void MainMajorNoLastMinor_MainMajorLastVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(25, callConsequenceCount);
        }
        [Test]
        public void MainMajorNotLastMinor_ExactConversion_NotLibraries() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);

            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.NumPad2).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //   moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            var arguments = string.Format("{0} {1}", @"c:\test\testSolution", "16.1.2");
            moqFile.Setup(x => x.ProcessStart(@"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe", arguments)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(26)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(26, callConsequenceCount);
        }
        [Test]
        public void MainMajorNotLastMinor_ExactConversion_LibrariesExist() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);

            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.NumPad2).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //   moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));


            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(25, callConsequenceCount);
        }
        [Test]
        public void MainMajorNoLastMinor_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("16.1.2")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.2.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(9, 15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange<int>(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            //moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            //moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(21, callConsequenceCount);
        }

        [Test]
        public void MajorLastMinor_OpenSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.7")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(22, callConsequenceCount);
        }
        [Test]
        public void MajorLastMinor_MainMajorLastVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.7")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testSolution""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(23, callConsequenceCount);
        }
        [Test]
        public void MajorLastMinor_Openfolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("15.2.7")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("152.7.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(9)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(12)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(14)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(16)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));

            //  moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));
            //moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            // moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            //  moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testSolution""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(19, callConsequenceCount);
        }

        [Test]
        public void NotIstalledMajor_ExactConversion_LibrariesPersist() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9,11)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            //moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            //moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10,14)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12,16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10,14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { "test" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(25, callConsequenceCount);
        }
        [Test]
        public void NotIstalledMajor_ExactConversion_LibrariesNotPersist() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            //moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            //moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D1).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            var arguments = string.Format("{0} {1}", @"c:\test\testSolution", "14.2.5");
            moqFile.Setup(x => x.ProcessStart(@"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe", arguments)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));

            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(26)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(26, callConsequenceCount);
        }
        [Test]
        public void NotIstalledMajor_MainMajorLastVersion() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            //moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            //moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.D2).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

            moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

            moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            //var arguments = string.Format("{0} {1}", @"c:\test\testSolution", "14.2.5");
          //  moqFile.Setup(x => x.ProcessStart(@"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe", arguments)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));
            moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testSolution""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution\testSolution.sln")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(25, callConsequenceCount);
        }
        [Test]
        public void NotIstalledMajor_OpenFolder() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\testSolution.rar");
            var moqFile = new Mock<IWorkWithFile>(MockBehavior.Strict);
            proc.MyWorkWithFile = moqFile.Object;
            int callConsequenceCount = -1;

            moqFile.Setup(x => x.CreateDirectory(@"c:\test\testSolution")).Returns(new DirectoryInfo(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(0)));
            moqFile.Setup(x => x.ProcessStart(It.IsAny<string>(), @" x ""c:\test\testSolution.rar"" ""c:\test\testSolution""")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(1)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.sln", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution.sln" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(2)));
            moqFile.Setup(x => x.EnumerateFiles(@"c:\test\testSolution", "*.csproj", SearchOption.AllDirectories)).Returns(new string[] { @"c:\test\testSolution\testSolution\testSolution.csproj" }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(3)));
            var moqCSProj = new Mock<ICSProjProcessor>(MockBehavior.Strict);
            proc.csProjProccessor = moqCSProj.Object;

            var lstRegistryVersions = new List<string>();
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 15.2\Components\");
            lstRegistryVersions.Add(@"C:\Program Files (x86)\DevExpress 16.1\Components\");
            moqFile.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(lstRegistryVersions).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(4)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 15.2\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(5)));
            moqFile.Setup(x => x.AssemblyLoadFileFullName(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter.exe")).Returns(@"ProjectConverter, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a").Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(6)));
            moqCSProj.Setup(x => x.GetCurrentVersion()).Returns(new Version("14.2.5")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(7)));
            var moqMessage = new Mock<IMessenger>(MockBehavior.Strict);
            proc.MyMessenger = moqMessage.Object;

            moqMessage.Setup(x => x.ConsoleWrite("The current project version is ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(8)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWriteLine());
            //moqMessage.Setup(x => x.ConsoleWrite("To open solution press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(10)));
            //moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(11)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("142.5.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(9, 11)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("1", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(13)));
            moqMessage.Setup(x => x.ConsoleWrite("To convert to : ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(10, 14)));
            moqMessage.Setup(x => x.ConsoleWrite("161.4.0", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(15)));
            moqMessage.Setup(x => x.ConsoleWrite(" press ")).Callback(() => Assert.That(++callConsequenceCount, Is.InRange(12, 16)));
            moqMessage.Setup(x => x.ConsoleWrite("2", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(17)));
            moqMessage.Setup(x => x.ConsoleWrite("To open folder press: ")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(18)));
            moqMessage.Setup(x => x.ConsoleWrite("9", ConsoleColor.Red)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(19)));

            moqMessage.Setup(x => x.ConsoleReadKey(false)).Returns(ConsoleKey.NumPad9).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(20)));

          //  moqCSProj.Setup(x => x.DisableUseVSHostingProcess()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
           // moqCSProj.Setup(x => x.RemoveLicense()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(22)));
            //moqCSProj.Setup(x => x.SetSpecificVersionFalse()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));

         //   moqCSProj.Setup(x => x.SaveNewCsProj()).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(23)));
            //moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\dxExample""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(999)));
            //moqFile.Setup(x => x.DirectoryEnumerateFiles(@"c:\test\testSolution", "DevExpress*.dll", SearchOption.AllDirectories)).Returns(new string[] { }).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            //var arguments = string.Format("{0} {1}", @"c:\test\testSolution", "14.2.5");
            //  moqFile.Setup(x => x.ProcessStart(@"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe", arguments)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(25)));
       //     moqFile.Setup(x => x.ProcessStart(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", @"""c:\test\testSolution""", true)).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(24)));
            moqFile.Setup(x => x.ProcessStart(@"c:\test\testSolution")).Callback(() => Assert.That(++callConsequenceCount, Is.EqualTo(21)));
            //act
            proc.ProcessArchive();
            //assert
            Assert.AreEqual(21, callConsequenceCount);
        }

    }
}



