using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    /// <summary>
    /// Module that causes the light bulbs to "burn out"
    /// </summary>
    public class ModuleLightsReliability : FailureModule
    {
        ModuleLight lightModule;

        public override string DebugName { get { return "DangItLights"; } }
        public override string InspectionName { get { return "Light bulb"; } }
        public override string FailureMessage { get { return "A light bulb has burned out."; } }
        public override string RepairMessage { get { return "Bulb replaced."; } }
        public override string FailGuiName { get { return "Fail light bulb"; } }
        public override string EvaRepairGuiName { get { return "Replace light bulb"; } }
        public override string MaintenanceString { get { return "Replace light bulb"; } }


        public override bool PartIsActive()
        {
            return this.lightModule.isOn;
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.lightModule = this.part.Modules.OfType<ModuleLight>().Single();
            }
        }


        protected override void DI_FailBegin()
        {
            return;
        }

        protected override void DI_Disable()
        {
            this.lightModule.LightsOff();
            this.part.Modules.Remove(this.lightModule);

        }


        protected override void DI_EvaRepair()
        {
            this.part.Modules.Add(this.lightModule); 
        }

    }
}
