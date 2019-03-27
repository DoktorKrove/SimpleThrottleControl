using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // =====================================================
        // Settings 
        // Modify these settings to adjust how everything works
        // =====================================================

        //these values effect how quickly the ship will correct its speed over time

        //This setting effects the slope of the thrusters
        //larger values mean quicker changes in thrust
        const float multiplier = 1.5f;


        //This is the Dead Zone for speed, if speed is within this range +/-
        //the ship won’t keep trying to adjust the speed
        const float deadZone = 0.0f;

        //This is the tag for LCD's that you want to display some stats on
        const string LCD_TAG = "!Throttle";

        //Frequency LCD's are updated
        //Smaller numbers will decrees performance
        //minimum value 0
        const int UPDATE_INTERVAL = 30;

        //The delay between key presses to activate different modes
        //this is in ticks, there are 60 ticks in a second.
        const int DOUBLE_TAP_DELAY = 15;

        // ========================================
        // DO NOT EDIT BELOW THIS LINE!!!
        // ========================================

        //Lists of blocks for use by the script
        List<IMyCockpit> cockpits = new List<IMyCockpit>();
        List<IMyThrust> thrusters = new List<IMyThrust>();
        List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
        List<IMyThrust> forwardThrusters;
        List<IMyThrust> backwardThrusters;

        //basic variables
        bool setup = true;
        bool enableCruisControl = false;
        bool lastKeyForward = false;
        bool lastKeyBackward = false;
        int tick = 0;
        int lastTickForward = 0;
        int lastTickBackward = 0;
        int lastLCDTick = 0;
        byte flightMode = 0;
        double targetSpeed;
        double currentSpeed;
        float throttle;

        //Get the blocks required to run the script
        IMyCockpit controlSeat;


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            //disable any thrust overrides when a saved game was started
            DisableThusterOveride(forwardThrusters);
            DisableThusterOveride(backwardThrusters);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            //These are values that require to be run first
            if (setup)
            {
                //Get the blocks on the grid
                GridTerminalSystem.GetBlocksOfType(cockpits, cockPit => cockPit.IsMainCockpit);
                GridTerminalSystem.GetBlocksOfType(thrusters);
                GridTerminalSystem.GetBlocksOfType(textPanels, blockName => blockName.DisplayNameText.Contains(LCD_TAG));

                //enable all the LCD's to show text
                foreach (IMyTextPanel lcd in textPanels)
                    lcd.ShowPublicTextOnScreen();
                //Make sure there is a Main Cockpit
                if (!cockpits.Any<IMyCockpit>())
                {
                    Echo("No Main cockpit!");
                    flightMode = 0;
                }
                else if (cockpits.Count > 1)
                {
                    Echo("There is more then one Main Cockpit!");
                    flightMode = 0;
                }
                else
                {
                    controlSeat = cockpits[0];
                    //update the forward thruster list when a player enters the main cockpit
                    if (forwardThrusters == null)
                    {
                        Echo("Enter the main cockpit to initialise thrusters");
                        WriteStatsToLCD("Enter the main cockpit to initialise thrusters");
                        if (controlSeat.IsUnderControl)
                        {
                            GetForwardThrusters(thrusters, ref forwardThrusters);
                            GetBackwardThrusters(thrusters, ref backwardThrusters);
                            setup = false;
                            flightMode = 1;
                        }

                    }
                }
            }


            //update tick for LCD's and time for throttle
            //make sure ticks does not go above its maximum
            if (tick > 2000000000)
                tick = 0;

            tick++;

            //handle arguments
			string arg = argument.ToLower();
            if (arg == "stop")
            {
                flightMode = 0;
                DisableThusterOveride(forwardThrusters);
                DisableThusterOveride(backwardThrusters);
            }
            else if (arg == "cruise")
                flightMode = 1;
            else if (arg == "decoupled")
                flightMode = 2;

            else if (arg != "")
                try //Make sure the program dosent crash if the user enters something that is not a double
                {
                    targetSpeed = double.Parse(argument);
                    if (targetSpeed > 0)
                        enableCruisControl = true;
                }
                catch { }; //we don’t care about any problems as they wont effect anything


            //update displays
            if (forwardThrusters != null)
            {
                double timeToDistance = Math.Round(GetTimeToSpeed(targetSpeed, currentSpeed, forwardThrusters, controlSeat));
                WriteStatsToLCD(currentSpeed, targetSpeed, throttle, timeToDistance, flightMode);
            }

            //if flightMode not 0 run the script
            if (flightMode > 0)
            {
                //get the ships speed
                currentSpeed = GetShipDirectionalSpeed(controlSeat, Base6Directions.Direction.Forward);

                //determines if the ship is trying to move forwards, backwards or nither
                switch (GetShipsDesiredDirection(controlSeat).Z)
                {

                    case -1:    //forward (yes, a negative value means forward)
                        enableCruisControl = true;
                        DisableThusterOveride(forwardThrusters);
                        DisableThusterOveride(backwardThrusters);
                        targetSpeed = GetShipDirectionalSpeed(controlSeat, Base6Directions.Direction.Forward);
                        lastKeyForward = true;
                        break;
                    case 1:     //backwards (because vectors are weird)
                        if (flightMode < 2)
                        {
                            enableCruisControl = false;
                            targetSpeed = 0;
                        }
                        else
                            targetSpeed = GetShipDirectionalSpeed(controlSeat, Base6Directions.Direction.Forward);
                        DisableThusterOveride(forwardThrusters);
                        DisableThusterOveride(backwardThrusters);
                        lastKeyBackward = true;
                        break;
                    case 0:     //Nothing (finally something that makes sense)
                        if (enableCruisControl)
                        {
                            //get the speed wanted
                            float speedDifferance = (float)(targetSpeed - currentSpeed);
                            throttle = GetShipThrottle(multiplier, speedDifferance, 1.0f, 0.0001f);
                            //If vessel is going faster then the wanted speed
                            if (speedDifferance > deadZone)
                            {
                                SetThrusterPercent(throttle, ref forwardThrusters);
                            }
                            else if (speedDifferance < -deadZone)
                            {
                                SetThrusterPercent(throttle, ref backwardThrusters);
                            }
                            else
                            {
                                SetThrusterByVal(0.0001f, ref forwardThrusters);
                                SetThrusterByVal(0.0001f, ref backwardThrusters);
                            }
                        }

                        //Double tap check on key releas
                        if (lastKeyForward)
                        {
                            bool check = CheckTapForward();
                            if (check)
                                flightMode = getFlightMode(flightMode, true);
                            lastKeyForward = false;
                        }

                        if (lastKeyBackward)
                        {
                            bool check = CheckTapBackward();
                            if (check)
                            {
                                flightMode = getFlightMode(flightMode, false);
                            }
                            lastKeyBackward = false;
                        }
                        break;


                }
            }
			else if (flightMode == 0)
			{
				switch (GetShipsDesiredDirection(controlSeat).Z)
                {

                    case -1:    //forward (yes, a negative value means forward)
                        lastKeyForward = true;
                        break;
                    case 1:     //backwards (because vectors are weird)
                        lastKeyBackward = true;
                        break;
                    case 0:     //Nothing (finally something that makes sense)

                        //Double tap check on key releas
                        if (lastKeyForward)
                        {
                            bool check = CheckTapForward();
                            if (check)
                                flightMode = getFlightMode(flightMode, true);
                            lastKeyForward = false;
                        }

                        if (lastKeyBackward)
                        {
                            bool check = CheckTapBackward();
                            if (check)
                            {
                                flightMode = getFlightMode(flightMode, false);
                            }
                            lastKeyBackward = false;
                        }
                        break;


                }
			}
        }

        /*
        * Returns the thrusters facing forward for a given list of thrusters
        */
        public void GetForwardThrusters(List<IMyThrust> allThrusters, ref List<IMyThrust> foundForwardThrusters)
        {
            foundForwardThrusters = new List<IMyThrust>();
            foreach (IMyThrust currThruster in allThrusters)
                if (currThruster.GridThrustDirection.Z == 1)
                    foundForwardThrusters.Add(currThruster);
        }

        /*
        * Returns the thrusters facing forward for a given list of thrusters
        */
        public void GetBackwardThrusters(List<IMyThrust> allThrusters, ref List<IMyThrust> foundForwardThrusters)
        {
            foundForwardThrusters = new List<IMyThrust>();
            foreach (IMyThrust currThruster in allThrusters)
                if (currThruster.GridThrustDirection.Z == -1)
                    foundForwardThrusters.Add(currThruster);
        }

        /*
        * Returns the direction a ship is trying to go
        */
        public Vector3I GetShipsDesiredDirection(IMyCockpit oriantationBlock)
        {
            Vector3I direction;

            direction.X = (int)Math.Ceiling(oriantationBlock.MoveIndicator.X);
            direction.Y = (int)Math.Ceiling(oriantationBlock.MoveIndicator.Y);
            direction.Z = (int)Math.Ceiling(oriantationBlock.MoveIndicator.Z);

            return direction;
        }

        /*
        * Sets all the thrusters in the list to a percent where 0 is off and 1 is full
        */
        public void SetThrusterPercent(float percent, ref List<IMyThrust> thrusters)
        {
            if (thrusters != null)
            {
                for (int i = 0; i < thrusters.Count; i++)
                {
                    thrusters[i].ThrustOverridePercentage = percent;
                }
            }
        }

        /*
        * Sets all the thrusters in the list to a percent where 0 is off and 1 is full
        */
        public void SetThrusterByVal(float val, ref List<IMyThrust> thrusters)
        {
            if (thrusters != null)
            {
                for (int i = 0; i < thrusters.Count; i++)
                {
                    thrusters[i].ThrustOverride = Math.Abs(val);
                }
            }
        }

        public void DisableThusterOveride(List<IMyThrust> thrusters)
        {
            if (thrusters != null)
                foreach (IMyThrust thruster in thrusters)
                {
                    thruster.ThrustOverride = -1;
                }
        }


        private void WriteStatsToLCD(double speed, double targetSpeed, float throttleOutput, double timeToSpeed, int running)
        {
            if (tick - lastLCDTick > UPDATE_INTERVAL)
            {
                lastLCDTick = tick;
                if (textPanels != null)
                {
                    string state = "NORMAL";
                    string normal = "Normal";
                    string cruise = "Cruise";
                    string decoupled = "Decoupled";
                    if (flightMode == 0)
                    {
                        state = "NORMAL";
                        normal = "Normal         <=";
                    }
                    else if (flightMode == 1)
                    {
                        state = "CRUISE";
                        cruise = "Cruise          <=";
                    }
                    else if (flightMode == 2)
                    {
                        state = "DECOUPLED";
                        decoupled = "Decoupled   <=";
                    }
                    foreach (IMyTextPanel lcd in textPanels)
                        lcd.WritePublicText(
                                                "Speed: 	         " + Math.Round(speed, 2) + "m/s" + '\n' +
                                                "Target:          " + Math.Round(targetSpeed, 2) + "m/s" + '\n' +
                                                "Delay:           " + timeToSpeed + "s" + '\n' +
                                                "Throttle:        " + Math.Round(throttleOutput * 100, 1) + "%" + '\n' +
                                                "FlightMode:  " + state + '\n' +
                                                decoupled + '\n' +
                                                cruise + '\n' +
                                                normal + '\n'
                                           );
                }
            }
        }

        private void WriteStatsToLCD(string message)
        {
            if (tick - lastLCDTick > UPDATE_INTERVAL)
            {
                lastLCDTick = tick;
                if (textPanels != null)
                {
                    foreach (IMyTextPanel lcd in textPanels)
                        lcd.WritePublicText(message);
                }
            }

        }
		
		/*
		* Returns a flight mode based on the current flight mode and the direction
		*/
		public byte getFlightMode(byte currentMode, bool goUp)
		{

            switch (currentMode)
			{
				case 0:
					if (goUp)
                        currentMode++;
                    break;
				case 1:
					if (goUp)
                        currentMode++;
					else
					{
                        currentMode--;
						enableCruisControl = false;
						DisableThusterOveride(forwardThrusters);
						DisableThusterOveride(backwardThrusters);
						targetSpeed = 0;
					}
                    break;
				case 2:
                    if (!goUp)
                    {
                        currentMode--;
                        enableCruisControl = false;
                        DisableThusterOveride(forwardThrusters);
                        DisableThusterOveride(backwardThrusters);
                        targetSpeed = 0;
                    }
                    break;
			}
			
			return currentMode;
		}

        /*
        * Returns the time it will take for a ship to reach a set speed
        * Uses a list of thrusters, cockpit, a current speed and a wanted speed
        */
        public double GetTimeToSpeed(double wantedSpeed, double currentSpeed, List<IMyThrust> thrustersInDirection, IMyCockpit cockpit)
        {
            float maxthrust = 0;
            float totalMass = cockpit.CalculateShipMass().TotalMass;
            float speedToReach = (float)(wantedSpeed - currentSpeed);
            float acceleration;

            //get the total thrust of the thrusters
            foreach (IMyThrust thruster in thrustersInDirection)
                maxthrust += thruster.MaxEffectiveThrust;

            //get how quickly the ship can accelerate
            acceleration = maxthrust / totalMass;

            return Math.Abs(speedToReach / acceleration);
        }

        /*
        * Returns the speed given a direction
        * Example direction: Base6Directions.Direction.Forward
        */
        public double GetShipDirectionalSpeed(IMyCockpit cockpit, Base6Directions.Direction direction)
        {
            //get the velocity of the ship as a vector
            Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;

            //given a direction calculate the "length" for that direction, length is the speed in this case
            return velocity.Dot(cockpit.WorldMatrix.GetDirectionVector(direction));
        }

        /*
         * Returns the throttle between two values
         * Throttle is calculated using a parabola
         */
        public float GetShipThrottle(float m, float differance, float max, float min)
        {
            float value = (float)(m * Math.Pow(differance, 2));
            if (value >= max)
                return max;
            else if (value <= min)
                return min;
            else
                return value;
        }

        public bool CheckTapForward()
        {
            bool output = false;
            int diff = 0;
            //Double Click detection
            diff = Math.Abs(tick - lastTickForward);
            if (diff <= DOUBLE_TAP_DELAY)
                output = true;
            else
                lastTickForward = tick;
            return output;
        }

        public bool CheckTapBackward()
        {
            bool output = false;
            int diff = 0;
            //Double Click detection

            diff = Math.Abs(tick - lastTickBackward);
            if (diff <= DOUBLE_TAP_DELAY)
                output = true;
            lastTickBackward = tick;
            return output;
        }
    }
}

//that’s all folks
