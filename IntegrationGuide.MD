# Integration
![TypicalMess](/Images/TypicalMess.JPG)
At the moment the app only supports local network communication over MQTT. 
It may not sound like much, but that actually covers a huge amount of custom industrial hardware
* Arduinos 
* Other microcontrollers with build in Ethernet
* ESP8266 - Community Favourite
* MCU-based PLCs



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
Sensor Types are defined by the database - if your device sends invalid sensor type, the 
data will not be accepted. The table below sumamrises sensor types. You only need the ID 

| ID |  Parameter  | Range   	| Unit | Placement          	| Subsystem     |
|----|:-----------:|:----------:|:----:|:----------------------:|:-------------:|
| 1  | Water level | 0 - 100  	| %    | Acid Tank          	| Acidity / PH  |
| 2  | Water level | 0 - 100  	| %    | Base Tank	      		| Acidity / PH  |
| 3  | Ph Sensor   | 0 - 14   	|      | Returning Solution 	| Acidity / PH  |
| 4  | Ph Sensor   | 0 - 14   	|      | Solution Tank  	  	| Acidity / PH  |
| 5  | Humidity	   | 0 - 100  	| %    | Ambient Indoors		| Climate		|
| 6  | Temperature |  N/A	  	| ℃   | Ambient Indoors			| Climate	    |
| 7  | Temperature |  N/A 	  	| ℃   | Returning Solution		| Climate	    |
| 8  | Temperature |  N/A 	  	| ℃   | Solution Tank			| Cliamte	    |
| 9  | Temperature |  N/A 	  	| ℃   | Substrate				| Climate       |
| 10 | CO₂ 		   | 0 - 10,000	| ‰    | Ambient Indoors		| CO₂		    |
| 11 | Light	   | 0 - 150	| Klx  | Ambient Indoors		| Light 	    |
| 12 | Conductivity| 0 - 10  	| mS/m | Returning Solution		| Nutrient	    |
| 13 | Conductivity| 0 - 10  	| mS/m | Solution Tank			| Nutrient      |
| 14 | Water level | 0 - 100  	| %    | Nutrient Tank			| Nutrient      |
| 15 | O₂		   | ?       	| ‰    | Solution Tank			| Oxygen        |
| 16 | Water Flow  | 0 - 100  	| L/m  | Water Inlet			| Water         |
| 17 | Humidity    | 0 - 100  	| %    | Substrate				| Water         |
| 18 | Light	   |  0 - 150	| Klx  | Solution Tank			| Water         |
| 19 | Water level | 0 - 100  	| %    | Solution Tank			| Water         |

## Raw data, in JSON for convenience
* [SensorTypesTable](/docs/SensorTypesTable.json)
* [SubsystemsTable](/docs/SubsystemsTable.json)
* [PlacementTable](/docs/PlacementTable.json)
* [ParametersTable](/docs/ParametersTable.json)
* 