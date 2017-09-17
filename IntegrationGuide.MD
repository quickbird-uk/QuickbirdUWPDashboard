# integration


To Integrate your arduino device with the 

Over the network they must send the following **binary** datatype (Json support coming soon): 
	struct Reading
	{
		float value;
		//in microseconds
		int32_t duration;
		//Defined in the database
		byte SensorTypeID;
	};

What do I mean by binary? Well, this is a 9 byte, in size, struct. Shove it into an array: 

Be carefull, on a 32bit MCU this code might not work out of the box, 32 bit MCU migh actually use 4 bytes for the 
boolean data type in this struct. Mbed framework does that. 


# Sensor Types

| ID |  Parameter  | Range    | Unit | Placement    | Subsystem     |
|----|:-----------:|:--------:|:----:|:------------:|:-------------:|
| 1  | Water level | 0 - 100  | %    | Acid Tank    | Acidity / PH  |
| 2  | Water level | 0 - 100  | %    | Base Tank	| Acidity / PH  |


## Id 1
Water level sensor, 0 - 100%, Acid Tank,	Acidity Subsystem
    "ID": 1,    "SubsystemID": 1,    "PlaceID": 1,    "ParamID": 5
	
## Id 2
Water level sensor, 0 - 100%, Base Tank, Acidity Subsystem
    "ID": 2,    "SubsystemID": 1,    "PlaceID": 2,    "ParamID": 5
	
## Id 3 
Ph Sensor, 1 - 14 (no units), Returning Solution, Acidity Subsystem 
    "ID": 3,     "SubsystemID": 1,    "PlaceID": 6,    "ParamID": 7
	
## Id 4 
Ph Sensor, 1 -14 (no units), Solution Tank, Acidity Subsystem 
    "ID": 4,    "SubsystemID": 1,    "PlaceID": 5,    "ParamID": 7

## Id 5
humidity, 0 - 100%,  Ambient Indoors, Climate
    "ID": 5,    "SubsystemID": 2,    "PlaceID": 10,    "ParamID": 4