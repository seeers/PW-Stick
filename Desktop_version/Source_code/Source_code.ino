
/*
This project is free software; you can redistribute it and/or
 modify it under the terms of the CC BY-NC-SA 3.0 license.
 Please see the included documents for further information.
 
 Commercial use of this project is not allowed
 	
 Contact: seeers@gmx.de
 Desktop version : MatthGyver
 
 Used Arduino Version:
 arduino-1.0.5-r2
 
 Used Libraries:
 https://github.com/DavyLandman/AESLib
 https://github.com/adafruit/Adafruit_NeoPixel
 http://playground.arduino.cc/Code/Password
 */

#include "cButton.h"
#include <EEPROM.h>
#include <Password.h>
#include "EEPROMAnything.h"
#include <Adafruit_NeoPixel.h>
#include <AESLib.h>
#include <Keyboard.h>
//#define DEBUG

#define BUTTON1_PIN               A0   // Button 1
#define BUTTON2_PIN               15   // Button 2
#define BUTTON3_PIN               2   // Button 3
#define BUTTON4_PIN               16   // Button 4
#define BUTTON5_PIN               A3   // Button 5
#define BUTTON6_PIN               14   // Button 6
#define BUTTON7_PIN               A2   // Button 7
#define BUTTON8_PIN               8    // Button 8
#define BUTTON9_PIN               A1   // Button 9
#define BUTTON10_PIN              9    // Button 10

#define LED_PIN                   5    //LED
#define NUM_LED                   3    //Number of LED

#define DEFAULT_LONGPRESS_LEN    25  // Min nr of loops for a long press
#define DELAY                    20  // Delay per loop in ms - orig 20

#define buttoncount				10
//////////////////////////////////////////////////////////////////////////////
//EEPROM
#define Get_All_EEPROM        0x40  
#define WriteACC1             0x41  
#define WriteACC2             0x42   
#define WriteACC3             0x43   
#define WriteACC4             0x44   
#define WriteACC5             0x45
#define WriteACC6             0x46
#define WriteACC7             0x47
#define WriteACC8             0x48
#define WriteACC9             0x49
//#define WriteACC10            0x50
#define LockDevice			  0x33
#define ChangePin             0x34
#define IsReady               0x35
#define ERASE                 0x36
#define GetVersion            0x37
#define LearnIRBtn			  0x38
#define SendKeyDebug		  0x39
#define ReadConfig			  0x60
#define WriteConfig			  0x61
#define GetRawEEProm		  0x66
#define UserDoneByte          0x02
#define PWDoneByte			  0x03
#define EOTByte				  0x04


#define EEGlobalEnter		  960
#define EELockWSTime		  961
#define EELockWS_WIN_L		  962



#define DoneMessage          "þDONE"
#define Version				 "1.4.5.0"
//changed Delays 

// Define LED colours
int LEDGreen[3] = { 0,90,0 };
int LEDOrange[3] = { 50,30,0 };
int LEDBlue[3] = { 0,0,90 };
int LEDRed[3] = { 90,0,0 };
int LEDOff[3] = { 0,0,0 };

enum { 
  EV_NONE=0, EV_SHORTPRESS, EV_LONGPRESS };
//////////////////////////////////////////////////////////////////////////////
// Class definition
class ButtonHandler {
public:
  // Constructor
  ButtonHandler(int pin, int longpress_len=DEFAULT_LONGPRESS_LEN);

  // Initialization done after construction, to permit static instances
  void init();

  // Handler, to be called in the loop()
  int handle();

protected:
  boolean was_pressed;     // previous state
  int pressed_counter;     // press running duration
  const int pin;           // pin to which button is connected
  const int longpress_len; // longpress duration
};

ButtonHandler::ButtonHandler(int p, int lp)
: 
pin(p), longpress_len(lp)
{
}

void ButtonHandler::init()
{
  pinMode(pin, INPUT);
  digitalWrite(pin, HIGH); // pull-up
  was_pressed = false;
  pressed_counter = 0;
}

int ButtonHandler::handle()
{
  int event;
  int now_pressed = !digitalRead(pin);

  if (!now_pressed && was_pressed) {
    // handle release event
    if (pressed_counter < longpress_len)
      event = EV_SHORTPRESS;
    else
      event = EV_LONGPRESS;
  }
  else{
    event = EV_NONE;
  }	


  // update press running duration
  if (now_pressed)
    ++pressed_counter;
  else
    pressed_counter = 0;

  // remember state, and we're done
  was_pressed = now_pressed;

  return event;
}


typedef struct
{
  char User[48];
  char PW[48];
  byte enter;
} 
UserPW_t;



char Pin[5];

uint8_t	    byteCount = 0; // reveived bytes
uint8_t     writeCount = 0;
byte        gRxBuffer[100];   // Receive Buffer
byte		b;
byte		settings[3];



UserPW_t users[buttoncount-1];
Adafruit_NeoPixel rgbled = Adafruit_NeoPixel(NUM_LED, LED_PIN, NEO_GRB + NEO_KHZ800);
byte value;

boolean userdone = false;
boolean pwdone = false;
boolean startwrite = true;
boolean locked = true;
boolean locktimer = false;
boolean shiftmode = false;
boolean GlobalReturn = false;
boolean LockWorkstation = false;
byte	wrongPinCount = 0;
unsigned long lockaftertime = 0;
unsigned long unlocktime = 0;


Password password = Password("0000");
aes_context ctx;
uint8_t iv[] = { 
  0, 163, 83, 45, 67, 234, 203, 41, 145, 68, 58, 72, 85, 13, 94, 87 };
uint8_t key[16] = { 
  64, 26, 2, 3, 42, 5, 0, 0, 8, 9, 0, 0, 12, 89, 0, 15 };

//
//const char passChars[] = {
//	'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E',
//	'F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T',
//	'U','V','W','X','Y','Z','a','b','c','d','e','f','g','h','i',
//	'j','k','l','m','n','o','p','q','r','s','t','u','v','w','x', //last normal as number 62 (idx 61)
//	'y','z','!','"','#','$','%','&','@','?','(',')','[',']','-', // <75 (idx 75)
//	'.',',','+','{','}','_','/','<','>','=','|','\'','\\', 
//	';',':',' ','*'// <- 92 (idx 91)
//};

//////////////////////////////////////////////////////////////////////////////

// Instanciate button objects
ButtonHandler buttons[buttoncount] = {
  ButtonHandler(BUTTON1_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON2_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON3_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON4_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON5_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON6_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON7_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON8_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON9_PIN, DEFAULT_LONGPRESS_LEN),
  ButtonHandler(BUTTON10_PIN, DEFAULT_LONGPRESS_LEN)
  };


  void setup()
  {
    rgbled.begin();
    rgbled.show();
    Serial.begin(9600);
    key[10] = 10;
    for (int i=0; i < buttoncount; i++)
    {
      buttons[i].init();
    }
    key[6] = 45;

    locked = true;
    LEDaction(LEDOrange);
    EEPROM_readAnything(1000, Pin);

    if (isValidNumber(Pin))
      password = Password(Pin);
    else
      password = Password("0000");
    key[14] = 167;
    wrongPinCount = EEPROM.read(950);
    iv[0] = 124;
    key[11] = 181;
#if defined(ENABLEIR) 
#	LoadIRButtons();
#	irrecv.enableIRIn();
#endif

    ReadConfigSettings();
    key[7] = 68;
    //	Serial.print(F("FREEMEM1: ")); Serial.println(freeRam());

  }

void ReadConfigSettings()
{
  settings[0] = EEPROM.read(EEGlobalEnter);
  settings[1] = EEPROM.read(EELockWSTime);
  settings[2] = EEPROM.read(EELockWS_WIN_L);

  if (settings[0] != 0)
    GlobalReturn = true;
  else
    GlobalReturn = false;

  if (settings[1] != 0)
  {		
    locktimer = true;
    lockaftertime = settings[1];
    lockaftertime = lockaftertime * 1000;
    lockaftertime = lockaftertime * 60;
  }

  if (settings[2] != 0)
    LockWorkstation = true;
  else
    LockWorkstation = false;
}


void writeConfigToEEprom()
{

  if (startwrite == true)
  {
    //Serial.println("Clear Pin Object...");
    for (int i = 0; i < 3; i++){
      settings[i] = 0;
    }		
    startwrite = false;
  }

  gRxBuffer[byteCount] = b;
  byteCount++;

  if (b == EOTByte)
  {
    Serial.print("Enter:");
    Serial.print(settings[0]);
    Serial.print("Lock:");
    Serial.print(settings[1]);
    Serial.print("LockWS:");
    Serial.print(settings[2]);

    EEPROM.write(EEGlobalEnter, settings[0]);
    EEPROM.write(EELockWSTime, settings[1]);
    EEPROM.write(EELockWS_WIN_L, settings[2]);

    Serial.print(DoneMessage);
    byteCount = 0;
    unlocktime = millis();
    ReadConfigSettings();
    return;
  }
  //	Serial.print("Config");
  //	Serial.println(writeCount);
  //	Serial.println(b);
  settings[writeCount] = b;
  writeCount++;
}

// LED Functions

void LEDaction( int LEDparam[] ){
  int LEDnum;
  for ( LEDnum = 0; LEDnum < NUM_LED; LEDnum ++){
    rgbled.setPixelColor(LEDnum,LEDparam[0],LEDparam[1],LEDparam[2]);
  }
  rgbled.show();
}

void FlashLED( int initColor[], int flashColor[], int flashDelay, int flashNb){
  int flashCount;

  for ( flashCount = 0; flashCount <= flashNb; flashCount ++){
    LEDaction(LEDOff);
    delay(flashDelay);
    LEDaction(flashColor);
    delay(flashDelay);
  }

  LEDaction(initColor);
}


// Main loop
void loop()
{
  if (locked == true) {

    InputPin();

#ifdef DEBUG
    Serial.print(F("FREEMEM: ")); 
    Serial.println(freeRam ());
#endif

    if(password.evaluate())
    {
      Serial.println("PW OK!");
      if (wrongPinCount != 0)
      {
        wrongPinCount = 0;
        EEPROM.write(950,0);
        Serial.println("Write EEprom wrongPinCnt = 0");
      }
      Serial.flush();
      LEDaction(LEDGreen);
      unlocktime = millis();

      locked = false;
      for (int i=0; i <buttoncount-1; i++)
      {				
        EEPROM_readAnything(i*97,users[i]);
      }

    }
    else
    {
      //WRONG PIN!!!!!!!!
      FlashLED(LEDOrange, LEDRed, 200, 4);
      password.reset();
      wrongPinCount++;

      if (wrongPinCount > 3)
      {
        Serial.println("format EEPROM...");
        FormatEEProm();
        wrongPinCount = 0;
      }
      else
      {
        Serial.print("write EEprom wrongPinCnt = ");
        Serial.println(wrongPinCount);
        EEPROM.write(950, wrongPinCount);
      }

    }

  }
  else
  {  
    if (locktimer == true && millis() - unlocktime > lockaftertime) {
      locked=true;
      LEDaction(LEDOrange);
      password.reset();
    }

    serialstuff();



    for (int i=0; i <buttoncount; i++)
    {
      int event = buttons[i].handle();
      //if (i != 7 && i != 6)
      if (i != 9)
      {
        switch (event) {
        case EV_NONE:
          break;
        case EV_SHORTPRESS:	
          unlocktime = millis();
          PrintUserPW(i);
          FlashLED(LEDGreen, LEDGreen, 50, 2);
          break;
        case EV_LONGPRESS:
          unlocktime = millis();
          Keyboard.print(users[i].PW);
          if ((users[i].enter == true) || (GlobalReturn == true))
            Keyboard.write(10);
          FlashLED(LEDGreen, LEDGreen, 150, 2);
          break;
        }
      }
      else
      {
        if (i = 9){
          if (event == EV_SHORTPRESS)
          {						
            //lockWindows();
            if (LockWorkstation == true)
              lockWindows();
            locked=true;
            LEDaction(LEDOrange);
            password.reset();
          }
        }
      }
    }
    delay(DELAY);

#ifdef DEBUG
    Serial.print(F("FREEMEM: ")); 
    Serial.println(freeRam ());
#endif
  }
}

void PrintUserPW(int i)
{

  EEPROM_readAnything(i * 97, users[i]);
  ctx = aes128_cbc_dec_start(key, iv);
  aes128_cbc_dec_continue(ctx, users[i].PW, 48);
  aes128_cbc_dec_finish(ctx);

  ctx = aes128_cbc_dec_start(key, iv);
  aes128_cbc_dec_continue(ctx, users[i].User, 48);
  aes128_cbc_dec_finish(ctx);

  if (users[i].User[0] != 0x00 && users[i].User[0] != 0x40 && users[i].User[0] != 0x21 && users[i].User[0] != 0x24)
  {
    Keyboard.print(users[i].User);
    Keyboard.write(9);
  }
  if (users[i].User[0] == 0x40) //@
  {
    sendKey(0x52);
    delay(1000);
  }
  if (users[i].User[0] == 0x21) //!
  {
    sendAltCTRLDel();
    delay(1000);
  }
  if (users[i].User[0] == 0x24) //$
  {
    startRun();
    delay(300);
  }

  Keyboard.print(users[i].PW);
  if ((users[i].enter == true) || (users[i].User[0] == 0x40) || (GlobalReturn == true) || (users[i].User[0] == 0x24) || (users[i].User[0] == 0x21))
  {
    Keyboard.write(10);
  }


}


byte colorswitcher = 0;
void InputPin() {          // Eingabe Modus
  //digitalWrite(LED_PIN, HIGH);
  value = 0;
  for(int i = 1; i < 5; i++)
  {   // request 4 digits
    do
    {  
      // warte darauf das Taster gedrückt wird
      serialLockedstuff();
      /*
			rgbled.setPixelColor(0, colorswitcher , 10, 0);
       			rgbled.show();
       			value++;
       
       			if (value == 120)
       				colorswitcher++;
       			
       			//if (value == 250)
       			//	colorswitcher--;
       			*/
      delay(100);
    }

    while ((digitalRead(BUTTON1_PIN) == HIGH) && (digitalRead(BUTTON2_PIN) == HIGH) && (digitalRead(BUTTON3_PIN) == HIGH) && (digitalRead(BUTTON4_PIN) == HIGH)
      && (digitalRead(BUTTON5_PIN) == HIGH) && (digitalRead(BUTTON6_PIN) == HIGH) && (digitalRead(BUTTON7_PIN) == HIGH) && (digitalRead(BUTTON8_PIN) == HIGH)
      && (digitalRead(BUTTON9_PIN) == HIGH) && (digitalRead(BUTTON10_PIN) == HIGH));


    if(digitalRead(BUTTON1_PIN) == LOW){ // Abfrage welcher Taster gedrückt wurde
      while (digitalRead(BUTTON1_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('1');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    }
    else if(digitalRead(BUTTON2_PIN) == LOW){
      while (digitalRead(BUTTON2_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('2');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    }
    else if(digitalRead(BUTTON3_PIN) == LOW){
      while (digitalRead(BUTTON3_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('3');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    }
    else if(digitalRead(BUTTON4_PIN) == LOW){
      while (digitalRead(BUTTON4_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('4');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    } 
    else if(digitalRead(BUTTON5_PIN) == LOW){
      while (digitalRead(BUTTON5_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('5');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    } 
    else if(digitalRead(BUTTON6_PIN) == LOW){
      while (digitalRead(BUTTON6_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('6');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    } 
    else if(digitalRead(BUTTON7_PIN) == LOW){
      while (digitalRead(BUTTON7_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('7');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    } 
    else if (digitalRead(BUTTON8_PIN) == LOW){
      while (digitalRead(BUTTON8_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('8');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    }
    else if (digitalRead(BUTTON9_PIN) == LOW){
      while (digitalRead(BUTTON9_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('9');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    }
    else if (digitalRead(BUTTON10_PIN) == LOW){
      while (digitalRead(BUTTON10_PIN) == LOW)
      {
        __asm__("nop\n\t");
      }
      password.append('0');
      FlashLED(LEDOrange, LEDOrange, 50, 1);
    }
  }

}

void serialLockedstuff() 
{

  while (Serial.available())
  {
    b = Serial.read();

    if ((byteCount == 0) && (b == 0xFE))   gRxBuffer[byteCount++] = b;  // first header byte 
    else if ((byteCount == 1) && (b == 0xFE))   gRxBuffer[byteCount++] = b;  // 2nd header byte
    else if (byteCount == 2)
    {
      writeCount = 0;
      userdone = false;
      pwdone = false;
      startwrite = true;
      if ((b == IsReady) || (b == ERASE) || (b == GetVersion) || (b == LearnIRBtn) || (b == SendKeyDebug))
      {
        gRxBuffer[byteCount++] = b;
      }
      else
      {
        Serial.print("locked...");
        Serial.print(DoneMessage);
        Serial.flush();
        byteCount = 0;
      }
    }
    else if (byteCount > 2)
    {
      switch (gRxBuffer[2])
      {
      case IsReady:
        Serial.print("locked...");
        Serial.print(DoneMessage);
        byteCount = 0;
        break;
      case ERASE:
        FormatEEProm();
        byteCount = 0;
        break;
      case GetVersion:
        printVersion();
        byteCount = 0;
        break;
      case SendKeyDebug:
        Serial.print("Sending HID: ");
        Serial.println(b,HEX);
        Keyboard.pressold(b);
        Keyboard.pressold('l');
        delay(100);
        Keyboard.releaseAll();
        byteCount = 0;
        break;
      default:
        Serial.print("locked...");
        Serial.print(DoneMessage);
        Serial.flush();
        byteCount = 0;

      }

    }

  }

}

void FormatEEProm()
{
  byte doflash = 0; 
  for (int i = 873 ; i < 1024; i++)
  {
    EEPROM.write(i, 0);
    doflash++; 
    if (doflash == 10)
    {
      LEDaction(LEDBlue);
      rgbled.show();

    }
    if (doflash == 20)
    {
      LEDaction(LEDOff);
      rgbled.show();
      doflash = 0;
    }

  }


  //Encrypt Zero Values...
  doflash = 0;
  for (int i = 0; i <buttoncount - 1; i++)
  {
    for (int z = 0; z < 47; z++){
      users[i].PW[z] = 0;
      users[i].User[z] = 0;
      //doflash++;


    }

    //rgbled.setPixelColor(0, 0, 5 * i, 150 - (i * 10));
    ctx = aes128_cbc_enc_start(key, iv);
    aes128_cbc_enc_continue(ctx, users[i].PW, 48);
    aes128_cbc_enc_finish(ctx);



    ctx = aes128_cbc_enc_start(key, iv);
    aes128_cbc_enc_continue(ctx, users[i].User, 48);
    aes128_cbc_enc_finish(ctx);
    LEDaction(LEDBlue);
    rgbled.show();

    EEPROM_writeAnything(i * 97, users[i]);

    LEDaction(LEDOff);
    rgbled.show();
    delay(20);

  }


  Serial.print("EraseDone");
  Serial.print(DoneMessage);
  password = Password("0000");

  locked = true;
  LEDaction(LEDOrange);
  password.reset();
  byteCount = 0;
}




void writeUserToEEprom(int id) 
{
  if (startwrite == true)
  {
    //Serial.println("Clear Object...");
    for (int i=0; i <= 47; i++){
      users[id].User[i]=0;
      users[id].PW[i] =0;
    } 
    users[id].enter = false; 
    startwrite =false;
  }	

  gRxBuffer[byteCount]=b;
  byteCount++;

  if (b==UserDoneByte) 
  {
    userdone = true; 
    writeCount = 0;
    /*Serial.print("User ");
     		Serial.print(id+1);
     		Serial.println("Done");*/
    return;
  }

  if (b == PWDoneByte)
  {
    pwdone = true; 
    //Serial.println("PW1Done");
    return;
  }

  if (b == EOTByte)
  {
    Serial.print("WriteUser");
    Serial.print(id+1);
    Serial.print("Name:");
    Serial.print(users[id].User);
    Serial.print(DoneMessage);

    ctx = aes128_cbc_enc_start(key, iv);
    aes128_cbc_enc_continue(ctx, users[id].PW, 48);
    aes128_cbc_enc_finish(ctx);
    ctx = aes128_cbc_enc_start(key, iv);
    aes128_cbc_enc_continue(ctx, users[id].User, 48);
    aes128_cbc_enc_finish(ctx);

    EEPROM_writeAnything(id*97,users[id]);
    byteCount=0;
    return;
  }

  if (userdone == false)
  {					
    users[id].User[writeCount] = b;
    writeCount++;
    return;
  }
  else if (pwdone == false)
  {					
    users[id].PW[writeCount] = b;
    writeCount++;
    return;
  }
  else
  {
    if (b == 1) 
      users[id].enter = true;
    else
      users[id].enter = false;
    return;
  }		

}


void writePinToEEprom()
{
  if (startwrite == true)
  {
    //Serial.println("Clear Pin Object...");
    for (int i = 0; i < 4; i++){
      Pin[i] = '0';
    }
    Pin[4] = '\0';
    startwrite = false;
  }

  gRxBuffer[byteCount] = b;
  byteCount++;

  if (b == EOTByte)
  {
    Serial.print("NEWPIN:");
    Serial.print(Pin);
    EEPROM_writeAnything(1000, Pin);
    Serial.print(DoneMessage);
    password = Password(Pin);
    byteCount = 0;
    return;
  }

  Pin[writeCount] = b;
  writeCount++;
}

void serialstuff() {

  while (Serial.available()) 
  {
    b = Serial.read();  

    if      ( (byteCount==0) && (b==0xFE))   gRxBuffer[byteCount++]=b;  // first header byte 
    else if ( (byteCount==1) && (b==0xFE))   gRxBuffer[byteCount++]=b;  // 2nd header byte
    else if (  byteCount==2)
    {
      writeCount = 0;
      //pwCount = 0;
      userdone = false;
      pwdone = false;
      startwrite = true;
      //Unsauber ---> Aendern!!
      if ((b == WriteACC2) || (b == Get_All_EEPROM) || (b == WriteACC1) || (b == WriteACC3) 
        || (b == WriteACC4) || (b == WriteACC5) || (b == WriteACC6) || (b == WriteACC7) 
        || (b == WriteACC8) || (b == WriteACC9) || (b == LockDevice)|| (b == ChangePin) 
        || (b == IsReady) || (b == ERASE) || (b == GetVersion) 	|| (b == WriteConfig)
        || (b == ReadConfig) || (b == GetRawEEProm))
        gRxBuffer[byteCount++]=b; 
      else
        byteCount=0;
    }
    else if (byteCount>2)
    {
      switch (gRxBuffer[2])
      {

      case Get_All_EEPROM:
        for (int i=0; i < buttoncount-1; i++)
        {
          EEPROM_readAnything(i*97,users[i]);

          ctx = aes128_cbc_dec_start(key, iv);
          aes128_cbc_dec_continue(ctx, users[i].PW, 48);
          aes128_cbc_dec_finish(ctx);
          ctx = aes128_cbc_dec_start(key, iv);
          aes128_cbc_dec_continue(ctx, users[i].User, 48);
          aes128_cbc_dec_finish(ctx);

          Serial.print(users[i].User);
          Serial.write(0x86);
          Serial.print(users[i].PW);
          Serial.write(0x86);
          Serial.print(users[i].enter);
          Serial.write(0x87);
        } 
        Serial.write(13);
        Serial.print(DoneMessage);
        byteCount=0;            
        break;

      case LockDevice:
        locked=true;
        LEDaction(LEDOrange);
        password.reset();
        byteCount=0;  
        break;
      case WriteACC1:		
        writeUserToEEprom(0);
        break;
      case WriteACC2:            
        writeUserToEEprom(1);
        break;
      case WriteACC3:
        writeUserToEEprom(2);
        break;
      case WriteACC4:    
        writeUserToEEprom(3);
        break;
      case WriteACC5:    
        writeUserToEEprom(4);
        break;
      case WriteACC6:    
        writeUserToEEprom(5);
        break;
      case WriteACC7:    
        writeUserToEEprom(6);
        break;
      case WriteACC8:    
        writeUserToEEprom(7);
        break;
      case WriteACC9:
        writeUserToEEprom(8);
        break;
      case ChangePin:   
        writePinToEEprom();
        break;
      case IsReady:
        Serial.print("OK");
        Serial.print(DoneMessage);
        byteCount = 0;
        break;
      case ERASE:
        FormatEEProm();
        byteCount = 0;
        break;
      case GetVersion:
        printVersion();
        byteCount = 0;
        break;
      case ReadConfig:
        printSettings();
        byteCount = 0;
        break;
      case WriteConfig:
        writeConfigToEEprom();
        break;
      case GetRawEEProm:
        SendRawEEPROM();
        break;
      default:
        byteCount=0;  
      }  
    } 


  }

}

void SendRawEEPROM()
{
  for (int i = 0; i < 1023; i++)
  {
    value = EEPROM.read(i);
    Serial.print(i);
    Serial.print("\t");
    Serial.print(value, HEX);
    Serial.println();
  }
}

void printSettings()
{
  settings[0] = EEPROM.read(EEGlobalEnter);
  Serial.print(settings[0]);
  Serial.write(0x87);
  settings[1] = EEPROM.read(EELockWSTime);
  Serial.print(settings[1]);
  Serial.write(0x87);
  settings[2] = EEPROM.read(EELockWS_WIN_L);
  Serial.print(settings[2]);
  Serial.write(0x87);

  Serial.print(DoneMessage);
}

void printVersion()
{
  Serial.print("Version");
  Serial.print(Version);
  Serial.print(DoneMessage);
}

void sendAltCTRLDel()
{
  Keyboard.pressold(KEY_LEFT_CTRL);
  Keyboard.pressold(0x82);
  Keyboard.pressold(KEY_DELETE);
  delay(100);
  Keyboard.releaseAll();
}

void lockWindows()
{	
  Keyboard.pressold(0x83);
  Keyboard.pressold('l');
  delay(100);
  Keyboard.releaseAll();
}

void startRun()
{
  Keyboard.pressold(0x83);
  Keyboard.pressold('r');
  delay(100);
  Keyboard.releaseAll();
}


//Fuer spezielle Zeichen und Keyboard Befehle.....
void sendKey(byte key)
{
  KeyReport report = {
    0  };  // Create an empty KeyReport

  /* First send a report with the keys and modifiers pressed */
  report.keys[0] = key;  // set the KeyReport to key
  report.modifiers =  0x00;  // set the KeyReport's modifiers
  report.reserved = 1;
  Keyboard.sendReport(&report);  // send the KeyReport

  /* Now we've got to send a report with nothing pressed */
  for (int i=0; i<6; i++)
    report.keys[i] = 0;  // clear out the keys
  report.modifiers = 0x00;  // clear out the modifires
  report.reserved = 0;
  Keyboard.sendReport(&report);  // send the empty key report
}

boolean isValidNumber(String str){
  for (byte i = 0; i<str.length(); i++)
  {
    if (isDigit(str.charAt(i))) return true;
  }
  return false;
}

int freeRam () {
  extern int __heap_start, *__brkval;
  int v;
  return (int) &v - (__brkval == 0 ? (int) &__heap_start : (int) __brkval);
}


void print_event(const char* button_name, int event)
{
  if (event)
    Serial.print(button_name);
  Serial.print(".SL"[event]);
}

