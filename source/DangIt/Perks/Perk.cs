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


        /// <summary>
        /// Return a compact string representation of the perk.
        /// e.g: "Mechanic:Skilled"
        /// </summary>
        public override string ToString()
        {
            return this.Specialty.ToString() + ":" + this.SkillLevel.ToString();
        }


        /// <summary>
        /// Tries to parse a string into a Perk object.
        /// The string must have the same format as used by Perk.ToString()
        /// </summary>
        public static Perk FromString(string nodeString)
        {
            char[] sep = { ':' };
            string[] parts = nodeString.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            // Decode the string
            Specialty specialty = (Specialty)Enum.Parse(typeof(Specialty), parts[0]);
            SkillLevel level = (SkillLevel)Enum.Parse(typeof(SkillLevel), parts[1]);

            return new Perk(specialty, level);
        }


        /// <summary>
        /// Checks if the perks of a kerbal satisfy a list of required perks.
        /// A requirement is met if the specialty is the same and the level is equal or higher.
        /// </summary>
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
}
