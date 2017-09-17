# Data Model
The data model is a SQL relational one, designed in MySQL Workbench found in [/docs](/docs), 
but just for visualisation purposes. The actual implementation is EntityFramework Code First. 
![DatabaseModel](/docs/DatabaseDiagram.png)
You will find that all the code files are in [/DbStructure](/DbStructure) folder. 
The [dbStructure(Server)](dbStructure(Server)) folder simply contains a library that
actually references files from [/DbStructure](/DbStructure). It is only there because 
Asp.net 4.6 has different references for EFFramework annotations. 

### Shared Global Data — RED
**Subsystem** - Relays and sensors in the end belong to a sybsytem. 
For example we have a Climate control subsystem, that includes relays for controlling 
heaters, temeperature and humidity sensors, etc. 
**SensorType** - The type of sensor we are dealing with, it has two attributes:
* **Placement** - is in on the floor, on the roof, etc? 
* **Parameter** - What is it measuring, temperature, humidity, etc. 
**RelayType** - relays that control different stuff
* CropType - a special dataType that's shared but anyone can create new ones. 

### User Personal Data 
* Devices - each arduino or whatever is a single device
* Sensor - each device can have sensors 
* Relays - the device can also have relays
* Locations - is unused atm, but suppose to mean facilities / buildings / installations

### Time-series data
**SensorHistory** is a time-series record of various readings that were taken by each sensor. 
The readings can be sent to the app at arbitrary frequency (every second, 10 times a second, whatever).
The app will average the data into 1-minute records. The actual Data Type is a little unusual in that it is 
all datapoints taken on a given day are serialised into a single record that is stored in the SQL database 
in a binary format for performance reasons. 
**ControlHistory** is unused

# SyncStrategy
## Pull Order
![DatabaseModel](/docs/PullOrder.png)
Reds first since they are an independent set, and they do not require authentication.
1.	PlacementType
2.	Parameter
3.	Subsystem
4.	Placement_has_Parameter (actually called ParamAtPlace)
5.	ControlTypes
Now for the big part
1.	Greenhouse
2.	CropType
3.	Devices
4.	Cycle
5.	Sensors
6.	Relay-Controllable (Are there any issues in the order here?)
7.	
8.	ControlHistory
9.	SensorData
![DatabaseModel](/docs/SyncStrategy.png)
