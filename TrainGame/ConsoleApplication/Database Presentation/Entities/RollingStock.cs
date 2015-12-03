﻿using MySql.Data.MySqlClient;
using System;

namespace Database_Presentation.Entities
{
    public class RollingStock : DBEntity
    {
        public RollingStock()
        {
        }

        public RollingStock(MySqlDataReader reader)
        {
            this.SetUp(reader);
        }

        public override void SetUp(MySqlDataReader reader)
        {
            var value = 0;
            Int32.TryParse(reader["OnTrain"].ToString(), out value);
            this.TrainNumber = value;
            this.CarID = reader["UsingCar"].ToString();
            this.YardName = reader["AtYard"].ToString();
            this.CarType = reader["CarType"].ToString();
            this.Description = reader["Description"].ToString();
            Int32.TryParse(reader["CarLength"].ToString(), out value);
            this.CarLength = value;
        }

        public override DBEntity GetNew(MySqlDataReader reader)
        {
            return new RollingStock(reader);
        }

        public int TrainNumber { get; set; }
        public string CarID { get; set; } //
        public string YardName { get; set; } //
        public string CarType { get; set; } //
        public string Description { get; set; } //
        public int CarLength { get; set; } //
    }
}