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
    partial class OrbitalAltitude : MyGridProgram
    {
        public void Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }


        List<IMyThrust> UpThrust = new List<IMyThrust>();
        List<IMyThrust> DownThrust = new List<IMyThrust>();
        List<IMyThrust> ForwardThrust = new List<IMyThrust>();
        List<IMyThrust> ReverseThrust = new List<IMyThrust>();
        IMyRemoteControl Remote;
        IMyTextPanel LCD;
        double TargetAltitude = 35000; //target orbital altitude in meters
        float maxThrust = 0.04F; //thrust step amount

        void Main(string argument)
        {
            if (UpThrust.Count == 0 || DownThrust.Count == 0 || Remote == null || ForwardThrust.Count == 0 || ReverseThrust.Count == 0 || LCD == null)
            {
                Setup();
            }

            double altitude;
            Remote.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out altitude);
            LCD.WritePublicText("Orbital Translation", false);
            LCD.WritePublicText("\n-------------------", true);
            LCD.WritePublicText("\n\nAltitude: " + altitude.ToString("0.00"), true);

            if (altitude - TargetAltitude < -1) //under target altitude
            {
                double speedlimit = Math.Sqrt(TargetAltitude - altitude);
                double yspeed;
                //yspeed = Remote.GetShipVelocities().LinearVelocity.Y;

                Vector3D vel = Remote.GetShipVelocities().LinearVelocity;
                double len = vel.Length();
                double theta = Math.Asin(vel.Y / len);
                yspeed = (vel.Y / Math.Tan(theta));

                LCD.WritePublicText("\n\nSpeed: " + yspeed.ToString("0.00") + "m/s", true);
                LCD.WritePublicText("\nLimit: " + speedlimit.ToString("0.00") + "m/s", true);
                if (Math.Abs(yspeed) < speedlimit & yspeed >= 0) //under speed limit, and moving up
                {
                    //increase thrustoverride for upthrust
                    //decrease thrustoverride for downthrust
                    foreach (IMyThrust thruster in UpThrust)
                    {


                        thruster.ThrustOverridePercentage = maxThrust * 2;

                    }
                    foreach (IMyThrust thruster in DownThrust)
                    {
                        thruster.ThrustOverridePercentage = 0F;
                    }
                }
                else if (Math.Abs(yspeed) > speedlimit & yspeed > 0) //over speed limit, and moving up
                {
                    if (speedlimit > 1)
                    {
                        //decrease upthrust, increase downthrust
                        foreach (IMyThrust thruster in UpThrust)
                        {
                            thruster.ThrustOverridePercentage = 0F;
                        }
                        foreach (IMyThrust thruster in DownThrust)
                        {


                            thruster.ThrustOverridePercentage = maxThrust * 2;

                        }
                    }
                    else
                    {
                        foreach (IMyThrust thruster in UpThrust)
                        {


                            thruster.ThrustOverridePercentage = 0F;

                        }
                        foreach (IMyThrust thruster in DownThrust)
                        {
                            thruster.ThrustOverridePercentage = 0F;
                        }
                    }
                }
                else if (Math.Abs(yspeed) < speedlimit & yspeed < 0) //under speed limit, moving down
                {
                    //increase upthrust, decrease downthrust
                    foreach (IMyThrust thruster in UpThrust)
                    {


                        thruster.ThrustOverridePercentage = maxThrust;

                    }
                    foreach (IMyThrust thruster in DownThrust)
                    {
                        thruster.ThrustOverridePercentage = 0F;
                    }
                }
                else if (Math.Abs(yspeed) > speedlimit & yspeed < 0) //over speed limit, moving down
                {
                    //increase upthrust, decrease downthrust
                    foreach (IMyThrust thruster in UpThrust)
                    {


                        thruster.ThrustOverridePercentage = maxThrust * 2;

                    }
                    foreach (IMyThrust thruster in DownThrust)
                    {
                        thruster.ThrustOverridePercentage = 0F;
                    }
                }
            }
            else if (altitude - TargetAltitude > 1) //over target altitude
            {
                double speedlimit = Math.Sqrt(altitude - TargetAltitude);
                double yspeed;
                //yspeed = Remote.GetShipVelocities().LinearVelocity.Y;
                Vector3D vel = Remote.GetShipVelocities().LinearVelocity;
                double len = vel.Length();
                double theta = Math.Asin(vel.Y / len);
                yspeed = (vel.Y / Math.Tan(theta));

                LCD.WritePublicText("\n\nSpeed: " + yspeed.ToString("0.00") + "m/s", true);
                LCD.WritePublicText("\nLimit: " + speedlimit.ToString("0.00") + "m/s", true);
                if (Math.Abs(yspeed) < speedlimit & yspeed > 0) //under speed limit, and moving up
                {
                    //increase downthrust, decrease upthrust
                    foreach (IMyThrust thruster in UpThrust)
                    {
                        thruster.ThrustOverridePercentage = 0F;
                    }
                    foreach (IMyThrust thruster in DownThrust)
                    {


                        thruster.ThrustOverridePercentage = maxThrust;

                    }
                }
                else if (Math.Abs(yspeed) > speedlimit & yspeed > 0) //over speed limit, and moving up
                {
                    //increase downthrust, decrease upthrust
                    foreach (IMyThrust thruster in UpThrust)
                    {
                        thruster.ThrustOverridePercentage = 0F;
                    }
                    foreach (IMyThrust thruster in DownThrust)
                    {


                        thruster.ThrustOverridePercentage = maxThrust * 2;

                    }
                }
                else if (Math.Abs(yspeed) < speedlimit & yspeed < 0) //under speed limit, moving down
                {
                    //increase downthrust, decrease upthrust
                    foreach (IMyThrust thruster in UpThrust)
                    {
                        thruster.ThrustOverridePercentage = 0F;
                    }
                    foreach (IMyThrust thruster in DownThrust)
                    {


                        thruster.ThrustOverridePercentage = maxThrust;

                    }
                }
                else if (Math.Abs(yspeed) > speedlimit & yspeed < 0) //over speed limit, moving down
                {
                    if (speedlimit > 1)
                    {
                        //increase upthrust, decrease downthrust
                        foreach (IMyThrust thruster in UpThrust)
                        {


                            thruster.ThrustOverridePercentage = maxThrust * 2;

                        }
                        foreach (IMyThrust thruster in DownThrust)
                        {
                            thruster.ThrustOverridePercentage = 0F;
                        }
                    }
                    else
                    {
                        foreach (IMyThrust thruster in UpThrust)
                        {


                            thruster.ThrustOverridePercentage = 0F;

                        }
                        foreach (IMyThrust thruster in DownThrust)
                        {
                            thruster.ThrustOverridePercentage = 0F;
                        }
                    }
                }
            }
            else
            {
                foreach (IMyThrust thruster in UpThrust)
                {
                    thruster.ThrustOverride = 0.0F;
                    thruster.ThrustOverridePercentage = 0.0F;
                }
                foreach (IMyThrust thruster in DownThrust)
                {
                    thruster.ThrustOverride = 0.0F;
                    thruster.ThrustOverridePercentage = 0.0F;
                }

                Vector3D vel = Remote.GetShipVelocities().LinearVelocity;
                double len = vel.Length();
                double theta = Math.Asin(vel.Z / len);
                double myvel = (vel.Z / Math.Tan(theta));

                //double myvel = Remote.GetShipVelocities().LinearVelocity.Z;
                LCD.WritePublicText("\n\nLimit: " + CalcOrbitSpeed(TargetAltitude) + "Nanm /s", true);
                LCD.WritePublicText("\nForward Velocity: " + (myvel).ToString("0.00"), true);
                if (myvel < CalcOrbitSpeed(TargetAltitude))
                {
                    //speed up
                    foreach (IMyThrust thruster in ForwardThrust)
                    {
                        thruster.ThrustOverridePercentage = maxThrust * 2;
                    }
                    foreach (IMyThrust thruster in ReverseThrust)
                    {
                        thruster.ThrustOverride = 0.0F;
                    }
                }
                else if (myvel > CalcOrbitSpeed(TargetAltitude))
                {
                    //slow down
                    foreach (IMyThrust thruster in ForwardThrust)
                    {
                        thruster.ThrustOverride = 0.0F;
                    }
                    foreach (IMyThrust thruster in ReverseThrust)
                    {
                        thruster.ThrustOverridePercentage = maxThrust * 2;
                    }
                }
                else
                {
                    //stop accelerating and coast
                    foreach (IMyThrust thruster in ForwardThrust)
                    {
                        thruster.ThrustOverride = 0.0F;
                    }
                    foreach (IMyThrust thruster in ReverseThrust)
                    {
                        thruster.ThrustOverride = 0.0F;
                    }
                }
            }
        }
        void Setup()
        {
            Echo("Beginning Setup...");
            List<IMyRemoteControl> rems = new List<IMyRemoteControl>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(rems);
            Remote = rems[0];

            List<IMyTextPanel> lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds);
            LCD = lcds[1];

            List<IMyThrust> allthrust = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(allthrust);

            Matrix remOrientation;
            Remote.Orientation.GetMatrix(out remOrientation);
            Vector3D forward = remOrientation.Forward;
            Vector3D backward = remOrientation.Backward;
            Vector3D down = remOrientation.Down;
            Vector3D up = remOrientation.Up;

            foreach (IMyThrust thruster in allthrust)
            {
                Matrix thrustOrient;
                thruster.Orientation.GetMatrix(out thrustOrient);

                if (thrustOrient.Backward == up)
                {
                    UpThrust.Add(thruster);
                }
                else if (thrustOrient.Backward == down)
                {
                    DownThrust.Add(thruster);
                }
                else if (thrustOrient.Backward == forward)
                {
                    ForwardThrust.Add(thruster);
                }
                else if (thrustOrient.Backward == backward)
                {
                    ReverseThrust.Add(thruster);
                }
            }

            Echo("Setup Complete");
            return;
        }
        double CalcOrbitSpeed(double targetalt)
        {
            double vel = Math.Sqrt((8.82 * ((60000 / (60000 + targetalt)) * (60000 / (60000 + targetalt)))) * 60000);
            return vel;
        }
    }
}
