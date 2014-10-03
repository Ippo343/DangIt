using System;
using IO = System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;
using System.Text;
using System.Xml.Serialization;

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

        public List<TrainingCost> trainingCosts;

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
        /// Returns the location of the mod's configuration files
        /// Likely, GameData/DangIt/PluginData/DangIt/
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

            /*  The training costs are stored in an xml file.
             *  The list is obtained by deserializing that file.
             *  The best solution would be a dictionary, but it's challenging to write and load with ConfigNode,
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
            catch (Exception e)
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