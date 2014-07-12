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
    // Original code by TaranisElsu in TAC Life Support
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModules : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            ProtoScenarioModule psm = game.scenarios.Find(s => s.moduleName == typeof(DangIt).Name);
            if (psm == null)
            {
                Debug.Log("Adding the scenario module.");
                psm = game.AddProtoScenarioModule(typeof(DangIt), GameScenes.EDITOR,
                                                                  GameScenes.FLIGHT,
                                                                  GameScenes.SPH);
            }
            else
            {
                if (!psm.targetScenes.Any(s => s == GameScenes.FLIGHT))
                    psm.targetScenes.Add(GameScenes.FLIGHT);
                if (!psm.targetScenes.Any(s => s == GameScenes.EDITOR))
                    psm.targetScenes.Add(GameScenes.EDITOR);
                if (!psm.targetScenes.Any(s => s == GameScenes.SPH))
                    psm.targetScenes.Add(GameScenes.SPH);
            }
        }
    }


    public partial class DangIt : ScenarioModule
    {
        public static DangIt Instance { get; private set; }

        public List<string> LeakBlackList;
        public DangItSettings Settings;
        public string SettingsFilePath
        {
            get { return IOUtils.GetFilePathFor(this.GetType(), "DangIt.cfg"); }
        }


        public override void OnLoad(ConfigNode node)
        {
            try
            {
                ConfigNode globalSettings;
                if (System.IO.File.Exists(this.SettingsFilePath))
                    globalSettings = ConfigNode.Load(this.SettingsFilePath);
                else
                {
                    Debug.Log("[DangIt]: the global settings file does not exist, creating a default one");
                    globalSettings = CreateDefaultSettings();
                }

                LeakBlackList = new List<string>();
                ConfigNode blackListNode = globalSettings.GetNode("BLACKLIST");
                foreach (string item in blackListNode.GetValues("ignore"))
                    LeakBlackList.Add(item);

                Settings = new DangItSettings(globalSettings.GetNode("SETTINGS"));
            }
            catch (Exception e)
            {
                Debug.Log("BANANA: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }


        public DangIt()
        {
            Instance = this;
        }


        /// <summary>
        /// Creates the default global configuration node
        /// and saves it to the default path
        /// </summary>
        private ConfigNode CreateDefaultSettings()
        {
            ConfigNode defaultSettings = new ConfigNode("DANGIT");

            ConfigNode blacklist = new ConfigNode("BLACKLIST");
            blacklist.AddValue("ignore", "ElectricCharge");
            blacklist.AddValue("ignore", "SolidFuel");
            blacklist.AddValue("ignore", Spares.Name);
            defaultSettings.AddNode(defaultSettings);

            defaultSettings.AddNode(new DangItSettings().ToNode());

            defaultSettings.Save(this.SettingsFilePath);

            return defaultSettings;
        }

    }

}