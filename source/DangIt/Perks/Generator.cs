using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace ippo
{
    class PerkGenerator : ICrewDataGenerator
    {
        public static readonly string NodeName = "DANGIT_PERKS"; 


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
                //TODO: Improve generation algorithm

                SkillLevel level = (SkillLevel)( (int)Math.Floor(UnityEngine.Random.Range(0f, intelligence) / 0.25f) );
                Perk perk = new Perk(spec, level);
                perksNode.AddValue("perk", perk.ToString());
            }

            return perksNode;
        }
    }

}