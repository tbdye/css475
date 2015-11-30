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
                        ReportTrainInformationAndCrews();
                        break;
                    case 2:
                        SelectCrewAndSelectTrainFromCrew();
                        break;
                    case 4:

                        break;
                    case 5:

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
            Console.WriteLine("\t\t5\t\tDisplay industries and yards from a module\n");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("Game play example");
            Console.WriteLine("_________________________________________________________________________________\n");
            Console.WriteLine("\t\t6\t\tMove train from one module to another module");
            Console.WriteLine("\t\t7\t\tDisplay rolling stock cars at an industry");
            Console.WriteLine("\t\t8\t\tDrop off rolling stock at an industry on a selected module");
            Console.WriteLine("\t\t9\t\tPick up rolling stock from an industry on a selected module");
            Console.WriteLine("_________________________________________________________________________________");
            Console.Write("\nPlease enter the number next to your selection (or q to exit):  ");
        }

        // Option 1
        private void ReportTrainInformationAndCrews()
        {
            Console.Clear();

            Console.WriteLine("Trains");
            Console.WriteLine("--------------------------------------------");
            GetPrintAllTrainInformation();
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Crew");
            Console.WriteLine("--------------------------------------------");
            GetPrintAllCrewInformation();
            Console.WriteLine();

            EndFunction();
            
        }

        private IEnumerable<Train> GetPrintAllTrainInformation()
        {
            IEnumerable<Train> trains = DB.GetTrainInfoDB();
            if (trains == null)
            {
                return null;
            }
            Console.WriteLine("Train Number\tLead Power\tDCC Address");
            Console.WriteLine("--------------------------------------------");
            foreach (var train in trains)
            {
                Console.Write(train.TrainNumber);
                Console.Write("\t\t" + train.LeadPower);
                Console.WriteLine("\t\t" + train.DCCAddress);
            }
            
            return trains;
        }

        // Option 2 
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

            var crewIndex = getInputAndReturnNumber() - 1;

            List<Train> trains = GetPrintAllTrainInformation().ToList();
            if (trains == null)
            {
                EndFunctionError();
                return new Tuple<Crew, Train>(crew[crewIndex], null);
            }

            Console.Write("Please select the Train Number you want to retrieve:  ");
            var trainIndex = getInputAndReturnNumber() - 1;

            var toReturn = new Tuple<Crew, Train>(crew[crewIndex], trains[trainIndex]);

            Console.WriteLine("Crew Person: {0} and Train #{1}, retrieved", toReturn.Item1.CrewName, toReturn.Item2.TrainNumber);

            EndFunction();

            return toReturn;
        }


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
            Console.WriteLine("Name\tDescription");
            Console.WriteLine("--------------------------------------------");
            var count = 0;
            foreach (var crew in crews)
            {
                Console.Write("{0}) {1}", ++count, crew.CrewName);
                Console.WriteLine("\t\t" + crew.Description);
            }

            return crews;
        }

    }
}
