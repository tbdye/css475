Instructions:

This README.txt file contains basic instructions to compile the demonstration
presentation console client for TrainGame v1.1.

Use the Database Presentation.sln file to open the solution for this Visual
Studio project.

Ensure that Visual Studio 2013 EntityFramework is installed and up-to-date.
You can do this by drilling down from the Visual Studio menu bar on
TOOLS->NuGet Package Manager->Manage NuGet Packages for Solution...  Update
EntityFramework if necessary for the project.

You will need to connect a database to this application in DBHandler.cs.
Supply the appropriate server, port, schema, userName, and password strings for
connection to your locally hosted MySQL server.

From the Visual Studio menu bar:  BUILD->Clean Solution.

From the Visual Studio menu bar:  BUILD->Build Solution.

The compiled application executable located at bin/Debug/Database Presentation.exe