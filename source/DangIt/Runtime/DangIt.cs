using DangIt.Utilities;
using KSP.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Mod's runtime controller and manager.
    /// Provides general utilities, definitions, and handles the user's settings.
    /// </summary>
    public partial class CDangIt : ScenarioModule
    {
        protected static HashSet<string> _blacklist = null;
        /// <summary>
        /// List of resources that must be ignored by tank leaks.
        /// </summary>
        public static HashSet<string> LeakBlackList
        {
            get
            {
                if (CDangIt._blacklist == null)
                {
                    CDangIt._blacklist = new HashSet<string> { "ElectricCharge", "SolidFuel", "SpareParts" };
                    try
                    {
                        string path = CUtils.GetConfigFilePath("blacklist.txt");
                        CUtils.Log("Loading the leak blacklist from " + path);
                   
                        foreach (string resource in File.ReadAllLines(path)                     // read all lines
                                                        .Select(l => l.Trim())                  // trim whitespace
                                                        .Where(l => !String.IsNullOrEmpty(l))   // ignore blanks
                                                        .Where(l => !l.StartsWith("//")         // allow C-style comments
                                                                 && !l.StartsWith("#")))        // allow bash-style comments
                        {
                            CUtils.Log("Adding " + resource + " to the leak blacklist");
                            CDangIt._blacklist.Add(resource);
                        }

                    }
                    catch (Exception e)
                    {
                        CUtils.Log("Exception while loading the leak blacklist: " + e.Message);
                        throw;
                    }
                }

                return CDangIt._blacklist;
            }
        }

        private CDangIt.Settings currentSettings;
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
				CDangIt.Instance.StartPartInfoCacheReload ();
				if (FindObjectOfType<AlarmManager> () != null) {
					FindObjectOfType<AlarmManager> ().UpdateSettings ();
				}
            }
        }
        

        /// <summary>
        /// Return the current running instance.
        /// </summary>
        public static CDangIt Instance { get; private set; }


        /// <summary>
        /// Returns true if the instance is initialized and ready to work.
        /// </summary>
        public bool IsReady { get; private set; }        

        public CDangIt()
        {
            Debug.Log("[CDangIt]: Instantiating runtime.");

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
            Debug.Log("[CDangIt][Runtime]: " + msg);
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