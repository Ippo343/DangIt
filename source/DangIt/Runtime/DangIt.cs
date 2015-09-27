using KSP.IO;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
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
				DangIt.Instance.StartPartInfoCacheReload ();
                currentSettings = value;
				if (FindObjectOfType<AlarmManager> () != null) {
					FindObjectOfType<AlarmManager> ().UpdateSettings ();
				}
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

		public void StartPartInfoCacheReload(){
			Log ("Starting refresh of Part Info cache");
			StartCoroutine(RefreshPartInfo());
		}

		private IEnumerator RefreshPartInfo()
		{
			yield return null;
			try{
				foreach (var ap in PartLoader.LoadedPartsList.Where(ap => ap.partPrefab.Modules != null))
				{
					AvailablePart.ModuleInfo target = null;
					foreach (var mi in ap.moduleInfos) {
						if (mi.moduleName == "Reliability Info") {
							target = mi;
						}
					}

					if (target != null & !this.currentSettings.EnabledForSave) {
						ap.moduleInfos.Remove (target);
					}

					if (target == null & this.currentSettings.EnabledForSave) {
						IEnumerable<ModuleReliabilityInfo> reliabilityModules = ap.partPrefab.Modules.OfType<ModuleReliabilityInfo>();
						if (reliabilityModules.Count()!=0){
							AvailablePart.ModuleInfo newModuleInfo = new AvailablePart.ModuleInfo ();
							newModuleInfo.moduleName = "Reliability Info";
							newModuleInfo.info = reliabilityModules.First().GetInfo();
							ap.moduleInfos.Add (newModuleInfo);
						}
					}
				}
				Log("Refresh Finished");
			}catch (Exception e){
				this.Log("ERROR ["+e.GetType().ToString()+"]: " + e.Message + "\n" + e.StackTrace);
			}
		}
    }
}