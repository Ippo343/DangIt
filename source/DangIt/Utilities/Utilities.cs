using KSP.IO;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DangIt.Utilities
{
    /// <summary>
    /// A collection of various small utilities to reduce boilerplate.
    /// </summary>
    public static class CUtils
    {
        /// <summary>
        /// Maps the priority given as a string to its corresponding integer value
        /// </summary>
        public static readonly Dictionary<string, int> PriorityIntValues = new Dictionary<string, int>
            {
                { "LOW", 1 },
                { "MEDIUM", 2 },
                { "HIGH", 3 },
            };

        public static void Log(string msg)
        {
            Debug.Log("[DangIt]" + msg);
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
        /// Likely, GameData/CDangIt/PluginData/CDangIt/ + filename
        /// </summary>
        internal static string GetConfigFilePath(string fileName)
        {
            return IOUtils.GetFilePathFor(typeof(CDangIt), fileName);
        }
     
        /// <summary>
        /// Adds a new entry to the flight events log.
        /// Automatically adds the MET at the beginning of the log
        /// </summary>
        public static void FlightLog(string msg)
        {
            //TODO: the flightlogger doesn't give me a useful MET anymore, we need to find the MET somehow.
            FlightLogger.eventLog.Add(msg);
        }

        /// <summary>
        /// Broadcasts a message at the top-center of the screen
        /// The message is ignored if the settings have disabled messages, unless
        /// overrideMute is true
        /// </summary>
        public static void Broadcast(string message, bool overrideMute = false, float time = 5f)
        {
            if (overrideMute || CDangIt.Instance.CurrentSettings.Messages)
                ScreenMessages.PostScreenMessage(message, time, ScreenMessageStyle.UPPER_CENTER);
        }

        /// <summary>
        /// Posts a new message to the messaging system unless notifications have been disabled in the general settings.
        /// </summary>
        public static void PostMessage(string title, string message, MessageSystemButton.MessageButtonColor messageButtonColor, MessageSystemButton.ButtonIcons buttonIcons,
            bool overrideMute = false)
        {
            if (CDangIt.Instance.CurrentSettings.Messages || overrideMute)
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
        public static void ResetShipGlow(Vessel vessel)
        {
            try
            {
                if (vessel.Parts != null)
                {
                    ResetPartGlow(vessel.rootPart);
                }                
            }
            catch (Exception e)
            {
                CUtils.Log("Could not reset the glow for vessel." + e.Message);
            }
            
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
            if (CDangIt.Instance.CurrentSettings.Glow)
            {
                if (part.Modules.OfType<FailureModule>().Any(fm => fm.HasFailed && !fm.Silent))               
                {
                    // If any module has failed, glow red and stop searching (just one is sufficient)
                    part.SetHighlightColor(Color.red);
                    part.SetHighlightType(Part.HighlightType.AlwaysOn);
                    part.SetHighlight(true, false);
                }
            }

            // Reset the glow for all the child parts
            foreach (Part child in part.children)
                CUtils.ResetPartGlow(child);
        }
    }
}
