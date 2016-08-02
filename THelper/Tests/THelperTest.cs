

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
            var v = new Version(stver,true); 
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
            string stver ="15.1.15.296";
            //act
            var v = new Version(stver, false);
            //assert
            Assert.AreEqual(151, v.Major);
            Assert.AreEqual(15, v.Minor);
            Assert.AreEqual(296, v.Build);

        }
    }

}


