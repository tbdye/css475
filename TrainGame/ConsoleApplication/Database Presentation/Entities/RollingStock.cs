using MySql.Data.MySqlClient;
using System;

namespace Database_Presentation.Entities
{
    public class RollingStock : DBEntity
    {
        public RollingStock()
        {
        }

        public RollingStock(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            var value = 0;
            this.CarID = reader["CarID"].ToString();
            this.CarType = reader["CarType"].ToString();
            this.Description = reader["Description"].ToString();
            Int32.TryParse(reader["CarLength"].ToString(), out value);
            this.CarLength = value;
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new RollingStock(reader);
        }

        public string CarID { get; set; }
        public string CarType { get; set; }
        public string Description { get; set; }
        public int CarLength { get; set; }
    }
}