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
        private const string server = "home.tfenet.com";
        private const string port = "3306";
        private const string schema = "css475group";
        private const string userName = "css475group";
        private const string password = "b193297cc9c356";
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

        public void UpdateShippingTimeStamp(int id, string type) 
        {
            string cmd = "";
            if (type.Equals("PICKUP"))
            {
                cmd = String.Format("UPDATE Shipments SET TimePickedUp = CURRENT_TIMESTAMP where ShipmentId = {0}", id);
            }
            else if (type.Equals("DELIVERY"))
            {
                cmd = String.Format("UPDATE Shipments SET TimePickedUp = CURRENT_TIMESTAMP where ShipmentId = {0}", id);
            }
            if (cmd.Equals(""))
                return;
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

        public IEnumerable<Shipment> GetAllShippingInformationDB()
        {
            string cmd = String.Format("SELECT * FROM Shipments ORDER BY ShipmentID;");
            List<Shipment> values = new List<Shipment>();
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
                                var shipment = new Shipment();

                                shipment.ShipmentID = (int)reader["ShipmentID"];

                                shipment.ProductType = reader["ProductType"].ToString();

                                shipment.FromIndustry = reader["FromIndustry"].ToString();

                                shipment.FromSiding = (int)reader["FromSiding"];

                                shipment.ToIndustry = reader["ToIndustry"].ToString();

                                shipment.ToSiding = (int)reader["ToSiding"];

                                shipment.TimeCreated = Convert.ToDateTime(reader["TimeCreated"].ToString());

                                shipment.TimePickedUp = Convert.ToDateTime(reader["TimePickedUp"].ToString());

                                shipment.TimeDelivered = Convert.ToDateTime(reader["TimeDelivered"].ToString());

                                values.Add(shipment);
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
                                train.onModule = reader["OnModule"].ToString();
                                train.TimeCreated = DateTime.Parse(reader["TimeCreated"].ToString());
                                train.TimeUpdated = DateTime.Parse(reader["TimeUpdated"].ToString());

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

        public DisplayUserTrain DisplayUserTrainDB(int id)
        {
            // Make sproc for this
            string cmd = String.Format("Select tc.WithCrew, t.DCCAddress, cc.UsingCar, rsc.CarType, s.ProductType, s.ToIndustry from Trains as t "
                                            +"inner join ConsistedCars as cc "
                                            +"on cc.OnTrain = t.TrainNumber "
                                            +"inner join TrainCrews as tc "
                                            +"on tc.OnTrain = t.TrainNumber "
                                            +"inner join Waybills as wb "
                                            +"on wb.OnCar = cc.UsingCar "
                                            +"inner join Shipments as s "
                                            +"on s.ShipmentID = wb.USingShipmentID "
                                            +"inner join RollingStockCars as rsc "
                                            +"on rsc.CarID = wb.OnCar "
                                            +"where t.TrainNumber = {0} "
                                            +"order by t.TrainNumber;", id);
            var userTrain = new DisplayUserTrain();
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
                            var value = -1;
                            reader.Read();

                            userTrain.Crew = (string)reader["WithCrew"];

                            Int32.TryParse(reader["DCCAddress"].ToString(), out value);
                            userTrain.DCCAddress = value;

                            userTrain.UsingCar.Add((string)reader["UsingCar"]);
                            userTrain.CarType.Add((string)reader["CarType"]);
                            userTrain.ProductType.Add((string)reader["ProductType"]);
                            userTrain.ToIndustry.Add((string)reader["ToIndustry"]);

                            while (reader.Read())
                            {
                                userTrain.UsingCar.Add((string)reader["UsingCar"]);
                                userTrain.CarType.Add((string)reader["CarType"]);
                                userTrain.ProductType.Add((string)reader["ProductType"]);
                                userTrain.ToIndustry.Add((string)reader["ToIndustry"]);
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
            return userTrain;
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

                            industry.IndustryName = reader["IndustryName"].ToString();
                            industry.OnModule = reader["OnModule"].ToString();
                            industry.OnMainLine = reader["OnMainLine"].ToString();
                            industry.isAvaliable = (bool)reader["IsAvailable"];
                            industry.ActivityLevel = (int)reader["ActivityLevel"];
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
                            junction.JunctionID = (int)reader["JunctionID"];

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

        public IEnumerable<Module> GetModuleInfoDB()
        {
            string cmd = String.Format("SELECT * FROM Modules ORDER BY ModuleName;");
            var modules = new List<Module>();
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
                                var module = new Module();

                                module.Name = reader["ModuleName"].ToString();

                                module.Owner = reader["ModuleOwner"].ToString();

                                module.IsAvaliable = (bool)reader["IsAvailable"];

                                module.Type = reader["ModuleType"].ToString();
                                module.Shape = reader["ModuleShape"].ToString();
                                module.Description = reader["Description"].ToString();

                                modules.Add(module);
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
            return modules;
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
