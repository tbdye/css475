using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.Entity;
using Database_Presentation.Entities;

namespace Database_Presentation
{
    public class DBHandler
    {
        // Set these values to the specific server before running. 
        private const string server = "FIX ME";
        private const string port = "FIX ME";
        private const string schema = "FIX ME";
        private const string userName = "FIX ME";
        private const string password = "FIX ME";
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

        public DBHandler(DbConnection existingConnection, bool contextOwnsConnection)
        {

        }

        public IEnumerable<string> GetProductTypeForIndustryDB(string Industry)
        {
            string cmd = String.Format("SELECT UsingProductType FROM IndustryProducts WHERE ForIndustry = '{0}';", Industry);
            List<string> ListOfProductTypes = new List<string>();
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
                                ListOfProductTypes.Add(reader["UsingProductType"].ToString());
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
            return ListOfProductTypes;
        }

        public IEnumerable<Train> GetTrainInfoDB(int trainNumber)
        {
            string cmd = String.Format("SELECT * FROM Trains "
                                +"WHERE TrainNumber = {0};", trainNumber);
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
                            reader.Read();
                            var train = new Train();
                            int value = -1;

                            Int32.TryParse(reader["TrainNumber"].ToString(), out value);
                            train.TrainNumber = value;

                            Int32.TryParse(reader["LeadPower"].ToString(), out value);
                            train.LeadPower = value;

                            Int32.TryParse(reader["DCCAddress"].ToString(), out value);
                            train.DCCAddress = value;

                            train.onModule = reader["OnModule"].ToString();
                            train.TimeCreated = DateTime.Parse(reader["TimeCreated"].ToString());
                            train.TimeCreated = DateTime.Parse(reader["TimeUpdated"].ToString());

                            ListOfTrains.Add(train);
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

        public Industry GetIndustryInfoByNameDB(string industryName)
        {
            string cmd = String.Format("SELECT * FROM Industries WHERE IndustryName = '" + industryName + "';");
            var industry = new Industry();
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
                            reader.Read();
                            int value = -1;

                            industry.IndustryName = reader["IndustryName"].ToString();
                            industry.OnModule = reader["OnModule"].ToString();
                            industry.OnMainLine = reader["OnMainLine"].ToString();

                            Int32.TryParse(reader["IsAvailable"].ToString(), out value);
                            industry.isAvaliable = value == 1;

                            Int32.TryParse(reader["ActivityLevel"].ToString(), out value);
                            industry.ActivityLevel = value;
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return industry;
        }

        public Junction GetJunctionInfoByJunctionIDDB(int junctionID)
        {
            string cmd = String.Format("SELECT * FROM Junctions WHERE JunctionID = '" + junctionID + "';");
            var junction = new Junction();
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
                            reader.Read();
                            int value = -1;

                            Int32.TryParse(reader["JunctionID"].ToString(), out value);
                            junction.JunctionID = value;

                            junction.OnModule = reader["OnModule"].ToString();
                            junction.FromLine = reader["FromLine"].ToString();
                            junction.ToLine = reader["ToLine"].ToString();

                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return junction;
        }

        public string GetProductRollingStockTypeByProductTypeNameDB(string typeName)
        {
            string toReturn;
            string cmd = String.Format("SELECT OnRollingStockType FROM ProductTypes WHERE ProductTypeName = '"+ typeName + "';");
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
                            reader.Read();
                            toReturn = reader["OnRollingStockType"].ToString();
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return toReturn;
        }
    }
}
