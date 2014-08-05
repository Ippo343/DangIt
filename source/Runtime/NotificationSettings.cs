using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    public class NotificationSettings
    {
        public bool Messages = true;
        public bool Glow = true;
        public bool Sounds = true;

        public NotificationSettings(ConfigNode node)
        {
            if (node != null)
            {
                Messages = DangIt.Parse<bool>(node.GetValue("messages"), true);
                Glow = DangIt.Parse<bool>(node.GetValue("glow"), true);
                Sounds = DangIt.Parse<bool>(node.GetValue("sounds"), true);
            }
        }
    }
}
