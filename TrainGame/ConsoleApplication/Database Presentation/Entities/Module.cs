using MySql.Data.MySqlClient;
using System;

namespace Database_Presentation.Entities
{
    public class Module : DBEntity
    {
        public Module()
        {
        }

        public Module(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            this.Name = reader["ModuleName"].ToString();
            this.Owner = reader["ModuleOwner"].ToString();
            this.IsAvaliable = (bool)reader["IsAvailable"];
            this.Shape = reader["ModuleShape"].ToString();
            this.Description = reader["Description"].ToString();
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Module(reader);
        }

        public string Name { get; set; }
        public string Owner { get; set; }
        public bool IsAvaliable { get; set; }
        public string Type { get; set; }
        public string Shape { get; set; }
        public string Description { get; set; }
    }
}