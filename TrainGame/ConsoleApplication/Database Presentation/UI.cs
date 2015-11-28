using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database_Presentation.Entities;

namespace Database_Presentation
{
    public class UI
    {
        private DBHandler DB;
        public UI()
        {
            DB = new DBHandler();
        }
        public void Start()
        {
            PrintOptions();
            String Input = Console.ReadLine();
            Int16 numInput = -1;
            while (Int16.TryParse(Input, out numInput))
            {
                switch (numInput)
                {
                    case 1:
                        GetProductByIndustry();
                        break;
                    case 2:
                        GetIndustryByName();
                        break;
                    case 3:
                        GetJunctionByID();
                        break;
                    case 4:
                        GetProductRollingStockTypeByProductTypeName();
                        break;
                    case 5:
                        UpdateShipmentInformation();
                        break;
                    case 6:
                        ReportTrainArrival();
                        break;
                    default:
                        Console.WriteLine("Sorry '{0}' was not a good input", numInput);
                        break;
                }

                ClearScreen();
                PrintOptions();
                Input = getInput();
                if (Input.ToUpper().Equals("EXIT") || Input.ToUpper().Equals("QUIT")
                    || Input.ToUpper().Equals("E") || Input.ToUpper().Equals("Q"))
                {
                    return;
                }
            }
        }

        private int getInputAndReturnNumber()
        {
            int toReturn = 0;
            while(!Int32.TryParse(Console.ReadLine(), out toReturn))
            {
                Console.WriteLine("This input {0} is not a valid input, \nPlease try again", toReturn);
            }
            return toReturn;
        }

        private string getInput()
        {
            return Console.ReadLine();
        }

        private void ClearScreen()
        {
            Console.Clear();
        }

        private void PrintOptions()
        {
            Console.WriteLine("\nChoose an entry");
            Console.WriteLine("_________________________________________________________________________________");
            Console.WriteLine("\tOption Number\tOption Name");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("\t1\t\tGet Product By Industry");
            Console.WriteLine("\t2\t\tGet Industry By Name ");
            Console.WriteLine("\t3\t\tGet Junction By ID");
            Console.WriteLine("\t4\t\tGet Product Rolling Stock Type by Product Type Name");
            Console.WriteLine("\t5\t\tUpdate Shipment Information");
            Console.WriteLine("\t6\t\tReport Train Arival At Module");
            Console.WriteLine("_________________________________________________________________________________");
            Console.Write("\n\t");
        }

        private int AskForNumber(string type)
        {
            Console.Clear();
            switch (type) { 
                case "Train":
                    Console.Write("Please enter the train number you are interested in:  ");
                    break;

                case "Junction":
                    Console.Write("Please enter the junction ID you are interested in:  ");
                    break;

                default: 
                    return -1;
        }
            return getInputAndReturnNumber();
        }

        private void GetProductByIndustry()
        {
            Console.Clear();
            Console.Write("Please enter the industry you want to see products in: ");
            string industry = Console.ReadLine();
            Console.WriteLine();
            List<string> results = DB.GetProductTypeForIndustryDB(industry).ToList();
            if(results == null) 
            {
                EndFunctionError();
                return;
            }
            Console.WriteLine("Industry: " + industry);
            Console.WriteLine("Product(s)");
            Console.WriteLine("____________________________________________________________________");
            for (int i = 0; i < results.Count(); i++ )
            {
                Console.WriteLine("\t{0}) {1}", i+1, results[i]);
            }
            Console.WriteLine("____________________________________________________________________");
            EndFunction();

        }

        private void GetIndustryByName()
        {
            Console.Clear();
            Console.Write("Please enter the industry you want to see: ");
            string industry = Console.ReadLine();
            Console.WriteLine();
            var result = DB.GetIndustryInfoByNameDB(industry);
            if (result == null)
            {
                EndFunctionError();
                return;
            }
            Console.WriteLine("Industry: " + industry);
            Console.WriteLine("____________________________________________________________________");
            Console.WriteLine("On Module: " + result.OnModule);
            Console.WriteLine("On Main Line:" + result.OnMainLine);
            Console.WriteLine("Avaliable: " + (result.isAvaliable ? "True" : "False"));
            Console.WriteLine("Activity Level: " + result.ActivityLevel);
            Console.WriteLine("____________________________________________________________________");
            EndFunction();
        }

        private void GetJunctionByID()
        {
            int junctionID = AskForNumber("Junction");
            var result = DB.GetJunctionInfoByJunctionIDDB(junctionID);
            if (result == null)
            {
                EndFunctionError();
                return;
            }
            Console.WriteLine("Junction ID: " + junctionID);
            Console.WriteLine("____________________________________________________________________");
            Console.WriteLine("On Module: " + result.OnModule);
            Console.WriteLine("From Line:" + result.FromLine);
            Console.WriteLine("To Line: " + result.ToLine);
            Console.WriteLine("____________________________________________________________________");
            EndFunction();
        }
        private void GetProductRollingStockTypeByProductTypeName()
        {
            Console.Clear();
            Console.Write("Please enter the Product Type Name you want to see: ");
            string ProductTypeName = Console.ReadLine();
            Console.WriteLine();
            var result = DB.GetProductRollingStockTypeByProductTypeNameDB(ProductTypeName);
            if (result == null)
            {
                EndFunctionError();
                return;
            }
            Console.WriteLine("Product Type Name: " + ProductTypeName);
            Console.WriteLine("____________________________________________________________________");
            Console.WriteLine("Rolling Stock Type: " + result);
            Console.WriteLine("____________________________________________________________________");
            EndFunction();
        }

        private void UpdateShipmentInformation()
        {
            Console.Clear();
            IEnumerable<Shipment> Shipments = DB.GetAllShippingInformationDB();
            if (Shipments == null)
            {
                EndFunctionError();
                return;
            }
            Console.WriteLine("Please select an ID from the folling options:"); 
            Console.Write("\nID");
            Console.CursorLeft += 9;
            Console.WriteLine("Product Type\t\t\tTime Created\t\tTime Picked Up\t\tTime Delivered");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------");
            foreach (var shipment in Shipments)
            {
                Console.Write(shipment.ShipmentID);
                Console.CursorLeft += 11 - shipment.ShipmentID.ToString().Length;
                Console.Write(shipment.ProductType); // If this is has more than 20 characters stuff will overlap (this is a temp fix)
                Console.CursorLeft += 20 - shipment.ProductType.Length;
                Console.Write("\t\t" + shipment.TimeCreated);
                Console.Write("\t" + shipment.TimePickedUp);
                Console.WriteLine("\t" + shipment.TimeDelivered);
            }
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------\n\n");
            int idInput = getInputAndReturnNumber();
            bool goodValue = false;
            while (!goodValue)
            {
                foreach (var shipment in Shipments)
                {
                    if (idInput == shipment.ShipmentID)
                    {
                        goodValue = true;
                        break;
                    }
                }
                if (goodValue)
                    continue;
                Console.WriteLine("That value {0} was not in the given set, please try again: ", idInput);
                idInput = getInputAndReturnNumber();
            }

            Console.WriteLine("Please enter which you would like to update.");
            Console.WriteLine("\t-Pickup [P]");
            Console.WriteLine("\t-Delivery [D]");
            Console.WriteLine("----------------------------------------------");
            string typeInput = getInput();
            while (!((typeInput.ToUpper().Equals("PICKUP") || typeInput.ToUpper().Equals("DELIVERY"))
                    || typeInput.ToUpper().Equals("P") || typeInput.ToUpper().Equals("D")))
            {
                Console.WriteLine("Your input {0} was not a valid input.", typeInput);
                Console.WriteLine("Pickup [P], Delivery [D], or Quit [Q] are your three options. Please retry");
                typeInput = getInput();
                if (typeInput.ToUpper().Equals("QUIT") || typeInput.ToUpper().Equals("Q"))
                    EndFunction();
            }
            DB.UpdateShippingTimeStamp(idInput, typeInput.ToUpper());
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("ID: {0} Updated... ", idInput);
            EndFunction();
        }

        private void ReportTrainArrival()
        {
            Console.Clear();
            Console.WriteLine("Which Train would you like to update?");
            IEnumerable<Train> trains = DB.GetTrainInfoDB();
            if (trains == null)
            {
                EndFunctionError();
            }
            Console.WriteLine("Train Number\tLead Power\tDCC Address\t\tCurrent Module\t\tLast Updated");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            foreach (var train in trains)
            {
                Console.Write(train.TrainNumber);
                Console.Write("\t\t" + train.LeadPower);
                Console.Write("\t\t" + train.DCCAddress);
                Console.Write("\t\t\t" + train.onModule);
                Console.WriteLine("\t" + train.TimeUpdated);
            }
            Console.Write("\n\nPlease select an Train Number:  ");

            int id = getInputAndReturnNumber();
            bool goodValue = false;
            while (!goodValue)
            {
                foreach (var train in trains)
                {
                    if (id == train.TrainNumber)
                    {
                        goodValue = true;
                        break;
                    }
                }
                if (goodValue)
                    continue;
                Console.WriteLine("That value {0} was not in the given set, please try again: ", id);
                id = getInputAndReturnNumber();
            }

            Console.WriteLine("Where would you like to update it to? ");
            IEnumerable<Module> modules = DB.GetModuleInfoDB();
            Console.WriteLine("Select Number that is next to the desired name");
            Console.WriteLine("Avaliable Modules");
            Console.WriteLine("----------------------");
            // Assumption: Only wanting to print the avaliable modules
            var count = 0;
            foreach (var module in modules)
            {
                if (!module.IsAvaliable)
                    continue;
                Console.Write(++count + ") ");
                Console.WriteLine(module.Name);
            }

            Console.WriteLine("---------------------------------------");

            int desiredModuleIndex = getInputAndReturnNumber() - 1;
            goodValue = false;
            while (!goodValue)
            {
                foreach (var module in modules)
                {
                    if (module.Name.Equals(modules.ToList()[desiredModuleIndex].Name))
                    {
                        goodValue = true;
                        break;
                    }
                }
                if (goodValue)
                    continue;
                Console.WriteLine("That value {0} was not in the given set, please try again: ", desiredModuleIndex);
                desiredModuleIndex = getInputAndReturnNumber();
            }

            DB.UpdateTrainLocation(modules.ToList()[desiredModuleIndex].Name, id);

            Console.WriteLine("Train #{0} has been updated to module {1}.", id, modules.ToList()[desiredModuleIndex].Name);
            
            EndFunction();
        }

        private void EndFunction()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void EndFunctionError()
        {
            Console.WriteLine("There was an error retrieving the results");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

    }
}
