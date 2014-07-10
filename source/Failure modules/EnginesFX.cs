using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Module that causes engines failures.
    /// </summary>
    public class ModuleEngineFXReliability : FailureModule
    {
        ModuleEnginesFX engineModule;

        public override string DebugName { get { return "DangItEnginesFX"; } }
        public override string InspectionName { get { return "Engine"; } }
        public override string FailureMessage { get { return "ENGINE FAILURE!"; } }
        public override string RepairMessage { get { return "Engine repaired."; } }
        public override string FailGuiName { get { return "Fail engine"; } }
        public override string EvaRepairGuiName { get { return "Repair engine"; } }


        protected override float LambdaMultiplier()
        {
            float x = this.engineModule.currentThrottle;
            return (2*x*x - 2 * x + 1.25f);
        }


        // Returns true when the engine is actually in use
        public override bool PartIsActive()
        {
            return DangIt.EngineIsActive(this.engineModule);
        }



        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.engineModule = this.GetModule<ModuleEnginesFX>();
            }
        }


        protected override void DI_FailBegin()
        {
            return;
        }

        protected override void DI_Disable()
        {
            // Shutdown the engine and disable the module
            this.engineModule.Shutdown();
            this.engineModule.enabled = false;

            // The particle effects need to be shut down separately
            this.engineModule.DeactivateLoopingFX();
        }


        protected override void DI_EvaRepair()
        {
            this.engineModule.enabled = true;
        }

    }
}
