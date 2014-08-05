using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace ippo
{
    /// <summary>
    /// Module that causes leaks in resource tanks
    /// </summary>
    public class ModuleBatteryReliability : FailureModule
    {
        protected PartResource battery;

        public override string DebugName { get { return "DangItBattery"; } }
        public override string InspectionName { get { return "Battery"; } }
        public override string FailureMessage { get { return "A battery has short-circuited!"; } }
        public override string RepairMessage { get { return "Battery repaired."; } }
        public override string FailGuiName { get { return "Fail battery"; } }
        public override string EvaRepairGuiName { get { return "Repair battery"; } }
        public override string MaintenanceString { get { return "Replace battery"; } }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                int idx = part.Resources.list.FindIndex(r => r.resourceName == "ElectricCharge");
                if (idx < 0)
                {
                    throw new Exception("No ElectricCharge was found in the part!");
                }
                else
                {
                    battery = part.Resources[idx];
                } 
            }
        }


        protected override void DI_FailBegin()
        {
            return;
        }

        protected override void DI_Disable()
        {
            battery.amount = 0;
            battery.flowMode = PartResource.FlowMode.None;
        }


        protected override void DI_EvaRepair()
        {
            battery.flowMode = PartResource.FlowMode.Both;
        }

    }
}
