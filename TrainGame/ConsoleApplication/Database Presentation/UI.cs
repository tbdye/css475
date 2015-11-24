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
                        PrintTrainInfoByNumber();
                        break;
                    case 2:
                        GetProductByIndustry();
                        break;
                    case 3:
                        GetIndustryByName();
                        break;
                    case 4:
                        GetJunctionByID();
                        break;
                    case 5:
                        GetProductRollingStockTypeByProductTypeName();
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
            Console.WriteLine("\t1\t\tPrint Train info By Number");
            Console.WriteLine("\t2\t\tGet Product By Industry");
            Console.WriteLine("\t3\t\tGet Industry By Name ");
            Console.WriteLine("\t4\t\tGet Junction By ID");
            Console.WriteLine("\t5\t\tGet Product Rolling Stock Type by Product Type Name");
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
            int value;
            string input = Console.ReadLine();
            while(!Int32.TryParse(input, out value))
            {
                Console.WriteLine("Your Input: \n " + input + "\nIs not a valid input, please try again");
                input = Console.ReadLine();
            }
            return value;
        }

        private void PrintTrainInfoByNumber()
        {
            int trainNo = AskForNumber("Train");
            Train train = DB.GetTrainInfoDB(trainNo).First();
            if (train == null)
            {
                EndFunctionError();
                return;
            }

            Console.WriteLine("\tTrain Number: {0} \n\t"
                                + "Lead Power: {1} \n\t" 
                                + "DCC Address: {2} \n\t"
                                + "On Module: {3}\n\t"
                                + "Time Created: {4}\n\t"
                                + "Time Updated: {5}\n",
                                train.TrainNumber,
                                train.LeadPower,
                                train.DCCAddress,
                                train.onModule,
                                train.TimeCreated,
                                train.TimeUpdated);
            EndFunction();
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
