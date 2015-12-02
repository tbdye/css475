﻿using System;
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
            ExecuteNonQuery(cmd);
        }

        public IEnumerable<AllRollingStock> GetAllRollingStockForTrainDB(int trainNumber)
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
                                        + "Where OnTrain = {0}; ", trainNumber);
            return convert<AllRollingStock>(ExecuteReaderList(cmd, new AllRollingStock()));
        }

        public IEnumerable<Train> GetAllTrainInfoDB()
        {
            string cmd = String.Format("SELECT * FROM Trains as t "
                                        + "INNER JOIN TrainLocations as tl "
                                        + "ON t.TrainNumber = tl.TrainNumber "
                                        + "ORDER BY t.TrainNumber");
            return convert<Train>(ExecuteReaderList(cmd, new Train())).ToList();
        }

        public IEnumerable<Crew> GetCrewInfoDB()
        {
            string cmd = "SELECT * FROM Crews;";
            return convert<Crew>(ExecuteReaderList(cmd, new Crew()));
        }

        public IEnumerable<Industry> GetAllIndustriesOnModuleDB(string ModuleName)
        {
            string cmd = String.Format("SELECT * FROM Industries as i "
                                        +"INNER JOIN IndustriesAvailable AS ia "
                                        + "ON ia.IndustryName = i.IndustryName "
                                        + "inner join IndustryActivities as iaa "
                                        + "on iaa.IndustryName = i.IndustryName "
                                        + "WHERE i.OnModule = '{0}'", ModuleName);
            var toReturn = convert<Industry>(ExecuteReaderList(cmd, new Industry()));
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

        #region Helper Methods

        public Train GetTrain(int trainNumber)
        {
            string cmd = String.Format("SELECT * FROM Trains AS t\n"
                                        + "INNER JOIN TrainLocations as tl\n"
                                        + "ON tl.TrainNumber = t.TrainNumber\n"
                                        + "WHERE t.TrainNumber = {0};", trainNumber);
            return (Train)ExecuteReader(cmd, new Train());
        }

        public IEnumerable<Train> GetTrains()
        {
            string cmd = String.Format("SELECT * FROM Trains as t INNER JOIN TrainLocations as tl on t.TrainNumber = tl.TrainNumber");
            return convert<Train>(ExecuteReaderList(cmd, new Train()));
        }

        public Module GetModule(Train train)
        {
            string cmd = String.Format("SELECT * FROM Modules WHERE ModuleName = '{0}'", train.Module);
            return (Module)ExecuteReader(cmd, new Module());
        }

        public IEnumerable<Module> GetModules()
        {
            string cmd = String.Format("SELECT * FROM Modules;");
            return convert<Module>(ExecuteReaderList(cmd, new Module()));
        }

        public IEnumerable<Product> GetAllProductsForindustryDB(string industryName)
        {
            string cmd = String.Format("SELECT * FROM IndustryProducts AS ip "
                                        + "INNER JOIN ProductTypes AS pt "
                                        + "ON pt.ProductTypeName = ip.UsingProductType "
                                        + "where ip.ForIndustry = '{0}';", industryName);
            return convert<Product>(ExecuteReaderList(cmd, new Product()));
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
            return convert<Siding>(ExecuteReaderList(cmd, new Siding()));
        }

        public IEnumerable<Yard> GetAllYardsOnModuleDB(string moduleName)
        {
            string cmd = String.Format("SELECT * FROM Yards WHERE OnModule = '{0}'", moduleName);
            return convert<Yard>(ExecuteReaderList(cmd, new Yard()));
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
                Console.WriteLine(ex.Message);
                return null;
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
                Console.WriteLine(ex.Message);
                return null;
            }
            return toReturn;
        }

        private void ExecuteNonQuery(string cmd)
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
                Console.WriteLine(ex.Message);
            }
        }

        private IEnumerable<T> convert<T>(IEnumerable<DBEntity> values) where T : DBEntity
        {
            List<T> toReturn = new List<T>();
            foreach (var value in values)
            {
                toReturn.Add((T)value);
            }
            return toReturn;
        }

        #endregion
    }
}
