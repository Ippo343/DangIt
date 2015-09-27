using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ippo
{
    public partial class DangIt
    {
		/// <summary>
		/// Converts a string representing a priority to a int representing it
		/// </summary>
		/// <returns>The int representation</returns>
		/// <param name="modeString">Priority string</param>
		public static int PriorityIntFromString(string modeString)
		{
			print ("[DangIt] [Static] Translating '" + modeString + "' to int...");
			var keys = new List<string> ();
			keys.Add ("LOW");
			keys.Add ("MEDIUM");
			keys.Add ("HIGH");
			return keys.IndexOf (modeString.ToUpper ())+1; //+1 so that LOW = 1
		}

        /// <summary>
        /// Returns the in-game universal time
        /// </summary>
        public static float Now()
        {
            return (float)Planetarium.GetUniversalTime();
        }


        /// <summary>
        /// Returns the full path to a given file in the configuration folder.
        /// Likely, GameData/DangIt/PluginData/DangIt/ + filename
        /// </summary>
        internal static string GetConfigFilePath(string fileName)
        {
            return IOUtils.GetFilePathFor(typeof(DangIt), fileName);
        }
     

        /// <summary>
        /// Adds a new entry to the flight events log.
        /// Automatically adds the MET at the beginning of the log
        /// </summary>
        public static void FlightLog(string msg)
        {
            string fmt = "00";
            StringBuilder sb = new StringBuilder();

            sb.Append("[");

            if (FlightLogger.met_years > 0) sb.Append(FlightLogger.met_years.ToString(fmt) + ":");
            if (FlightLogger.met_days > 0) sb.Append(FlightLogger.met_days.ToString(fmt) + ":");

            sb.Append(FlightLogger.met_hours.ToString(fmt) + ":" +
                      FlightLogger.met_mins.ToString(fmt) + ":" +
                      FlightLogger.met_secs.ToString(fmt));

			sb.Append("]: ");

            FlightLogger.eventLog.Add(sb.ToString() + msg);
        }




        



        /// <summary>
        /// Broadcasts a message at the top-center of the screen
        /// The message is ignored if the settings have disabled messages, unless
        /// overrideMute is true
        /// </summary>
        public static void Broadcast(string message, bool overrideMute = false, float time = 5f)
        {
            if (overrideMute || DangIt.Instance.CurrentSettings.Messages)
                ScreenMessages.PostScreenMessage(message, time, ScreenMessageStyle.UPPER_CENTER);
        }


        /// <summary>
        /// Posts a new message to the messaging system unless notifications have been disabled in the general settings.
        /// </summary>
        public static void PostMessage(string title, string message, MessageSystemButton.MessageButtonColor messageButtonColor, MessageSystemButton.ButtonIcons buttonIcons,
            bool overrideMute = false)
        {
            if (DangIt.Instance.CurrentSettings.Messages || overrideMute)
            {
                MessageSystem.Message msg = new MessageSystem.Message(
                        title,
                        message,
                        messageButtonColor,
                        buttonIcons);
                MessageSystem.Instance.AddMessage(msg); 
            }

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
            if (DangIt.Instance.CurrentSettings.Glow)
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
                        part.SetHighlight(true, false);
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
