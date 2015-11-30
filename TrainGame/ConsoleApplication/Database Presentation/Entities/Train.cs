using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{

    public class Train
    {
        public Train() { }
        public int TrainNumber { get; set; }
        public int LeadPower { get; set; }
        public int DCCAddress { get; set; }
        public string OnModule { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeModuleUpdated { get; set; }
    }
}
