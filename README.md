**[PW-Stick](https://github.com/seeers/MarkupTest)** is a hardware password manager for Keepass
======


## Features
* Stores up to 9 user names and passwords / each up to 47 characters
* Supports Keepass with a plugin
* AES Encrypted Memory
* Protected by a 4 digit pin
* Secure erase after 4 wrong pin attempts
* Optional auto lock timer
* Works as HID Keyboard

![Stick](pictures/stick.jpg?raw=true)


## Usage
###### 1. Plug in the Password Stick USB device
   * The orange LED indicates that the device is locked

![Stick](pictures/locked.jpg?raw=true)

###### 2. Enter your 4 digit pin to unlock the device (Default Pin is 0000)
   * A short red LED flash indicates a wrong pin attempt
   * If you enter a wrong pin 4 times, the stick will delete all entries while flashing blue
   * A green LED indicates that the device is unlocked an ready

![Stick](pictures/unlocked.jpg?raw=true)

###### 3. Type your Username/Password by pressing the button of the entry
   * Press the button for >1sec to type only the password


## Assignment of keys

| 1 | 2 |  
| ------------- |:-------------:|
| 3 | 4 |
| 5 | 6 |
| 7 | 8 |
| 9 | 0 | *0 is also the lock key*


## Keepass plugin

After the device is unlocked you can read all stored entries from the stick to Keepass by clicking Tools ==> ReadFromStick

![Stick](pictures\KeepassMenu.png?raw=true)

------

A new Group "Password Stick" will be created or refreshed with the entries:

![Stick](pictures\KeepassStickGroup.png?raw=true)

------


You can send any existing username with password to a button of the stick by right clicking ==> SendToHWPW-Stick

![Stick](pictures\KeepassContextMenu.png?raw=true)

------

You can modify some settings of the Stick by Tools ==> Stick Settings:

![Stick](pictures\KeepassStickSettings.png?raw=true)

* You can enter a auto lock time to lock the stick after * minutes of the last pressed button
* You can enable/disable
* You can enable/disable locking your computer (WIN_Key+L) after locking the stick with button 0


## Special features
  * Username "!"  = ALT + CTRL + DEL ==> Wait 1sec. ==> types password ==> return key
  * Username "$"  = WIN + R ==> Password (Command) ==> Return
  example: Username $   Password: Command   ==> if you press the button a cmd will open


## Hardware

* Atmega32u4 with Arduino Leonardo bootloader
* 16 MHz Crystal
* WS2812b RGB LED
* 10 Tactile Switches
* Some resistors and capacitors


![Stick](pcb_b.jpg?raw=true)

![Stick](pcb_t.jpg?raw=true)


**Source code / schematic:** [https://github.com/seeers/MarkupTest](https://github.com/seeers/MarkupTest)


**Released:** Mar 23, 2016

**Status:** Operational, maybe some bugs, works for me since some months.

**Licence:**  CC BY-NC-SA 3.0

## Contact

**Point of Contact:**  
* Twitter: [@seeers0](https://twitter.com/seeers0)
* E-Mail: <seeers@gmx.de>




-----
