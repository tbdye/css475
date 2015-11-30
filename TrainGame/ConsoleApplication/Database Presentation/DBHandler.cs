using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using System.Data.Common;
using Database_Presentation.Entities;

namespace Database_Presentation
{
    public class DBHandler
    {
        // Set these values to the specific server before running. 
        private const string server = "";
        private const string port = "";
        private const string schema = "";
        private const string userName = "";
        private const string password = "";
        private string connectionString = string.Format(
            "server={0};port={1};database={2};uid={3};password={4}",
            server,
            port,
            schema,
            userName,
            password);

        public DBHandler()
        {

        }

        // Will need (needs to be updated to v 1.1)
        public void UpdateTrainLocation(string module, int id)
        {
            string cmd = String.Format("UPDATE Trains SET OnModule = '{0}', TimeUpdated = CURRENT_TIMESTAMP where TrainNumber = {1}", module, id);
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = cmd;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public IEnumerable<Train> GetTrainInfoDB()
        {
            string cmd = String.Format("SELECT * FROM Trains ORDER BY TrainNumber");
            List<Train> ListOfTrains = new List<Train>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = cmd;
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var value = -1;
                                var train = new Train(); 

                                train.TrainNumber = (int)reader["TrainNumber"];

                                Int32.TryParse(reader["LeadPower"].ToString(), out value);
                                train.LeadPower = value;
                                Int32.TryParse(reader["DCCAddress"].ToString(), out value);
                                train.DCCAddress = value;

                                train.TimeCreated = DateTime.Parse(reader["TimeCreated"].ToString());
                                  
                                ListOfTrains.Add(train);
                            }
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return ListOfTrains;
        }

        public IEnumerable<Crew> GetCrewInfoDB()
        {
            string cmd = "SELECT * FROM Crews;";
            var values = new List<Crew>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = cmd;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var crew = new Crew();

                                crew.CrewName = reader["CrewName"].ToString();
                                crew.Description = reader["Description"].ToString();
                                values.Add(crew);
                            }
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return values;
        }

    }
}
