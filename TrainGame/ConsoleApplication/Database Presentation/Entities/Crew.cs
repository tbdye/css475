using MySql.Data.MySqlClient;

namespace Database_Presentation.Entities
{
    public class Crew : DBEntity
    {
        public Crew()
        {
        }

        public Crew(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            this.CrewName = reader["CrewName"].ToString();
            this.Description = reader["Description"].ToString();
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new Crew(reader);
        }

        public string CrewName { get; set; }
        public string Description { get; set; }
    }
}