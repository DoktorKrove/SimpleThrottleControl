// Doktor's Throttle control
// Version 2.0
// Date: 2018-Mar-15

/*
This Script allows you to change how a ship will fly

Setup
Place a program block on the ship of your choosing.
Make sure you have the main cockpit selected for your ship.
If you want an LCD to show some stats add !Throttle to the name, multiple displays are supported.

How to set a speed
Method 1: Hold forward until you reach your desired speed.
Release forward. The ship will now maintain that speed. To stop simply press the revers key. The ship will go back to normal operation, unless you are in another mode. See “Flight modes" for more on how flight modes effect flight behavior.

Method 2: Run the ship with a value in the argument between 1 and the max speed of your world. To stop simply press the revers key. The ship will go back to normal operation.

Flight modes
Flight modes are a way to change how the script effects your ship. Each mode will affect how the ship operates and handles.
The flight modes are in a hierarchy list to allow multiple modes to be accessible with only two actions.
The list is as follows with Cruise being the default
Decoupled
Cruise
Normal

Mode descriptions
Cruise: This is the default mode. When you hold forward the ship will accelerate, when you release forward the ship will set your current speed as the target speed and it will do everything it can to maintain this speed. Pressing backwards will set the target speed to 0 and the ship will operate like normall. This mode lets you quickly switch between cruise control and normal inertia dampener mode (as long as inertia dampeners are on) as pressing backwards will stop the ship in an emergency.
Decoupled: This mode is like cruise but unlike cruise, when backwards is pressed the ship will slow down but instead of setting the target to 0 it sets the target to your speed just like it would if you held forward. This mode allows you to adjust your speed without the ship trying to stop every time you press backwards. However the problem is that if you need to stop you ship completely have to change the mode first to another mode like “Cruise” or “Stopped”.
Normal: This mode disables the speed control so you can fly like it was off. You can still change modes however.
There are two ways to change flight modes
Method 1: Double press forward or backwards to move through the flight mode list.
Method 2: Write the flight mode name as an argument, this allows you to change to any flight mode you want regardless of its hierarchy position.

Arguments
“stop” -- stops the script
“cruise” – special flight mode where backwards stops the ship and forward sets the speed 
"decoupled" -- Special flight mode where backwards slows down but doesn’t stop
“number” – set the speed to whatever number is set to	
*/