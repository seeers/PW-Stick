
/*
This project is free software; you can redistribute it and/or
 modify it under the terms of the CC BY-NC-SA 3.0 license.
 Please see the included documents for further information.
 
 Commercial use of this project is not allowed
 	
 Contact: seeers@gmx.de
 Desktop & Touch version : MatthGyver
  
 You need an old Arduino IDE because the keyboard emulation was different.
 With the recent Arduino, keyboard is'nt into the core and all library that
 I've tested don't support extended ASCII so don't work well for other langage that US.
 Original core code was modified by Seers to support them but only work with old Arduino vesion.

 Used Libraries:
 https://github.com/DavyLandman/AESLib
 https://github.com/adafruit/Adafruit_NeoPixel
 http://playground.arduino.cc/Code/Password
 https://bitbucket.org/teckel12/arduino-toneac/wiki/Home
 https://github.com/adafruit/Adafruit_MPR121


Buzzer connection like in the library documentation : pin 9 & 10
Keypad connection is in 2IC so :
  - SDA = Pin digital 2
  - SCL = Pin digital 3
  - IRQ = Not connected
 */

#include "cButton.h"
#include "Wire.h"
#include <EEPROM.h>
#include <Password.h>
#include "EEPROMAnything.h"
#include <Adafruit_NeoPixel.h>
#include <AESLib.h>
#include "Adafruit_MPR121.h"
#include <toneAC.h>

// #define DEBUG

// If you want that the device is unlock at start (usefull for tests)
#define LOCKED  'true'

// If DEVMODE = true, AES key is'nt regenerated so stored data is reachable after new burn
#define DEVMODE 'false'

// ----------------------------------------------------------------------------
// Set keyboard layout
// ----------------------------------------------------------------------------

// #define KEYBOARD_LAYOUT 'US'
#define KEYBOARD_LAYOUT 'FR'

#if KEYBOARD_LAYOUT == 'FR'
#include <KeyboardFR.h>
#else
#include <Keyboard.h>
#endif
// ----------------------------------------------------------------------------

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
#define Version				 "2.0"

// Define LED colours
uint8_t LEDGreen[3] = { 0,90,0 };
uint8_t LEDOrange[3] = { 50,30,0 };
uint8_t LEDBlue[3] = { 0,0,90 };
uint8_t LEDRed[3] = { 90,0,0 };
uint8_t LEDOff[3] = { 0,0,0 };
// uint8_t LockLEDcolor[3] = { 0,0,0 };
uint8_t LockLEDcolor[3] = { 50,30,0 };

// Define sound On/Off
boolean soundOn = true;
int soundVolume = 5;

// Mapping keyboard touch (not usable for pin code but simplify code in main loop)
int keyMap[12] = { 1,4,7,100,2,5,8,0,3,6,9,200 };

// You can have up to 4 on one i2c bus but one is enough for testing!
Adafruit_MPR121 cap = Adafruit_MPR121();

int was_pressed = 0;
int now_pressed = 0;
uint8_t longPressCount = 0;

// Delay between print keyboard char (in ms)
int keyboardDelay = 30;

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

uint8_t pressed_counter = 0;
uint8_t longpress_len = 25;

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

// If DEVMODE = false, the key is generated randomly
# if DEVMODE == 'true'
uint8_t iv[] = { 124, 163, 83, 45, 67, 234, 203, 41, 145, 68, 58, 72, 85, 13, 94, 87 };
uint8_t key[16] = { 64, 26, 2, 3, 42, 5, 112, 84, 8, 9, 234, 198, 12, 89, 137, 15 };
#else
uint8_t i1  = rand() %256;
uint8_t i2  = rand() %256;
uint8_t i3  = rand() %256;
uint8_t i4  = rand() %256;
uint8_t i5  = rand() %256;
uint8_t i6  = rand() %256;
uint8_t i7  = rand() %256;
uint8_t i8  = rand() %256;
uint8_t i9  = rand() %256;
uint8_t i10 = rand() %256;
uint8_t i11 = rand() %256;
uint8_t i12 = rand() %256;
uint8_t i13 = rand() %256;
uint8_t i14 = rand() %256;
uint8_t i15 = rand() %256;
uint8_t i16 = rand() %256;

uint8_t iv[] = { i1,i2,i3,i4,i5,i6,i7,i8,i9,i10,i11,i12,i13,i14,i15,i16 };

uint8_t k1  = rand() %256;
uint8_t k2  = rand() %256;
uint8_t k3  = rand() %256;
uint8_t k4  = rand() %256;
uint8_t k5  = rand() %256;
uint8_t k6  = rand() %256;
uint8_t k7  = rand() %256;
uint8_t k8  = rand() %256;
uint8_t k9  = rand() %256;
uint8_t k10 = rand() %256;
uint8_t k11 = rand() %256;
uint8_t k12 = rand() %256;
uint8_t k13 = rand() %256;
uint8_t k14 = rand() %256;
uint8_t k15 = rand() %256;
uint8_t k16 = rand() %256;

uint8_t key[16] = { k1,k2,k3,k4,k5,k6,k7,k8,k9,k10,k11,k12,k13,k14,k15,k16 };
#endif

// ----------------------------------------------------------------------------
// Setup
// ----------------------------------------------------------------------------
void setup()
{ 
  Keyboard.begin();
  rgbled.begin();
  rgbled.show();
  
#if LOCKED == 'false'
  locked = false;
#else
  locked = true;
#endif

  LEDcolor(LockLEDcolor);
  EEPROM_readAnything(1000, Pin);

  if (isValidNumber(Pin))
    password = Password(Pin);
  else
    password = Password("0000");

  wrongPinCount = EEPROM.read(950);

  ReadConfigSettings();

  Serial.begin(9600);
  
  cap.begin(0x5A);
}

// ----------------------------------------------------------------------------
// Read configuration function
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// Write configuration function
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// Play sound function. Used when long key press
// ----------------------------------------------------------------------------
void long_key_sound(){
  toneAC(4500,soundVolume,100);
}

// ----------------------------------------------------------------------------
// Play sound function. Used when short key press
// ----------------------------------------------------------------------------
void short_key_sound(){
  toneAC(1700,soundVolume,100);
}

// ----------------------------------------------------------------------------
// Play sound function. Used when pin code is wrong
// ----------------------------------------------------------------------------
void wrong_sound(){
  toneAC(300,soundVolume,200);
  toneAC(150,soundVolume,200);
}

// ----------------------------------------------------------------------------
// Play sound function. Used when pin code is OK and device unlock
// ----------------------------------------------------------------------------
void unlock_sound(){
  toneAC(3000,soundVolume,100);
  toneAC(3500,soundVolume,100);
}

// ----------------------------------------------------------------------------
// Play sound function. Used when lock device
// ----------------------------------------------------------------------------
void lock_sound(){
  toneAC(3500,soundVolume,100);
  toneAC(3000,soundVolume,100);
}

// ----------------------------------------------------------------------------
// LED function. Set same color for all LEDs
// ----------------------------------------------------------------------------
void LEDcolor( uint8_t LEDparam[] ){
  uint8_t LEDnum;
  for ( LEDnum = 0; LEDnum < NUM_LED; LEDnum ++){
    rgbled.setPixelColor(LEDnum,LEDparam[0],LEDparam[1],LEDparam[2]);
  }
  rgbled.show();
}

// ----------------------------------------------------------------------------
// LED function. Blink LED. Alternate LED color and LED off
// ----------------------------------------------------------------------------
void FlashLED( uint8_t initColor[], uint8_t flashColor[], uint8_t flashDelay, uint8_t flashNb){
  uint8_t flashCount;

  for ( flashCount = 0; flashCount <= flashNb; flashCount ++){
    LEDcolor(LEDOff);
    delay(flashDelay);
    LEDcolor(flashColor);
    delay(flashDelay);
  }

  LEDcolor(initColor);
}

// ----------------------------------------------------------------------------
// Main loop
// ----------------------------------------------------------------------------
void loop()
{
  if (locked == true) {

    InputPin();

    if(password.evaluate())
    {
      Serial.println("PW OK!");
      if ( soundOn == true ) { unlock_sound(); }
      if (wrongPinCount != 0)
      {
        wrongPinCount = 0;
        EEPROM.write(950,0);
        Serial.println("Write EEprom wrongPinCnt = 0");
      }
      Serial.flush();
      LEDcolor(LEDGreen);

      unlocktime = millis();
      
      locked = false;
      for (int i=0; i < buttoncount-1; i++)
      {				
        EEPROM_readAnything(i*97,users[i]);
      }
      was_pressed = 0;
    }
    else
    {
      //WRONG PIN!!!!!!!!
      if ( soundOn == true ) { wrong_sound(); }
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
      password.reset();
      if ( soundOn == true ) { lock_sound(); }
      LEDcolor(LockLEDcolor);      
    }

    serialstuff();
    // **************************
    now_pressed = cap.touched();
    for (uint8_t i = 0; i < 12; i ++) {
      if ( now_pressed & _BV(3) || now_pressed & _BV(7) || now_pressed & _BV(11) ){
        if (LockWorkstation == true){ lockWindows() ; }
        locked=true;
        LEDcolor(LockLEDcolor);
        if ( soundOn == true ) { lock_sound(); }
        password.reset();
        break;
      }else{
        if ( ( !(now_pressed & _BV(i) ) && (was_pressed & _BV(i) ) ) && ( pressed_counter < longpress_len ) ){
          if ( soundOn == true ) { short_key_sound(); }
          unlocktime = millis();
          PrintUserPW(keyMap[i]);
          FlashLED(LEDGreen, LEDGreen, 50, 2);
        }else if ( (now_pressed & _BV(i)) && (pressed_counter == longpress_len ) ){
          if ( soundOn == true ) { long_key_sound(); }
          FlashLED(LEDGreen, LEDGreen, 100, 3);
          unlocktime = millis();
          PrintPW(keyMap[i]);
        }
      }
    }

    if ( now_pressed ){
      pressed_counter ++;
    }else{
      pressed_counter = 0;
    }
    
    was_pressed = now_pressed;

    delay(DELAY);

#ifdef DEBUG
    Serial.print(F("FREEMEM: ")); 
    Serial.println(freeRam ());
#endif
  }
}

// ----------------------------------------------------------------------------
// Function to evaluate PIN code
// ---------------------------------------------------------------------------- 
void InputPin() {
  value = 0;
  int i = 0;
  while ( i < 4){   // request 4 digits
    do {
      serialLockedstuff();
      delay(100);
    }
    while ( ! (cap.touched()) );
    
    now_pressed = cap.touched();
    for (uint8_t k = 0; k < 12; k ++) {
      if ( now_pressed & _BV(k) ) {      
        if ( soundOn == true ) { short_key_sound(); }
        FlashLED(LEDOrange, LEDOrange, 50, 1);
        switch(k){
          case 0 :
            password.append('1');
            break;
          case 1 :
            password.append('4');
            break;
          case 2 :
            password.append('7');
            break;
          case 4 :
            password.append('2');
            break;
          case 5 :
            password.append('5');
            break;
          case 6 :
            password.append('8');
            break;
          case 7 :
            password.append('0');
            break;
          case 8 :
            password.append('3');
            break;
          case 9 :
            password.append('6');
            break;
          case 10 :
            password.append('9');
            break;
          default :
            password.append('a');
        }
        i ++;
      }
    }
  }
}

// ----------------------------------------------------------------------------
// Function for serial messages when device is locked
// ----------------------------------------------------------------------------
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
        Keyboard.press(b);
        Keyboard.press('l');
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

// ----------------------------------------------------------------------------
// Function to tap login/password char by char with delay between them
// ----------------------------------------------------------------------------
void keyboardModifier( char *stringToModify ){
  for ( int i = 0; i < strlen(stringToModify); i++ ){
    Keyboard.print(stringToModify[i]);
    delay(keyboardDelay);
  }
}

// ----------------------------------------------------------------------------
// Function to tap password only
// ----------------------------------------------------------------------------
void PrintPW(int i)
{
  // i - 1 for mapping key / password entry match
  i = i - 1;

  EEPROM_readAnything(i * 97, users[i]);
  ctx = aes128_cbc_dec_start(key, iv);
  aes128_cbc_dec_continue(ctx, users[i].PW, 48);
  aes128_cbc_dec_finish(ctx);

  ctx = aes128_cbc_dec_start(key, iv);
  aes128_cbc_dec_continue(ctx, users[i].User, 48);
  aes128_cbc_dec_finish(ctx);


  keyboardModifier(users[i].PW);

  if ((users[i].enter == true) || (GlobalReturn == true)){
      Keyboard.write(10);
  }
  
}

// ----------------------------------------------------------------------------
// Function to tap login, tabulation and password
// ----------------------------------------------------------------------------
void PrintUserPW(int i)
{
  // i - 1 for mapping key / password entry match
  i = i - 1;

#ifdef DEBUG
  Serial.print("encrypted:");
  Serial.println(users[i].PW);
  Serial.println(users[i].User);
#endif

  EEPROM_readAnything(i * 97, users[i]);
  ctx = aes128_cbc_dec_start(key, iv);
  aes128_cbc_dec_continue(ctx, users[i].PW, 48);
  aes128_cbc_dec_finish(ctx);

  ctx = aes128_cbc_dec_start(key, iv);
  aes128_cbc_dec_continue(ctx, users[i].User, 48);
  aes128_cbc_dec_finish(ctx);

#ifdef DEBUG
  Serial.print("decrypted:");
  Serial.println(users[i].PW);
  Serial.println(users[i].User);
#endif

  if (users[i].User[0] != 0x00 && users[i].User[0] != 0x40 && users[i].User[0] != 0x21 && users[i].User[0] != 0x24)
  {
    keyboardModifier(users[i].User);
    Keyboard.write(9);
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

  keyboardModifier(users[i].PW);
  if ((users[i].enter == true) || (users[i].User[0] == 0x40) || (GlobalReturn == true) || (users[i].User[0] == 0x24) || (users[i].User[0] == 0x21))
  {
    Keyboard.write(10);
  }
}

// ----------------------------------------------------------------------------
// Function to format EEprom
// ----------------------------------------------------------------------------
void FormatEEProm()
{
  byte doflash = 0; 
  for (int i = 873 ; i < 1024; i++)
  {
    EEPROM.write(i, 0);
    doflash++; 
    if (doflash == 10)
    {
      LEDcolor(LEDBlue);
      rgbled.show();

    }
    if (doflash == 20)
    {
      LEDcolor(LEDOff);
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
    }

    //rgbled.setPixelColor(0, 0, 5 * i, 150 - (i * 10));
    ctx = aes128_cbc_enc_start(key, iv);
    aes128_cbc_enc_continue(ctx, users[i].PW, 48);
    aes128_cbc_enc_finish(ctx);

    ctx = aes128_cbc_enc_start(key, iv);
    aes128_cbc_enc_continue(ctx, users[i].User, 48);
    aes128_cbc_enc_finish(ctx);
    LEDcolor(LEDBlue);
    rgbled.show();

    EEPROM_writeAnything(i * 97, users[i]);

    LEDcolor(LockLEDcolor);
    rgbled.show();
    delay(20);

  }

  Serial.print("EraseDone");
  Serial.print(DoneMessage);
  password = Password("0000");

  locked = true;
  LEDcolor(LockLEDcolor);
  password.reset();
  byteCount = 0;
}

// ----------------------------------------------------------------------------
// Function to write login/password to EEprom
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// Function to write PIN code to EEprom
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// Function for serial messages when device is unlocked
// ----------------------------------------------------------------------------
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
#if LOCKED == 'false'
        locked = false;
#else
        locked = true;
#endif      
        LEDcolor(LockLEDcolor);
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
// ----------------------------------------------------------------------------
// Function to print RAW EEProm data
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// Function to print settings
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// Function to print version
// ----------------------------------------------------------------------------
void printVersion()
{
  Serial.print("Version");
  Serial.print(Version);
  Serial.print(DoneMessage);
}

// ----------------------------------------------------------------------------
// Function to send CtrlAltDel key sequence
// ----------------------------------------------------------------------------
void sendAltCTRLDel()
{
  Keyboard.press(KEY_LEFT_CTRL);
  Keyboard.press(0x82);
  Keyboard.press(KEY_DELETE);
  delay(100);
  Keyboard.releaseAll();
}

// ----------------------------------------------------------------------------
// Function to send win+l key sequence
// ----------------------------------------------------------------------------
void lockWindows()
{	
  Keyboard.press(0x83);
  Keyboard.press('l');
  delay(100);
  Keyboard.releaseAll();
}

// ----------------------------------------------------------------------------
// Function to send win+r key sequence
// ----------------------------------------------------------------------------
void startRun()
{
  Keyboard.press(0x83);
  Keyboard.press('r');
  delay(100);
  Keyboard.releaseAll();
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