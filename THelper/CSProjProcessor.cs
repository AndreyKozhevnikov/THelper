using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace THelper {

    public class CSProjProcessor {
        private string csProjFileName;

        public CSProjProcessor(string _csProjFileName) {
            csProjFileName = _csProjFileName;
            OpenFile();
        }

        XElement xlroot;
        void OpenFile() {
            XmlTextReader reader = new XmlTextReader(csProjFileName);
            xlroot = XElement.Load(reader);
            reader.Close();
            RootElements = xlroot.Elements();
        }
        IEnumerable<XElement> RootElements;
        public Version GetCurrentVersion() {
            
           
            var references = RootElements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);
            string _dxLibraryString = null;
            if (dxlibraries.Count() > 0)
                _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();
            Version v = new Version(_dxLibraryString,true);
            return v;
            //return Version.Zero;
        }
        public void DisableUseVSHostingProcess() {
            var UseVSHostingProcess = RootElements.SelectMany(x => x.Elements()).Where(y => y.Name.LocalName == "UseVSHostingProcess").FirstOrDefault();
            if (UseVSHostingProcess != null)
                UseVSHostingProcess.SetValue("false");
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

    public    void SaveNewCsProj() {
            string resultString = xlroot.ToString();
            StreamWriter sw = new StreamWriter(csProjFileName, false);
            sw.Write(resultString);
            sw.Close();
        }
    }
}
