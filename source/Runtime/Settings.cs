using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public partial class DangIt
    {
        public class Settings
        {
            public bool ManualFailures = false;
            public float MaxDistance = 1f;

            // Notification settings
            public bool Messages = true;
            public bool Glow = true;

            public Settings() { }


            public Settings(ConfigNode node)
            {
                if (node != null)
                {
                    ManualFailures = DangIt.Parse<bool>(node.GetValue("ManualFailures"), false);
                    MaxDistance = DangIt.Parse<float>(node.GetValue("MaxDistance"), 1f);

                    Messages = DangIt.Parse<bool>(node.GetValue("Messages"), true);
                    Glow = DangIt.Parse<bool>(node.GetValue("Glow"), true);
                }
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

            internal Settings ShallowClone()
            {
                return (DangIt.Settings)this.MemberwiseClone();
            }
        }
    }
}
