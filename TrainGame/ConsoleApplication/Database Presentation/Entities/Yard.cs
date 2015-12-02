using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;

namespace Database_Presentation.Entities
{
    public class Yard : DBEntity
    {
        public Yard() { }

        public Yard(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            this.Name = reader["YardName"].ToString();
            this.Module = reader["OnModule"].ToString();
            this.MainLine = reader["OnMainLine"].ToString();
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Yard(reader);
        }

        public string Name { get; set; }
        public string Module { get; set; }
        public string MainLine { get; set; }
    }
}
