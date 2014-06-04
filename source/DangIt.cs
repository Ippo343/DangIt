using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Project-wise constants and functions
    /// </summary>
    public static class DangIt
    {
        /// <summary>
        /// Constants related to the spare parts resource
        /// </summary>
        public static class Spares
        {
            public const double Increment = 1f;
            public const double MaxEvaAmount = 10f;
            public const string Name = "SpareParts";
        }



#if DEBUG
        public const float EvaRepairDistance = 20f;
        public const bool EnableGuiFailure = true; 
#else
        public const float EvaRepairDistance = 0.5f;
        public const bool EnableGuiFailure = false; 
#endif



        /// <summary>
        /// Broadcasts a message in the middle of the screen.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="time">Time duration of the message</param>
        public static void Broadcast(string message, float time = 5f)
        {
            ScreenMessages.PostScreenMessage(message, time, ScreenMessageStyle.UPPER_CENTER);
        }



        /// <summary>
        /// Finds the active EVA vessel and returns its root part, or null if no EVA is found.
        /// </summary>
        public static Part FindEVA()
        {
            int idx = FlightGlobals.Vessels.FindIndex(v => ((v.vesselType == VesselType.EVA) && v.isActiveVessel));
            return ((idx < 0) ? null : FlightGlobals.Vessels[idx].rootPart);
        }


        public static void ResetShipGlow(Vessel v)
        {
#if DEBUG
            Debug.Log("DangIt: Resetting the ship's glow");
#endif
            ResetGlow(v.rootPart);
        }

        /// <summary>
        /// Makes the part glow red, or restores the default glow.
        /// </summary>
        public static void ResetGlow(Part part)
        {
            // Set the highlight to default
            part.SetHighlightDefault();

            // Scan all the failure modules, if any
            List<ModuleBaseFailure> failModules = part.Modules.OfType<ModuleBaseFailure>().ToList();
            for (int i = 0; i < failModules.Count; i++)
            {
                if (failModules[i].hasFailed)
                {   
                    // If any module has failed, glow red and stop searching (just one is sufficient)
                    part.SetHighlightColor(Color.red);
                    part.SetHighlightType(Part.HighlightType.AlwaysOn);
                    part.SetHighlight(true);

                    break;
                }
            }

            // Reset the glow for all the child parts
            foreach (Part child in part.children)
            {
                DangIt.ResetGlow(child);
            }
        }

    }
}