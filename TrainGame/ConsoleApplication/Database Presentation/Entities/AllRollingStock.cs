using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class AllRollingStock
    {
        public AllRollingStock() { }
        public int TrainNumber { get; set; }
        public string CarID { get; set; }
        public string YardName { get; set; }
        public string CarType { get; set; }
        public string Description { get; set; }
        public int CarLength { get; set; }
    }
}
