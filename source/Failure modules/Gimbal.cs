using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{

    /// <summary>
    /// Module that causes failures in the thrust vectoring of engines.
    /// </summary>
    public class ModuleGimbalReliability : FailureModule
    {
        ModuleGimbal gimbalModule;
        ModuleEngines engineModule;

        public override string DebugName { get { return "DangItGimbal"; } }
        public override string FailureMessage { get { return "Gimbal failure!"; } }
        public override string RepairMessage { get { return "Gimbal repaired."; } }
        public override string FailGuiName { get { return "Fail gimbal"; } }
        public override string EvaRepairGuiName { get { return "Repair gimbal"; } }


        public override bool PartIsActive()
        {
            return DangIt.EngineIsActive(this.engineModule);
        }


        protected override void DI_OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.gimbalModule = part.Modules.OfType<ModuleGimbal>().First();
                this.engineModule = part.Modules.OfType<ModuleEngines>().First(); 
            }
        }


        protected override void DI_Fail()
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