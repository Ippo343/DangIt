using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public partial class DangIt
    {
        /// <summary>
        /// General user settings about notifications and gameplay elements
        /// </summary>
        public class Settings
        {
			public bool EnabledForSave = true;      // is enabled for this save file
            public bool ManualFailures = false;     // initiate failures manually
            public float MaxDistance = 2f;          // maximum distance for EVA activities
            public bool Messages = true;            // enable messages and screen posts
			public bool Glow = true;                // enable the part's glow upon failure
			public bool RequireExperience = true;   // enable requiring experience levels
			public int  Pri_Low_SoundLoops = 0;     // number of times to beep
			public int  Pri_Medium_SoundLoops = 2;  // number of times to beep
			public int  Pri_High_SoundLoops = -1;   // number of times to beep
			public int  AlarmVolume = 100;          // volume of the alarm (1-100)   
			public bool DebugStats = false;			// show debug stats of the part in the right-click menu

            public Settings() { }

			public int GetSoundLoopsForPriority(string priority)
			{
				return GetSoundLoopsForPriority (DangIt.PriorityIntFromString (priority));
			}

			public int GetSoundLoopsForPriority(int priority)
			{
				if (priority == 1)
					return Pri_Low_SoundLoops;
				if (priority==2)
					return Pri_Medium_SoundLoops;
				if (priority==3)
					return Pri_High_SoundLoops;
				return 0;
			}

			public float GetMappedVolume()
			{
				return ((float)this.AlarmVolume / 100f);
			}
				
            public Settings(ConfigNode node)
            {
                if (node != null && node.name == "SETTINGS")
                {
					EnabledForSave = DangIt.Parse<bool>(node.GetValue("EnabledForSave"), true);
                    ManualFailures = DangIt.Parse<bool>(node.GetValue("ManualFailures"), false);
                    MaxDistance = DangIt.Parse<float>(node.GetValue("MaxDistance"), 1f);
                    Messages = DangIt.Parse<bool>(node.GetValue("Messages"), true);
					Glow = DangIt.Parse<bool>(node.GetValue("Glow"), true);
					Pri_Low_SoundLoops = DangIt.Parse<int>(node.GetValue("Pri_Low_Loops"), 0);
					Pri_Medium_SoundLoops = DangIt.Parse<int>(node.GetValue("Pri_Medium_Loops"), 0);
					Pri_High_SoundLoops = DangIt.Parse<int>(node.GetValue("Pri_High_Loops"), 0);
					AlarmVolume = DangIt.Parse<int>(node.GetValue("AlarmVolume"), 100);
					RequireExperience = DangIt.Parse<bool>(node.GetValue("RequireExperience"), true);
					DebugStats = DangIt.Parse<bool>(node.GetValue("DebugStats"), false);
                }
                else
                    throw new Exception("Invalid node!");
            }
            

            public ConfigNode ToNode()
            {
                ConfigNode result = new ConfigNode("SETTINGS");

				result.AddValue("EnabledForSave", EnabledForSave.ToString ());
                result.AddValue("ManualFailures", ManualFailures.ToString());
                result.AddValue("MaxDistance", MaxDistance.ToString());

				result.AddValue("Messages", Messages.ToString());
				result.AddValue("Glow", Glow.ToString());

				result.AddValue("Pri_Low_Loops", Pri_Low_SoundLoops.ToString());
				result.AddValue("Pri_Medium_Loops", Pri_Medium_SoundLoops.ToString());
				result.AddValue("Pri_High_Loops", Pri_High_SoundLoops.ToString());

				result.AddValue("AlarmVolume", AlarmVolume.ToString());

				result.AddValue ("RequireExperience", RequireExperience.ToString ());
				result.AddValue ("DebugStats", DebugStats.ToString ());

                return result;
            }


            /// <summary>
            /// Returns a shallow copy of the object (field-wise).
            /// </summary>
            public Settings ShallowClone()
            {
                return (DangIt.Settings)this.MemberwiseClone();
            }

			// Get the max servicing temp from DANGIT_SETTINGS
			public int GetMaxServicingTemp(){
				UrlDir.UrlConfig[] node = GameDatabase.Instance.GetConfigs ("DANGIT_SETTINGS");
				foreach (UrlDir.UrlConfig curSet in node)
				{
					int val = DangIt.Parse<int> (curSet.config.GetValue ("MaxServicingTemp"), 400);
					DangIt.Instance.Log ("Found a DANGIT_SETTINGS, its MaxServiceTemp is " + val.ToString ());
					return val;
				}
				return 400;
			}
        }
    }
}
