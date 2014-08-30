using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public class ModuleEngineReliability : FailureModule
    {
        EngineManager engines;

        public override string DebugName { get { return "DangItEngines"; } }
        public override string InspectionName { get { return "Engine"; } }
        public override string FailureMessage { get { return "ENGINE FAILURE!"; } }
        public override string RepairMessage { get { return "Engine repaired."; } }
        public override string FailGuiName { get { return "Fail engine"; } }
        public override string EvaRepairGuiName { get { return "Repair engine"; } }
        public override string MaintenanceString { get { return "Clean engine"; } }


        protected override float LambdaMultiplier()
        {
            float x = this.engines.CurrentThrottle;
            return (2*x*x - 2*x + 1.25f);
        }


        public override bool PartIsActive()
        {
            return this.engines.IsActive;
        }



        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.engines = new EngineManager(this.part);
            }
        }


        protected override bool DI_FailBegin()
        {
            return true;
        }

        protected override void DI_Disable()
        {
            this.engines.Disable();
        }

        protected override void DI_EvaRepair()
        {
            this.engines.Enable();
        }

    }
}
