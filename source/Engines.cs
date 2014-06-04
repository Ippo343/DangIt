using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Module that causes engines failures.
    /// </summary>
    public class ModuleEngineReliability : ModuleBaseFailure
    {
        ModuleEngines engineModule;

        public override string DebugName { get { return "DangItEngines"; } }
        public override string FailureMessage { get { return "ENGINE FAILURE!"; } }
        public override string RepairMessage { get { return "Engine repaired."; } }
        public override string FailGuiName { get { return "Fail engine"; } }
        public override string EvaRepairGuiName { get { return "Repair engine"; } }
        public override bool AgeOnlyWhenActive { get { return true; } }


        public override float LambdaMultiplier()
        {
            float x = this.engineModule.currentThrottle;
            return (x + (float)(0.5 * Math.Pow(x, 5)));
        }


        // Returns true when the engine is actually in use
        public override bool PartIsActive()
        {
            return (this.enabled && 
                    this.engineModule.EngineIgnited && 
                   (this.engineModule.currentThrottle > 0));
        }



        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            this.engineModule = part.Modules.OfType<ModuleEngines>().First<ModuleEngines>();
        }


        public override void DI_Fail()
        {
            // Shutdown the engine and disable the module
            this.engineModule.Shutdown();
            this.engineModule.enabled = false;

            // The particle effects need to be shut down separately
            this.engineModule.DeactivatePowerFX();
            this.engineModule.DeactivateRunningFX();
        }


        public override void DI_EvaRepair()
        {
            this.engineModule.enabled = true;
        }

    }
}
