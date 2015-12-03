using MySql.Data.MySqlClient;

namespace Database_Presentation.Entities
{
    public abstract class DBEntity
    {
        public abstract void SetUp(MySqlDataReader reader);

        public abstract DBEntity GetNew(MySqlDataReader reader);
    }
}