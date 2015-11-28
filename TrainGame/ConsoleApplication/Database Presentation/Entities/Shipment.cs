using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class Shipment
    {
        public Shipment() { }

        public int ShipmentID { get; set; }
        public string ProductType { get; set; }
        public string FromIndustry { get; set; }
        public int FromSiding { get; set; }
        public string ToIndustry { get; set; }
        public int ToSiding { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimePickedUp { get; set; }
        public DateTime TimeDelivered { get; set; }

    }
}
