using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DangIt.Perks
{
    public enum Specialty
    {
        Mechanic,
        Electrician
    }

    public enum SkillLevel
    {
        None = 0,
        Unskilled = 1,
        Normal = 2,
        Skilled = 3
    }


    public class Perk
    {
        private readonly string NodeName = "PERK";

        public readonly Specialty Specialty;
        public readonly SkillLevel SkillLevel;


        public Perk(Specialty specialty, SkillLevel level)
        {
            Specialty = specialty;
            SkillLevel = level;
        }


        public static Perk FromNode(ConfigNode node)
        {
            if (!node.HasValue("specialty") || !node.HasValue("level"))
                throw new Exception("Invalid perk node!");

            return new Perk((Specialty)Enum.Parse(typeof(Specialty), node.GetValue("specialty")), 
                            (SkillLevel)Enum.Parse(typeof(SkillLevel), node.GetValue("level")));
        }


        public ConfigNode ToNode()
        {
            ConfigNode node = new ConfigNode(NodeName);

            node.AddValue("specialty", Specialty.ToString());
            node.AddValue("level", SkillLevel.ToString());

            return node;
        }


        public static bool MeetsRequirement(Perk requirement, Perk perk)
        {
            return ((requirement.Specialty == perk.Specialty) && 
                    (perk.SkillLevel >= requirement.SkillLevel));
        }

        public static bool MeetsRequirement(Perk requirement, List<Perk> perks)
        {
            return perks.Any(p => MeetsRequirement(requirement, p));
        }

        public static bool MeetsRequirement(List<Perk> requirements, List<Perk> perks)
        {
            return requirements.All(r => MeetsRequirement(r, perks));
        }
    }

}
