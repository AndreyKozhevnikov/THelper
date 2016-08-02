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

namespace THelper {
#if DEBUGTEST
    [TestFixture]
    public class THelperTest {


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
            Assert.AreEqual(3, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.MainMajorLastVersion, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.ExactConversion, proc.TestMessageList[1]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[2]);
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
            Assert.AreEqual(2, proc.TestMessageList.Count);
            Assert.AreEqual(ConverterMessages.OpenSolution, proc.TestMessageList[0]);
            Assert.AreEqual(ConverterMessages.OpenFolder, proc.TestMessageList[1]);
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
            //     ProjectProcessor proc = new ProjectProcessor(null);

            ProjectProcessor proc = new ProjectProcessor(null);
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
        [Test]
        public void Message_IsNotDxSolution() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(null);
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
        [Test]
        public void GetArgsForWinRar() {
            //arrange
            ProjectProcessor proc = new ProjectProcessor(@"c:\test\test.dxsample");

            //act
            var res = proc.Test_GetArgsForWinRar();

            //assert
            Assert.AreEqual(" x \"c:\\test\\test.dxsample\" \"c:\\test\\test\"", res);
        }

     
        //        [Test]
        //public void Test_TryGetSolutionFiles() {
        //    //arrange
        //    ProjectProcessor proc = new ProjectProcessor(null);
        //    string slnPath = null;
        //    string csProjPath = null;
        //    //act
        //    DirectoryInfo[] list = new DirectoryInfo[3];
        //    list[0] = new DirectoryInfo(@"c:\test\testsln.sln");
        //    list[1] = new DirectoryInfo(@"c:\test\testcsproj.csproj");
        //    list[2] = new DirectoryInfo(@"c:\test\testtxt.txt");

        //    DirectoryInfo di = Mock.Of<DirectoryInfo>(x => x.GetDirectories() == list);

        //    var b = proc.TryGetSolutionFiles(di, out slnPath, out csProjPath);
        //    //assert
        //    Assert.AreEqual(true, b);
        //    Assert.AreEqual(@"c:\test\testsln.sln", b);
        //    Assert.AreEqual(@"c:\test\testcsproj.csproj", b);
        //}

    }


#endif
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
            var lic= proc.RootElements.SelectMany(x => x.Elements()).Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).FirstOrDefault();
            Assert.AreEqual(null, lic);
        }
        
    }

    [TestFixture]
    public class Versions_Test {
        [Test]
        public void GetVersionFromContainingStringTest() {

            string st = @"Include=""DevExpress.Data.v15.1сргу, Version=15.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL""";
            Version v = new Version(st, true);
            Assert.AreEqual(v.Major, 151, "major");
            Assert.AreEqual(v.Minor, 5, "minor");


            string st2 = @"Include=""DevExpress.Data.v15.1""";
            Version v2 = new Version(st2, true);
            Assert.AreEqual(151, v2.Major, "major2");
            Assert.AreEqual(0, v2.Minor, "minor2");
        }
    }

}


