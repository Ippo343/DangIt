using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    /// <summary>
    /// Module that produces the reliability info about a part to display in the VAB / SPH info tab.
    /// It aggregates the information from all the failure modules into one, instead of many separate tabs.
    /// </summary>
    public class ModuleReliabilityInfo : PartModule
    {
        public override string GetInfo()
        {
            List<FailureModule> fails = this.part.Modules.OfType<FailureModule>().ToList();

            if (fails.Count == 0)   // no failure module, return a placeholder message
                return "This part has been built to last";
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (FailureModule fm in fails)
                {
                    double EOL = Math.Round(Math.Max(-fm.LifeTime * Math.Log(1 / fm.MTBF), 0));

                    sb.AppendLine(fm.ScreenName);
                    sb.AppendLine(" - MTBF: " + fm.MTBF + " hours");
                    sb.AppendLine(" - Lifetime: " + fm.LifeTime + " hours");
                    sb.AppendLine(" - EOL : " + EOL + " hours");
                    sb.AppendLine(" - Repair cost: " + fm.RepairCost);

                    if (fm.PerkRequirements != null && fm.PerkRequirements.Count > 0)
                    {
                        sb.AppendLine("Servicing:");
                        foreach (Perk p in fm.PerkRequirements)
                            sb.AppendLine(" - " + p.ToString());
                    }

                    sb.AppendLine();
                }

                return sb.ToString();
            }

        }
    }
}
