using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Database_Presentation.Entities
{
    public class Industry : DBEntity
    {
        public Industry()
        {
            this.UsingProductTypes = new List<Product>();
            this.Sidings = new List<Siding>();
        }

        public Industry(MySqlDataReader reader)
            : this()
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            var value = 0;
            this.IndustryName = reader["IndustryName"].ToString();
            this.OnModule = reader["OnModule"].ToString();
            this.OnMainLine = reader["OnMainLine"].ToString();
            this.isAvaliable = (bool)reader["IsAvailable"];
            Int32.TryParse(reader["ActivityLevel"].ToString(), out value);
            this.ActivityLevel = value;
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Industry(reader);
        }

        public string IndustryName { get; set; }
        public string OnModule { get; set; }
        public string OnMainLine { get; set; }
        public bool isAvaliable { get; set; }
        public int ActivityLevel { get; set; }
        public List<Product> UsingProductTypes { get; set; }
        public List<Siding> Sidings { get; set; }
    }
}