// Doktor's Throttle control
// Version 3.6
// Date: 2020-11-25

/*
This script provides a form of throttle to any ship making it easy to fly a ship at a fixed speed.

Setup
Place a program block on the ship of your choosing.
Add the word !main to the Cockpit or remote control block that you will use to control the ship. If you want an LCD to show some 
stats add !Throttle to the name, multiple displays are supported.
The Main cockpit will also show some stats, to change which display shows the stats change "LCD_INDEX"

Notes on remote control blocks and cockpits
If you add !main to a remote control block and a cockpit make sure they face the same direction. If they point in different 
directions the remote control block wont work correctly

How to set a speed
Method 1: Hold forward until you reach your desired speed.
Release forward. The ship will now maintain that speed. To stop simply press the revers key. The ship will go back to normal 
operation (unless you are in another mode). See “Flight modes" for more on how flight modes effect flight behavior.

Method 2: Run the ship with a value in the argument between 0 and the max speed of your world. To stop simply press the revers 
key. The ship will go back to normal operation.

Flight modes
Flight modes are a way to change how the script effects your ship. Each mode will affect how the ship operates.
The flight modes are in a hierarchy list to allow multiple modes to be accessible with only two actions.
The list is as follows with Cruise being the default
Decoupled
Cruise+
Cruise
Normal

Mode descriptions
Cruise: This is the default mode. When you hold forward the ship will accelerate, when you release forward the ship will set 
                your current speed as the target speed and it will do everything it can to maintain this speed. Pressing backwards will 
                set the target speed to 0 and the ship will operate like normal. This mode lets you quickly switch between cruise control 
                and normal inertia dampener mode (as long as inertia dampeners are on) as pressing backwards will allow inertia 
                dampeners to do there job.
Cruise+: This mode is like cruise however, when backwards is pressed the ship will slow down but instead stopping, it sets your 
                new speed to your current speed just like it would if you held forward. This mode allows you to adjust your speed without 
                the ship trying to stop every time you press backwards. However, the problem is that if you need to stop your ship 
                completely you have to change the mode first to another mode like “Cruise” or “Stopped”.
Decoupled: This mode is like cruise+ except that it works in all directions. NOTE: Decoupled mode can be finicky to use as the ship 
                will use the cockpit as the reference point, so if you are flying up and you rotate 90 degrees up so you are now moving 
                forward, your ship will try to make you fly up again making you stop going forward. Also the speed is additive, so if you 
                then press forward, the ship will now try to make you fly up and forward.

There are two ways to change flight modes
Method 1: Double press forward or backwards to move through the flight mode list.
Method 2: Write the flight mode name as an argument, this allows you to change to any flight mode you want regardless of its 
hierarchy position.



Arguments
“stop” -- stops the script
“cruise” –- special flight mode where backwards stops the ship and forward sets the speed 
"cruise+" -- Special flight mode where backwards slows down but doesn’t stop
"decoupled" -- Cruise+ in any direction. Be careful with this mode!
“number” –- set the target speed to whatever number is set to
“eco” – Toggle Eco mode on/off
“eco on” – Toggle eco mode on
“eco off” – Toggle eco mode off

*/