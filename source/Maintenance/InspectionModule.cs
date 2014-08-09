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

            this.Events["Inspect"].unfocusedRange = DangIt.Instance.Settings.MaxDistance;
        }


        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 1f, externalToEVAOnly = true)]
        public void Inspect()
        {
            StringBuilder sb = new StringBuilder();

            List<FailureModule> failModules = part.Modules.OfType<FailureModule>().ToList();

            if (failModules.Count == 0)
                sb.AppendLine("This part seems to be as good as new");

            foreach (FailureModule fm in failModules)
            {
                fm.TimeOfLastInspection = DangIt.Now();
                sb.AppendLine(fm.InspectionName + ": " + fm.InspectionMessage());
            }

            //ScreenMessages.PostScreenMessage(sb.ToString(), 5f, ScreenMessageStyle.UPPER_LEFT);

            // Post the inspection result as a new message in the message system
            MessageSystem.Message msg = new MessageSystem.Message(
                "Inspection result",
                sb.ToString(),
                MessageSystemButton.MessageButtonColor.BLUE,
                MessageSystemButton.ButtonIcons.MESSAGE);
            MessageSystem.Instance.AddMessage(msg);
        }

    }
}
