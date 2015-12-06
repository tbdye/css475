#Game play and database testing interactions go here.
#The following user variables are in use:
#	@player
#	@playerTrain
#	@playerModule
#	@playerIndustry
#	@playerCar
#	@playerYard

#Create Demo Test Train
#Notes:  Creates a #4 train, two cars in a yard, and a new player.  Shipping
#	orders are created for the two cars.  Waybills associate the shipping order
#	to a car.  The cars are removed from a yard and attached (consisted) to a
#	train.
INSERT INTO Trains VALUES (4, 1234, 1234, DEFAULT);
INSERT INTO TrainLocations VALUES (4, 'Black River Yard', DEFAULT);

INSERT INTO RollingStockCars VALUES ('XA', 'Reefer', NULL);
INSERT INTO RollingStockAtYards VALUES ('XA', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Dairy', 'Half Circle Farms', 1, 'MMI Transfer Site 3', 1, DEFAULT);
INSERT INTO Waybills VALUES ('XA', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (4, 'XA', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'XA';

INSERT INTO RollingStockCars VALUES ('XB', 'Tank Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('XB', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Gasses', 'LGP Professionals', 4, 'Palin Interchange', 1, DEFAULT);
INSERT INTO Waybills VALUES ('XB', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (4, 'XB', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'XB';

INSERT INTO Crews VALUES ('Demo Player', NULL);

#Add Crew To Train
#The user selects an existing train to crew.
#The system assigns that train to that crew and records the time.
SET @player = 'Demo Player';
SET @playerTrain = 4;
INSERT INTO TrainCrews VALUES (@playerTrain, @player, DEFAULT);

#Display List of Active Modules:
#The user requests to see modules in the layout.
#The system displays modules that have been added and are marked as active.
SELECT 
    *
FROM
    ViewActiveModules;

#Display User Train:
#The user selects a train to view.
#The system displays the train number, locomotive number, digital command
#	controller (DCC) address, which crew is associated with the train, the
#	module the train is currently residing on, and when the train's last
#	movement was.
SET @playerTrain = 4;
SELECT 
    *
FROM
    ViewUserTrain
WHERE
    TrainNumber = @playerTrain;
		
#Display Train Consist:
#The user selects a train to view waybill information.
#The system displays the train's consist (attached cars).  For each car, it
#	displays: car ID, car type, loaded product if exists, and car next
#	destination.
SET @playerTrain = 4;
SELECT 
    *
FROM
    ViewTrainConsist
WHERE
    OnTrain = @playerTrain;
			
#Check-In at Module (Move Train)
#The user reports that his or her train has arrived or is doing work at a module.
#The system records the train’s current location and records the time.
SET @playerModule = '180 Farms';
SET @playerTrain = 4;
UPDATE TrainLocations SET OnModule = @playerModule WHERE TrainNumber = @playerTrain;	

#Display Module:
#The user requests to see industries on a module.
#The system displays active industries on that module and which main line they are on.
SET @playerModule = '180 Farms';
SELECT 
    *
FROM
    ViewModule
WHERE
    OnModule = @playerModule;
		
#Load Rolling Stock (formerly Deliver Rolling Stock)
#The user reports that his or her train is servicing an industry and indicates
#	that a rolling stock car has been dropped off for loading.
#The system ensures the product type carried matches a product type produced by
#	the industry and verifies that the shipping order has not previously been
#	recorded as being picked up.  The car is removed from the train and added
#	to the industry siding.  The shipping order is updated to record that the
#	load has been picked up.
SET @playerIndustry = 'Half Circle Farms';
SET @playerCar = 'XA';
CALL LoadRollingStock(@playerIndustry, @playerCar);

#Display Rolling Stock At Industry:
#The user requests to see rolling stock servicing an industry.
#The system displays, for each car:  car ID, car type, and arrival time.
SET @playerIndustry = 'Half Circle Farms';
SELECT 
    *
FROM
    ViewRollingStockAtIndustry
WHERE
    AtIndustry = @playerIndustry;

#Receive Rolling Stock:
#The user selects a car servicing an industry to add to a train.
#The system removes the car from the industry siding, consists the car to a
#	train, and records the time.
SET @playerCar = 'XA';
SET @playerTrain = 4;
SET SQL_SAFE_UPDATES = 0;
DELETE FROM RollingStockAtIndustries WHERE CarID = @playerCar;
SET SQL_SAFE_UPDATES = 1;
INSERT INTO ConsistedCars VALUES (@playerTrain, @playerCar, DEFAULT);	

#Check-In at Module (Move Train) [Duplicate Reference]
#The user reports that his or her train has arrived or is doing work at a module.
#The system records the train's current location and records the time.
SET @playerModule = 'Black River Yard';
SET @playerTrain = 4;
UPDATE TrainLocations SET OnModule = @playerModule WHERE TrainNumber = @playerTrain;

#Display Industry:
#The user requests to see detailed information about an industry.
#The system displays industry siding information and identifies which product
#	types can be delivered to specific sidings, if siding assignments are
#	defined.
SET @playerIndustry = 'MMI Transfer Site 3';
SELECT 
    s.*, p.UsingProductType
FROM
    IndustrySidings s
        JOIN
    IndustryProducts p ON s.ForIndustry = p.ForIndustry
WHERE
    p.UsingProductType NOT IN (SELECT 
            ForProductType
        FROM
            SidingAssignments
        WHERE
            ForIndustry = @playerIndustry)
        AND s.SidingNumber NOT IN (SELECT 
            SidingNumber
        FROM
            SidingAssignments
        WHERE
            ForIndustry = @playerIndustry)
        AND s.ForIndustry = @playerIndustry 
UNION SELECT 
    s.*, p.UsingProductType
FROM
    IndustrySidings s
        JOIN
    IndustryProducts p ON s.ForIndustry = p.ForIndustry
WHERE
    p.UsingProductType IN (SELECT 
            ForProductType
        FROM
            SidingAssignments
        WHERE
            ForIndustry = @playerIndustry)
        AND s.SidingNumber IN (SELECT 
            SidingNumber
        FROM
            SidingAssignments
        WHERE
            ForIndustry = @playerIndustry)
        AND s.ForIndustry = @playerIndustry;

#Unload Rolling Stock (formerly Deliver Rolling Stock)
#The user reports that his or her train is servicing an industry and indicates
#	that a rolling stock car has been dropped off for unloading.
#The system ensures the product type carried matches a product type consumed by
#	the industry and verifies that the shipping order has not previously been
#	recorded as being delivered.  The car is removed from the train and added
#	to the industry siding.  The shipping order is updated to record that the
#	load has been delivered.
SET @playerIndustry = 'MMI Transfer Site 3';
SET @playerCar = 'XA';
CALL UnloadRollingStock(@playerIndustry, @playerCar);

#Receive Rolling Stock: [Duplicate Reference]
#The user selects a car servicing an industry to add to a train.
#The system removes the car from the industry siding, consists the car to a
#	train, and records the time.
SET @playerCar = 'XA';
SET @playerTrain = 4;
SET SQL_SAFE_UPDATES = 0;
DELETE FROM RollingStockAtIndustries WHERE CarID = @playerCar;
SET SQL_SAFE_UPDATES = 1;
INSERT INTO ConsistedCars VALUES (@playerTrain, @playerCar, DEFAULT);

#Return Rolling Stock to Yard:
#The user selects an empty car from a train to return to a yard to be
#	reclassified into other trains.
#The system removes the car from the player's train.  The car is assigned to
#	the yard and the waybill is destroyed.
SET @playerTrain = 4;
SET @playerCar = 'XA';
SET @playerYard = 'Black River Yard';
DELETE FROM ConsistedCars WHERE OnTrain = @playerTrain AND UsingCar = @playerCar;
INSERT INTO RollingStockAtYards VALUES (@playerCar, @playerYard, DEFAULT);
SET SQL_SAFE_UPDATES = 0;
DELETE FROM Waybills WHERE OnCar = @playerCar;
SET SQL_SAFE_UPDATES = 1;

#Remove Crew From Train
#The user is finished with a game session.
#The system disassociates a crew from a train.
SET @playerTrain = 4;
DELETE FROM TrainCrews WHERE OnTrain = @playerTrain;

#End of demo

#Clean up from previous demo
DELETE FROM Crews WHERE CrewName = 'Demo Player';
DELETE FROM ConsistedCars WHERE OnTrain = 4;
DELETE FROM Waybills WHERE OnCar = 'XB';
DELETE FROM RollingStockAtYards WHERE CarID = 'XA';
DELETE FROM RollingStockCars WHERE CarID = 'XA';
DELETE FROM RollingStockCars WHERE CarID = 'XB';
DELETE FROM Trains WHERE TrainNumber = 4;