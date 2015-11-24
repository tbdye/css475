using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class Industry
    {
        public Industry() { }
        public string IndustryName { get; set; }
        public string OnModule { get; set; }
        public string OnMainLine { get; set; }
        public bool isAvaliable { get; set; }
        public int ActivityLevel { get; set; }
    }
}
