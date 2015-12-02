using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Database_Presentation.Entities;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;

namespace Database_Presentation.Entities
{
    public abstract class DBEntity
    {
        public abstract void SetUp(MySqlDataReader reader);

        public abstract DBEntity GetNew(MySqlDataReader reader);
    }
}
