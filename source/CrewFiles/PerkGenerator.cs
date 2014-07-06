using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace DangIt
{
    class PerkGenerator : ICrewDataGenerator
    {
        public static string NodeName { get { return "DANGIT_PERKS"; } }        


        public bool MustRun(ScenePhase phase, ConfigNode kerbalData)
        {
            return (!kerbalData.HasNode(NodeName));             
        }


        public ConfigNode Generate(ScenePhase phase, ConfigNode kerbalData)
        {
            ConfigNode perksNode = new ConfigNode(NodeName);

            float stupidity = DangIt.Parse<float>(kerbalData.GetValue("stupidity"), 0.5f);

            if (UnityEngine.Random.Range(0f, 1f) > stupidity)
            {
                ConfigNode mechNode = new ConfigNode("PERK");
                mechNode.AddValue("name", "mechanic");
                mechNode.AddValue("level", UnityEngine.Random.Range(1, 3));
                perksNode.AddNode(mechNode);
            }

            if (UnityEngine.Random.Range(0f, 1f) > stupidity)
            {
                ConfigNode mechNode = new ConfigNode("PERK");
                mechNode.AddValue("name", "electrician");
                mechNode.AddValue("level", UnityEngine.Random.Range(1, 3));
                perksNode.AddNode(mechNode);
            }

            return perksNode;
        }
    }

}