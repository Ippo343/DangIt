using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DangIt
{
    class InspectionModule : PartModule
    {
        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = Static.EvaRepairDistance, externalToEVAOnly = true)]
        public void Inspect()
        {
            StringBuilder sb = new StringBuilder();

            List<FailureModule> failModules = part.Modules.OfType<FailureModule>().ToList();

            if (failModules.Count == 0)
                sb.AppendLine("This part seems to be as good as new");

            foreach (FailureModule fm in failModules)
            {
                fm.TimeOfLastInspection = Static.Now();
                sb.AppendLine(fm.InspectionName + ": " + fm.InspectionMessage());
            }

            ScreenMessages.PostScreenMessage(sb.ToString(), 5f, ScreenMessageStyle.UPPER_LEFT);
        }

    }
}
