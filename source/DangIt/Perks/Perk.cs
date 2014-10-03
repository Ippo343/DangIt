using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    public class Perk
    {
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
            if (requirements.Count == 0)
                return true;
            else
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


        public static int MinDistance(this List<Perk> perks, List<Perk> requirements)
        {
            if (requirements.Count == 0)
                return perks.Select(p => (int)p.SkillLevel).Max();
            else
            {
                int min = 3;

                foreach (Perk p in perks)
                {
                    Perk other = requirements.Where(o => o.Specialty == p.Specialty).SingleOrDefault();
                    if (other != null)
                    {
                        int diff = p.SkillLevel - other.SkillLevel;
                        min = (diff < min) ? diff : min;
                    }
                }

                return min;
            }            
        }


    }

}
