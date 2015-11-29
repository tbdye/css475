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
                        UpdateShipmentInformation();
                        break;
                    case 2:
                        ReportTrainArrival();
                        break;
                    case 3:
                        ReportTrainInformation();
                        break;
                    case 4:
                        RemoveMainLine();
                        break;
                    default:
                        Console.WriteLine("Sorry '{0}' was not a good input", numInput);
                        break;
                }

                Console.Clear();
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

        private void PrintOptions()
        {
            Console.WriteLine("\nChoose an entry");
            Console.WriteLine("_________________________________________________________________________________");
            Console.WriteLine("\tOption Number\tOption Name");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("\t1\t\tUpdate Shipment Information");
            Console.WriteLine("\t2\t\tReport Train Arival At Module");
            Console.WriteLine("\t3\t\tShow Train Report");
            Console.WriteLine("\t4\t\tDelete Main Line");
            Console.WriteLine("_________________________________________________________________________________");
            Console.Write("\n\t");
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
            
            int id = 0;
            IEnumerable<Train> trains = GetPrintValidateAllTrainInformation(out id);
            

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
            bool goodValue = false;
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

        private void RemoveMainLine()
        {
            Console.Clear();
            Console.WriteLine("Please Select from the following list to remove:");
            Console.WriteLine("---------------------------------------------------");
            List<MainLine> values = DB.GetMainLinesDB().ToList();
            Console.WriteLine("Name\t\tModule\tContiguous");
            Console.WriteLine("---------------------------------------------------");
            var count = 0;
            foreach (var value in values)
            {
                Console.Write("{0}) {1}", ++count, value.Name);
                Console.Write(" {0}", value.Module);
                Console.WriteLine(" {0}", value.IsContiguous ? "Yes" : "No");
            }
            int indexInput = getInputAndReturnNumber() - 1;
            Console.WriteLine("You selected the name {0} and the module {1}", values[indexInput].Name, values[indexInput].Module);
            var result = DB.TryToRemoveMainLineDB(values[indexInput]);
            if (result)
            {
                Console.WriteLine("Main Line successfully removed.");
            }
            else
            {
                Console.WriteLine("Main Line unable to remove, currently in use.");
            }
            EndFunction();
        }

        private void ReportTrainInformation()
        {
            Console.Clear();

            int id = 0;
            bool error = false;
            IEnumerable<Train> trains = GetPrintValidateAllTrainInformation(out id);

            if (trains == null)
            {
                Console.WriteLine("There was an error while retrieving your information..");
                EndFunctionError();
                return;
            }

            DisplayUserTrain result = DB.DisplayUserTrainDB(id);

            if (result == null)
            {
                Console.WriteLine("There was an error while retrieving your information..");
                EndFunctionError();
                return;
            }

            PrintsDisplayUserTrainResult(result);

            EndFunction();
            

        }

        private IEnumerable<Train> GetPrintValidateAllTrainInformation(out int input)
        {
            input = -1;
            Console.WriteLine("Which Train would you like to view?");
            IEnumerable<Train> trains = DB.GetTrainInfoDB();
            if (trains == null)
            {
                return null;
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
            input = getInputAndReturnNumber();
            bool goodValue = false;
            while (!goodValue)
            {
                foreach (var train in trains)
                {
                    if (input == train.TrainNumber)
                    {
                        goodValue = true;
                        break;
                    }
                }
                if (goodValue)
                    continue;
                Console.WriteLine("That value {0} was not in the given set, please try again: ", input);
                input = getInputAndReturnNumber();
            }
            return trains;
        }

        

        private void PrintsDisplayUserTrainResult(DisplayUserTrain result)
        {
            Console.WriteLine("Crew\t\tDCC Number");
            Console.WriteLine("------------------------------------------------");
            Console.Write(result.Crew);
            Console.WriteLine("\t\t" + result.DCCAddress);
            Console.WriteLine("\tCar\t\tProduct\t\tNext Industry");
            Console.WriteLine("------------------------------------------------");
            for(int i = 0; i < result.UsingCar.Count(); i++) // This will handle all of the looping
            {
                Console.Write("\t" + result.UsingCar[i]);
                Console.Write("\t\t" + result.ProductType[i]); // This is a non-nullable value. 
                Console.WriteLine("\t\t" + result.ToIndustry[i]);
            }
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
