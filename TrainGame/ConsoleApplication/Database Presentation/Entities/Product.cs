using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;

namespace Database_Presentation.Entities
{
    public class Product : DBEntity
    {
        public Product() { }
        public Product(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }
        public override void SetUp(MySqlDataReader reader)
        {
            this.ProductTypeName = reader["UsingProductType"].ToString();
            this.RollingStockType = reader["OnRollingStockType"].ToString();
            this.IndustryName = reader["ForIndustry"].ToString();
            this.isProducer = (bool)reader["IsProducer"];
        }
        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Product(reader);
        }

        public string ProductTypeName { get; set; }
        public string RollingStockType { get; set; }
        public string IndustryName { get; set; }
        public bool isProducer { get; set; }
    }
}
