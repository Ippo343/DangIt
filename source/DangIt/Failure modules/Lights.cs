using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public class ModuleLightsReliability : FailureModule
    {
        ModuleLight lightModule;

        public override string DebugName { get { return "DangItLights"; } }
        public override string ScreenName { get { return "Light bulb"; } }
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


        protected override bool DI_FailBegin()
        {
            return true;
        }

        protected override void DI_Disable()
        {
            this.lightModule.LightsOff();

            // The module needs to be entirely removed from the part,
            // since setting enabled = false still makes it respond to the
            // master light switch at the top of the screen.
            this.part.Modules.Remove(this.lightModule);
        }


        protected override void DI_EvaRepair()
        {
            this.part.Modules.Add(this.lightModule); 
        }

    }
}
