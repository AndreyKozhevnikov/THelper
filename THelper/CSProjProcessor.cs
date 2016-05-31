using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace THelper {

    public class CSProjProcessor {
        private string csProjFileName;

        public CSProjProcessor(string _csProjFileName) {
            csProjFileName = _csProjFileName;
        }

        XElement xlroot;
        void OpenFile() {
            XmlTextReader reader = new XmlTextReader(csProjFileName);
            xlroot = XElement.Load(reader);
            reader.Close();
        }

        public Version GetCurrentVersion() {
            OpenFile();
            var elements = xlroot.Elements();
            var references = elements.Where(x => x.Name.LocalName == "ItemGroup" && x.Elements().Count() > 0 && x.Elements().First().Name.LocalName == "Reference");
            var dxlibraries = references.Elements().Where(x => x.Attribute("Include").Value.IndexOf("DevExpress", StringComparison.OrdinalIgnoreCase) >= 0);
            string _dxLibraryString = null;
            if (dxlibraries.Count() > 0)
                _dxLibraryString = dxlibraries.First().Attribute("Include").ToString();
            Version v = new Version(_dxLibraryString,true);
            return v;
            //return Version.Zero;
        }
    }
}
