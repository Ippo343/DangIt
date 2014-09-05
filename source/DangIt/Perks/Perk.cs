using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{

    public class Perk
    {
        // A perk object is immutable
        // once these are set by the constructor, they can never change
        public readonly Specialty Specialty;
        public readonly SkillLevel SkillLevel;

        public Perk(Specialty specialty, SkillLevel level)
        {
            Specialty = specialty;
            SkillLevel = level;
        }


        public override string ToString()
        {
            return this.Specialty.ToString() + ":" + this.SkillLevel.ToString();
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
}
