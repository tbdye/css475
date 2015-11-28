using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class DisplayUserTrain
    {
        public DisplayUserTrain() 
        {
            this.UsingCar = new List<string>();
            this.CarType = new List<string>();
            this.ProductType = new List<string>();
            this.ToIndustry = new List<string>();
        }

        public string Crew { get; set; }
        public int DCCAddress { get; set; }
        public List<string> UsingCar { get; set; }
        public List<string> CarType { get; set; }
        public List<string> ProductType { get; set; }
        public List<string> ToIndustry { get; set; }

    }
}
