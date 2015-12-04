using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation.Entities
{
    public class TupleNode
    {
        public Industry industry { get; set; }
        public IEnumerable<RollingStock> rollingStock { get; set; }

        public TupleNode() :
            this(new Industry(), new List<RollingStock>())
        {
        }

        public TupleNode(Industry industry, IEnumerable<RollingStock> rollingStock)
        {
            this.industry = industry;
            this.rollingStock = rollingStock;
        }

        public Industry getIndustry()
        {
            return this.industry;
        }

        public IEnumerable<RollingStock> getRollingStock()
        {
            return this.rollingStock;
        }
    }
}