/*
 * This is a minimal Demo application, meant to work with WIZnet5200 chip / shield from DFROBOT
 * It will work broadly the same for any arduino with an ethernet shield
 * It requires the W5200 library, found in this same folder. 
 * This code was tested with Arduino IDE 1.8.4
 */

#include <SPI.h>
#include <DhcpV2_0.h>
#include <DnsV2_0.h>
#include <EthernetClientV2_0.h>
#include <EthernetServerV2_0.h>
#include <EthernetUdpV2_0.h>
#include <EthernetV2_0.h>
#include <utilV2_0.h>
#include <PubSubClient.h>

//Constants and variables
byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED }; 
const uint8_t DeviceIP[4] = { 10, 0, 0, 240}; //IP of arduino, replace as appropriate

IPAddress serverIP(10, 0, 0, 2); //IP of the computer running the app, replace as appropriate
const uint16_t MqttPort = 1883;

static const byte rBufferLength = 400; 
char recieveBuffer[rBufferLength];
byte rBufferIndex = 0; 
boolean overflow = false; 


EthernetClient EthClient; 
PubSubClient _pubSubClient; 

/*Globally unique identifier is how the app knowns who is connecting. 
 * It needs to be different for every device. 
 * You can get yourself one at https://guidgenerator.com/online-guid-generator.aspx
 */
char deviceID[48] = "1221e8e7aab2443eb570ffecedd28820"; 


void setup() {
  Serial2.begin(115200);
  
    while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }
  

  /*Start the Ethernet Shield - this is unique to the DFROBOT shield
   * You can reset the shield on bootup, ensuring it;s correct operation. 
   * See https://blog.quickbird.uk/making-iot-contraptions-reliable-b1b8c6f2ff04 
   */
   pinMode(SS, OUTPUT);  
   pinMode(nRST, OUTPUT);
   pinMode(nPWDN, OUTPUT);
   pinMode(nINT, INPUT);
   digitalWrite(nPWDN, LOW);  //enable power
   digitalWrite(nRST, LOW);  //Reset W5200
   delay(100);
   digitalWrite(nRST, HIGH);
   delay(200);       // wait for W5200 to start

   Ethernet.begin(mac)
   _pubSubClient.setServer(serverIP, MqttPort).setClient(EthClient);
}

void loop() {
    bool connected = _pubSubClient.connect(deviceID);
    if(connected == false )
      Serial.println("Can't copnnect to server")
    else
    {
      
    }
    
    Ethernet.maintain();
    _pubSubClient.loop();
    
  

}
