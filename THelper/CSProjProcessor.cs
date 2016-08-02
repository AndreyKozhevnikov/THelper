using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace THelper {

    public interface IWorkWithFile {
        XDocument LoadXDocument(string projectPath);
    }
    public class CustomWorkWithFile : IWorkWithFile {
        public XDocument LoadXDocument(string projectPath) {
            return XDocument.Load(projectPath);
        }
    }

    public class CSProjProcessor {
        public string csProjFileName;
        public IWorkWithFile MyWorkWithFile;

        public CSProjProcessor(string _csProjFileName, IWorkWithFile _workWithFile) {//tested 
            csProjFileName = _csProjFileName;
            MyWorkWithFile = _workWithFile;
            
            RootDocument = MyWorkWithFile.LoadXDocument(csProjFileName);
            RootElements = RootDocument.Elements();
      
        }


        public XDocument RootDocument;
        public IEnumerable<XElement> RootElements;




        public Version GetCurrentVersion() {//0+


            var references = RootElements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);
            string _dxLibraryString = null;
            if (dxlibraries.Count() > 0) {
                _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();
                Version v = new Version(_dxLibraryString, true);
                return v;
            }
            else {
                return Version.Zero;
            }
        }
        public void DisableUseVSHostingProcess() {
            var UseVSHostingProcess = RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
            if (UseVSHostingProcess != null) {
                UseVSHostingProcess.SetValue("false");
            }
            else {
                //var v = RootElements.Where(x => x.Name.LocalName == "PropertyGroup").ToList();
                //var v1 = v.Where(x => x.HasAttributes).ToList();
                //var v2 = v1.Select(x => x.FirstAttribute).ToList();
                var pGroup = RootElements.Where(x => x.Name.LocalName == "PropertyGroup" && x.HasAttributes && x.FirstAttribute.Value.Contains("Debug")).First();
                XName xName = XName.Get("UseVSHostingProcess", pGroup.Name.Namespace.NamespaceName);
                XElement useVSElement = new XElement(xName, "False");
                pGroup.Add(useVSElement);
            }
        }

        internal void RemoveLicense() {
            var licGroup = RootElements.SelectMany(x => x.Elements()).Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).FirstOrDefault();
            if (licGroup != null)
                licGroup.Remove();
        }

        internal void SetSpecificVersionFalse() {
            var references = RootElements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);

            string _dxLibraryString = null;
            if (dxlibraries.Count() > 0)
                _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();


            foreach (XElement dxlib in dxlibraries) {
                var specificVersionNode = dxlib.Element(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName));
                if (specificVersionNode != null)
                    dxlib.SetElementValue(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName), false);
                else {
                    XName xName = XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName);
                    XElement xatr = new XElement(xName, "False");
                    dxlib.Add(xatr);
                }
            }
        }

        public void SaveNewCsProj() {
            string resultString = RootDocument.ToString();
            StreamWriter sw = new StreamWriter(csProjFileName, false);
            sw.Write(resultString);
            sw.Close();
        }

#if DEBUGTEST
        public void Test_SetRootElements(string _rootElements) {
        var    xlroot = XElement.Parse(_rootElements);
            RootElements = xlroot.Elements();
        }
#endif

    }
}
