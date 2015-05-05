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
			List<FailureModule> raw_fails = this.part.Modules.OfType<FailureModule>().ToList();

			List<FailureModule> fails = new List<FailureModule>();

			foreach (FailureModule fm in raw_fails) {
				if (fm.DI_ShowInfoInEditor ()) { //Make sure the module wants to show info in the editor
					fails.Add (fm);
				}
			}

			if (fails.Count == 0)   // no failure module, return a placeholder message
                return "This part has been built to last";
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (FailureModule fm in fails)
                {

					double EOL = Math.Round (Math.Max (-fm.LifeTime * Math.Log (1 / fm.MTBF), 0));

					sb.AppendLine (fm.ScreenName);
					sb.AppendLine (" - MTBF: " + fm.MTBF + " hours");
					sb.AppendLine (" - Lifetime: " + fm.LifeTime + " hours");
					sb.AppendLine (" - EOL : " + EOL + " hours");
					sb.AppendLine (" - Repair cost: " + fm.RepairCost);
					sb.AppendLine (" - Priority: " + fm.Priority);

					if (fm.ExtraEditorInfo != "") {
						sb.AppendLine (" - " + fm.ExtraEditorInfo); //Append any extra info the module wants to add
					}

                    if (!string.IsNullOrEmpty(fm.ExperienceRequirements.Key))
                    {
                        sb.AppendLine(string.Format(
							" - Servicing requires a level {0} {1}",
                            fm.ExperienceRequirements.Value,
                            fm.ExperienceRequirements.Key));
                    }

					sb.AppendLine ();
                }

                return sb.ToString();
            }

        }
    }
}
