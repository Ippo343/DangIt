using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DangIt
{
    public class DangItSettings
    {
        public bool Messages = true;
        public bool Glow = true;
        public bool Sounds = true;

        public float RepairDistance = 1f;
        public bool ManualFailures = false;

        public DangItSettings() { }
        public DangItSettings(ConfigNode node)
        {
            if (node != null)
            {
                Messages = DangIt.Parse<bool>(node.GetValue("messages"), true);
                Glow = DangIt.Parse<bool>(node.GetValue("glow"), true);
                Sounds = DangIt.Parse<bool>(node.GetValue("sounds"), true);

                RepairDistance = DangIt.Parse<float>(node.GetValue("repairDistance"), 1f);
                ManualFailures = DangIt.Parse<bool>(node.GetValue("manualFailures"), false);
            }
        }

        public ConfigNode ToNode()
        {
            ConfigNode result = new ConfigNode("SETTINGS");

            result.AddValue("messages", Messages);
            result.AddValue("glow", Glow);
            result.AddValue("sounds", Sounds);

            result.AddValue("repairDistance", RepairDistance);
            result.AddValue("manualFailures", ManualFailures);

            return result;
        }

    }
}
