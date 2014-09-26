using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewFilesInterface;

namespace ippo
{
    public static class ProtoCrewMemberExtensions
    {
        public static List<Perk> GetPerks(this ProtoCrewMember kerbal)
        {
            ConfigNode perksNode = kerbal.GetNode(PerkGenerator.NodeName);

            if (perksNode == null)
                throw new Exception(kerbal.name + " doesn't have a perks node!");
            else
                return Perk.FromNode(perksNode);
        }

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
