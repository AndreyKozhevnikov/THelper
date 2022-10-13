

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
using Newtonsoft.Json.Linq;

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
            var res = proc.GetIsFirstVersionGreaterOrEqual("v2.0", "v4.5.2");
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
            var doc = XDocument.Parse(Properties.Resources.OldFramework);
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
        public void GetCurrentversion_PackageReference() {
            //assert
            string st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            st = st + "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">";
            st = st + "  <ItemGroup>";
            st = st + "    <PackageReference Include=\"DevExpress.ExpressApp\" Version=\"22.1.5\" />";
            st = st + "  </ItemGroup>";
            st = st + " </Project>";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);

            //act
            Version v = proc.GetCurrentVersion();

            //assert
            Assert.AreEqual(221, v.Major);
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
        public void AddImagesLibraries_Exist() {
            //arrange
            string st = @"
<Project>
<PropertyGroup>
</PropertyGroup>
 <ItemGroup>
    <Reference Include=""DevExpress.Data.v19.1"">
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
	<Reference Include=""DevExpress.Images.v19.1"" >
	  <SpecificVersion>False</SpecificVersion>
    </Reference>
  </ItemGroup>
<ItemGroup>
 <Content Include=""Default.aspx"" />
</ItemGroup>
</Project>
";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.AddImagesLibraries();
            //assert
            var countImages = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).SelectMany(x => x.Attributes()).Select(x => x.Value).Where(y => y == "DevExpress.Images.v19.1").Count();
            Assert.AreEqual(1, countImages);

        }
        [Test]
        public void AddImagesLibraries_NotExist() {
            string st = @"
<Project>
<PropertyGroup>
</PropertyGroup>
 <ItemGroup>
    <Reference Include=""DevExpress.Data.v19.1"">
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
<ItemGroup>
 <Content Include=""Default.aspx"" />
</ItemGroup>
</Project>
";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.AddImagesLibraries();
            //assert
            var countImages = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).SelectMany(x => x.Attributes()).Select(x => x.Value).Where(y => y == "DevExpress.Images.v19.1").Count();
            Assert.AreEqual(1, countImages);
        }

        [Test]
        public void AddImagesLibraries_NotExist_SeveralProjs() {
            string st = @"
<Project>
<PropertyGroup>
</PropertyGroup>
 <ItemGroup>
    <Reference Include=""DevExpress.Data.v19.1"">
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
<ItemGroup>
 <Content Include=""Default.aspx"" />
</ItemGroup>
</Project>
";
            string st2 = @"
<Project>
<PropertyGroup>
</PropertyGroup>
 <ItemGroup>
    <Reference Include=""DevExpress.Data.v19.1"">
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
<ItemGroup>
 <Content Include=""Default.aspx"" />
</ItemGroup>
</Project>
";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument("1.csproj")).Returns(XDocument.Parse(st));
            moqFile.Setup(x => x.LoadXDocument("Module2.csproj")).Returns(XDocument.Parse(st2));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "Module2.csproj", "1.csproj" }, moqFile.Object);
            //act
            proc.AddImagesLibraries();
            //assert
            var countImages = proc.RootDocuments[1].RootElements.SelectMany(x => x.Elements()).SelectMany(x => x.Attributes()).Select(x => x.Value).Where(y => y == "DevExpress.Images.v19.1").Count();
            Assert.AreEqual(1, countImages);
        }


        [Test]
        public void AddImagesLibraries_VersionCheck() {
            string st = @"
<Project>
<PropertyGroup>
</PropertyGroup>
 <ItemGroup>
    <Reference Include=""DevExpress.Data.v17.2"">
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
<ItemGroup>
 <Content Include=""Default.aspx"" />
</ItemGroup>
</Project>
";
            var moqFile = new Mock<IFileWorker>();
            moqFile.Setup(x => x.LoadXDocument(It.IsAny<string>())).Returns(XDocument.Parse(st));
            CSProjProcessor proc = new CSProjProcessor(new List<string>() { "anystring" }, moqFile.Object);
            //act
            proc.AddImagesLibraries();
            //assert
            var countImages = proc.RootDocuments[0].RootElements.SelectMany(x => x.Elements()).SelectMany(x => x.Attributes()).Select(x => x.Value).Where(y => y == "DevExpress.Images.v17.2").Count();
            Assert.AreEqual(1, countImages);
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

        [Test]
        public void IsModuleProject_0() {
            //arrange
            var csProjWin = @"c:\temp\Module\OfficeApplication.Web\OfficeApplication.Win\OfficeApplication.Win.csproj";
            var csProjModule = @"c:\temp\OfficeApplication.Web\OfficeApplication.Module.Win\OfficeApplication.Module.Win.csproj";
            CSProjProcessor proc = new CSProjProcessor(new List<string>(), new FileWorker());
            //act
            var bProjWin = proc.IsModuleProject(csProjWin);
            var bProjModule = proc.IsModuleProject(csProjModule);
            //assert
            Assert.AreEqual(false, bProjWin);
            Assert.AreEqual(true, bProjModule);
        }
    }
}


