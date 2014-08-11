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
            public float MaxDistance = 1f;          // maximum distance for EVA activities
            public bool Messages = true;            // enable messages and screen posts
            public bool Glow = true;                // enable the part's glow upon failure

            public Settings() { }


            public Settings(ConfigNode node)
            {
                if (node != null && node.name == "SETTINGS")
                {
                    ManualFailures = DangIt.Parse<bool>(node.GetValue("ManualFailures"), false);
                    MaxDistance = DangIt.Parse<float>(node.GetValue("MaxDistance"), 1f);
                    Messages = DangIt.Parse<bool>(node.GetValue("Messages"), true);
                    Glow = DangIt.Parse<bool>(node.GetValue("Glow"), true);
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
