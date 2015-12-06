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
        public bool UpdateTrainLocation(string moduleName, int trainNumber)
        {
            string cmd = String.Format("UPDATE TrainLocations SET OnModule = '{0}', TimeUpdated = DEFAULT WHERE TrainNumber = {1}", moduleName, trainNumber);
            return ExecuteNonQuery(cmd);
        }

        public IEnumerable<RollingStock> GetAllRollingStockForTrainDB(int trainNumber)
        {
            string cmd = String.Format("Select * from ConsistedCars as cc "
                                        + "inner join RollingStockCars as rsc "
                                        + "on cc.UsingCar = rsc.CarID "
                                        + "inner join RollingStockTypes as rst "
                                        + "on rsc.CarType = rst.CarTypeName "
                                        + "Where OnTrain = {0}; ", trainNumber);
            return ConvertToList<RollingStock>(ExecuteReaderList(cmd, new RollingStock()));
        }

        public IEnumerable<Train> GetAllTrainInfoDB()
        {
            string cmd = String.Format("SELECT * FROM Trains as t "
                                        + "INNER JOIN TrainLocations as tl "
                                        + "ON t.TrainNumber = tl.TrainNumber "
                                        + "ORDER BY t.TrainNumber");
            return ConvertToList<Train>(ExecuteReaderList(cmd, new Train())).ToList();
        }

        /// <summary>
        /// returns false when there is a car with that ID,
        /// and returns true when there is not a car with that ID
        /// </summary>
        /// <param name="CarID"></param>
        /// <returns></returns>
        public bool VerifyThatCarIDDoesNotExistDB(string CarID)
        {
            string cmd = String.Format("Select CarID From RollingStockCars WHERE CarID = '{0}';", CarID);
            List<string> values = GetListSingleString(cmd, "CarID").ToList();
            if (values != null && values.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public IEnumerable<string> RunGenericQuery(string cmd, string key)
        {
            return GetListSingleString(cmd, key);
        }

        private IEnumerable<string> GetListSingleString(string cmd, string key)
        {
            var values = new List<string>();
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
                                values.Add(reader[key].ToString());
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

        public IEnumerable<Crew> GetCrewInfoDB()
        {
            string cmd = "SELECT * FROM Crews;";
            return ConvertToList<Crew>(ExecuteReaderList(cmd, new Crew()));
        }

        public IEnumerable<Industry> GetAllIndustriesOnModuleDB(string ModuleName)
        {
            string cmd = String.Format("SELECT * FROM Industries as i "
                                        + "INNER JOIN IndustriesAvailable AS ia "
                                        + "ON ia.IndustryName = i.IndustryName "
                                        + "inner join IndustryActivities as iaa "
                                        + "on iaa.IndustryName = i.IndustryName "
                                        + "WHERE i.OnModule = '{0}'", ModuleName);
            var toReturn = ConvertToList<Industry>(ExecuteReaderList(cmd, new Industry()));
            if (toReturn != null)
            {
                foreach (var value in toReturn)
                {
                    value.UsingProductTypes = GetAllProductsForindustryDB(value.IndustryName).ToList();
                    value.Sidings = GetAllSidingsForIndustryDB(value.IndustryName).ToList();
                }
            }
            return toReturn;
        }

        public IEnumerable<Industry> GetAllIndustriesDB()
        {
            string cmd = "SELECT * FROM Industries AS i\n"
                            + "INNER JOIN IndustriesAvailable AS ia\n"
                            + "ON i.IndustryName = ia.IndustryName\n"
                            + "INNER JOIN IndustryActivities AS iaa\n"
                            + "ON ia.IndustryName = iaa.IndustryName;";

            return ConvertToList<Industry>(ExecuteReaderList(cmd, new Industry()));
        }

        /// <summary>
        ///  This function moves a car from one industry to another,
        ///  Upon success that car will also be removed from the train.
        /// </summary>
        /// <param name="trainNumber">The train number of the currentplayer</param>
        /// <param name="carId">The carID of the car that you want to remove</param>
        /// <param name="industryName">The industry which the car is currently at</param>
        /// <returns></returns>
        public bool DropOffCarAtLocationDB(int trainNumber, string carId, string industryName)
        {
            string cmd = String.Format("INSERT INTO RollingStockAtIndustries \n "
                                        + "VALUES ('{0}', '{1}', DEFAULT);"
                                        , carId, industryName);

            var success = ExecuteNonQuery(cmd);
            if (success)
            {
                cmd = String.Format("DELETE FROM ConsistedCars "
                                     + "WHERE OnTrain = {0} "
                                     + "AND UsingCar = '{1}'"
                                     , trainNumber, carId);
                return ExecuteNonQuery(cmd);
            }
            // Could not add the car to the industry, so do not remove it from car.
            return false;
        }

        public IEnumerable<RollingStock> GetAllRollingStockForModuleDB(string ModuleName)
        {
            string cmd = String.Format("Select * from ConsistedCars as cc "
                                        + "inner join RollingStockAtYards as rsy "
                                        + "on cc.UsingCar = rsy.CarID "
                                        + "inner join RollingStockAtIndustries as rsi "
                                        + "on cc.UsingCar = rsi.CarID "
                                        + "inner join RollingStockCars as rsc "
                                        + "on cc.UsingCar = rsc.CarID "
                                        + "inner join RollingStockTypes as rst "
                                        + "on rsc.CarType = rst.CarTypeName "
                                        + "inner join Yards as y "
                                        + "on y.YardName = AtYard "
                                        + "Where OnModule = '{0}';", ModuleName);
            return ConvertToList<RollingStock>(ExecuteReaderList(cmd, new RollingStock()));
        }

        public bool AddCarToTrainDB(int trainNumber, string carId, string industryName)
        {
            string cmd = String.Format("INSERT INTO ConsistedCars VALUES ({0}, '{1}', DEFAULT);",
                                            trainNumber, carId);
            var success = ExecuteNonQuery(cmd);
            if (success)
            {
                cmd = String.Format("DELETE FROM RollingStockAtIndustries WHERE CarID = '{0}' AND AtIndustry = '{1}';", carId, industryName);
                return ExecuteNonQuery(cmd);
            }
            // Could not add the car to the train, so do not remove it from industry.
            return false;
        }

        public bool CreateAndAddCarToTrain(int trainNumber, string carID, string carType)
        {
            string cmd = string.Format("INSERT INTO RollingStockCars VALUES ('{0}', '{1}', DEFAULT);",
                                            carID, carType);
            var success = ExecuteNonQuery(cmd);
            if (success)
            {
                cmd = string.Format("INSERT INTO ConsistedCars VALUES ({0}, '{1}', DEFAULT);",
                                                trainNumber, carID);
                success = ExecuteNonQuery(cmd);
                if (!success)
                {
                    // Car doesnt need to be added to the game if it can't get immediatly get added to your car.
                    cmd = string.Format("DELETE FROM RollingStockCars WHERE CarID = '{0}' AND CarType = '{1}'",
                                            carID, carType);
                    ExecuteNonQuery(cmd);
                }
                return success;
            }
            return false;
        }

        public bool RemoveCarFromTrain(int trainNumber, string CarID)
        {
            string cmd = string.Format("DELETE FROM ConsistedCars WHERE OnTrain = {0} AND UsingCar = '{1}';", trainNumber, CarID);
            return ExecuteNonQuery(cmd);
        }

        public IEnumerable<Module> GetAllOtherModulesYourTrainIsNotOnDB(string module)
        {
            var cmd = String.Format("SELECT * FROM Modules as m "
                                    + "INNER JOIN ModulesAvailable AS ma "
                                    + "ON ma.ModuleName = m.ModuleName "
                                    + "WHERE m.ModuleName <> '{0}'",
                                    module);
            return ConvertToList<Module>(ExecuteReaderList(cmd, new Module()));
        }

        #region Helper Methods

        public Train GetTrain(int trainNumber)
        {
            string cmd = String.Format("SELECT * FROM Trains AS t\n"
                                        + "INNER JOIN TrainLocations as tl\n"
                                        + "ON tl.TrainNumber = t.TrainNumber\n"
                                        + "WHERE t.TrainNumber = {0};", trainNumber);
            return (Train)ExecuteReader(cmd, new Train());
        }

        public IEnumerable<Product> GetAllProductsForindustryDB(string industryName)
        {
            string cmd = String.Format("SELECT * FROM IndustryProducts AS ip "
                                        + "INNER JOIN ProductTypes AS pt "
                                        + "ON pt.ProductTypeName = ip.UsingProductType "
                                        + "where ip.ForIndustry = '{0}';", industryName);
            return ConvertToList<Product>(ExecuteReaderList(cmd, new Product()));
        }

        public IEnumerable<Siding> GetAllSidingsForIndustryDB(string industryName)
        {
            string cmd = String.Format("SELECT * FROM IndustrySidings AS i "
                                        + "INNER JOIN SidingAssignments as sa "
                                        + "ON i.ForIndustry = sa.ForIndustry "
                                        + "INNER JOIN SidingsAvailableLength as sal "
                                        + "ON sal.ForIndustry = sa.ForIndustry "
                                        + "WHERE i.ForIndustry = '{0}' "
                                        + "GROUP BY SidingLength", industryName);
            return ConvertToList<Siding>(ExecuteReaderList(cmd, new Siding()));
        }

        public IEnumerable<Yard> GetAllYardsOnModuleDB(string moduleName)
        {
            string cmd = String.Format("SELECT * FROM Yards WHERE OnModule = '{0}'", moduleName);
            return ConvertToList<Yard>(ExecuteReaderList(cmd, new Yard()));
        }

        public IEnumerable<RollingStock> GetRollingStockValuesByIndustryNameDB(string IndustryName)
        {
            string cmd = String.Format("Select * from RollingStockAtIndustries as rsi "
                               + "inner join RollingStockCars as rsc "
                               + "on rsi.CarID = rsc.CarID "
                               + "inner join RollingStockTypes as rst "
                               + "on rsc.CarType = rst.CarTypeName "
                               + "Where AtIndustry = '{0}'; ", IndustryName);
            return ConvertToList<RollingStock>(ExecuteReaderList(cmd, new RollingStock()));
        }

        private IEnumerable<DBEntity> ExecuteReaderList(string cmd, DBEntity type)
        {
            var values = new List<DBEntity>();
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
                                values.Add(type.GetNew(reader));
                            }
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                throw new DBException(ex.Message);
            }
            return values;
        }

        private DBEntity ExecuteReader(string cmd, DBEntity type)
        {
            DBEntity toReturn = null;
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
                            toReturn = type.GetNew(reader);
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                throw new DBException(ex.Message);
            }
            return toReturn;
        }

        private bool ExecuteNonQuery(string cmd)
        {
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
                throw new DBException(ex.Message);
            }
            return true;
        }

        private IEnumerable<T> ConvertToList<T>(IEnumerable<DBEntity> values) where T : DBEntity
        {
            if (values != null)
            {
                List<T> toReturn = new List<T>();
                foreach (var value in values)
                {
                    toReturn.Add((T)value);
                }
                return toReturn;
            }
            return null;
        }

        #endregion Helper Methods
    }
}