using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    public class ModuleReliabilityInfo : PartModule
    {
        public override string GetInfo()
        {
            List<FailureModule> fails = this.part.Modules.OfType<FailureModule>().ToList();

            if (fails.Count == 0)
                return "This part has been built to last";
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (FailureModule fm in fails)
                {
                    double EOL = Math.Round(Math.Max(-fm.LifeTime * Math.Log(1 / fm.MTBF), 0));

                    sb.AppendLine(fm.InspectionName);
                    sb.AppendLine(" - MTBF: " + fm.MTBF + " hours");
                    sb.AppendLine(" - Lifetime: " + fm.LifeTime + " hours");
                    sb.AppendLine(" - EOL : " + EOL + " hours");
                    sb.AppendLine(" - Repair cost: " + fm.RepairCost);
                    sb.AppendLine(" ");
                }

                return sb.ToString();
            }

        }
    }
}
