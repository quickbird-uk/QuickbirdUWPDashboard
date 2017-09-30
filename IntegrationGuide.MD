# Integration
![TypicalMess](/Images/EquipmentOverview.JPG)
At the moment the app only supports local network communication over MQTT. 
It may not sound like much, but that actually covers a huge amount of custom industrial hardware
* Arduinos 
* Other microcontrollers with build in Ethernet
* ESP8266 - Community Favourite
* MCU-based PLCs



To Integrate your arduino device with the 

Over the network they must send the following **binary** datatype (Json support coming soon): 
```C
	struct Reading
	{
		float value;
		//in microseconds
		int32_t duration;
		//Defined in the database
		byte SensorTypeID;
	};
```

What do I mean by binary? Well, this is a 9 byte, in size, struct. Shove it into an array: 

Be carefull, on a 32bit MCU this code might not work out of the box, 32 bit MCU migh actually use 4 bytes for the 
boolean data type in this struct. Mbed framework does that. 

## MQTT API - planned
Note that at this point permissions are not yet implemented. 
The server operates as a normal MQTT server, with the following topics being reserved:
* `services` - services provided by the server, the clients are only allowed to talk to the server through this topic. Any messages published to `services/` topic will not be propagated to other clients. There is no point subscribing to this topic.
* `devices` - this potic is reserved for mesages that are spesifically reserved for your device. Your device is identified by `devices/{your_device_id}`. Nobody else can subscribe to this topic. It has two sub-topics:
	* `/fromserver` - messages that come from here can be trusted
	* `/fromnetwork` - anyone can publish messages here, and you may or may not want to subscribe to them. 
* `readings` - This area is used to submit readings that are then displayed on the dashboard. Note that it is up to you how you wish to structure subtopics - you could have `readings/livingroom/couch` if you wanted. The server only cares about two things:
	* Proper data format is used, otherwise the readings will be rejected as invalid and won't be propagated furhter, to other clients. 
	* The data readings must have correct `DeviceID` so that other devices can know where the data came from. Otherwise it's impossible to trust any readings that come in. 
	
### Details
* **Registration** - The server will allow a client to connect without a password only if it has an ID - Guid that is all zeroes. Otherwise you need a password to connect. The password is a set of random 32 human-readable characters. At that point your client should disconnect, and re-connect using that ID and Password. `devices/{your_device_id}/fromserver/registration`
* **Commands** - `devices/{your_device_id}/fromserver/commands` To recieve commands, your device must be subscribed to the topic . You will not be allowed to subscribe to a topic that does not belong to your device. Only the server will be allowed to publish commands to you, so you can consider them trusted. 
* **Time Service** - `services/time` - subscribe to recive time in UTC, as a binary in Uint32 format. These are broadcast every second. Unsubscribe as needed. 
* **Guid** - `services/guid` - publishing to this channel will cause the app to generate a GUID and send it to your device at `devices/{your_device_id}/fromserver/guid`. You should send an empty publish message, any contents of this message will be ignored. 
* **Presence** - `services/presence` - publishing to this channel will cause the server to send a list of currently connected clients to `devices/{your_device_id}/fromserver/presence`. Your message doesn't need to have any content. 

## Arduino-Ethernet

## ESP8266-Arduino Core

Watch out - the mappings on those boards are not the standard ones.
Furthermore, 3 pins cannot be used because the chip itself uses them for SPI
http://linksprite.com/wiki/index.php5?title=LinkNode_D1

### Pin Map: 
| Arduino-Pins | ESP8266 Pins |
|--------------|--------------|
| D0		   |  GPIO3       |
| D1           |  GPIO1       |
| D2           |  GPIO16      |
| D3		   |  GPIO5       |
| D4           |  GPIO4       |
| D5           |  GPIO14      |
| D6		   |  GPIO12      |
| D7           |  GPIO13      |
| D8           |  GPIO0       |
| D9		   |  GPIO2       |
| D10          |  GPIO15      |
| D11		   |  GPIO13      |
| D13          |  GPIO12      |
| D13		   |  GPIO14      |
| D14          |  GPIO4       |
| D15          |  GPIO5       |


API Docs
http://arduino-esp8266.readthedocs.io/en/latest/esp8266wifi/generic-examples.html



## Sensor Types
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

### Raw data, in JSON for convenience
* [SensorTypesTable](/docs/SensorTypesTable.json)
* [SubsystemsTable](/docs/SubsystemsTable.json)
* [PlacementTable](/docs/PlacementTable.json)
* [ParametersTable](/docs/ParametersTable.json)
