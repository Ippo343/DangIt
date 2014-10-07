using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace ippo
{
    /// <summary>
    /// Perk generator: generates a random set of perks for each new kerbonaut that spawns in the applicants list.
    /// </summary>
    class PerkGenerator : ICrewDataGenerator
    {
        public static readonly string NodeName = "DANGIT_PERKS"; 

        /// <summary>
        /// This generator is run for each kerbal that doesn't already have a node with this name.
        /// </summary>
        public bool MustRun(string phase, ConfigNode kerbalData)
        {
            return (!kerbalData.HasNode(NodeName));             
        }


        /// <summary>
        /// Generates a random set of perks based on the intelligence of the kerbal.
        /// The lower the stupidity, the higher the chance of the kerbal having more skilled perks.
        /// </summary>
        public ConfigNode Generate(string phase, ConfigNode kerbalData)
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