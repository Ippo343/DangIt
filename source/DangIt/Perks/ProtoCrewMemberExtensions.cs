using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace ippo
{
    /// <summary>
    /// Extension methods for ProtoCrewMember to deal with perks
    /// </summary>
    public static class ProtoCrewMemberExtensions
    {
        /// <summary>
        /// Fetch the perks node from CrewFiles
        /// </summary>
        public static ConfigNode GetPerksNode(this ProtoCrewMember kerbal)
        {
            if (!CrewFilesManager.Server.Contains(kerbal))
                CrewFilesManager.Server.RefreshDatabase();

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
                return perksNode.ToPerks();
        }


        /// <summary>
        /// Sets the perks of a kerbal in its CrewFiles record.
        /// </summary>
        public static void SetPerks(this ProtoCrewMember kerbal, List<Perk> perks)
        {
            if (!CrewFilesManager.Server.Contains(kerbal))
                CrewFilesManager.Server.RefreshDatabase();

            ConfigNode kerbalFile = kerbal.GetFullFile();
            ConfigNode perksNode = perks.ToNode();

            if (kerbalFile.HasNode(perksNode.name))
                kerbalFile.SetNode(perksNode.name, perksNode);
            else
                throw new Exception(kerbal.name + " doesn't have a perks node!");
        }

    }
}
