using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THelper {
  public  class Project {
      private string p;

      public Project(string p) {
          // TODO: Complete member initialization
          this.p = p;
      }
      
      public int Major { get; set; }
      public int Minor { get; set; }
    }
}
