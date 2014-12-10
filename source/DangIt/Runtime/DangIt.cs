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
        public static List<string> LeakBlackList
        {
            get
            {
                if (_leakBlackList == null) // Load the file on the first call
                {
                    _leakBlackList = new List<string>();
                    ConfigNode blacklistFile = ConfigNode.Load(DangIt.GetConfigFilePath("BlackList.cfg"));
                    try
                    {
                        ConfigNode blackListNode = blacklistFile.GetNode("BLACKLIST");
                        foreach (string item in blackListNode.GetValues("ignore"))
                            _leakBlackList.Add(item);
                    }
                    catch (Exception e)
                    {
                        _leakBlackList.Add("ElectricCharge");
                        _leakBlackList.Add("SolidFuel");
                        _leakBlackList.Add("SpareParts");

                        Debug.Log("[DangIt]: An exception occurred while loading the resource blacklist and a default one has been created. " + e.Message);
                    } 
                }

                return _leakBlackList;
            }
            set
            {
                _leakBlackList = value;
            }
        }
        internal static List<string> _leakBlackList = null;


        /// <summary>
        /// List of the costs (science and funds) that are requested to train a kerbal.
        /// </summary>
        public List<TrainingCost> trainingCosts;


        private DangIt.Settings currentSettings;
		public  AlarmManager    alarmManager;
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

        public DangIt()
        {
            Debug.Log("[DangIt]: Instantiating runtime.");

			Debug.Log ("[DangIt]: Starting Alarm Controller...");
			this.alarmManager=gameObject.AddComponent<AlarmManager> ();

            #region Training costs

            /*  The training costs are stored in an xml file instead of a ConfigNode because it allows easy
             *  serialization and deserialization. The best solution would be a dictionary, but it's challenging to write and load with ConfigNode,
             *  while xml serialization of a list is a piece of cake.
             */
            trainingCosts = this.DefaultTrainingCosts();
            XmlSerializer serializer = new XmlSerializer(typeof(List<TrainingCost>));

            if (IO.File.Exists(DangIt.GetConfigFilePath("Training.xml")))
            {
                IO.FileStream fs = null;
                try
                {
                    fs = IO.File.Open(DangIt.GetConfigFilePath("Training.xml"),
                                      IO.FileMode.OpenOrCreate, 
                                      IO.FileAccess.Read,
                                      IO.FileShare.None);
                    trainingCosts = (List<TrainingCost>)serializer.Deserialize(fs);
                }
                catch (Exception e)
                {
                    trainingCosts = this.DefaultTrainingCosts();
                    this.Log("An exception occurred when loading the training costs list and a default one has been created. " + e.Message + e.StackTrace);
                }
                finally
                {
                    if (fs != null) fs.Close();
                }
            }
            else
            {
                this.Log("The training costs list didn't exist, trying to write the default one.");

                IO.FileStream fs = null;
                try
                {
                    fs = IO.File.Open(DangIt.GetConfigFilePath("Training.xml"),
                                      IO.FileMode.Create,
                                      IO.FileAccess.Write,
                                      IO.FileShare.None);
                    serializer.Serialize(fs, trainingCosts);
                }
                catch (Exception e)
                {
                    trainingCosts = this.DefaultTrainingCosts();
                    this.Log("An exception occurred when writing the training costs list and a default one has been created. " + e.Message + e.StackTrace);
                }
                finally
                {
                    if (fs != null) fs.Close();
                }
            }
            

            #endregion

            // Now the instance is built and can be exposed, but it is not yet ready until after OnLoad
            Instance = this;
            this.IsReady = false;

            // Add the button to the stock toolbar
            this.StartCoroutine("AddAppButton");
        }


        private List<TrainingCost> DefaultTrainingCosts()
        {
            List<TrainingCost> result = new List<TrainingCost>();

            result.Add(new TrainingCost(SkillLevel.Unskilled, science: 10, funds: 10000));
            result.Add(new TrainingCost(SkillLevel.Normal, science: 50, funds: 50000));
            result.Add(new TrainingCost(SkillLevel.Skilled, science: 120, funds: 120000));

            return result;
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