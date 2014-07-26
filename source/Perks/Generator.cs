using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace DangIt.Perks
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
            float intelligence = 1 - stupidity;

            // Generate a random perk for each perk type
            foreach (Specialty spec in Enum.GetValues(typeof(Specialty)))
            {
                SkillLevel level = (SkillLevel)( (int)Math.Floor(UnityEngine.Random.Range(0f, intelligence) / 0.25f) );
                Perk perk = new Perk(spec, level);
                perksNode.AddNode(perk.ToNode());
            }

            return perksNode;
        }
    }

}