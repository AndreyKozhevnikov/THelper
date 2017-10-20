using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using THelper.Classes;

namespace THelper {

    //public interface IWorkWithFile {
    //    XDocument LoadXDocument(string projectPath);
    //    void SaveXDocument(XDocument projDocument, string projectPath);
    //}
    //public class CustomWorkWithFile : IWorkWithFile {
    //    public XDocument LoadXDocument(string projectPath) {
    //        return XDocument.Load(projectPath);
    //    }
    //    public void SaveXDocument(XDocument projDocument, string projectPath) {
    //        projDocument.Save(projectPath);
    //    }
    //}
    public interface ICSProjProcessor {
        void DisableUseVSHostingProcess();
        Version GetCurrentVersion();
        void RemoveLicense();
        void SaveNewCsProj();
        void SetSpecificVersionFalseAndRemoveHintPath();
        int DXLibrariesCount { get; }
    }
    public class CSProjProcessor : ICSProjProcessor {
        public List<string> csProjFileNames;
        public IFileWorker MyWorkWithFile;

        public CSProjProcessor(List<string> _csProjFileNames, IFileWorker _workWithFile) {//tested 
            csProjFileNames = _csProjFileNames;
            MyWorkWithFile = _workWithFile;
            RootDocuments = new List<DXProjDocument>();
            foreach(var fl in csProjFileNames) {
                var doc= MyWorkWithFile.LoadXDocument(fl);
                var dxDoc = new DXProjDocument(doc,fl);
                RootDocuments.Add(dxDoc);
            }
        }


        public List<DXProjDocument> RootDocuments;
       // public IEnumerable<XElement> RootElements;

        public int DXLibrariesCount {
            get; set;
        }

        public Version GetCurrentVersion() {
            var references = RootDocuments[0].RootElements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);
            string _dxLibraryString = null;
            DXLibrariesCount = dxlibraries.Count();
            if (DXLibrariesCount > 0) {
                var lstHasVersion = dxlibraries.Where(x => x.Attribute("Include").ToString().IndexOf("Version") > 0);
                if (lstHasVersion.Count() > 0)
                    _dxLibraryString = lstHasVersion.First().Attribute("Include").ToString();
                else
                    _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();
                Version v = new Version(_dxLibraryString, true);
                return v;
            }
            else {
                return Version.Zero;
            }
        }
        public void DisableUseVSHostingProcess() {
            foreach(var doc in RootDocuments) {
                var UseVSHostingProcess = doc.RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
                if(UseVSHostingProcess != null) {
                    UseVSHostingProcess.SetValue("False");
                } else {
                    var pGroup = doc.RootElements.Where(x => x.Name.LocalName == "PropertyGroup" && x.HasAttributes && x.FirstAttribute.Value.Contains("Debug")).First();
                    XName xName = XName.Get("UseVSHostingProcess", pGroup.Name.Namespace.NamespaceName);
                    XElement useVSElement = new XElement(xName, "False");
                    pGroup.Add(useVSElement);
                }
            }
        }

        public void RemoveLicense() {
            foreach(var doc in RootDocuments) {
                var licGroup = doc.RootElements.SelectMany(x => x.Elements()).Where(y => y.Attribute("Include") != null && y.Attribute("Include").Value.IndexOf("licenses.licx", StringComparison.InvariantCultureIgnoreCase) > -1).FirstOrDefault();
                if(licGroup != null)
                    licGroup.Remove();
            }
        }

        public void SetSpecificVersionFalseAndRemoveHintPath() {
            foreach(var doc in RootDocuments) {
                var references = doc.RootElements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
                var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);

                foreach(XElement dxlib in dxlibraries) {
                    var specificVersionNode = dxlib.Element(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName));
                    if(specificVersionNode != null)
                        dxlib.SetElementValue(XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName), false);
                    else {
                        XName xName = XName.Get("SpecificVersion", dxlib.Name.Namespace.NamespaceName);
                        XElement xatr = new XElement(xName, "False");
                        dxlib.Add(xatr);
                    }
                    var hintPathNode = dxlib.Element(XName.Get("HintPath", dxlib.Name.Namespace.NamespaceName));
                    if(hintPathNode != null)
                        hintPathNode.Remove();
                }
            }
        }

        public void SaveNewCsProj() {
            foreach(var doc in RootDocuments) {
                MyWorkWithFile.SaveXDocument(doc.RootDocument, doc.csProjFileName);
            }
        }
      

    }
}
