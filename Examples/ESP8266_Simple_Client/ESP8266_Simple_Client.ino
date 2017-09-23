/*
 *  This sketch sends data via HTTP GET requests to data.sparkfun.com service.
 *
 *  You need to get streamId and privateKey at data.sparkfun.com and paste them
 *  below. Or just customize this script to talk to other HTTP servers.
 *
 */

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>


/*WIFI STUFF */
const char* ssid     = "Enter_Wifi_name";
const char* password = "Enter_wifi_Password";

/* UDP STUFF */
WiFiUDP Udp;
unsigned int localUdpPort = 44000;
const char* UdpBeacon = "sekret";
const int BeaconLength = 6; 
char incomingPacket[255];

/* MQTT STUF */
bool serverFound = false; 
IPAddress serverIP;
const uint16_t port = 1883;




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

  //StartUDP
  Udp.begin(localUdpPort);
}

int value = 0;

void loop() {
    serverFound = UDPLoop();

    if(serverFound)
    {
      
    }
}

bool UDPLoop(){
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
}

