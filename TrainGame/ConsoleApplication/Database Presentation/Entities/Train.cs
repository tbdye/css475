using MySql.Data.MySqlClient;
using System;

namespace Database_Presentation.Entities
{
    public class Train : DBEntity
    {
        public Train()
        {
        }

        public Train(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            var value = 0;

            Int32.TryParse(reader["TrainNumber"].ToString(), out value);
            this.TrainNumber = value;
            Int32.TryParse(reader["LeadPower"].ToString(), out value);
            this.LeadPower = value;
            Int32.TryParse(reader["DCCAddress"].ToString(), out value);
            this.DCCAddress = value;
            this.TimeModuleUpdated = DateTime.Parse(reader["TimeUpdated"].ToString());
            this.TimeCreated = DateTime.Parse(reader["TimeCreated"].ToString());
            this.Module = reader["OnModule"].ToString();
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Train(reader);
        }

        public int TrainNumber { get; set; }
        public int LeadPower { get; set; }
        public int DCCAddress { get; set; }
        public string Module { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeModuleUpdated { get; set; }
    }
}