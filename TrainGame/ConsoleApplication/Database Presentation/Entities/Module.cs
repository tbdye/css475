using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;

namespace Database_Presentation.Entities
{
    public class Module : DBEntity
    {
        public Module() { }

        public Module(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            throw new NotImplementedException();
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Module(reader);
        }

        public string Name {get; set;}
        public string Owner {get; set;}
        public bool IsAvaliable {get; set;}
        public string Type {get; set;}
        public string Shape {get; set;}
        public string Description {get; set;}
    }
}
