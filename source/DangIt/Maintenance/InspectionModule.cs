using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    /// <summary>
    /// Module that allows the kerbals to inspect a part to get some information about its state.
    /// </summary>
    public class InspectionModule : PartModule
    {
        public override void OnStart(PartModule.StartState state)
        {
            // Sync settings with the runtime
            if (HighLogic.LoadedSceneIsFlight)
                this.StartCoroutine("RuntimeFetch");
        }


        // Coroutine that waits for the runtime to be ready and the syncs with the settings
        IEnumerator RuntimeFetch()
        {
            while (DangIt.Instance == null || !DangIt.Instance.IsReady)
                yield return null;

            this.Events["Inspect"].unfocusedRange = DangIt.Instance.CurrentSettings.MaxDistance;
        }


        /// <summary>
        /// Inspect the part and produce a report with the messages collected from all the failure modules in the part.
        /// </summary>
        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 2f, externalToEVAOnly = true)]
        public void Inspect()
        {
            StringBuilder sb = new StringBuilder();

            List<FailureModule> failModules = part.Modules.OfType<FailureModule>().ToList();

            // The part doesn't have any failure module:
            // instead of a black message, return a placeholder
            if (failModules.Count == 0)
                sb.AppendLine("This part seems to be as good as new");
            else
            {
                foreach (FailureModule fm in failModules)
                {
                    fm.TimeOfLastInspection = DangIt.Now();     // set the time of inspection so that the module gains the inspection bonus
                    sb.AppendLine(fm.ScreenName + ":");
                    sb.AppendLine(fm.InspectionMessage());
                    sb.AppendLine("");
                }
            }

            DangIt.PostMessage("Inspection results", 
                               sb.ToString(), 
                               MessageSystemButton.MessageButtonColor.BLUE,
                               MessageSystemButton.ButtonIcons.MESSAGE,
                               overrideMute: true);
        }

    }
}
