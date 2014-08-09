using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    public partial class DangIt
    {
        public class DangSettings
        {
            // Debug / cheat options
            public bool ManualFailures = false;
            public bool FreeRepairs = false;
            public float MaxDistance = 1f;

            // Notification settings
            public bool Messages = true;
            public bool Glow = true;
            public bool Sounds = true;

            public DangSettings() { }


            public DangSettings(ConfigNode node)
            {
                if (node != null)
                {
                    ManualFailures = DangIt.Parse<bool>(node.GetValue("ManualFailures"), false);
                    FreeRepairs = DangIt.Parse<bool>(node.GetValue("FreeRepairs"), false);
                    MaxDistance = DangIt.Parse<float>(node.GetValue("MaxDistance"), 1f);

                    Messages = DangIt.Parse<bool>(node.GetValue("Messages"), true);
                    Glow = DangIt.Parse<bool>(node.GetValue("Glow"), true);
                    Sounds = DangIt.Parse<bool>(node.GetValue("Sounds"), true);
                }
            }
            

            public ConfigNode ToNode()
            {
                ConfigNode result = new ConfigNode("SETTINGS");

                result.AddValue("ManualFailures", ManualFailures.ToString());
                result.AddValue("FreeRepairs", FreeRepairs.ToString());
                result.AddValue("MaxDistance", MaxDistance.ToString());

                result.AddValue("Messages", Messages.ToString());
                result.AddValue("Glow", Glow.ToString());
                result.AddValue("Sounds", Sounds.ToString());

                return result;
            }

        }
    }
}
