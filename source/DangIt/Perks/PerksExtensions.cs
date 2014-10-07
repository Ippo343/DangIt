using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    /// <summary>
    /// Extensions related to perks.
    /// </summary>
    public static class PerksExtensions
    {
        /// <summary>
        /// Convert a list of perks to a ConfigNode.
        /// The node can be parsed by Perk.FromNode
        /// </summary>
        public static ConfigNode ToNode(this List<Perk> perks)
        {
            ConfigNode result = new ConfigNode(PerkGenerator.NodeName);

            foreach (Perk p in perks)
                result.AddValue("perk", p.ToString());

            return result;
        }


        /// <summary>
        /// Converts a ConfigNode containing perks to a list of Perks.
        /// </summary>
        public static List<Perk> ToPerks(this ConfigNode node)
        {
            List<Perk> result = new List<Perk>();

            foreach (string item in node.GetValues("perk"))
                result.Add(Perk.FromString(item));

            return result;
        }


        /// <summary>
        /// Returns the minimum distance between two list of perks.
        /// The distance is computed by matching the perks by specialty and finding
        /// the minimum of the difference between their SkillLevel.
        /// </summary>
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
