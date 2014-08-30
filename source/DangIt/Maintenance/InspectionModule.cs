using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    class InspectionModule : PartModule
    {

        public override void OnStart(PartModule.StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.StartCoroutine("RuntimeFetch");
            }
        }


        IEnumerator RuntimeFetch()
        {
            while (DangIt.Instance == null || !DangIt.Instance.IsReady)
                yield return null;

            this.Events["Inspect"].unfocusedRange = DangIt.Instance.CurrentSettings.MaxDistance;
        }


        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 2f, externalToEVAOnly = true)]
        public void Inspect()
        {
            StringBuilder sb = new StringBuilder();

            List<FailureModule> failModules = part.Modules.OfType<FailureModule>().ToList();

            if (failModules.Count == 0)
                sb.AppendLine("This part seems to be as good as new");

            foreach (FailureModule fm in failModules)
            {
                fm.TimeOfLastInspection = DangIt.Now();
                sb.AppendLine(fm.InspectionName + ":");
                sb.AppendLine(fm.InspectionMessage());
                sb.AppendLine("");
            }

            DangIt.PostMessage("Inspection results", 
                               sb.ToString(), 
                               MessageSystemButton.MessageButtonColor.BLUE,
                               MessageSystemButton.ButtonIcons.MESSAGE,
                               overrideMute: true);
        }

    }
}
