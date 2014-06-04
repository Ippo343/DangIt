using System;
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
    public class ModuleBatteryReliability : ModuleBaseFailure
    {
        /// <summary>
        /// List of all the Electric Charge resources defined by the part.
        /// I can't think of a reason why you should have more than one, but you never know.
        /// </summary>
        protected List<PartResource> batteries;

        public override string DebugName { get { return "DangItBattery"; } }
        public override string FailureMessage { get { return "A battery has short-circuited!"; } }
        public override string RepairMessage { get { return "Battery repaired."; } }
        public override string FailGuiName { get { return "Fail battery"; } }
        public override string EvaRepairGuiName { get { return "Repair battery"; } }
        public override bool AgeOnlyWhenActive { get { return false; } }


        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            batteries = part.Resources.list.FindAll(r => r.resourceName == "ElectricCharge");
            if (batteries.Count < 1)
            {
                Debug.Log(this.DebugName + "[" + this.GetInstanceID() + "]: no batteries found, disabling.");
                this.enabled = false;
            }
        }


        public override void DI_Fail()
        {
            foreach (PartResource b in batteries)
            {
                b.amount = 0;
                b.flowMode = PartResource.FlowMode.None;
            }
        }


        public override void DI_EvaRepair()
        {
            foreach (PartResource b in batteries)
            {
                b.flowMode = PartResource.FlowMode.Both;
            } 
        }

    }
}
