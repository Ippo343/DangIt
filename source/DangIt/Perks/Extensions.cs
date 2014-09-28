using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace ippo
{
    public static class ProtoCrewMemberExtensions
    {
        /// <summary>
        /// Fetch the perks node from CrewFiles
        /// </summary>
        public static ConfigNode GetPerksNode(this ProtoCrewMember kerbal)
        {
            return kerbal.GetNode(PerkGenerator.NodeName);
        }


        /// <summary>
        /// Fetch the list of perks from CrewFiles.
        /// </summary>
        public static List<Perk> GetPerks(this ProtoCrewMember kerbal)
        {
            ConfigNode perksNode = kerbal.GetPerksNode();

            if (perksNode == null)
                throw new Exception(kerbal.name + " doesn't have a perks node!");
            else
                return Perk.FromNode(perksNode);
        }


        /// <summary>
        /// Sets the perks of a kerbal in its CrewFiles record.
        /// </summary>
        public static void SetPerks(this ProtoCrewMember kerbal, List<Perk> perks)
        {
            ConfigNode kerbalFile = kerbal.GetFullFile();
            ConfigNode perksNode = perks.ToNode();

            if (kerbalFile.HasNode(perksNode.name))
                kerbalFile.SetNode(perksNode.name, perksNode);
            else
                throw new Exception(kerbal.name + " doesn't have a perks node!");
        }

    }
}
