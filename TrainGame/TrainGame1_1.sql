#Clean up old data if any exists

SET FOREIGN_KEY_CHECKS = 0;
#Framework Associations
DROP TABLE IF EXISTS RollingStockTypes;
DROP TABLE IF EXISTS ProductTypes;
DROP TABLE IF EXISTS Modules;
DROP TABLE IF EXISTS ModulesAvailable;
DROP TABLE IF EXISTS Regions;
DROP TABLE IF EXISTS MainLines;
DROP TABLE IF EXISTS Junctions;
DROP TABLE IF EXISTS Yards;
DROP TABLE IF EXISTS Industries;
DROP TABLE IF EXISTS IndustriesAvailable;
DROP TABLE IF EXISTS IndustryActivities;
DROP TABLE IF EXISTS IndustryProducts;
DROP TABLE IF EXISTS IndustrySidings;
DROP TABLE IF EXISTS SidingsAvailableLength;
DROP TABLE IF EXISTS SidingAssignments;

#Game and Session
DROP TABLE IF EXISTS Trains;
DROP TABLE IF EXISTS TrainLocations;
DROP TABLE IF EXISTS RollingStockCars;
DROP TABLE IF EXISTS RollingStockAtYards;
DROP TABLE IF EXISTS RollingStockAtIndustries;
DROP TABLE IF EXISTS Shipments;
DROP TABLE IF EXISTS ShipmentsPickedUp;
DROP TABLE IF EXISTS ShipmentsDelivered;
DROP TABLE IF EXISTS Waybills;
DROP TABLE IF EXISTS ConsistedCars;
DROP TABLE IF EXISTS Crews;
DROP TABLE IF EXISTS TrainCrews;
SET FOREIGN_KEY_CHECKS = 1;

#Begin Framework Associations

#RollingStockTypes
#Define a rolling stock car by name and length.
#Notes:  None.
#Pre-conditions:  None.
#Input Constraint:  CarLength > 0
#Constrains:  ProductTypes, RollingStockCars
#Assumptions:  CarLength is fully and non-transitively dependent on
#	CarTypeName.
CREATE TABLE IF NOT EXISTS RollingStockTypes (
    CarTypeName VARCHAR(255) NOT NULL PRIMARY KEY,
    CarLength INT NOT NULL
);

#RollingStockTypes
#Trigger to ensure that CarLength > 0.
DELIMITER $$
CREATE TRIGGER RollingStockTypesTrigger BEFORE INSERT ON RollingStockTypes
	FOR EACH ROW
	BEGIN
		IF NEW.CarLength <= 0
		THEN
			SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'CarLength must be greater than 0.';
		END IF;
	END$$
DELIMITER ;

#ProductTypes
#Define a product type and which rolling stock car type it is carried by.
#Notes:  None.
#Pre-conditions:  A valid RollingStockTypes entity must exist.
#Input Constraint:  N/A
#Constrains:  IndustryProducts, SidingAssignments, Shipments
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS ProductTypes (
    ProductTypeName VARCHAR(255) NOT NULL PRIMARY KEY,
    OnRollingStockType VARCHAR(255) NOT NULL,
    FOREIGN KEY (OnRollingStockType)
        REFERENCES RollingStockTypes (CarTypeName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Modules
#Defines properties of a physical space containing industries and railroad.
#Notes:  None.
#Pre-conditions:  None.
#Input Constraint:  N/A
#Constrains:  ModulesAvailable, Regions, MainLines, Junctions, Yards,
#	Industries, TrainLocations
#Assumptions:  ModuleType, ModuleShape, and Description are
#	optional descriptive attributes that are also fully and non-transitively
#	dependent on ModuleName.
CREATE TABLE IF NOT EXISTS Modules (
    ModuleName VARCHAR(255) NOT NULL PRIMARY KEY,
    ModuleOwner VARCHAR(255) NOT NULL,
    ModuleType VARCHAR(20),
    ModuleShape VARCHAR(20),
    Description VARCHAR(255)
);

#ModulesAvailable
#Defines the availability of a previously created module.
#Notes:  A module is available if it is part of a layout, but can remain
#	defined and not be available.
#Pre-conditions:  A valid Modules entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  IsAvailable is fully and non-transitively dependent on
#	ModuleName.
CREATE TABLE IF NOT EXISTS ModulesAvailable (
    ModuleName VARCHAR(255) NOT NULL PRIMARY KEY,
    IsAvailable BOOL NOT NULL,
    FOREIGN KEY (ModuleName)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Regions
#Defines groupings of modules and assigns a common name for the purpose of
#	layout maps.
#Notes:  Regions are used by a routing engine to indicate a path for a train to
#	follow (version 2.0).  They also provide information to the shipping order
#	generator to allow for physical layout spacing between load and unload
#	points (version 2.0).
#Pre-conditions:  A valid Modules entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS Regions (
    RegionName VARCHAR(255) NOT NULL,
    Module VARCHAR(255) NOT NULL,
    PRIMARY KEY (RegionName , Module),
    FOREIGN KEY (Module)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#MainLines
#Defines a railroad track on a module that allows passage from at least one
#	side of a module.
#Notes:  A main line is contiguous if it allows through traffic to pass over
#	the entirety of a module.  Main lines are used by a routing engine and will
#	indicate a path for a train to follow (version 2.0).
#Pre-conditions:  A valid Modules entity must exist.
#Input Constraint:  N/A
#Constrains:  Junctions, Yards, Industries
#Assumptions:  IsContiguous is fully and non-transitively dependent on
#	LineName.
CREATE TABLE IF NOT EXISTS MainLines (
    LineName VARCHAR(255) NOT NULL,
    OnModule VARCHAR(255) NOT NULL,
    IsContiguous BOOL NOT NULL,
    PRIMARY KEY (LineName , OnModule),
    FOREIGN KEY (OnModule)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Junctions
#Defines a connection point between two main lines on a module where trains may
#	cross from one main line to another.
#Notes:  Physically, junctions are represented by turnouts, and are currently
#	non-directional in nature.  Junctions are used by a routing engine and will
#	indicate a path for a train to follow (version 2.0).
#Pre-conditions:  A valid Modules entity and two MainLines entities must exist.
#Input Constraint:  FromLine <> ToLine
#Constrains:  None.
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS Junctions (
    JunctionID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    OnModule VARCHAR(255) NOT NULL,
    FromLine VARCHAR(255) NOT NULL,
    ToLine VARCHAR(255) NOT NULL,
    FOREIGN KEY (OnModule)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (FromLine)
        REFERENCES MainLines (LineName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ToLine)
        REFERENCES MainLines (LineName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Junctions
#Trigger to ensure that FromLine and ToLine are not the same entities.
DELIMITER $$
CREATE TRIGGER JunctionsTrigger BEFORE INSERT ON Junctions
	FOR EACH ROW
	BEGIN
		IF (NEW.FromLine = NEW.ToLine)
		THEN
			SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A junction can only exist between two different main lines.';
		END IF;
	END$$
DELIMITER ;

#Yards
#Defines a collection of tracks used to store trains and rolling stock when not
#	actively being used by crew in a game session.
#Notes:  Yards are the typical origination point of empty rolling stock cars
#	and the eventual destination for empty rolling stock cars after shipments
#	have been completed.
#Pre-conditions:  A valid Modules entity and a MainLines entity must exist.
#Input Constraint:  N/A
#Constrains:  RollingStockAtYards, Waybills
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS Yards (
    YardName VARCHAR(255) NOT NULL PRIMARY KEY,
    OnModule VARCHAR(255) NOT NULL,
    OnMainLine VARCHAR(255) NOT NULL,
    FOREIGN KEY (OnModule)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (OnMainLine)
        REFERENCES MainLines (LineName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Industries
#Defines a business, accessable by rail, producing or consuming goods, and
#	transports those goods via rolling stock cars.
#Notes:  None.
#Pre-conditions:  A valid Modules entity and a MainLines entity must exist.
#Input Constraints:  N/A
#Constrains:  IndustriesAvailable, IndustryActivities, IndustryProducts,
#	IndustrySidings, SidingsAvailableLength, SidingAssignments, Shipments,
#	RollingStockAtIndustries
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS Industries (
    IndustryName VARCHAR(255) NOT NULL PRIMARY KEY,
    OnModule VARCHAR(255) NOT NULL,
    OnMainLine VARCHAR(255) NOT NULL,
    FOREIGN KEY (OnModule)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (OnMainLine)
        REFERENCES MainLines (LineName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#IndustriesAvailable
#Defines the availability of an existing industry to be used in creating new
#	shipping orders.
#Notes:  Industries have the ability to be disabled during a game session to
#	prevent new shipping orders from being created.
#Pre-conditions:  A valid Industries entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  IsAvailable is fully and non-transitively dependent on
#	IndustryName.
CREATE TABLE IF NOT EXISTS IndustriesAvailable (
    IndustryName VARCHAR(255) NOT NULL PRIMARY KEY,
    IsAvailable BOOL NOT NULL,
    FOREIGN KEY (IndustryName)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#IndustryActivities
#Defines the overall frequency of an existing industry to be considered for new
#	shipping orders.
#Pre-conditions:  A valid Industries entity must exist.
#Input Constraint:  ActivityLevel = {1, 2, 3} where 1 is lowest and 3 is
#	highest.
#Constrains:  None.
#Assumptions:  ActivityLevel is fully and non-transitively dependent on
#	IndustryName.
CREATE TABLE IF NOT EXISTS IndustryActivities (
    IndustryName VARCHAR(255) NOT NULL PRIMARY KEY,
    ActivityLevel INT NOT NULL,
    FOREIGN KEY (IndustryName)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#IndustryActivities
#Trigger to ensure that ActivityLevel is limited to values 1, 2, or 3.
DELIMITER $$ 
CREATE TRIGGER IndustryActivitiesTrigger BEFORE INSERT ON IndustryActivities
    FOR EACH ROW
    BEGIN
        IF  NEW.ActivityLevel <= 0 OR 
            NEW.ActivityLevel > 3
        THEN 
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Activity level must be set to 1, 2, or 3.';
        END IF;
    END $$
DELIMITER ;

#IndustryProducts
#Defines which product types an industry produces or consumes.
#Notes:  An industry product entity must be created for each product type
#	served.  If this entity exists and IsProducer is FALSE, then the industry
#	is a consumer for that product type.
#Pre-conditions:  A valid Industries entity and a ProductTypes entity must
#	exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  IsProducer is fully and non-transitively dependent on
#	ForIndustry.
CREATE TABLE IF NOT EXISTS IndustryProducts (
    ForIndustry VARCHAR(255) NOT NULL,
    UsingProductType VARCHAR(255) NOT NULL,
    IsProducer BOOL NOT NULL,
    PRIMARY KEY (ForIndustry , UsingProductType),
    FOREIGN KEY (ForIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (UsingProductType)
        REFERENCES ProductTypes (ProductTypeName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#IndustrySidings
#Defines which track siding an industry uses for rolling stock cars.
#Notes:  An industry must have at least one track siding to be accessable.
#Pre-conditions:  A valid Industries entity must exist.
#Input Constraints:  SidingLength > 0
#Constrains:  SidingsAvailableLength, SidingAssignments, Shipments
#Assumptions:  SidingLength is fully and non-transitively dependent on
#	ForIndustry and SidingNumber.
CREATE TABLE IF NOT EXISTS IndustrySidings (
    ForIndustry VARCHAR(255) NOT NULL,
    SidingNumber INT NOT NULL,
    SidingLength INT NOT NULL,
    PRIMARY KEY (ForIndustry , SidingNumber),
    FOREIGN KEY (ForIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#IndustrySidings
#Trigger to ensure SidingLength > 0.
DELIMITER $$ 
CREATE TRIGGER IndustrySidingsTrigger BEFORE INSERT ON IndustrySidings
    FOR EACH ROW
    BEGIN
        IF (NEW.SidingLength <= 0)
        THEN 
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Siding length must be greater than 0.';
		END IF;
    END $$
DELIMITER ;

#SidingsAvailableLength
#Declares the remaining length available for an industry siding when occupied
#	by rolling stock cars.
#Notes:  AvailableLength is used to determine if industries should be
#	considered for new shipping orders.  If AvailableLength is less than the
#	length of an incoming rolling stock car, the siding is considered to be
#	"full" and should not be considered for new shipping orders.
#Pre-conditions:  A valid Industries must exist with at least one declared
#	IndustrySidings entity.
#Input Constraint:  AvailableLength > 0, AvailableLength <= SidingLength
#Constrains:  None.
#Assumptions:  AvailableLength is fully and non-transitively dependent on
#	ForIndustry and SidingNumber.
CREATE TABLE IF NOT EXISTS SidingsAvailableLength (
    ForIndustry VARCHAR(255) NOT NULL,
    SidingNumber INT NOT NULL,
    AvailableLength INT NOT NULL,
    PRIMARY KEY (ForIndustry , SidingNumber),
	FOREIGN KEY (ForIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ForIndustry , SidingNumber)
        REFERENCES IndustrySidings (ForIndustry , SidingNumber)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#SidingsAvailableLength
#Trigger to ensure AvailableLength > 0 and AvailableLength <= SidingLength
DELIMITER $$ 
CREATE TRIGGER SidingsAvailableLengthTrigger BEFORE INSERT ON SidingsAvailableLength
    FOR EACH ROW
    BEGIN
        SET @sidingLength = (SELECT SidingLength FROM IndustrySidings WHERE ForIndustry = NEW.ForIndustry AND SidingNumber = NEW.SidingNumber);
        IF (NEW.AvailableLength <= 0)
		THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Avaliable length must be greater than 0.';
		ELSEIF (NEW.AvailableLength > @sidingLength)
        THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Value exceeds maximum siding length.';
		END IF;
    END $$
DELIMITER ;

#SidingAssignments
#Defines preferences for specific product types going to only specific industry
#	sidings.
#Notes:  An industry siding will accept all product types for an industry by
#	default.  Siding assignments will restrict a siding to only authorized
#	product types.  Siding assignments should not be used unless at least two
#	industry sidings exist.
#Pre-conditions:  A valid Industries entity must exist more than one declared
#	IndustrySidings entities and at least one ProductTypes entity.
#Input Constraint:  The count for total industry sidings at an industry must be
#	at least 2.
#Constrains:  None.
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS SidingAssignments (
    ForIndustry VARCHAR(255) NOT NULL,
    SidingNumber INT NOT NULL,
    ForProductType VARCHAR(255) NOT NULL,
    PRIMARY KEY (ForIndustry , SidingNumber , ForProductType),
    FOREIGN KEY (ForIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ForIndustry , SidingNumber)
        REFERENCES IndustrySidings (ForIndustry , SidingNumber)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ForProductType)
        REFERENCES ProductTypes (ProductTypeName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#SidingAssignments
#Trigger to ensure the count for total industry sidings at an industry must be at
#	least 2 before a siding assignment can be applied.
DELIMITER $$
CREATE TRIGGER SidingAssignmentsTrigger BEFORE INSERT ON SidingAssignments
	FOR EACH ROW
	BEGIN
		SET @industry = NEW.ForIndustry;
        SET @sidingCount = (SELECT COUNT(*) FROM IndustrySidings WHERE ForIndustry = @industry GROUP BY ForIndustry);
        IF @sidingCount < 2
		THEN
			SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'At least two sidings must exist at this industry before siding assingments can be applied.';
		END IF;
	END$$
DELIMITER ;

#Begin Game and Session

#Trains
#Represents a physical locomotive (or locomotive group) for a player to control
#	in a game session.
#Notes:  TimeCreated is for reference only and should not be updated.
#Pre-conditions:  A Modules entity must exist for player origination.
#Input Constraint:  N/A
#Constrains:  TrainLocations, ConsistedCars, TrainCrews
#Assumptions:  LeadPower, DCCAddress, and TimeCreated are fully and
#	non-transitively dependent on TrainNumber.
CREATE TABLE IF NOT EXISTS Trains (
    TrainNumber INT NOT NULL PRIMARY KEY,
    LeadPower VARCHAR(255),
    DCCAddress CHAR(4),
    TimeCreated TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

#TrainLocations
#Represents the location of a train on a layout.
#Notes:  TimeUpdated will apply the current timestamp automatically with any
#	activity.
#Pre-conditions:  A valid Trains entity and Modules entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  OnModule and TimeUpdated is fully and non-transitively dependent
#	on TrainNumber.
CREATE TABLE IF NOT EXISTS TrainLocations (
    TrainNumber INT NOT NULL PRIMARY KEY,
    OnModule VARCHAR(255) NOT NULL,
    TimeUpdated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (TrainNumber)
        REFERENCES Trains (TrainNumber)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (OnModule)
        REFERENCES Modules (ModuleName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#RollingStockCars
#Represents a physical train car used by players in a game session.
#Notes:  None.
#Pre-conditions:  A valid RollingStockCarTypes entity must exist.
#Input Constraint:  N/A
#Constrains:  RollingStockAtIndustries, RollingStockAtYards, Waybills,
#	ConsistedCars
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS RollingStockCars (
    CarID VARCHAR(255) NOT NULL PRIMARY KEY,
    CarType VARCHAR(255) NOT NULL,
    Description VARCHAR(255),
    FOREIGN KEY (CarType)
        REFERENCES RollingStockTypes (CarTypeName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#RollingStockAtYards
#Declares the identies of rolling stock cars currently at a specific train
#	yard.
#Notes:  Rolling stock cars not consisted to a train or reported at an industry
#	must report at a yard.
#Pre-conditions:  A valid RollingStockCars entity and Yards entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  TimeArrived is fully and non-transitively dependent on CarID.
CREATE TABLE IF NOT EXISTS RollingStockAtYards (
    CarID VARCHAR(255) NOT NULL PRIMARY KEY,
    AtYard VARCHAR(255) NOT NULL,
    TimeArrived TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CarID)
        REFERENCES RollingStockCars (CarID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (AtYard)
        REFERENCES Yards (YardName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#RollingStockAtIndustries
#Declares the identities of rolling stock cars current at a specific industry.
#Notes:  Rolling stock cars not consisted to a train or reported at a yard must
#	report at an industry.
#Pre-conditions:  A valid RollingStockCars entity and Industries entity must
#	exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  TimeArrived is fully and non-transitively dependent on CarID.
CREATE TABLE IF NOT EXISTS RollingStockAtIndustries (
    CarID VARCHAR(255) NOT NULL PRIMARY KEY,
    AtIndustry VARCHAR(255) NOT NULL,
    TimeArrived TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CarID)
        REFERENCES RollingStockCars (CarID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (AtIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Shipments
#Declares a shipping order of a product type for pickup at one industry and
#	delivery to another industry.
#Notes:  Shipping orders are created on demand when rolling stock cars are
#	added to a game session (version 2.0).
#Pre-conditions:  A valid ProductTypes entity and two Industries entities must
#	exist.  For each industry, one IndustrySidings entity must exist.
#Input Constraint:  FromIndustry <> ToIndustry
#Constrains:  ShipmentsPickedUp, ShipmentsDelivered, Waybills
#Assumptions:  TimeCreated is fully and non-transitively dependent on ShipmentID.
CREATE TABLE IF NOT EXISTS Shipments (
    ShipmentID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ProductType VARCHAR(255) NOT NULL,
    FromIndustry VARCHAR(255) NOT NULL,
    FromSiding INT NOT NULL,
    ToIndustry VARCHAR(255) NOT NULL,
    ToSiding INT NOT NULL,
	TimeCreated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProductType)
        REFERENCES ProductTypes (ProductTypeName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (FromIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (FromIndustry , FromSiding)
        REFERENCES IndustrySidings (ForIndustry , SidingNumber)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ToIndustry)
        REFERENCES Industries (IndustryName)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ToIndustry , ToSiding)
        REFERENCES IndustrySidings (ForIndustry , SidingNumber)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Shipments
#Trigger to ensure that FromIndustry and ToIndustry are not the same entities.
DELIMITER $$
CREATE TRIGGER ShipmentsTrigger BEFORE INSERT ON Shipments
	FOR EACH ROW
	BEGIN
		IF (NEW.FromIndustry = NEW.ToIndustry)
		THEN
			SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A shipment can only be created to service two different industries.';
		END IF;
	END$$
DELIMITER ;

#ShipmentsPickedUp
#Declares if a shipping order has been picked up from an Industry producing a
#	certain product type.
#Note:  If this entity exists, the product has been loaded onto a rolling stock
#	car.
#Pre-conditions:  A valid Shipments entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  TimePickedUp is fully and non-transitively dependent on
#	ShipmentID.
CREATE TABLE IF NOT EXISTS ShipmentsPickedUp (
    ShipmentID INT NOT NULL PRIMARY KEY,
    TimePickedUp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ShipmentID)
        REFERENCES Shipments (ShipmentID)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#ShipmentsDelivered
#Declares if a shipping order has been delivered to an Industry consuming a
#	certain product type.
#Note:  If this entity exists, the product has been unloaded from a rolling
#	stock car.
#Pre-conditions:  A valid Shipments entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  TimeDelivered is fully and non-transitively dependent on
#	ShipmentID.
CREATE TABLE IF NOT EXISTS ShipmentsDelivered (
    ShipmentID INT NOT NULL PRIMARY KEY,
    TimeDelivered TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ShipmentID)
        REFERENCES Shipments (ShipmentID)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#Waybills
#Declares the association of a shipping order and a specific rolling stock car.
#Notes:  ReturnToYard determines the location empty rolling stock will be sent
#	to after the shipping order is complete.
#Pre-conditions:  At least one valid entity for RollingStockCars, Shipments,
#	and Yards must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS Waybills (
    OnCar VARCHAR(255) NOT NULL PRIMARY KEY,
    UsingShipmentID INT NOT NULL,
    ReturnToYard VARCHAR(255) NOT NULL,
    FOREIGN KEY (OnCar)
        REFERENCES RollingStockCars (CarID)
        ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY (UsingShipmentID)
        REFERENCES Shipments (ShipmentID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ReturnToYard)
        REFERENCES Yards (YardName)
        ON DELETE CASCADE ON UPDATE CASCADE
);

#ConsistedCars
#Represents the association of individual rolling stock cars attached to
#	trains.
#Notes:  None.
#Pre-conditions:  A valid Trains entity must exist, and for each car added, a
#	RollingStockCars entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  TimeAdded is fully and non-transitively dependent on OnTrain and
#	UsingCar.
CREATE TABLE IF NOT EXISTS ConsistedCars (
    OnTrain INT NOT NULL,
    UsingCar VARCHAR(255) NOT NULL UNIQUE,
    TimeAdded TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (OnTrain , UsingCar),
    FOREIGN KEY (OnTrain)
        REFERENCES Trains (TrainNumber)
        ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY (UsingCar)
        REFERENCES RollingStockCars (CarID)
        ON DELETE NO ACTION ON UPDATE NO ACTION
);

#Crews
#Represents a player identity in a game session.
#Notes:  None.
#Pre-conditions:  None.
#Input Constraint:  N/A
#Constrains:  TrainCrews
#Assumptions:  None.
CREATE TABLE IF NOT EXISTS Crews (
    CrewName VARCHAR(255) NOT NULL PRIMARY KEY,
    Description VARCHAR(255)
);

#TrainCrews
#Declares the association of a crew with a train.
#Notes:  None.
#Pre-conditions:  A valid Trains entity and valid Crews entity must exist.
#Input Constraint:  N/A
#Constrains:  None.
#Assumptions:  WithCrew and TimeJoined is fully and non-transitively dependent
#	on OnTrain.
CREATE TABLE IF NOT EXISTS TrainCrews (
    OnTrain INT NOT NULL PRIMARY KEY,
    WithCrew VARCHAR(255) NOT NULL UNIQUE,
    TimeJoined TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OnTrain)
        REFERENCES Trains (TrainNumber)
        ON DELETE NO ACTION ON UPDATE NO ACTION,
    FOREIGN KEY (WithCrew)
        REFERENCES Crews (CrewName)
        ON DELETE NO ACTION ON UPDATE NO ACTION
);

#Functions

#GetNextDestination
DROP FUNCTION IF EXISTS GetNextDestination;
DELIMITER $$
CREATE FUNCTION GetNextDestination(
	ShippingOrder INT) 
RETURNS VARCHAR(255)
DETERMINISTIC
BEGIN
	DECLARE Destination VARCHAR(255);
    CASE
		WHEN ShippingOrder IN (SELECT ShipmentID FROM ShipmentsPickedUp WHERE ShipmentID = ShippingOrder)
			AND ShippingOrder IN (SELECT ShipmentID FROM ShipmentsDelivered WHERE ShipmentID = ShippingOrder)
            THEN
				SET Destination = (SELECT ReturnToYard FROM Waybills WHERE UsingShipmentID = ShippingOrder);
		WHEN ShippingOrder NOT IN (SELECT ShipmentID FROM ShipmentsPickedUp WHERE ShipmentID = ShippingOrder)
			THEN
				SET Destination = (SELECT FromIndustry FROM Shipments WHERE ShipmentID = ShippingOrder);
		WHEN ShippingOrder IN (SELECT ShipmentID FROM ShipmentsPickedUp WHERE ShipmentID = ShippingOrder)
			THEN
				SET Destination = (SELECT ToIndustry FROM Shipments WHERE ShipmentID = ShippingOrder);
    END CASE;
    RETURN Destination;
END$$
DELIMITER ;

#StoredProcedures

#LoadRollingStock
DROP PROCEDURE IF EXISTS LoadRollingStock;
DELIMITER $$
CREATE PROCEDURE LoadRollingStock (
    IN Industry VARCHAR(255),
    IN CarID VARCHAR(255))
BEGIN
	SET @shippingID := (SELECT UsingShipmentID FROM Waybills WHERE OnCar = CarID);
    SET @productType := (SELECT ProductType FROM Shipments WHERE ShipmentID = @shippingID);
    IF (
			#Don't attempt to deliver something that has already been delivered
			@shippingID NOT IN (SELECT ShipmentID FROM ShipmentsPickedUp WHERE ShipmentID = @shippingID)
        AND
			#Verify that the industry produces this product type.
			@productType IN (SELECT UsingProductType FROM IndustryProducts WHERE ForIndustry = Industry AND IsProducer = TRUE)
		) THEN
        #Remove the car from the train, add it to the industry, and mark the shipping order as picked up.
		DELETE FROM ConsistedCars WHERE UsingCar = CarID;
		INSERT INTO RollingStockAtIndustries VALUES (CarID, Industry, DEFAULT);
		INSERT INTO ShipmentsPickedUp VALUES (@shippingID, DEFAULT);
	ELSE
		SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Cannot load this rolling stock car at this industry.';
	END IF;
END $$
DELIMITER ;

#UnloadRollingStock
DROP PROCEDURE IF EXISTS UnloadRollingStock;
DELIMITER $$
CREATE PROCEDURE UnloadRollingStock (
    IN Industry VARCHAR(255),
    IN CarID VARCHAR(255))
BEGIN
	SET @shippingID := (SELECT UsingShipmentID FROM Waybills WHERE OnCar = CarID);
	SET @productType := (SELECT ProductType FROM Shipments WHERE ShipmentID = @shippingID);
    IF (
			@shippingID NOT IN (SELECT ShipmentID FROM ShipmentsDelivered WHERE ShipmentID = @shippingID)
        AND
			@productType IN (SELECT UsingProductType FROM IndustryProducts WHERE ForIndustry = Industry AND IsProducer = FALSE)
		) THEN
	DELETE FROM ConsistedCars WHERE UsingCar = CarID;
	INSERT INTO RollingStockAtIndustries VALUES (CarID, Industry, DEFAULT);
	INSERT INTO ShipmentsDelivered VALUES (@shippingID, DEFAULT);
	ELSE
		SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Cannot unload this rolling stock car at this industry.';
	END IF;
END $$
DELIMITER ;

#Views

#ViewActiveModules
DROP VIEW IF EXISTS ViewActiveModules;
CREATE VIEW ViewActiveModules AS
	SELECT 
		*
	FROM
		Modules
	WHERE
		ModuleName IN (SELECT 
				ModuleName
			FROM
				ModulesAvailable
			WHERE
				IsAvailable = TRUE);

#ViewUserTrain
DROP VIEW IF EXISTS ViewUserTrain;
CREATE VIEW ViewUserTrain AS
    SELECT 
        t.TrainNumber,
        t.LeadPower,
        t.DCCAddress,
        c.WithCrew,
        l.onModule,
        l.TimeUpdated
    FROM
        Trains t,
        TrainCrews c,
        TrainLocations l
    WHERE
        t.TrainNumber = c.OnTrain
            AND t.TrainNumber = l.TrainNumber;

#ViewTrainConsist
DROP VIEW IF EXISTS ViewTrainConsist;
CREATE VIEW ViewTrainConsist AS
    SELECT 
        c.OnTrain,
        r.CarID,
        r.CarType,
        s.ProductType,
        GETNEXTDESTINATION(s.ShipmentID) AS NextDestination,
        IF(GETNEXTDESTINATION(s.ShipmentID) IN (SELECT 
                    IndustryName
                FROM
                    Industries
                WHERE
                    IndustryName = GETNEXTDESTINATION(s.ShipmentID)),
            (SELECT 
                    OnModule
                FROM
                    Industries
                WHERE
                    IndustryName = GETNEXTDESTINATION(s.ShipmentID)),
            (SELECT 
                    OnModule
                FROM
                    Yards
                WHERE
                    YardName = GETNEXTDESTINATION(s.ShipmentID))) AS Module
    FROM
        ConsistedCars c,
        RollingStockCars r,
        Waybills w,
        Shipments s
    WHERE
        c.UsingCar = r.CarID
            AND r.CarID = w.OnCar
            AND w.UsingShipmentID = s.ShipmentID
            AND OnCar IN (SELECT 
                UsingCar
            FROM
                ConsistedCars);

#ViewModule
DROP VIEW IF EXISTS ViewModule;
CREATE VIEW ViewModule AS
	SELECT 
		*
	FROM
		Industries
	WHERE
		IndustryName IN (SELECT 
				IndustryName
			FROM
				IndustriesAvailable
			WHERE
				IsAvailable = TRUE);

#ViewRollingStockAtIndustry
DROP VIEW IF EXISTS ViewRollingStockAtIndustry;
CREATE VIEW ViewRollingStockAtIndustry AS
    SELECT 
        c.CarID, c.CarType, i.AtIndustry, i.TimeArrived
    FROM
        RollingStockCars c
            JOIN
        RollingStockAtIndustries i ON c.CarID = i.CarID;

#This test data creates a small model railroad layout, train cars, and players.
#	Data insertion is handled in a sequence order that is either required by
#	the database or encountered through normal program interaction.  This data,
#	as it is, will only be used for development testing.  Normal game operation
#	allows for the application or interface to supply data.

#Section A:  Initalize Game Parameters
#
#Expected Insertion Order:
#Define all RollingStockTypes.
#Define all ProductTypes.

#Define rolling stock car types
#Notes:  For each category of rolling stock in use, declare a car type and car
#	length.
INSERT INTO RollingStockTypes VALUES ('Box Car', 65);
INSERT INTO RollingStockTypes VALUES ('Centerbeam Flat', 85);
INSERT INTO RollingStockTypes VALUES ('Flat Car', 65);
INSERT INTO RollingStockTypes VALUES ('Gondola', 65);
INSERT INTO RollingStockTypes VALUES ('Long Hopper', 75);
INSERT INTO RollingStockTypes VALUES ('Short Hopper', 55);
INSERT INTO RollingStockTypes VALUES ('Open Hopper', 75);
INSERT INTO RollingStockTypes VALUES ('Ore Car', 40);
INSERT INTO RollingStockTypes VALUES ('Reefer', 65);
INSERT INTO RollingStockTypes VALUES ('Stock Car', 55);
INSERT INTO RollingStockTypes VALUES ('Tank Car', 65);
INSERT INTO RollingStockTypes VALUES ('Wood Chip Car', 75);

#Define product type assignments
#Notes:  For each category of rolling stock car type, multiple product types
#	can be carried.  Declare all product types and rolling stock car
#	associations.
INSERT INTO ProductTypes VALUES ('Crates', 'Box Car');
INSERT INTO ProductTypes VALUES ('Metal', 'Box Car');
INSERT INTO ProductTypes VALUES ('Paper', 'Box Car');
INSERT INTO ProductTypes VALUES ('Tools', 'Box Car');
INSERT INTO ProductTypes VALUES ('General Merchandise', 'Box Car');
INSERT INTO ProductTypes VALUES ('Lumber', 'Centerbeam Flat');
INSERT INTO ProductTypes VALUES ('Bulk Equipment', 'Flat Car');
INSERT INTO ProductTypes VALUES ('Logs', 'Flat Car');
INSERT INTO ProductTypes VALUES ('Aggregate', 'Gondola');
INSERT INTO ProductTypes VALUES ('Scrap Metal', 'Gondola');
INSERT INTO ProductTypes VALUES ('Feed', 'Long Hopper');
INSERT INTO ProductTypes VALUES ('Fertilizer', 'Long Hopper');
INSERT INTO ProductTypes VALUES ('Grain', 'Long Hopper');
INSERT INTO ProductTypes VALUES ('Coal', 'Open Hopper');
INSERT INTO ProductTypes VALUES ('Gravel', 'Open Hopper');
INSERT INTO ProductTypes VALUES ('Iron', 'Ore Car');
INSERT INTO ProductTypes VALUES ('Dairy', 'Reefer');
INSERT INTO ProductTypes VALUES ('Manufactured Foods', 'Reefer');
INSERT INTO ProductTypes VALUES ('Meats', 'Reefer');
INSERT INTO ProductTypes VALUES ('Produce', 'Reefer');
INSERT INTO ProductTypes VALUES ('Concrete', 'Short Hopper');
INSERT INTO ProductTypes VALUES ('Plastics', 'Short Hopper');
INSERT INTO ProductTypes VALUES ('Livestock', 'Stock Car');
INSERT INTO ProductTypes VALUES ('Chemicals', 'Tank Car');
INSERT INTO ProductTypes VALUES ('Fuels', 'Tank Car');
INSERT INTO ProductTypes VALUES ('Gasses', 'Tank Car');
INSERT INTO ProductTypes VALUES ('Garbage', 'Wood Chip Car');
INSERT INTO ProductTypes VALUES ('Wood Chips', 'Wood Chip Car');

#Section B:  Build Layout
#Notes:  Data declared in this section is likely to remain persistent and will
#	be used across multiple game sessions.
#
#Expected Insertion Order (for each Module):
#Declare Modules
#Declare MainLines for Modules
#Declare Junctions on MainLines
#Declare Yards on MainLines
#Declare Industries on MainLines
#Declare IndustriesAvailable for Industries
#Declare IndustryActivities for Industries
#Declare IndustryProducts for Industries
#Declare IndustrySidings for Industries
#Declare SidingsAvailableLength for IndustrySidings
#Declare SidingAssignments for IndustrySidings

#Populate the Black River Yard module.
INSERT INTO Modules VALUES ('Black River Yard', 'Mike Donnelly', 'oNeTrak', '3-Straight', 'Contains the Black River Yard');
INSERT INTO MainLines VALUES ('Red', 'Black River Yard', TRUE);
INSERT INTO MainLines VALUES ('Green', 'Black River Yard', FALSE);
INSERT INTO Junctions VALUES (DEFAULT, 'Black River Yard', 'Red', 'Green');
INSERT INTO Yards VALUE ('Black River Yard', 'Black River Yard', 'Red');
INSERT INTO Industries VALUES ('MMI Transfer Site 3', 'Black River Yard', 'Green');
INSERT INTO Industries VALUES ('E.E. Aldrin Sawmill', 'Black River Yard', 'Green');
INSERT INTO Industries VALUES ('B.R. Engine House', 'Black River Yard', 'Green');
INSERT INTO Industries VALUES ('Black River MOW Shop', 'Black River Yard', 'Green');
INSERT INTO IndustriesAvailable VALUES ('MMI Transfer Site 3', TRUE);
INSERT INTO IndustriesAvailable VALUES ('E.E. Aldrin Sawmill', TRUE);
INSERT INTO IndustriesAvailable VALUES ('B.R. Engine House', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Black River MOW Shop', TRUE);
INSERT INTO IndustryActivities VALUES ('MMI Transfer Site 3', 2);
INSERT INTO IndustryActivities VALUES ('E.E. Aldrin Sawmill', 2);
INSERT INTO IndustryActivities VALUES ('B.R. Engine House', 1);
INSERT INTO IndustryActivities VALUES ('Black River MOW Shop', 1);
INSERT INTO IndustryProducts VALUES ('B.R. Engine House', 'Scrap Metal', TRUE);
INSERT INTO IndustryProducts VALUES ('B.R. Engine House', 'Metal', FALSE);
INSERT INTO IndustryProducts VALUES ('B.R. Engine House', 'Bulk Equipment', FALSE);
INSERT INTO IndustryProducts VALUES ('Black River MOW Shop', 'Scrap Metal', TRUE);
INSERT INTO IndustryProducts VALUES ('Black River MOW Shop', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Black River MOW Shop', 'Bulk Equipment', FALSE);
INSERT INTO IndustryProducts VALUES ('MMI Transfer Site 3', 'General Merchandise', FALSE);
INSERT INTO IndustryProducts VALUES ('MMI Transfer Site 3', 'Dairy', FALSE);
INSERT INTO IndustryProducts VALUES ('MMI Transfer Site 3', 'Manufactured Foods', FALSE);
INSERT INTO IndustryProducts VALUES ('MMI Transfer Site 3', 'Meats', FALSE);
INSERT INTO IndustryProducts VALUES ('MMI Transfer Site 3', 'Produce', FALSE);
INSERT INTO IndustryProducts VALUES ('E.E. Aldrin Sawmill', 'Lumber', TRUE);
INSERT INTO IndustryProducts VALUES ('E.E. Aldrin Sawmill', 'Wood Chips', TRUE);
INSERT INTO IndustryProducts VALUES ('E.E. Aldrin Sawmill', 'Bulk Equipment', FALSE);
INSERT INTO IndustryProducts VALUES ('E.E. Aldrin Sawmill', 'Logs', FALSE);
INSERT INTO IndustrySidings VALUES ('B.R. Engine House', 1, 90);
INSERT INTO IndustrySidings VALUES ('B.R. Engine House', 3, 200);
INSERT INTO IndustrySidings VALUES ('MMI Transfer Site 3', 1, 100);
INSERT INTO IndustrySidings VALUES ('MMI Transfer Site 3', 2, 100);
INSERT INTO IndustrySidings VALUES ('MMI Transfer Site 3', 3, 150);
INSERT INTO IndustrySidings VALUES ('E.E. Aldrin Sawmill', 1, 300);
INSERT INTO SidingsAvailableLength VALUES ('B.R. Engine House', 1, 90);
INSERT INTO SidingsAvailableLength VALUES ('B.R. Engine House', 3, 200);
INSERT INTO SidingsAvailableLength VALUES ('MMI Transfer Site 3', 1, 100);
INSERT INTO SidingsAvailableLength VALUES ('MMI Transfer Site 3', 2, 100);
INSERT INTO SidingsAvailableLength VALUES ('MMI Transfer Site 3', 3, 150);
INSERT INTO SidingsAvailableLength VALUES ('E.E. Aldrin Sawmill', 1, 300);
INSERT INTO SidingAssignments VALUES ('B.R. Engine House', 1, 'Scrap Metal');
INSERT INTO SidingAssignments VALUES ('MMI Transfer Site 3', 3, 'General Merchandise');
INSERT INTO SidingAssignments VALUES ('MMI Transfer Site 3', 3, 'Manufactured Foods');

#Populate the Crossover module.
INSERT INTO Modules VALUES ('Crossover', 'Al Lowe', 'Ntrak', 'Straight', 'Access to all main lines available');
INSERT INTO MainLines VALUES ('Red', 'Crossover', TRUE);
INSERT INTO MainLines VALUES ('Yellow', 'Crossover', TRUE);
INSERT INTO MainLines VALUES ('Blue', 'Crossover', TRUE);
INSERT INTO Junctions VALUES (DEFAULT, 'Crossover', 'Red', 'Yellow');
INSERT INTO Junctions VALUES (DEFAULT, 'Crossover', 'Yellow', 'Blue');

#Populate the 180 Farms module.
INSERT INTO Modules VALUES ('180 Farms', 'Al Lowe', 'oNeTrak', '180 Corner', NULL);
INSERT INTO MainLines VALUES ('Red', '180 Farms', TRUE);
INSERT INTO Industries VALUES ('Half Circle Farms', '180 Farms', 'Red');
INSERT INTO IndustriesAvailable VALUES ('Half Circle Farms', TRUE);
INSERT INTO IndustryActivities VALUES ('Half Circle Farms', 2);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Fertilizer', TRUE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Dairy', TRUE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Produce', TRUE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Livestock', TRUE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Lumber', FALSE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Bulk Equipment', FALSE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Feed', FALSE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Grain', FALSE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Fuels', FALSE);
INSERT INTO IndustryProducts VALUES ('Half Circle Farms', 'Gasses', FALSE);
INSERT INTO IndustrySidings VALUES ('Half Circle Farms', 1, 600);
INSERT INTO SidingsAvailableLength VALUES ('Half Circle Farms', 1, 600);

#Populate the Grain Elevator module.
INSERT INTO Modules VALUES ('Grain Elevator', 'Al Lowe', 'oNeTrak', 'Straight', NULL);
INSERT INTO MainLines VALUES ('Red', 'Grain Elevator', TRUE);
INSERT INTO Industries VALUES ('Oatus Elevator', 'Grain Elevator', 'Red');
INSERT INTO IndustriesAvailable VALUES ('Oatus Elevator', TRUE);
INSERT INTO IndustryActivities VALUES ('Oatus Elevator', 2);
INSERT INTO IndustryProducts VALUES ('Oatus Elevator', 'Feed', TRUE);
INSERT INTO IndustryProducts VALUES ('Oatus Elevator', 'Grain', TRUE);
INSERT INTO IndustrySidings VALUES ('Oatus Elevator', 1, 200);
INSERT INTO SidingsAvailableLength VALUES ('Oatus Elevator', 1, 200);

#Populate the Palin Bridge module.
INSERT INTO Modules VALUES ('Palin Bridge', 'Al Lowe', 'oNeTrak', 'Straight', NULL);
INSERT INTO MainLines VALUES ('Red', 'Palin Bridge', TRUE);
INSERT INTO Industries VALUES ('Palin Interchange', 'Palin Bridge', 'Red');
INSERT INTO IndustriesAvailable VALUES ('Palin Interchange', TRUE);
INSERT INTO IndustryActivities VALUES ('Palin Interchange', 1);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Feed', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Fertilizer', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Grain', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Coal', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Gravel', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Concrete', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Livestock', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Fuels', FALSE);
INSERT INTO IndustryProducts VALUES ('Palin Interchange', 'Gasses', FALSE);
INSERT INTO IndustrySidings VALUES ('Palin Interchange', 1, 500);
INSERT INTO SidingsAvailableLength VALUES ('Palin Interchange', 1, 500);

#Populate the Bauxen Crate module.
INSERT INTO Modules VALUES ('Bauxen Crate', 'Al Lowe', 'Transition', 'Straight', 'Access to all lines available');
INSERT INTO MainLines VALUES ('Red', 'Bauxen Crate', TRUE);
INSERT INTO MainLines VALUES ('Alternate Blue', 'Bauxen Crate', FALSE);
INSERT INTO MainLines VALUES ('Yellow', 'Bauxen Crate', FALSE);
INSERT INTO MainLines VALUES ('Blue', 'Bauxen Crate', FALSE);
INSERT INTO Junctions VALUES (DEFAULT, 'Bauxen Crate', 'Red', 'Alternate Blue');
INSERT INTO Junctions VALUES (DEFAULT, 'Bauxen Crate', 'Red', 'Blue');
INSERT INTO Junctions VALUES (DEFAULT, 'Bauxen Crate', 'Red', 'Yellow');
INSERT INTO Industries VALUES ('Bauxen Crates', 'Bauxen Crate', 'Red');
INSERT INTO IndustriesAvailable VALUES ('Bauxen Crates', TRUE);
INSERT INTO IndustryActivities VALUES ('Bauxen Crates', 3);
INSERT INTO IndustryProducts VALUES ('Bauxen Crates', 'Crates', TRUE);
INSERT INTO IndustryProducts VALUES ('Bauxen Crates', 'Wood Chips', TRUE);
INSERT INTO IndustryProducts VALUES ('Bauxen Crates', 'Metal', FALSE);
INSERT INTO IndustryProducts VALUES ('Bauxen Crates', 'Lumber', FALSE);
INSERT INTO IndustrySidings VALUES ('Bauxen Crates', 3, 150);
INSERT INTO IndustrySidings VALUES ('Bauxen Crates', 4, 150);
INSERT INTO SidingsAvailableLength VALUES ('Bauxen Crates', 3, 150);
INSERT INTO SidingsAvailableLength VALUES ('Bauxen Crates', 4, 150);

#Populate the Scott Corner module.
INSERT INTO Modules VALUES ('Scott Corner', 'Al Lowe', 'Ntrak', 'Corner', NULL);
INSERT INTO MainLines VALUES ('Red', 'Scott Corner', TRUE);
INSERT INTO MainLines VALUES ('Yellow', 'Scott Corner', TRUE);
INSERT INTO MainLines VALUES ('Blue', 'Scott Corner', TRUE);

#Populate the Trainyard Mall module.
INSERT INTO Modules VALUES ('Trainyard Mall', 'Al Lowe', 'Ntrak', 'Corner', NULL);
INSERT INTO MainLines VALUES ('Red', 'Trainyard Mall', TRUE);
INSERT INTO MainLines VALUES ('Yellow', 'Trainyard Mall', TRUE);
INSERT INTO MainLines VALUES ('Blue', 'Trainyard Mall', TRUE);

#Populate the Chesterfield module.
INSERT INTO Modules VALUES ('Chesterfield', 'Al Lowe', 'Ntrak', '2-Straight', 'No crossovers');
INSERT INTO MainLines VALUES ('Red', 'Chesterfield', TRUE);
INSERT INTO MainLines VALUES ('Yellow', 'Chesterfield', TRUE);
INSERT INTO MainLines VALUES ('Blue', 'Chesterfield', TRUE);
INSERT INTO MainLines VALUES ('Alternate Blue', 'Chesterfield', TRUE);
INSERT INTO Junctions VALUES (DEFAULT, 'Chesterfield', 'Blue', 'Alternate Blue');
INSERT INTO Industries VALUES ('Chesterfield Power Plant', 'Chesterfield', 'Red');
INSERT INTO Industries VALUES ('Cobra Golf', 'Chesterfield', 'Blue');
INSERT INTO Industries VALUES ('Kesselring Machine Shop', 'Chesterfield', 'Yellow');
INSERT INTO Industries VALUES ('Max Distributing', 'Chesterfield', 'Yellow');
INSERT INTO Industries VALUES ('Lostry Mine', 'Chesterfield', 'Blue');
INSERT INTO Industries VALUES ('Puget Warehouse', 'Chesterfield', 'Blue');
INSERT INTO Industries VALUES ('Tuggle Manufacturing', 'Chesterfield', 'Yellow');
INSERT INTO Industries VALUES ('Wonder Model Trains', 'Chesterfield', 'Red');
INSERT INTO IndustriesAvailable VALUES ('Chesterfield Power Plant', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Cobra Golf', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Kesselring Machine Shop', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Max Distributing', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Lostry Mine', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Puget Warehouse', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Tuggle Manufacturing', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Wonder Model Trains', TRUE);
INSERT INTO IndustryActivities VALUES ('Chesterfield Power Plant', 3);
INSERT INTO IndustryActivities VALUES ('Cobra Golf', 2);
INSERT INTO IndustryActivities VALUES ('Kesselring Machine Shop', 1);
INSERT INTO IndustryActivities VALUES ('Max Distributing', 2);
INSERT INTO IndustryActivities VALUES ('Lostry Mine', 3);
INSERT INTO IndustryActivities VALUES ('Puget Warehouse', 3);
INSERT INTO IndustryActivities VALUES ('Tuggle Manufacturing', 2);
INSERT INTO IndustryActivities VALUES ('Wonder Model Trains', 1);
INSERT INTO IndustryProducts VALUES ('Chesterfield Power Plant', 'Bulk Equipment', FALSE);
INSERT INTO IndustryProducts VALUES ('Chesterfield Power Plant', 'Coal', FALSE);
INSERT INTO IndustryProducts VALUES ('Cobra Golf', 'Scrap Metal', TRUE);
INSERT INTO IndustryProducts VALUES ('Cobra Golf', 'Crates', FALSE);
INSERT INTO IndustryProducts VALUES ('Cobra Golf', 'Metal', FALSE);
INSERT INTO IndustryProducts VALUES ('Cobra Golf', 'Plastics', FALSE);
INSERT INTO IndustryProducts VALUES ('Kesselring Machine Shop', 'Bulk Equipment', TRUE);
INSERT INTO IndustryProducts VALUES ('Kesselring Machine Shop', 'Scrap Metal', TRUE);
INSERT INTO IndustryProducts VALUES ('Kesselring Machine Shop', 'Crates', FALSE);
INSERT INTO IndustryProducts VALUES ('Kesselring Machine Shop', 'Metal', FALSE);
INSERT INTO IndustryProducts VALUES ('Max Distributing', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Max Distributing', 'Crates', FALSE);
INSERT INTO IndustryProducts VALUES ('Max Distributing', 'Paper', FALSE);
INSERT INTO IndustryProducts VALUES ('Max Distributing', 'Tools', FALSE);
INSERT INTO IndustryProducts VALUES ('Lostry Mine', 'Aggregate', TRUE);
INSERT INTO IndustryProducts VALUES ('Lostry Mine', 'Coal', TRUE);
INSERT INTO IndustryProducts VALUES ('Lostry Mine', 'Iron', TRUE);
INSERT INTO IndustryProducts VALUES ('Lostry Mine', 'Tools', FALSE);
INSERT INTO IndustryProducts VALUES ('Lostry Mine', 'Lumber', FALSE);
INSERT INTO IndustryProducts VALUES ('Lostry Mine', 'Bulk Equipment', FALSE);
INSERT INTO IndustryProducts VALUES ('Puget Warehouse', 'General Merchandise', TRUE);
INSERT INTO IndustryProducts VALUES ('Puget Warehouse', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Scrap Metal', TRUE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Crates', FALSE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Metal', FALSE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Paper', FALSE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Tools', FALSE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Lumber', FALSE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Plastics', FALSE);
INSERT INTO IndustryProducts VALUES ('Tuggle Manufacturing', 'Chemicals', FALSE);
INSERT INTO IndustryProducts VALUES ('Wonder Model Trains', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Wonder Model Trains', 'Metal', FALSE);
INSERT INTO IndustryProducts VALUES ('Wonder Model Trains', 'Paper', FALSE);
INSERT INTO IndustryProducts VALUES ('Wonder Model Trains', 'Plastics', FALSE);
INSERT INTO IndustrySidings VALUES ('Chesterfield Power Plant', 1, 160);
INSERT INTO IndustrySidings VALUES ('Wonder Model Trains', 1, 160);
INSERT INTO IndustrySidings VALUES ('Max Distributing', 1, 200);
INSERT INTO IndustrySidings VALUES ('Tuggle Manufacturing', 1, 200);
INSERT INTO IndustrySidings VALUES ('Kesselring Machine Shop', 1, 200);
INSERT INTO IndustrySidings VALUES ('Puget Warehouse', 1, 200);
INSERT INTO IndustrySidings VALUES ('Cobra Golf', 1, 160);
INSERT INTO IndustrySidings VALUES ('Lostry Mine', 1, 160);
INSERT INTO SidingsAvailableLength VALUES ('Chesterfield Power Plant', 1, 160);
INSERT INTO SidingsAvailableLength VALUES ('Wonder Model Trains', 1, 160);
INSERT INTO SidingsAvailableLength VALUES ('Max Distributing', 1, 200);
INSERT INTO SidingsAvailableLength VALUES ('Tuggle Manufacturing', 1, 200);
INSERT INTO SidingsAvailableLength VALUES ('Kesselring Machine Shop', 1, 200);
INSERT INTO SidingsAvailableLength VALUES ('Puget Warehouse', 1, 200);
INSERT INTO SidingsAvailableLength VALUES ('Cobra Golf', 1, 160);
INSERT INTO SidingsAvailableLength VALUES ('Lostry Mine', 1, 160);

#Populate the Pure Oil module.
INSERT INTO Modules VALUES ('Pure Oil', 'Al Lowe', 'Transition', 'Straight', 'Access to all main lines available.');
INSERT INTO MainLines VALUES ('Red', 'Pure Oil', TRUE);
INSERT INTO MainLines VALUES ('Alternate Blue', 'Pure Oil', FALSE);
INSERT INTO MainLines VALUES ('Yellow', 'Pure Oil', FALSE);
INSERT INTO MainLines VALUES ('Blue', 'Pure Oil', FALSE);
INSERT INTO Junctions VALUES (DEFAULT, 'Pure Oil', 'Red', 'Alternate Blue');
INSERT INTO Junctions VALUES (DEFAULT, 'Pure Oil', 'Red', 'Blue');
INSERT INTO Junctions VALUES (DEFAULT, 'Pure Oil', 'Red', 'Yellow');
INSERT INTO Industries VALUES ('Sunset Feed', 'Pure Oil', 'Red');
INSERT INTO Industries VALUES ('Pure Oil', 'Pure Oil', 'Red');
INSERT INTO Industries VALUES ('LGP Professionals', 'Pure Oil', 'Red');
INSERT INTO IndustriesAvailable VALUES ('Sunset Feed', TRUE);
INSERT INTO IndustriesAvailable VALUES ('Pure Oil', TRUE);
INSERT INTO IndustriesAvailable VALUES ('LGP Professionals', TRUE);
INSERT INTO IndustryActivities VALUES ('Sunset Feed', 3);
INSERT INTO IndustryActivities VALUES ('Pure Oil', 2);
INSERT INTO IndustryActivities VALUES ('LGP Professionals', 1);
INSERT INTO IndustryProducts VALUES ('Pure Oil', 'Fuels', TRUE);
INSERT INTO IndustryProducts VALUES ('Sunset Feed', 'Feed', TRUE);
INSERT INTO IndustryProducts VALUES ('Sunset Feed', 'Garbage', TRUE);
INSERT INTO IndustryProducts VALUES ('Sunset Feed', 'Grain', FALSE);
INSERT INTO IndustryProducts VALUES ('Sunset Feed', 'Dairy', FALSE);
INSERT INTO IndustryProducts VALUES ('Sunset Feed', 'Meats', FALSE);
INSERT INTO IndustryProducts VALUES ('Sunset Feed', 'Produce', FALSE);
INSERT INTO IndustryProducts VALUES ('LGP Professionals', 'Gasses', TRUE);
INSERT INTO IndustrySidings VALUES ('Pure Oil', 3, 100);
INSERT INTO IndustrySidings VALUES ('Pure Oil', 4, 100);
INSERT INTO IndustrySidings VALUES ('Sunset Feed', 4, 200);
INSERT INTO IndustrySidings VALUES ('LGP Professionals', 4, 120);
INSERT INTO SidingsAvailableLength VALUES ('Pure Oil', 3, 100);
INSERT INTO SidingsAvailableLength VALUES ('Pure Oil', 4, 100);
INSERT INTO SidingsAvailableLength VALUES ('Sunset Feed', 4, 200);
INSERT INTO SidingsAvailableLength VALUES ('LGP Professionals', 4, 120);

#Section C:  Initialize Game Session
#Notes:  This section contains non-persistent data which typically only exists
#	for a single game session.  A game session is considered active when
#	trains, rolling stock cars, and crews are in existence.  To start a game,
#	select which modules are present from the library of previously defined
#	modules and declare which region they are to be in to determine layout
#	shape.
#
#Expected Insertion Order:
#Declare all ModulesAvailable for Modules.
#Declare all Regions for Modules.

#Activate available modules for gameplay.
INSERT INTO ModulesAvailable VALUES ('Black River Yard', TRUE);
INSERT INTO ModulesAvailable VALUES ('Crossover', TRUE);
INSERT INTO ModulesAvailable VALUES ('180 Farms', TRUE);
INSERT INTO ModulesAvailable VALUES ('Grain Elevator', TRUE);
INSERT INTO ModulesAvailable VALUES ('Palin Bridge', TRUE);
INSERT INTO ModulesAvailable VALUES ('Bauxen Crate', TRUE);
INSERT INTO ModulesAvailable VALUES ('Scott Corner', TRUE);
INSERT INTO ModulesAvailable VALUES ('Trainyard Mall', TRUE);
INSERT INTO ModulesAvailable VALUES ('Chesterfield', TRUE);
INSERT INTO ModulesAvailable VALUES ('Pure Oil', TRUE);

#Add modules into specific regions on the map.
INSERT INTO Regions VALUES ('South', 'Black River Yard');
INSERT INTO Regions VALUES ('South', 'Crossover');
INSERT INTO Regions VALUES ('West', '180 Farms');
INSERT INTO Regions VALUES ('West', 'Grain Elevator');
INSERT INTO Regions VALUES ('East', 'Palin Bridge');
INSERT INTO Regions VALUES ('East', 'Bauxen Crate');
INSERT INTO Regions VALUES ('East', 'Scott Corner');
INSERT INTO Regions VALUES ('North', 'Trainyard Mall');
INSERT INTO Regions VALUES ('North', 'Chesterfield');
INSERT INTO Regions VALUES ('North', 'Pure Oil');

#Section D:  Game Session
#Notes:  A locomotive is selected to declare a new train will exist,
#	originating on a particular module.  Available rolling stock for trains are
#	identified and must be assigned to an originating yard or industry.
#	Shipment orders are created on demand based off of current layout and
#	industry parameters.  For each rolling stock car, waybills must be
#	associated with a shipping order before that car is consisted to a train.
#	For each train, crews are declared and then assigned to a train.  The train
#	and player are then considered to be ready for game play.
#
#Expected Insertion Order:
#Declare Trains
#Declare TrainLocations for Trains
#Declare RollingStockCars
#Declare Shipments
#Declare Waybills for Shipments on RollingStockCars
#Declare ConsistedCars for Trains using RollingStockCars
#Declare Crews
#Declare TrainCrews for Crews on Trains

#Train 1
INSERT INTO Trains VALUES (1, 455, 0455, DEFAULT);
INSERT INTO TrainLocations VALUES (1, 'Black River Yard', DEFAULT);

INSERT INTO RollingStockCars VALUES ('AA', 'Box Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AA', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Crates', 'Bauxen Crates', 3, 'Tuggle Manufacturing', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AA', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (1, 'AA', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AA';

INSERT INTO RollingStockCars VALUES ('AB', 'Centerbeam Flat', NULL);
INSERT INTO RollingStockAtYards VALUES ('AB', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Lumber', 'E.E. Aldrin Sawmill', 1, 'Half Circle Farms', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AB', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (1, 'AB', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AB';

INSERT INTO RollingStockCars VALUES ('AC', 'Flat Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AC', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Bulk Equipment', 'Kesselring Machine Shop', 1, 'Half Circle Farms', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AC', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (1, 'AC', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AC';

INSERT INTO RollingStockCars VALUES ('AD', 'Box Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AD', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Crates', 'Bauxen Crates', 3, 'Kesselring Machine Shop', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AD', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (1, 'AD', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AD';

INSERT INTO Crews VALUES ('Brett', NULL);
#INSERT INTO TrainCrews VALUES (1, 'Brett', DEFAULT);

#Train 2
INSERT INTO Trains VALUES (2, 5342, 5342, DEFAULT);
INSERT INTO TrainLocations VALUES (2, 'Black River Yard', DEFAULT);

INSERT INTO RollingStockCars VALUES ('AE', 'Long Hopper', NULL);
INSERT INTO RollingStockAtYards VALUES ('AE', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Feed', 'Oatus Elevator', 1, 'Half Circle Farms', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AE', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (2, 'AE', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AE';

INSERT INTO RollingStockCars VALUES ('AF', 'Open Hopper', NULL);
INSERT INTO RollingStockAtYards VALUES ('AF', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Coal', 'Lostry Mine', 1, 'Palin Interchange', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AF', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (2, 'AF', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AF';

INSERT INTO RollingStockCars VALUES ('AG', 'Box Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AG', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Crates', 'Bauxen Crates', 4, 'Cobra Golf', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AG', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (2, 'AG', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AG';

INSERT INTO RollingStockCars VALUES ('AH', 'Reefer', NULL);
INSERT INTO RollingStockAtYards VALUES ('AH', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Dairy', 'Half Circle Farms', 1, 'Sunset Feed', 4, DEFAULT);
INSERT INTO Waybills VALUES ('AH', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (2, 'AH', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AH';

INSERT INTO Crews VALUES ('Thomas', NULL);
#INSERT INTO TrainCrews VALUES (2, 'Thomas', DEFAULT);

#Train 3
INSERT INTO Trains VALUES (3, 116, 1166, DEFAULT);
INSERT INTO TrainLocations VALUES (3, 'Black River Yard', DEFAULT);

INSERT INTO RollingStockCars VALUES ('AI', 'Box Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AI', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Crates', 'Bauxen Crates', 4, 'Max Distributing', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AI', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (3, 'AI', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AI';

INSERT INTO RollingStockCars VALUES ('AJ', 'Stock Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AJ', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Livestock', 'Half Circle Farms', 1, 'Palin Interchange', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AJ', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (3, 'AJ', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AJ';

INSERT INTO RollingStockCars VALUES ('AK', 'Tank Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AK', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'Fuels', 'Pure Oil', 3, 'Half Circle Farms', 1, DEFAULT);
INSERT INTO Waybills VALUES ('AK', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (3, 'AK', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AK';

INSERT INTO RollingStockCars VALUES ('AL', 'Box Car', NULL);
INSERT INTO RollingStockAtYards VALUES ('AL', 'Black River Yard', DEFAULT);
INSERT INTO Shipments VALUES (DEFAULT, 'General Merchandise', 'Puget Warehouse', 1, 'MMI Transfer Site 3', 3, DEFAULT);
INSERT INTO Waybills VALUES ('AL', LAST_INSERT_ID(), 'Black River Yard');
INSERT INTO ConsistedCars VALUES (3, 'AL', DEFAULT);
DELETE FROM RollingStockAtYards WHERE CarID = 'AL';

INSERT INTO Crews VALUES ('Zakk', NULL);
#INSERT INTO TrainCrews VALUES (3, 'Zakk', DEFAULT);