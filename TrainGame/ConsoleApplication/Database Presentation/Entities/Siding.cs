using MySql.Data.MySqlClient;
using System;

namespace Database_Presentation.Entities
{
    public class Siding : DBEntity
    {
        public Siding()
        {
        }

        public Siding(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            var value = 0;
            this.IndustryName = reader["ForIndustry"].ToString();
            this.ProductType = reader["ForProductType"].ToString();
            Int32.TryParse(reader["SidingNumber"].ToString(), out value);
            this.SidingNumber = value;
            Int32.TryParse(reader["SidingLength"].ToString(), out value);
            this.SidingLength = value;
            this.AvaliableSidingLength = value;
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Siding(reader);
        }

        public string IndustryName { get; set; }
        public string ProductType { get; set; }
        public int SidingNumber { get; set; }
        public int SidingLength { get; set; }
        public int AvaliableSidingLength { get; set; }
    }
}