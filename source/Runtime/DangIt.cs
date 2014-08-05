using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;
using System.Text;

namespace ippo
{
    public partial class DangIt : ScenarioModule
    {

#if DEBUG
        public const float EvaRepairDistance = 20f;
        public const bool DEBUG = true;
        public const bool EnableGuiFailure = true;
#else
        public const float EvaRepairDistance = 1.5f;
        public const bool DEBUG = false;
        public const bool EnableGuiFailure = true; //TODO: change this to false for the final release
#endif


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
        public static DangIt Instance { get; private set; }

        public string SettingsFilePath
        {
            get { return IOUtils.GetFilePathFor(this.GetType(), "DangIt.cfg"); }
        }


        public DangIt()
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
}