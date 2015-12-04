using Database_Presentation.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Database_Presentation
{
    public class UI
    {
        #region Set up methods

        private bool throwAway;

        private DBHandler DB;
        private Player curPlayer;

        public UI()
        {
            throwAway = false;
            DB = new DBHandler();
            curPlayer = new Player();
            DrawTrain();
            //SetUpPlayer(); // Hard Coding this, just cause its annoying...
            curPlayer.Name = "Brett"; // Remove these
            curPlayer.Train = DB.GetTrain(1); // Remove these
        }

        #endregion Set up methods

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
                        DisplayAllTrainsAtACertainIndustry();
                        break;

                    case 7:
                        DropOffRollingStockAtSpecifiedIndustryOnCurrentModule();
                        break;

                    case 8:
                        PickUpRollingStockAtAnIndustryOnTheCurrentModule();
                        break;

                    case 9:
                        InsertACarIntoYourTrain();
                        break;

                    case 10:
                        RemoveACarFromyourTrain();
                        break;

                    case 11:
                        MoveYourTrainFromOneModuleToAnother();
                        break;

                    default:
                        Console.WriteLine("Sorry '{0}' was not a good input", numInput);
                        EndFunction();
                        break;
                }

                Console.Clear();
                PrintOptions();
            }
        }

        #endregion Start

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

        #endregion Option 1

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
            Console.WriteLine("Displaying all crew information...\n\n");
            List<Crew> crew = GetPrintAllCrewInformation().ToList();
            if (crew == null)
            {
                EndFunctionError();
                return null;
            }

            Console.Write("Please select the Crew Number you want to retrieve ");

            var crewIndex = getInputAndReturnNumber(crew.Count());

            List<Train> trains = GetPrintAllTrainInformation(false).ToList();
            if (trains == null)
            {
                EndFunctionError();
                return new Tuple<Crew, Train>(crew[crewIndex], null);
            }

            Console.Write("Please select the number next to the train you want to retrieve ");
            var trainIndex = getInputAndReturnNumber(trains.Count());

            var toReturn = new Tuple<Crew, Train>(crew[crewIndex], trains[trainIndex]);

            Console.WriteLine("Crew Person: {0} and Train #{1}, retrieved", toReturn.Item1.CrewName, toReturn.Item2.TrainNumber);

            EndFunction();

            return toReturn;
        }

        #endregion Option 2

        #region Option 3

        private void DisplayAllTrainInformation()
        {
            Console.Clear();
            Console.WriteLine("Displaying All Train Information...\n");
            GetPrintAllTrainInformation(true);
            EndFunction();
        }

        #endregion Option 3

        #region Option 4

        private void DisplayRollingStockFromASelectedTrain()
        {
            Console.Clear();
            Console.WriteLine("Displaing All train information...\n");
            List<Train> trains = GetPrintAllTrainInformation(false).ToList();
            if (trains == null)
            {
                Console.WriteLine("There was an error retrieving the Train Infomration");
                EndFunctionError();
                return;
            }
            Console.Write("Please select the train that you want to show rolling stock for ");
            var trainIndex = getInputAndReturnNumber(trains.Count());
            DisplayRollingStockForSpecifiedTrain(trains[trainIndex].TrainNumber);
            EndFunction();
        }

        #endregion Option 4

        #region Option 5

        private void DisplayIndustriesAndYardsOnTheCurrentModule()
        {
            Console.Clear();
            Console.WriteLine("Displaying Industries and Yards of the module that your Train #{0} is currently at\n", curPlayer.Train.TrainNumber);
            GetPrintAllIndustryInformationForCurrentTrain();
            Console.WriteLine("Yards");
            GetPrintAllYardInformationForCurrentTrainLocation();
            EndFunction();
        }

        #endregion Option 5

        #region Option 6

        private void DisplayAllTrainsAtACertainIndustry()
        {
            Console.Clear();
            Console.WriteLine("Getting all Industries...\n");
            List<Industry> industryValues = GetPrintAllIndustryInformation().ToList();
            if (industryValues == null)
            {
                Console.WriteLine("There was an error gathering the current industry information...");
                EndFunctionError();
                return;
            }
            Console.WriteLine("Please select the number coresponding to the industry you want ");
            var industryIndex = getInputAndReturnNumber(industryValues.Count());
            Console.WriteLine("Getting all rolling stock at industry {0}\n", industryValues[industryIndex].IndustryName);
            List<RollingStock> stockValues = GetPrintRollingStockForIndustry(industryValues[industryIndex].IndustryName).ToList();
            if (stockValues == null)
            {
                Console.WriteLine("There was an error gathering the current rolling stock information...");
                EndFunctionError();
                return;
            }
            EndFunction();
        }

        #endregion Option 6

        #region Option 7

        private void DropOffRollingStockAtSpecifiedIndustryOnCurrentModule()
        {
            Console.Clear();
            Console.WriteLine("DEV WARNING!!!\nTHIS FUNCTION WILL REMOVE TEST DATA\nBE SURE TO PUT IT BACK\n....Or just like, quit this shit.....\n\n\n\n\n.......BRETT\n\n");
            Console.WriteLine("Displaying all of the Rolling Stock attached to your train...\n");
            List<RollingStock> rollingStockValues = GetPrintRollingStockForTrain(curPlayer.Train.TrainNumber).ToList();
            Console.Write("\nPlease select the number next to the rolling stock you want to drop off ");
            var rollingStockIndex = getInputAndReturnNumber(rollingStockValues.Count());
            Console.WriteLine("Displaying all of the industries attached to your current module...\n");
            List<Industry> industryValues = GetPrintAllIndustriesOnModule(curPlayer.Train.Module, false).ToList();
            Console.Write("\nPlease select the number next to the industry you want to drop off at ");
            var industryIndex = getInputAndReturnNumber(industryValues.Count());
            var success = DB.DropOffCarAtLocationDB(curPlayer.Train.TrainNumber, rollingStockValues[rollingStockIndex].CarID, industryValues[industryIndex].IndustryName);
            if (success)
            {
                Console.WriteLine("The car {0} was successfully dropped off at the industry {1} and removed from your train", rollingStockValues[rollingStockIndex].CarID, industryValues[industryIndex].IndustryName);
            }
            else
            {
                Console.WriteLine("There was an issue dropping the car {0} at the industry {1}. No cars were removed from your train.", rollingStockValues[rollingStockIndex].CarID, industryValues[industryIndex].IndustryName);
                EndFunctionError();
                return;
            }

            EndFunction();
        }

        #endregion Option 7

        #region Option 8

        private void PickUpRollingStockAtAnIndustryOnTheCurrentModule()
        {
            Console.Clear();
            Console.WriteLine("DEV WARNING!!!\nTHIS FUNCTION WILL REMOVE TEST DATA\nBE SURE TO PUT IT BACK\n....Or just like, quit this shit.....\n\n\n\n\n.......BRETT\n\n");
            Console.WriteLine("Displaying avaliable rolling stock to pickup at current module...\n");
            IEnumerable<TupleNode> allValues = new List<TupleNode>();
            bool hasRollingStock = false;
            List<Industry> industriesOnModule = GetPrintAllIndustriesOnModule(out allValues, out hasRollingStock).ToList();
            if (!hasRollingStock)
            {
                Console.WriteLine("The module that you are currently at has no rolling to pick up. \nPlease move to another module to add to your train.");
                EndFunction();
                return;
            }
            if (industriesOnModule == null)
            {
                EndFunctionError();
                return;
            }
            Console.Write("Please select the nuber next to the industry that holds the the rolling stock you want ");
            var industryIndex = getInputAndReturnNumber(industriesOnModule.Count());
            while (allValues.ToList()[industryIndex].rollingStock == null)
            {
                Console.Write("\nThe industry you chose: {0}. Has no rolling stock to pick up. \nPlease Choose Another ");
                industryIndex = getInputAndReturnNumber(industriesOnModule.Count());
            }
            Console.WriteLine("Re-Displaying all rolling stock from the selected industry {0}", industriesOnModule[industryIndex].IndustryName);
            List<RollingStock> rsValues = GetPrintRollingStockAtindustry(industriesOnModule[industryIndex].IndustryName).ToList();
            if (rsValues == null && rsValues.Count() == 0)
            {
                Console.WriteLine("There was an issue retrieving the rolling stock values from the industry {0}", industriesOnModule[industryIndex].IndustryName);
                EndFunctionError();
                return;
            }
            Console.Write("Now, please select the rolling stock that you want to pick up from that industry ");
            var rsIndex = getInputAndReturnNumber(rsValues.Count());
            var success = DB.AddCarToTrainDB(curPlayer.Train.TrainNumber, rsValues[rsIndex].CarID, industriesOnModule[industryIndex].IndustryName);
            if (!success)
            {
                Console.WriteLine("There was an issue picking up the car {0} at the industry {1}. No cars were added from your train or removed from the industry", rsValues[rsIndex].CarID, industriesOnModule[industryIndex].IndustryName);
                EndFunctionError();
                return;
            }
            Console.WriteLine("You successfully removed the Car: {0} from the Industry {1} and added it to your train", rsValues[rsIndex].CarID, industriesOnModule[industryIndex].IndustryName);
            EndFunction();
        }

        #endregion Option 8

        #region Option 9

        private void InsertACarIntoYourTrain()
        {
            Console.Clear();
            Console.WriteLine("DEV WARNING!!!\nTHIS FUNCTION WILL REMOVE TEST DATA\nBE SURE TO PUT IT BACK\n....Or just like, quit this shit.....\n\n\n\n\n.......BRETT\n\n");
            Console.WriteLine("Plus its not even written yet, dummy");

            EndFunction();
        }

        #endregion Option 9

        #region Option 10

        private void RemoveACarFromyourTrain()
        {
            Console.Clear();
            Console.WriteLine("DEV WARNING!!!\nTHIS FUNCTION WILL REMOVE TEST DATA\nBE SURE TO PUT IT BACK\n....Or just like, quit this shit.....\n\n\n\n\n.......BRETT\n\n");
            Console.WriteLine("Plus its not even written yet, dummy");
            EndFunction();
        }

        #endregion Option 10

        #region Option 11

        private void MoveYourTrainFromOneModuleToAnother()
        {
            Console.Clear();
            Console.WriteLine("DEV WARNING!!!\nTHIS FUNCTION WILL REMOVE TEST DATA\nBE SURE TO PUT IT BACK\n....Or just like, quit this shit.....\n\n\n\n\n.......BRETT\n\n");
            Console.WriteLine("Plus its not even written yet, dummy");
            EndFunction();
        }

        #endregion Option 11

        #region HelperMethods

        // Helper Functions
        private IEnumerable<Industry> GetPrintAllIndustriesOnModule(out IEnumerable<TupleNode> setMe, out bool industriesHaveRollingStock)
        {
            setMe = null;
            industriesHaveRollingStock = false;
            IEnumerable<Industry> industriesOnModule = DB.GetAllIndustriesOnModuleDB(curPlayer.Train.Module);
            if (industriesOnModule == null)
            {
                Console.WriteLine("There was an error retrieving the rolling stock avaliable to your train..");
                return null;
            }

            setMe = PrintIndustriesWithRollingStockInformation(industriesOnModule, out industriesHaveRollingStock);

            return industriesOnModule;
        }

        private IEnumerable<Crew> GetPrintAllCrewInformation()
        {
            IEnumerable<Crew> crews = DB.GetCrewInfoDB();
            if (crews == null)
            {
                return null;
            }
            PrintCrews(crews);
            return crews;
        }

        private IEnumerable<Industry> GetPrintAllIndustryInformation()
        {
            IEnumerable<Industry> industryValues = DB.GetAllIndustriesDB();
            if (industryValues == null)
                return null;
            PrintIndustries(industryValues, false);
            return industryValues;
        }

        private IEnumerable<RollingStock> GetPrintRollingStockForIndustry(string IndustryName)
        {
            IEnumerable<RollingStock> values = DB.GetRollingStockValuesByIndustryNameDB(IndustryName);
            if (values == null)
            {
                return null;
            }
            PrintRollingStock(values);
            return values;
        }

        private IEnumerable<RollingStock> GetPrintRollingStockForModule(string ModuleName)
        {
            IEnumerable<RollingStock> values = DB.GetAllRollingStockForModuleDB(ModuleName);
            if (values == null)
            {
                return null;
            }
            PrintRollingStock(values);
            return values;
        }

        private IEnumerable<RollingStock> GetPrintRollingStockForTrain(int trainNumber)
        {
            IEnumerable<RollingStock> values = DB.GetAllRollingStockForTrainDB(trainNumber);
            if (values == null)
            {
                return null;
            }
            PrintRollingStock(values);
            return values;
        }

        private IEnumerable<RollingStock> GetPrintRollingStockAtindustry(string IndustryName)
        {
            IEnumerable<RollingStock> values = DB.GetRollingStockValuesByIndustryNameDB(IndustryName);
            if (values == null)
            {
                return null;
            }
            PrintRollingStock(values);
            return values;
        }

        private IEnumerable<Industry> GetPrintAllIndustriesOnModule(string moduleName, bool rollingStock)
        {
            IEnumerable<Industry> industryValues = DB.GetAllIndustriesOnModuleDB(moduleName);
            if (industryValues == null)
                return null;
            if (rollingStock)
            {
                PrintIndustriesWithRollingStockInformation(industryValues, out throwAway);
            }
            else
            {
                PrintIndustries(industryValues, false);
            }
            return industryValues;
        }

        private IEnumerable<TupleNode> PrintIndustriesWithRollingStockInformation(IEnumerable<Industry> values, out bool thereIsRollingStock)
        {
            thereIsRollingStock = false;
            List<TupleNode> toReturn = new List<TupleNode>();
            var industryCount = 0;
            Console.WriteLine("\t{0}", "Industry Name");
            Console.WriteLine("\t-------------------------------------------------------------\n");
            foreach (var value in values)
            {
                var rsValues = DB.GetRollingStockValuesByIndustryNameDB(value.IndustryName);
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
                if (rsValues != null && rsValues.Count() > 0)
                {
                    thereIsRollingStock = true;
                    toReturn.Add(new TupleNode(value, rsValues));
                    Console.WriteLine("{0})\t{1}\n", ++industryCount, value.IndustryName);
                    Console.WriteLine("\t{0,-16}{1,-16}{2,-16}\t{3}", "CarID", "Car Length", "Car Type", "Description");
                    Console.WriteLine("\t---------------------------------------------------------------------------\n");
                    foreach (var rsValue in rsValues)
                    {
                        Console.Write("\t{0}", rsValue.CarID);
                        Console.Write("\t\t{0, -15}", rsValue.CarLength);
                        Console.Write("\t{0, -15}", rsValue.CarType);
                        Console.WriteLine("\t\t{0, -15}\n", rsValue.Description.Equals("") ? "None" : rsValue.Description);
                    }
                }
                else
                {
                    toReturn.Add(new TupleNode(value, null));
                    Console.WriteLine("\t{0}\t-\tUNAVALIABLE\n", value.IndustryName);
                    Console.WriteLine("\t\tThis industry has no cars currently\n");
                }
            }
            return toReturn;
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
            PrintTrains(trains, verbose);
            return trains;
        }

        /// <summary>
        /// Displays all of the rolling stock information for a passed in train number
        /// </summary>
        /// <param name="trainNumber"></param>
        private IEnumerable<RollingStock> DisplayRollingStockForSpecifiedTrain(int trainNumber)
        {
            Console.WriteLine("Displaying Rolling stock information for Train Number {0}", trainNumber);
            List<RollingStock> values = DB.GetAllRollingStockForTrainDB(trainNumber).ToList();
            if (values == null)
            {
                return null;
            }
            PrintRollingStock(values);
            return values;
        }

        private IEnumerable<Industry> GetPrintAllIndustryInformationForCurrentTrain()
        {
            Console.WriteLine("Retrieving all Industry Information for Train #{0}...", curPlayer.Train.TrainNumber);
            List<Industry> industries = DB.GetAllIndustriesOnModuleDB(curPlayer.Train.Module).ToList();
            if (industries == null)
            {
                return null;
            }
            PrintIndustries(industries, true);
            return industries;
        }

        private void PrintIndustries(IEnumerable<Industry> industries, bool verbose)
        {
            var count = 0;
            if (verbose)
            {
                foreach (var value in industries)
                {
                    Console.WriteLine("\tName\t\t\tModule\t\t\tMain Line\tAvalible\tActivity Level");
                    Console.WriteLine("\t----------------------------------------------------------------------------------------------");
                    Console.Write("{0})\t{1}", ++count, value.IndustryName);
                    Console.Write("\t{0,-15}", value.OnModule);
                    Console.Write("\t{0,-15}", value.OnMainLine);
                    Console.Write("\t{0,-15}", value.isAvaliable);
                    Console.WriteLine("\t{0,-15}", value.ActivityLevel);
                    if (value.Sidings.Count() != 0)
                    {
                        Console.WriteLine("\n\t\tSiding\n");
                        Console.WriteLine("\t\t{0, -15}\t\t{1, -15}\t\t{2, -15}", "Number", "Siding Length", "Avaliable Length");
                        Console.WriteLine("\t\t--------------------------------------------------------");
                        foreach (var i in value.Sidings)
                        {
                            Console.Write("\t\t{0, -15}", i.SidingNumber);
                            Console.Write("\t\t{0, -15}", i.SidingLength);
                            Console.WriteLine("\t\t{0, -15}", i.AvaliableSidingLength);
                        }
                    }
                    if (value.UsingProductTypes.Count() != 0)
                    {
                        Console.WriteLine("\n\t\tProducts\n");
                        Console.WriteLine("\t\t{0, -23}{1, -23}{2, -23}", "Name", "Rolling Stock Type", "Producer");
                        Console.WriteLine("\t\t--------------------------------------------------------");
                        foreach (var i in value.UsingProductTypes)
                        {
                            Console.Write("\t\t{0, -23}", i.ProductTypeName);
                            Console.Write("{0, -23}", i.RollingStockType);
                            Console.WriteLine("{0, -23}", i.isProducer);
                        }
                    }
                    Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------------\n");
                }
            }
            else
            {
                Console.WriteLine("\tIndustry Name");
                Console.WriteLine("-----------------------");
                foreach (var value in industries)
                {
                    Console.WriteLine("{0})\t{1}", ++count, value.IndustryName);
                }
            }
        }

        private void PrintRollingStock(IEnumerable<RollingStock> values)
        {
            var count = 0;
            Console.WriteLine("\tCar ID\t\tCar Type\t\tDescription\t\tCar Length");
            Console.WriteLine("----------------------------------------------------------------------------------");
            foreach (var value in values)
            {
                Console.Write("{0})\t{1, -15}", ++count, value.CarID);
                Console.Write("\t{0,-15}", value.CarType);
                Console.Write("\t\t{0,-15}", value.Description.Equals("") ? "NONE" : value.Description);
                Console.WriteLine("\t\t{0,-15}", value.CarLength);
            }
        }

        private void PrintCrews(IEnumerable<Crew> values)
        {
            Console.WriteLine("\tName\t\tDescription");
            Console.WriteLine("--------------------------------------------");
            var count = 0;
            foreach (var crew in values)
            {
                Console.Write("{0})\t{1,-15}", ++count, crew.CrewName);
                Console.WriteLine("\t\t" + crew.Description);
            }
        }

        private void PrintYards(IEnumerable<Yard> values)
        {
            var count = 0;
            Console.WriteLine("\tName\t\t\tMain Line");
            Console.WriteLine("\t---------------------------------");
            foreach (var value in values)
            {
                Console.Write("{0})\t{1}", ++count, value.Name);
                Console.WriteLine("\t{0}", value.MainLine);
            }
            Console.WriteLine();
        }

        private void PrintTrains(IEnumerable<Train> values, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine("\tTrain Number\tLead Power\t\tDCC Address\t\tModule\t\t\tTimeUpdated");
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("\tTrain Number\tLead Power\tDCC Address");
                Console.WriteLine("--------------------------------------------------------------------------");
            }
            var count = 0;
            if (verbose)
            {
                foreach (var train in values)
                {
                    Console.Write("{0})\t{1}", ++count, train.TrainNumber);
                    Console.Write("\t\t{0, -15}", train.LeadPower);
                    Console.Write("\t\t{0, -15}", train.DCCAddress);
                    Console.Write("\t\t{0, -15}", train.Module);
                    Console.WriteLine("\t{0, -15}", train.TimeModuleUpdated);
                }
            }
            else
            {
                foreach (var train in values)
                {
                    Console.Write("{0})\t{1}", ++count, train.TrainNumber);
                    Console.Write("\t\t" + train.LeadPower);
                    Console.WriteLine("\t\t" + train.DCCAddress);
                }
            }
        }

        private IEnumerable<Yard> GetPrintAllYardInformationForCurrentTrainLocation()
        {
            Console.WriteLine("Retrieving all Yard Information for Train #{0}...", curPlayer.Train.TrainNumber);
            List<Yard> yards = DB.GetAllYardsOnModuleDB(curPlayer.Train.Module).ToList();
            if (yards == null)
            {
                return null;
            }
            PrintYards(yards);
            return yards;
        }

        /// <summary>
        /// Prints all of the options for the user
        /// </summary>
        private void PrintOptions()
        {
            DrawTrain();
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
            Console.Write("\nPlease enter the number next to your selection ");
        }

        private void SetUpPlayer()
        {
            Console.WriteLine("Welcome to the train game!");
            Console.WriteLine("Let's set up your player.... \n\n\n");
            Console.WriteLine("Whats your name?");
            curPlayer.Name = Console.ReadLine();
            Console.WriteLine("Now, lets choose your train...");
            Console.WriteLine("Avaliable Train(s)");
            List<Train> trains = GetPrintAllTrainInformation(true).ToList();
            Console.Write("Please select the number related to your train ");
            var trainIndex = getInputAndReturnNumber(trains.Count());
            curPlayer.Train = trains[trainIndex];
            curPlayer.Train.Module = curPlayer.Train.Module;
            Console.WriteLine("Hello {0}, thank you for playing the game!\nYour Train Number is {1}... \nGood Luck!",
                    curPlayer.Name, curPlayer.Train.TrainNumber);
            EndFunction();
        }

        private void DrawTrain()
        {
            Console.WriteLine("             oooOOOOOOOOOOO");
            Console.WriteLine("             o   ____          :::::::::::::::::: :::::::::::::::::: :::::::::::::::::: :::::::::::::::::: :::::::::::::::::: :::::::::::::::::: :::::::::::::::::: __|-----|__");
            Console.WriteLine("             Y_,_|[]| --++++++ |[][][][][][][][]| |[][][][][][][][]| |[][][][][][][][]| |[][][][][][][][]| |[][][][][][][][]| |[][][][][][][][]| |[][][][][][][][]| |  [] []  |");
            Console.WriteLine("            {|_|_|__|;|______|;|________________|;|________________|;|________________|;|________________|;|________________|;|________________|;|________________|;|_________|;");
            Console.WriteLine("             /oo--OO   oo  oo   oo oo      oo oo   oo oo      oo oo   oo oo      oo oo   oo oo      oo oo   oo oo      oo oo   oo oo      oo oo   oo oo      oo oo   oo     oo");
            Console.WriteLine("+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+\n");
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
            Console.Write("Q or Quit will exit the program:   ");
            var input = Console.ReadLine();
            while (!Int32.TryParse(input, out toReturn) || (toReturn >= count + 1 || toReturn <= 0))
            {
                if (input.ToUpper().Equals("Q") || input.ToUpper().Equals("QUIT"))
                {
                    Console.WriteLine("Exiting the program.");
                    System.Environment.Exit(1);
                }
                Console.WriteLine("This input, {0} is not a valid input, \nPlease try again", input);
                if (toReturn >= count + 1 || toReturn <= 0)
                {
                    Console.WriteLine("Please select one of the numbers on the left of the list");
                }
                Console.Write("Q or Quit will exit the program");
                input = Console.ReadLine();
            }
            return toReturn - 1;
        }

        #endregion HelperMethods
    }
}