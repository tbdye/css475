﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class MainLine
    {
        public MainLine() { }
        public string Name { get; set; }
        public string Module { get; set; }
        public bool IsContiguous { get; set; } 
    }
}
