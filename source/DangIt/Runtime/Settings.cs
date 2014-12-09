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
            public bool ManualFailures = false;     // initiate failures manually
            public float MaxDistance = 2f;          // maximum distance for EVA activities
            public bool Messages = true;            // enable messages and screen posts
			public bool Glow = true;                // enable the part's glow upon failure
			public bool SoundNotifications = true;                // beep incessantly upon failure
			public int  SoundLoops = 10;                // number of times to beep

            public Settings() { }


            public Settings(ConfigNode node)
            {
                if (node != null && node.name == "SETTINGS")
                {
                    ManualFailures = DangIt.Parse<bool>(node.GetValue("ManualFailures"), false);
                    MaxDistance = DangIt.Parse<float>(node.GetValue("MaxDistance"), 1f);
                    Messages = DangIt.Parse<bool>(node.GetValue("Messages"), true);
					Glow = DangIt.Parse<bool>(node.GetValue("Glow"), true);
					SoundNotifications = DangIt.Parse<bool>(node.GetValue("SoundNotifications"), true);
					SoundLoops = DangIt.Parse<int>(node.GetValue("SoundLoops"), 10);
                }
                else
                    throw new Exception("Invalid node!");
            }
            

            public ConfigNode ToNode()
            {
                ConfigNode result = new ConfigNode("SETTINGS");

                result.AddValue("ManualFailures", ManualFailures.ToString());
                result.AddValue("MaxDistance", MaxDistance.ToString());

				result.AddValue("Messages", Messages.ToString());
				result.AddValue("Glow", Glow.ToString());

				result.AddValue("SoundNotifications", Messages.ToString());
				result.AddValue("SoundLoops", SoundLoops.ToString());

                return result;
            }


            /// <summary>
            /// Returns a shallow copy of the object (field-wise).
            /// </summary>
            public Settings ShallowClone()
            {
                return (DangIt.Settings)this.MemberwiseClone();
            }
        }
    }
}
