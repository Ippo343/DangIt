using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;
using System.Text;

namespace DangIt
{
    /// <summary>
    /// Runtime controller and settings manager
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DangItRuntime : MonoBehaviour
    {
        /// <summary>
        /// Lists the name of the resources that will be ignored by the tank leaks
        /// </summary>
        public List<string> LeakBlackList;

        /// <summary>
        /// Master switches to enable / disable notification types.
        /// A failure that is set to silent will still be silent even when these
        /// settings are set to true
        /// </summary>
        public NotificationSettings NotificationSettings;

        /// <summary>
        /// Returns the current instance of the class
        /// </summary>
        public static DangItRuntime Instance { get; private set; }

        public string SettingsFilePath
        {
            get { return IOUtils.GetFilePathFor(this.GetType(), "DangIt.cfg"); }
        }


        public DangItRuntime()
        {
            Instance = this;

            #region Load the global configuration file
            ConfigNode globalSettingsNode;
            if (System.IO.File.Exists(this.SettingsFilePath))
                globalSettingsNode = ConfigNode.Load(this.SettingsFilePath);
            else
            {
                Debug.Log("[DangIt]: Runtime error: the global settings file does not exist, creating a default one");
                globalSettingsNode = CreateDefaultSettings();
            } 
            #endregion

            #region Load the resource blacklist
            LeakBlackList = new List<string>();
            ConfigNode blackListNode = globalSettingsNode.GetNode("BLACKLIST");
            foreach (string item in blackListNode.GetValues("ignore"))
                LeakBlackList.Add(item); 
            #endregion

            #region Load the notification settings
            NotificationSettings = new NotificationSettings(globalSettingsNode.GetNode("NOTIFICATIONS"));
            #endregion

        }


        /// <summary>
        /// Creates the default global configuration node
        /// and saves it to the default path
        /// </summary>
        private ConfigNode CreateDefaultSettings()
        {
            ConfigNode result = new ConfigNode("SETTINGS");

            #region Default notification behaviour
            ConfigNode notificationsNode = new ConfigNode("NOTIFICATIONS");
            notificationsNode.AddValue("messages", true);
            notificationsNode.AddValue("glow", true);
            notificationsNode.AddValue("sounds", true);  // not yet implemented
            result.AddNode(notificationsNode); 
            #endregion

            #region Default resource blacklist
            ConfigNode blackListNode = new ConfigNode("BLACKLIST");
            blackListNode.AddValue("ignore", "ElectricCharge");
            blackListNode.AddValue("ignore", "SolidFuel");
            blackListNode.AddValue("ignore", "SpareParts");
            result.AddNode(blackListNode); 
            #endregion

            result.Save(this.SettingsFilePath);
            return result;
        }
    }



    public class NotificationSettings
    {
        public bool Messages = true;
        public bool Glow = true;
        public bool Sounds = true;

        public NotificationSettings(ConfigNode node)
        {
            if (node != null)
            {
                Messages = DangIt.Parse<bool>(node.GetValue("messages"), true);
                Glow = DangIt.Parse<bool>(node.GetValue("glow"), true);
                Sounds = DangIt.Parse<bool>(node.GetValue("sounds"), true); 
            }
        }
    }



    /// <summary>
    /// Contains shared functions and project constants
    /// </summary>
    public static class DangIt
    {

#if DEBUG
        public static readonly float EvaRepairDistance = 20f;
        public static readonly bool DEBUG = true;
        public static readonly bool EnableGuiFailure = true;
#else
        public const float EvaRepairDistance = 1.5f;
        public const bool DEBUG = false;
        public const bool EnableGuiFailure = true;
#endif



        /// <summary>
        /// Constants related to the spare parts resource
        /// </summary>
        public static class Spares
        {
            /// <summary>
            /// Amount of Spare Parts that is taken each time the button is pressed
            /// </summary>
            public static readonly double Increment = 1f;

            /// <summary>
            /// Maximum amount that a kerbal can carry
            /// </summary>
            public static readonly double MaxEvaAmount = 10f;

            /// <summary>
            /// Resource name as a string
            /// </summary>
            public static readonly string Name = "SpareParts";
        }



        /// <summary>
        /// Returns the in-game universal time
        /// </summary>
        public static float Now()
        {
            return (float)Planetarium.GetUniversalTime();
        }



        /// <summary>
        /// Adds a new entry to the flight events log.
        /// Automatically adds the MET at the beginning of the log
        /// </summary>
        public static void FlightLog(string msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            if (FlightLogger.met_years > 0) sb.Append(FlightLogger.met_years + ":");
            if (FlightLogger.met_days > 0) sb.Append(FlightLogger.met_days + ":");
            sb.Append(FlightLogger.met_hours + ":" + FlightLogger.met_mins + ":" + FlightLogger.met_secs);
            sb.Append("] ");

            FlightLogger.eventLog.Add(sb.ToString() + msg);
        }



        /// <summary>
        /// Returns true if an engine is currently in use
        /// </summary>
        public static bool EngineIsActive(ModuleEngines engineModule)
        {
            return (engineModule.enabled &&
                    engineModule.EngineIgnited &&
                   (engineModule.currentThrottle > 0));
        }



        /// <summary>
        /// Broadcasts a message at the top-center of the screen
        /// ONLY if message notifications are enabled in the global settings
        /// </summary>
        public static void Broadcast(string message, float time = 5f)
        {
            // Check first if the settings allow message notifications
            if (DangItRuntime.Instance.NotificationSettings.Messages)
                ScreenMessages.PostScreenMessage(message, time, ScreenMessageStyle.UPPER_CENTER);
        }



        /// <summary>
        /// Tries to parse a string and convert it to the type T.
        /// If the string is empty or an exception is raised it returns the
        /// specified default value.
        /// </summary>
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
        public static Part FindEVAPart()
        {
            int idx = FlightGlobals.Vessels.FindIndex(v => ((v.vesselType == VesselType.EVA) && v.isActiveVessel));
            return ((idx < 0) ? null : FlightGlobals.Vessels[idx].rootPart);
        }


        /// <summary>
        /// Finds the active EVA kerbal and returns its ProtoCrewMember
        /// </summary>
        /// <returns></returns>
        public static ProtoCrewMember FindEVAProtoCrewMember()
        {
            int idx = FlightGlobals.Vessels.FindIndex(v => ((v.vesselType == VesselType.EVA) && v.isActiveVessel));
            Vessel vessel = ((idx < 0) ? null : FlightGlobals.Vessels[idx]);

            if (vessel == null)
                return null;
            else
            {
                List<ProtoCrewMember> crew = vessel.GetVesselCrew();

                if (crew.Count == 1)
                    return crew.First();
                else
                {
                    throw new Exception("Error while searching for the EVA kerbal: found " + crew.Count + " crew elements, expected 1");
                }
            }
        }



        /// <summary>
        /// Resets the glow on all the vessel.
        /// Parts that have failed will glow red unless they are set to fail silently.
        /// </summary>
        /// <param name="v"></param>
        public static void ResetShipGlow(Vessel v)
        {
            Debug.Log("DangIt: Resetting the ship's glow");
            ResetPartGlow(v.rootPart);
        }



        /// <summary>
        /// Resets the glow on a single part and then recursively on all its children.
        /// </summary>
        /// <param name="part"></param>
        private static void ResetPartGlow(Part part)
        {
            // Set the highlight to default
            part.SetHighlightDefault();


            // If the glow is globally disabled, don't even bother looking for failures
            if (DangItRuntime.Instance.NotificationSettings.Glow)
            {
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
            }


            // Reset the glow for all the child parts
            foreach (Part child in part.children)
                DangIt.ResetPartGlow(child);
        }

    }
}