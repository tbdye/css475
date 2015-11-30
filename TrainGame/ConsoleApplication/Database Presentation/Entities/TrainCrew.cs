using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class TrainCrew
    {
        public TrainCrew() { }
        public int OnTrain { get; set; }
        public string WithCrew { get; set; }
        public DateTime TimeJoined { get; set; }
    }
}
