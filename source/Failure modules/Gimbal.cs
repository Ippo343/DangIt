using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public class ModuleGimbalReliability : FailureModule
    {
        ModuleGimbal gimbalModule;
        EngineManager engineManager;

        public override string DebugName { get { return "DangItGimbal"; } }
        public override string InspectionName { get { return "Gimbal"; } }
        public override string FailureMessage { get { return "Gimbal failure!"; } }
        public override string RepairMessage { get { return "Gimbal repaired."; } }
        public override string FailGuiName { get { return "Fail gimbal"; } }
        public override string EvaRepairGuiName { get { return "Repair gimbal"; } }
        public override string MaintenanceString { get { return "Lubricate gimbal"; } }


        public override bool PartIsActive()
        {
            return this.engineManager.IsActive;
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.gimbalModule = this.part.Modules.OfType<ModuleGimbal>().Single();
                this.engineManager = new EngineManager(this.part);
            }
        }


        protected override bool DI_FailBegin()
        {
            return true;
        }

        protected override void DI_Disable()
        {
            // Disable the gimbal module
            this.gimbalModule.enabled = false;

            this.gimbalModule.LockGimbal();
            this.gimbalModule.Events["FreeGimbal"].active = false; 
        }


        protected override void DI_EvaRepair()
        {
            // Restore the gimbaling module
            this.gimbalModule.enabled = true;
            this.gimbalModule.FreeGimbal();
            this.gimbalModule.Events["LockGimbal"].active = true; 
        }

    }
}