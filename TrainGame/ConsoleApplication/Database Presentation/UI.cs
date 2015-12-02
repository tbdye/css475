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
        #region Set up methods
        private DBHandler DB;
        private Player curPlayer;
        public UI()
        {
            DB = new DBHandler();
            curPlayer = new Player();
            //SetUpPlayer(); // Hard Coding this, just cause its annoying...
            curPlayer.Name = "Brett"; // Remove these
            curPlayer.Train = DB.GetTrain(1); // Remove these
        }

        #endregion

        #region Start
        public void Start()
        {
            Console.Clear();
            PrintOptions();
            String Input = Console.ReadLine();
            Int16 numInput = -1;
            while (Int16.TryParse(Input, out numInput))
            {
                switch (numInput)
                {
                    case 1:
                        ReportTrainInformationAndCrews();
                        break;
                    case 2:
                        SelectCrewAndSelectTrainFromCrew();
                        break;
                    case 3:
                        DisplayAllTrainInformation();
                        break;
                    case 4:
                        DisplayRollingStockFromASelectedTrain();
                        break;
                    case 5:
                        DisplayIndustriesAndYardsOnTheCurrentModule();
                        break;
                    case 6:
                        Console.WriteLine("Sorry, this is not currently implemented");
                        break;
                    case 7:
                        Console.WriteLine("Sorry, this is not currently implemented");
                        break;
                    case 8:
                        Console.WriteLine("Sorry, this is not currently implemented");
                        break;
                    case 9:
                        Console.WriteLine("Sorry, this is not currently implemented");
                        break;
                    case 10:
                        Console.WriteLine("Sorry, this is not currently implemented");
                        break;
                    case 11:
                        Console.WriteLine("Sorry, this is not currently implemented");
                        break;
                    default:
                        Console.WriteLine("Sorry '{0}' was not a good input", numInput);
                        break;
                }

                Console.Clear();
                PrintOptions();
                Input = Console.ReadLine();
                if (Input.ToUpper().Equals("EXIT") || Input.ToUpper().Equals("QUIT")
                    || Input.ToUpper().Equals("E") || Input.ToUpper().Equals("Q"))
                {
                    return;
                }
            }
        }
        #endregion

        #region Option 1
        private void ReportTrainInformationAndCrews()
        {
            Console.Clear();
            Console.WriteLine("Trains");
            Console.WriteLine("--------------------------------------------");
            GetPrintAllTrainInformation(false);
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Crew");
            Console.WriteLine("--------------------------------------------");
            GetPrintAllCrewInformation();
            Console.WriteLine();
            EndFunction();
        }
        #endregion

        #region Option 2
        /// <summary>
        /// Presents all the crew to the user as well as all of the trains, 
        /// and allows the user to choose which one they want.
        /// Note: Currently the return from this function is being disposed of,
        /// but with the Tuple implementation, it would not be hard to correctly consume. 
        /// </summary>
        /// <returns>A tuple containing the selected crew and selected train</returns>
        private Tuple<Crew, Train> SelectCrewAndSelectTrainFromCrew()
        {
            Console.Clear();
            List<Crew> crew = GetPrintAllCrewInformation().ToList();
            if (crew == null)
            {
                EndFunctionError();
                return null;
            }

            Console.Write("Please select the Crew Number you want to retrieve:  ");

            var crewIndex = getInputAndReturnNumber(crew.Count());

            List<Train> trains = GetPrintAllTrainInformation(false).ToList();
            if (trains == null)
            {
                EndFunctionError();
                return new Tuple<Crew, Train>(crew[crewIndex], null);
            }

            Console.Write("Please select the Train Number you want to retrieve:  ");
            var trainIndex = getInputAndReturnNumber(trains.Count());

            var toReturn = new Tuple<Crew, Train>(crew[crewIndex], trains[trainIndex]);

            Console.WriteLine("Crew Person: {0} and Train #{1}, retrieved", toReturn.Item1.CrewName, toReturn.Item2.TrainNumber);

            EndFunction();

            return toReturn;
        }
        #endregion

        #region Option 3
        private void DisplayAllTrainInformation()
        {
            Console.Clear();
            Console.WriteLine("Displaying All Train Information...");
            GetPrintAllTrainInformation(true);
            EndFunction();
        }
        #endregion
            
        #region Option 4
        private void DisplayRollingStockFromASelectedTrain()
        {
            Console.Clear();
            Console.WriteLine("Displaing All train information...");
            List<Train> trains = GetPrintAllTrainInformation(false).ToList();
            Console.Write("Please select the train that you want to show rolling stock for: ");
            var trainIndex = getInputAndReturnNumber(trains.Count());
            DisplayRollingStockForSpecifiedTrain(trains[trainIndex].TrainNumber);
            EndFunction();
        }
        #endregion

        #region Option 5
        private void DisplayIndustriesAndYardsOnTheCurrentModule()
        {
            Console.Clear();
            Console.WriteLine("Displaying Industries and Yards of the module that your Train #{0} is currently at", curPlayer.Train.TrainNumber);
            GetPrintAllIndustryInformationForCurrentTrain().ToList();
            Console.WriteLine("Yards");
            GetPrintAllYardInformationForCurrentTrainLocation().ToList();
            EndFunction();
        }
        #endregion

        #region Option 6

        #endregion

        #region Option 7

        #endregion

        #region Option 8

        #endregion

        #region Option 9

        #endregion

        #region Option 10

        #endregion

        #region Opetion 11

        #endregion

        #region HelperMethods
        // Helper Functions 
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
        private IEnumerable<Crew> GetPrintAllCrewInformation()
        {
            IEnumerable<Crew> crews = DB.GetCrewInfoDB();
            if (crews == null)
            {
                return null;
            }
            Console.WriteLine("\tName\tDescription");
            Console.WriteLine("--------------------------------------------");
            var count = 0;
            foreach (var crew in crews)
            {
                Console.Write("{0})\t{1}", ++count, crew.CrewName);
                Console.WriteLine("\t\t" + crew.Description);
            }

            return crews;
        }
        
        /// <summary>
        /// This will check a user input against a list, 
        /// and makes sure that it is a number
        /// </summary>
        /// <typeparam name="T">The type of list that you are passing</typeparam>
        /// <param name="obj"></param>
        /// <returns>Index of the list passed in</returns>
        private int getInputAndReturnNumber(int count)
        {
            int toReturn = 0;
            while (!Int32.TryParse(Console.ReadLine(), out toReturn) || (toReturn >= count + 1 || toReturn <= 0))
            {
                Console.WriteLine("This input {0} is not a valid input, \nPlease try again", toReturn);
                if(toReturn >= count + 1 || toReturn <= 0)
                {
                    Console.WriteLine("Please select one of the numbers on the left of the list");
                }
            }
            return toReturn - 1;
        }

        /// <summary>
        /// Prints all of the train information
        /// if verbose is true, all information related to trains will be printed. 
        /// </summary>
        /// <param name="verbose">Flag for more information</param>
        /// <returns></returns>
        private IEnumerable<Train> GetPrintAllTrainInformation(bool verbose)
        {
            IEnumerable<Train> trains = DB.GetAllTrainInfoDB();
            if (trains == null)
            {
                return null;
            }
            if (verbose)
            {
                Console.WriteLine("\tTrain Number\tLead Power\tDCC Address\tModule\t\t\tTimeUpdated");
                Console.WriteLine("-------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("\tTrain Number\tLead Power\tDCC Address");
                Console.WriteLine("--------------------------------------------");
            }
            var count = 0;
            if (verbose)
            {
                foreach (var train in trains)
                {
                    Console.Write("{0})\t{1}", ++count, train.TrainNumber);
                    Console.Write("\t\t{0}", train.LeadPower);
                    Console.Write("\t\t{0}", train.DCCAddress);
                    Console.Write("\t\t{0}", train.Module);
                    Console.WriteLine("\t{0}", train.TimeModuleUpdated);
                }
            }
            else
            {
                foreach (var train in trains)
                {
                    Console.Write("{0})\t{1}", ++count, train.TrainNumber);
                    Console.Write("\t\t" + train.LeadPower);
                    Console.WriteLine("\t\t" + train.DCCAddress);
                }
            }
            return trains;
        }

        /// <summary>
        /// Displays all of the rolling stock information for a passed in train number
        /// </summary>
        /// <param name="trainNumber"></param>
        private void DisplayRollingStockForSpecifiedTrain(int trainNumber)
        {
            Console.WriteLine("Displaying Rolling stock information for Train Number {0}", trainNumber);
            List<AllRollingStock> values = DB.GetAllRollingStockForTrainDB(trainNumber).ToList();
            Console.WriteLine("Car ID\t\tYard Name\t\tCar Type\t\tDescription\t\tCar Length");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            foreach (var value in values)
            {
                Console.Write("{0}", value.CarID);
                Console.Write("\t\t{0}", value.YardName);
                Console.Write("\t{0}", value.CarType);
                Console.Write("\t\t{0}", value.Description.Equals("") ? "NONE" : value.Description);
                Console.WriteLine("\t\t{0}", value.CarLength);
            }
        }

        private IEnumerable<Industry> GetPrintAllIndustryInformationForCurrentTrain()
        {
            Console.WriteLine("Retrieving all Industry Information for Train #{0}...", curPlayer.Train.TrainNumber);
            List<Industry> industries = DB.GetAllIndustriesOnModuleDB(curPlayer.Train.Module).ToList();
            var count = 0;
            foreach (var value in industries)
            {
                Console.WriteLine("\tName\t\t\tModule\t\t\tMain Line\tAvalible\tActivity Level");
                Console.WriteLine("\t----------------------------------------------------------------------------------------------");
                Console.Write("{0})\t{1}", ++count, value.IndustryName);
                Console.Write("\t{0}", value.OnModule);
                Console.Write("\t{0}", value.OnMainLine);
                Console.Write("\t\t{0}", value.isAvaliable);
                Console.WriteLine("\t\t{0}", value.ActivityLevel);
                if (value.Sidings.Count() != 0)
                {
                    Console.WriteLine("\n\t\tSiding\n");
                    Console.WriteLine("\t\tNumber\t\tSiding Length\t\tAvaliable Length");
                    Console.WriteLine("\t\t--------------------------------------------------------");
                    foreach (var i in value.Sidings)
                    {
                        Console.Write("\t\t{0}", i.SidingNumber);
                        Console.Write("\t\t{0}", i.SidingLength);
                        Console.WriteLine("\t\t\t{0}", i.AvaliableSidingLength);
                    }
                }
                if (value.UsingProductTypes.Count() != 0)
                {
                    Console.WriteLine("\n\t\tProducts\n");
                    Console.WriteLine("\t\tName\t\tRolling Stock Type\t\tProducer");
                    Console.WriteLine("\t\t--------------------------------------------------------");
                    foreach (var i in value.UsingProductTypes)
                    {
                        Console.Write("\t\t{0}", i.ProductTypeName);
                        Console.Write("\t\t{0}", i.RollingStockType);
                        Console.WriteLine("\t\t{0}", i.isProducer);
                    }
                }
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------------\n");
            }

            return industries;
        }

        private IEnumerable<Yard> GetPrintAllYardInformationForCurrentTrainLocation() 
        {
            Console.WriteLine("Retrieving all Yard Information for Train #{0}...", curPlayer.Train.TrainNumber);
            List<Yard> yards = DB.GetAllYardsOnModuleDB(curPlayer.Train.Module).ToList();
            if (yards == null)
            {
                return null;
            }
            var count = 0;
            Console.WriteLine("\tName\t\t\tMain Line");
            Console.WriteLine("\t---------------------------------");
            foreach (var value in yards)
            {
                Console.Write("{0})\t{1}", ++count, value.Name);
                Console.WriteLine("\t{0}", value.MainLine);
            }
            Console.WriteLine();

            return yards;
        }

        /// <summary>
        /// Prints all of the options for the user
        /// </summary>
        private void PrintOptions()
        {
            Console.WriteLine("\nTrain Game Main Menu");
            Console.WriteLine("_________________________________________________________________________________");
            Console.WriteLine("\t\tNumber\t\tName");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("Information");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("\t\t1\t\tDisplay all trains and all crew");
            Console.WriteLine("\t\t2\t\tSelect crew, then select a train");
            Console.WriteLine("\t\t3\t\tDisplay train information");
            Console.WriteLine("\t\t4\t\tDisplay rolling stock in a train");
            Console.WriteLine("\t\t5\t\tDisplay industries and yards the current module\n");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("Game play example");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("\t\t6\t\tDisplay rolling stock cars at an industry");
            Console.WriteLine("\t\t7\t\tDrop off rolling stock at an industry on a selected module");
            Console.WriteLine("\t\t8\t\tPick up rolling stock from an industry on a selected module");
            Console.WriteLine("\t\t9\t\tInsert cars to a Train");
            Console.WriteLine("\t\t10\t\tRemove Cars from a train");
            Console.WriteLine("\t\t11\t\tMove train from one module to another module");
            Console.WriteLine("_________________________________________________________________________________");
            Console.Write("\nPlease enter the number next to your selection (or q to exit):  ");
        }

        private void SetUpPlayer()
        {
            Console.WriteLine("Whats your name?");
            curPlayer.Name = Console.ReadLine();
            Console.WriteLine("Now, lets choose your train...");
            Console.WriteLine("Avaliable Train");
            List<Train> trains = GetPrintAllTrainInformation(true).ToList();
            Console.Write("Please select the number related to your train:  ");
            var trainIndex = getInputAndReturnNumber(trains.Count());
            curPlayer.Train = trains[trainIndex];
            curPlayer.Train.Module = curPlayer.Train.Module;
            Console.WriteLine("Hello {0}, thank you for playing the game!\nYour Train Number is {1}... \nGood Luck!",
                    curPlayer.Name, curPlayer.Train.TrainNumber);
            EndFunction();
        }
        #endregion
    }
}
