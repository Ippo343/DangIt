using System;
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
        public const bool DEBUG = true;
        public const bool EnableGuiFailure = true;
#else
        public const float EvaRepairDistance = 2f;
        public const bool DEBUG = false;
        public const bool EnableGuiFailure = true;
#endif

        public static float Now()
        {
            return (float)Planetarium.GetUniversalTime();
        }


        public static bool EngineIsActive(ModuleEngines engineModule)
        {
            return (engineModule.enabled &&
                    engineModule.EngineIgnited &&
                   (engineModule.currentThrottle > 0));
        }


        public static void Broadcast(string message, float time = 5f)
        {
            ScreenMessages.PostScreenMessage(message, time, ScreenMessageStyle.UPPER_CENTER);
        }



        public static T Parse<T>(string text, T defaultTo)
        {
            try
            {
                return (String.IsNullOrEmpty(text) ? defaultTo : (T)Convert.ChangeType(text, typeof(T)));
            }
            catch
            {
                return defaultTo;
            }
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
            Debug.Log("DangIt: Resetting the ship's glow");
            ResetPartGlow(v.rootPart);
        }


        public static void ResetPartGlow(Part part)
        {
            // Set the highlight to default
            part.SetHighlightDefault();

            // Scan all the failure modules, if any
            List<FailureModule> failModules = part.Modules.OfType<FailureModule>().ToList();
            for (int i = 0; i < failModules.Count; i++)
            {
                if (failModules[i].HasFailed && !failModules[i].Silent)
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
                DangIt.ResetPartGlow(child);
            }
        }

    }
}