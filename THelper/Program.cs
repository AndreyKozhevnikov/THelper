using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace THelper {
    class Program {
        static void Main(string[] args) {
            if (args.Count() > 0 && args[0] is string) {
                string filePath = args[0];
                ProjectProcessor p = new ProjectProcessor(filePath);
                p.MyFileWorker = new FileWorker();
                p.MyMessageProcessor = new MessageProcessor();
                p.ProcessArchive();
            }

        }

     

       
    }
}
