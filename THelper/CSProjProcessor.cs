using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}
