using KSP.IO;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using IO = System.IO;

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

        /// <summary>
        /// List of the costs (science and funds) that are requested to train a kerbal.
        /// </summary>
        public List<TrainingCost> trainingCosts;


        private DangIt.Settings currentSettings;
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
        

        /// <summary>
        /// Return the current running instance.
        /// </summary>
        public static DangIt Instance { get; private set; }


        /// <summary>
        /// Returns true if the instance is initialized and ready to work.
        /// </summary>
        public bool IsReady { get; private set; }        


        /// <summary>
        /// Returns the full path to a given file in the configuration folder.
        /// Likely, GameData/DangIt/PluginData/DangIt/ + filename
        /// </summary>
        internal string GetConfigFilePath(string fileName)
        {
            return IOUtils.GetFilePathFor(this.GetType(), fileName);
        }


        public DangIt()
        {
            Debug.Log("[DangIt]: Instantiating runtime.");

            #region Blacklist

            ConfigNode blacklistFile = ConfigNode.Load(this.GetConfigFilePath("BlackList.cfg"));
            LeakBlackList = new List<string>();
            try
            {
                ConfigNode blackListNode = blacklistFile.GetNode("BLACKLIST");
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

            #endregion

            #region Training costs

            /*  The training costs are stored in an xml file instead of a ConfigNode because it allows easy
             *  serialization and deserialization. The best solution would be a dictionary, but it's challenging to write and load with ConfigNode,
             *  while xml serialization of a list is a piece of cake.
             */

            XmlSerializer serializer = new XmlSerializer(typeof(List<TrainingCost>));
            try
            {
                trainingCosts = new List<TrainingCost>();
           
                IO.FileStream fs = new IO.FileStream(this.GetConfigFilePath("Training.xml"), IO.FileMode.Open);
                trainingCosts = (List<TrainingCost>)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception e) // In case of exception (e.g, file not found) create a default list.
            {
                trainingCosts.Clear();
                trainingCosts.Add(new TrainingCost(SkillLevel.Unskilled, science: 10, funds: 10000));
                trainingCosts.Add(new TrainingCost(SkillLevel.Normal, science: 50, funds: 50000));
                trainingCosts.Add(new TrainingCost(SkillLevel.Skilled, science: 120, funds: 120000));

                this.Log("An exception occurred when loading the training costs list and a default one has been created. " + e.Message + e.StackTrace);
            }
            finally
            {
                IO.FileStream fs = new IO.FileStream(this.GetConfigFilePath("Training.xml"), IO.FileMode.OpenOrCreate);
                serializer.Serialize(fs, trainingCosts);
                fs.Close();
            }

            #endregion

            // Now the instance is built and can be exposed, but it is not yet ready until after OnLoad
            Instance = this;
            this.IsReady = false;

            // Add the button to the stock toolbar
            this.StartCoroutine("AddAppButton");
        }


        /// <summary>
        /// Load the saved settings
        /// </summary>
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
            this.Log("Destroying instance.");

            // Remove the button from the toolbar
            if (appBtn != null)
                ApplicationLauncher.Instance.RemoveModApplication(this.appBtn);
        }


        private void Log(string msg)
        {
            Debug.Log("[DangIt][Runtime]: " + msg);
        }
    }
}