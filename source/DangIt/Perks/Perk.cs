using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    public class Perk
    {
        public struct UpgradeCost
        {
            public float Science;
            public float Funds;

            public UpgradeCost(float science, float funds)
            {
                this.Science = science;
                this.Funds = funds;
            }

            public static UpgradeCost FromString(string value)
            {
                char[] sep = { ':' };
                string[] parts = value.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                float science = float.Parse(parts[0]);
                float funds = float.Parse(parts[1]);

                return new UpgradeCost(science, funds);
            }
        }

        // A perk object is immutable
        // once these are set by the constructor, they can never change
        public Specialty Specialty;
        public SkillLevel SkillLevel;

        public Perk(Specialty specialty, SkillLevel level)
        {
            Specialty = specialty;
            SkillLevel = level;
        }


        public override string ToString()
        {
            return this.Specialty.ToString() + ":" + this.SkillLevel.ToString();
        }


        public static List<Perk> FromNode(ConfigNode node)
        {
            List<Perk> result = new List<Perk>();

            foreach (string item in node.GetValues("perk"))
                result.Add(Perk.FromString(item));

            return result;
        }



        public static Perk FromString(string nodeString)
        {
            // String format for a perk is perk = Specialty:Level
            // e.g: Electrician:Skilled
            // or Mechanic:1
            char[] sep = { ':' };
            string[] parts = nodeString.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            // Decode the string
            Specialty specialty = (Specialty)Enum.Parse(typeof(Specialty), parts[0]);
            SkillLevel level = (SkillLevel)Enum.Parse(typeof(SkillLevel), parts[1]);

            return new Perk(specialty, level);

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

    public static class PerksExtensions
    {
        public static ConfigNode ToNode(this List<Perk> perks)
        {
            ConfigNode result = new ConfigNode(PerkGenerator.NodeName);

            foreach (Perk p in perks)
                result.AddValue("perk", p.ToString());

            return result;
        }        
    }

}
