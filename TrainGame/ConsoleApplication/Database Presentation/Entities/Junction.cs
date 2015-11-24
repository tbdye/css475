using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class Junction
    {
        public Junction() { }
        public int JunctionID { get; set; }
        public string OnModule { get; set; }
        public string FromLine { get; set; }
        public string ToLine { get; set; }

    }
}
