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
    /// <summary>
    /// Mod's runtime controller and manager.
    /// Provides general utilities, definitions, and handles the user's settings.
    /// </summary>
    public partial class DangIt : ScenarioModule
    {

        /// <summary>
        /// List of resources that must be ignored by tank leaks.
        /// </summary>
        public List<string> LeakBlackList;

        public Dictionary<SkillLevel, Perk.UpgradeCost> trainingCosts;

        /// <summary>
        /// General settings about notifications and gameplay elements.
        /// </summary>
        public Settings CurrentSettings 
        {
            get { return currentSettings; }
            set
            {
                this.Log("Applying new settings:\n" + value.ToNode().ToString());
                currentSettings = value;
            }
        }
        private DangIt.Settings currentSettings;


        public static DangIt Instance { get; private set; }

        public bool IsReady { get; private set; }        


        /// <summary>
        /// Returns the location of the mod's configuration file.
        /// Likely, GameData/DangIt/PluginData/DangIt/DangIt.cfg
        /// </summary>
        internal string ConfigFilePath
        {
            get { return IOUtils.GetFilePathFor(this.GetType(), "DangIt.cfg"); }
        }


        public DangIt()
        {
            Debug.Log("[DangIt]: Instantiating runtime...");

            ConfigNode settingsFile = ConfigNode.Load(ConfigFilePath);

            LeakBlackList = new List<string>();
            try
            {
                ConfigNode blackListNode = settingsFile.GetNode("BLACKLIST");
                foreach (string item in blackListNode.GetValues("ignore"))
                    LeakBlackList.Add(item);
            }
            catch (Exception e)
            {                
                LeakBlackList.Add("ElectricCharge");
                LeakBlackList.Add("SolidFuel");
                LeakBlackList.Add("SpareParts");

                this.Log("An exception occurred while loading the resource blacklist and a default one has been created. " + e.Message);
            }


            trainingCosts = new Dictionary<SkillLevel, Perk.UpgradeCost>();
            try
            {
                ConfigNode trainingNode = settingsFile.GetNode("TRAINING");

                foreach (SkillLevel level in Enum.GetValues(typeof(SkillLevel)))
                {
                    string item = trainingNode.GetValue(level.ToString());
                    trainingCosts.Add(level, Perk.UpgradeCost.FromString(item));
                }

            }
            catch (Exception e)
            {
                trainingCosts.Clear();
                trainingCosts.Add(SkillLevel.Unskilled, new Perk.UpgradeCost(science: 10, funds: 10000));
                trainingCosts.Add(SkillLevel.Normal, new Perk.UpgradeCost(science: 50, funds: 50000));
                trainingCosts.Add(SkillLevel.Skilled, new Perk.UpgradeCost(science: 150, funds: 150000));

                this.Log("An exception occurred when loading the training costs dictionary and a default one has been created. " + e.Message);
           }


            Instance = this;

            // Not yet ready: will be ready only after OnLoad
            this.IsReady = false;

            // Start waiting for everything to be ready so that the settings button can be added
            this.StartCoroutine("AddAppButton");
        }



        public override void OnLoad(ConfigNode node)
        {
            if (node.HasNode("SETTINGS"))
                this.CurrentSettings = new Settings(node.GetNode("SETTINGS"));
            else
            {
                this.CurrentSettings = new Settings();
                this.Log("WARNING: No settings node to load, creating default one");
            }

            this.IsReady = true;
        }


        public override void OnSave(ConfigNode node)
        {
            this.Log("Saving settings...");

            if (node.HasNode("SETTINGS"))
            {
                node.SetNode("SETTINGS", CurrentSettings.ToNode());
            }
            else
            {
                node.AddNode(CurrentSettings.ToNode());
            }
        }



        public void OnDestroy()
        {
            this.Log("Destroying instance...");

            if (appBtn != null) ApplicationLauncher.Instance.RemoveModApplication(this.appBtn);
        }


        private void Log(string msg)
        {
            Debug.Log("[DangIt][Runtime]: " + msg);
        }
    }
}