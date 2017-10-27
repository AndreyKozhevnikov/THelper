using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace THelper.Classes {
    public class DXProjDocument {
        public XDocument RootDocument { get; set; }
        public IEnumerable<XElement> RootElements { get; set; }
        public string csProjFileName { get; set; }
        public DXProjDocument(XDocument doc, string _csProjFileName) {
            RootDocument = doc;
            RootElements = doc.Elements().Elements();
            csProjFileName = _csProjFileName;
        }
    }
}
