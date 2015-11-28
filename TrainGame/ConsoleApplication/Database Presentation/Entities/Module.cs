using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class Module
    {
        public Module() { }

        public string Name {get; set;}
        public string Owner {get; set;}
        public bool IsAvaliable {get; set;}
        public string Type {get; set;}
        public string Shape {get; set;}
        public string Description {get; set;}
    }
}
