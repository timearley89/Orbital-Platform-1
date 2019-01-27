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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }
        double CTRL_COEFF = 0.8; //Set lower if overshooting, set higher to respond quicker
        List<IMyGyro> Gyros;
        IMyRemoteControl Remote;
        IMyRadioAntenna Antenna;
        IMyTextPanel LCD;

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                if (Gyros.Count == 0)
                {
                    GridTerminalSystem.GetBlocksOfType<IMyGyro>(Gyros);
                }
                if (Remote == null)
                {
                    List<IMyRemoteControl> rems = new List<IMyRemoteControl>();
                    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(rems);
                    Remote = rems[0];
                }
                if (Antenna == null)
                {
                    List<IMyRadioAntenna> ants = new List<IMyRadioAntenna>();
                    GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(ants);
                    Antenna = ants[0];
                }
                if (LCD == null)
                {
                    List<IMyTextPanel> lcds = new List<IMyTextPanel>();
                    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds);
                    LCD = lcds[0];
                }
            }
            catch
            {
                Echo("Required Blocks Not Present!");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            //Blocks loaded and accessible, let's get it detecting planetary gravity.

            Vector3D PlanetGravity = Remote.GetNaturalGravity();
            if (PlanetGravity.Length() == 0)
            {
                Echo("Not Enough Gravity!");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }
            else
            {
                Orbit();
            }
        }
        void Orbit()
        {
            if (Remote.DampenersOverride == false) { Remote.DampenersOverride = true; }
            //Get orientation from rc
            Matrix or;
            Remote.Orientation.GetMatrix(out or);
            Vector3D down = or.Down;

            Vector3D grav = Remote.GetNaturalGravity();
            LCD.WritePublicText("\nGravity Vector", true);
            LCD.WritePublicText("\n--------------", true);
            LCD.WritePublicText("\nX: " + grav.X, true);
            LCD.WritePublicText("\nY: " + grav.Y, true);
            LCD.WritePublicText("\nZ: " + grav.Z, true);
            LCD.ShowPublicTextOnScreen();
            grav.Normalize();

            for (int i = 0; i < Gyros.Count; i++)
                {
                    var g = Gyros[i];

                    g.Orientation.GetMatrix(out or);
                    var localDown = Vector3D.Transform(down, MatrixD.Transpose(or));

                    var localGrav = Vector3D.Transform(grav, MatrixD.Transpose(g.WorldMatrix.GetOrientation()));

                    //Since the gyro ui lies, we are not trying to control yaw,pitch,roll but rather we
                    //need a rotation vector (axis around which to rotate)
                    Vector3D rot = Vector3D.Cross(localDown, localGrav);
                    double ang = rot.Length();
                    ang = Math.Atan2(ang, Math.Sqrt(Math.Max(0.0, 1.0 - ang * ang))); //More numerically stable than: ang=Math.Asin(ang)
                    
                    if (ang < 0.01)
                    {   //Close enough
                        //Echo("Level");
                        g.SetValueBool("Override", false);
                        continue;
                    }
                    Echo("Off level: " + (ang * 180.0 / 3.14).ToString("0.000") + "deg");
                    LCD.WritePublicText("\n\nOff level: " + (ang * 180.0 / 3.14).ToString("0.000") + "deg", true);

                    //Control speed to be proportional to distance (angle) we have left
                    double ctrl_vel = g.GetMaximum<float>("Yaw") * (ang / Math.PI) * CTRL_COEFF;
                    ctrl_vel = Math.Min(g.GetMaximum<float>("Yaw"), ctrl_vel);
                    ctrl_vel = Math.Max(0.01, ctrl_vel); //Gyros don't work well at very low speeds
                    rot.Normalize();
                    rot *= ctrl_vel;
                    g.SetValueFloat("Pitch", (float)rot.GetDim(0));
                    g.SetValueFloat("Yaw", -(float)rot.GetDim(1));
                    g.SetValueFloat("Roll", -(float)rot.GetDim(2));

                    g.SetValueFloat("Power", 1.0f);
                    g.SetValueBool("Override", true);
                }
            
            Echo("Planetary Alignment Complete");
            return;
        }
    }
}