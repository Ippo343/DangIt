using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Module that causes leaks in resource tanks
    /// </summary>
    public class ModuleTankReliability : ModuleBaseFailure
    {
        public override string DebugName { get { return "DangItTank"; } }
        public override string FailureMessage { get { return "A tank of " + leakables[leakIndex].resourceName + "is leaking!"; } }
        public override string RepairMessage { get { return "Duct tape applied."; } }
        public override string FailGuiName { get { return "Puncture tank"; } }
        public override string EvaRepairGuiName { get { return "Apply duct tape"; } }
        public override bool AgeOnlyWhenActive { get { return true; } }

        /// <summary>
        /// Resource percentage that is lost with each update
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        protected float DecayFraction = 0.005f; // per second

        // Maximum and minimum values for the decay fraction
        [KSPField(isPersistant = true, guiActive = false)]
        public float MinSeverity = 0.0001f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float MaxSeverity = 0.0010f;

        /// <summary>
        /// Index of the leaking resource
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        protected int leakIndex = 0;

        /// <summary>
        /// Previous state of the flow direction
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        protected PartResource.FlowMode oldFlowMode;

        /// <summary>
        /// This is the least of resources on the ship that can leak.
        /// It is re-scanned at every OnStart. Resources that are listed in the
        /// blackList will be ignored: if no leakables are found, the module
        /// disables itself.
        /// </summary>
        protected List<PartResource> leakables;

        /// <summary>
        /// Blacklist of resources that will be ignored by the module.
        /// </summary>
        protected List<string> blackList = new List<string>();


        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            if (!blackList.Contains("ElectricCharge")) blackList.Add("ElectricCharge");
            if (!blackList.Contains("SolidFuel")) blackList.Add("SolidFuel");
            if (!blackList.Contains("SpareParts")) blackList.Add("SpareParts");

            // Get the leakable resources
            leakables = part.Resources.list.FindAll(r => !blackList.Contains(r.resourceName));

            if (leakables.Count < 1)
            {
                this.Log("ERROR: No leakables found, disabling!");
                this.enabled = false;
            }

        }


        public override void DI_Update()
        {
            if (this.hasFailed)
            {
                // Is this better?
                //part.RequestResource(leakables[leakIndex].resourceName, amount)
#if DEBUG
                this.Log("DI_Update, draining " + (leakables[leakIndex].amount * this.DecayFraction));
#endif
                leakables[leakIndex].amount *= (1 - this.DecayFraction);
            }
        }


        public override void DI_Fail()
        {
            // Choose a random severity of the leak
            this.DecayFraction = UnityEngine.Random.Range(MinSeverity, MaxSeverity);
            this.Log("Chosen DecayFraction = " + DecayFraction + " (min = " + MinSeverity + ", max = " + MaxSeverity + ")");

            // Pick a random index to leak.
            this.leakIndex = UnityEngine.Random.Range(0, leakables.Count);

            oldFlowMode = leakables[leakIndex].flowMode;
            leakables[leakIndex].flowMode = PartResource.FlowMode.None; 
        }


        
        public override void DI_EvaRepair()
        {
            leakables[leakIndex].flowMode = oldFlowMode; 
        }

    }
}
