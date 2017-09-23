#include <DHT.h>

/*
 *  This sketch Connects to Wifi
 *  Detects Quickbird APP Broker ON the network
 *  Sends it Data, but instead of generating random data, 
 *  It read Humidity and temperature from DHT22 Sensor
 */
 
#include <PubSubClient.h>
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>


struct Reading
{
  float value;
  //in microseconds
  int32_t duration;
  //Defined in the database
  byte SensorTypeID;
};

/*WIFI STUFF */
const char* ssid     = "QBnet2.4";
const char* password = "asteroidsareamyth";

/* UDP STUFF */
WiFiUDP Udp;
unsigned int localUdpPort = 44000;
const char* UdpBeacon = "sekret";
const int BeaconLength = 6; 
char incomingPacket[255];

/* MQTT STUF */
bool serverFound = false; 
IPAddress serverIP;
const uint16_t MqttPort = 1883;
WiFiClient WifiClient;
PubSubClient _pubSubClient; 
bool mqttConnected; 
/*Globally unique identifier is how the app knowns who is connecting. 
 * It needs to be different for every device. 
 * You can get yourself one at https://guidgenerator.com/online-guid-generator.aspx
 */
char deviceID[48] = "1221e8e7aab2443eb570ffecedd28827"; 

/*Sensor Loop Stuff*/
uint32_t lastSensorTick = 0;
const byte NumberOfReadings =2; 
Reading readings[NumberOfReadings]; 
int DataSize = 18;

#define DHTPIN 3     // what pin we're connected to
#define DHTTYPE DHT22   // DHT 22  (AM2302)
DHT dht(DHTPIN, DHTTYPE); //// Initialize DHT sensor 


void setup() {
  Serial.begin(115200);
  delay(10);

  // We start by connecting to a WiFi network

  Serial.println();
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);
  
  WiFi.begin(ssid, password);
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");  
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());

  //Setup Sensors
  readings[0].SensorTypeID = 5;
  readings[0].duration = 500000; 
  readings[1].SensorTypeID = 6;
  readings[1].duration = 500000; 
  dht.begin();

  //StartUDP
  Udp.begin(localUdpPort);
  _pubSubClient.setServer(serverIP, MqttPort).setClient(WifiClient);
}

int value = 0;

void loop() {
  if(WiFi.status() != WL_CONNECTED) {
    Serial.println("Wifi COnnection Lost! Recconecting! ");
    WiFi.begin(ssid, password);

    while (WiFi.status() != WL_CONNECTED) {
      delay(500);
      Serial.print(".");
    }
  }

  bool serverChange = UDPLoop();
  if(serverChange)
  {
    serverFound = true; 
    _pubSubClient.setServer(serverIP, MqttPort); 
    Serial.printf("Set New MQTT server at %d.%d.%d.%d \n", serverIP[0], serverIP[1], serverIP[2], serverIP[3]);
  }

  if(serverFound && micros() - lastSensorTick > 500000)
  {
    Serial.println("TimeToSend");
    GetSensorReadings();
    ConnectAndSend();
    lastSensorTick = micros();
  }
}

void GetSensorReadings(){
  readings[0].value = dht.readTemperature();
  readings[1].value = dht.readHumidity();
}

void ConnectAndSend()
{
  mqttConnected = _pubSubClient.connected();
  if(mqttConnected == false)
  {
    mqttConnected = _pubSubClient.connect(deviceID);
  }
  if(mqttConnected == false)
  {
    Serial.println("Can't Connect");
    return; //if we still failed to connect, return
  }

  byte dataBuffer[DataSize];
  for (int i = 0; i < NumberOfReadings; i++)
  {
    int b = i * 9; 
    byte* dataPointer = (byte*)&(readings[i]); 
    for(int m = 0; m < 9; m++)
    {
      dataBuffer[b + m] = *(dataPointer + m); 
    }
  }
  
  _pubSubClient.publish("readings/v1/binary", dataBuffer, DataSize); 
  Serial.println("Sent a message");
}

/*This function reads UDP messages and sets the IP Address
 * for Quickbird App Host that it finds on the network
 */bool UDPLoop(){
  int packetSize = Udp.parsePacket();

  if(serverIP == Udp.remoteIP())  
    return false;//Only Execute if the IP address is different

  if(packetSize != BeaconLength)
    return false; //Wrong size, not the beacon

  int len = Udp.read(incomingPacket, 255);
  //Null-termination 
  if (len > 0)
  {
    incomingPacket[len] = 0;
  }

   Serial.printf("Recieved UDP packed that looks like beacon: %s\n", incomingPacket);
   
   bool isBeacon = true; //Check if this is the beacon packet
   for(int i =0; i < BeaconLength; i++)
   {
      if((UdpBeacon[i] == incomingPacket[i]) == false)
      {
        isBeacon = false;
        break; 
      }
   }

   if(isBeacon == false)
      return false; 

   serverIP = Udp.remoteIP(); //Set Correct remote IP
   Serial.printf("Recieved UDP beacon, Remote IP set to: %d.%d.%d.%d\n", serverIP[0],serverIP[1], serverIP[2], serverIP[3]); 
   return true; 
}
