using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Module that causes failures in the power generator of engines
    /// </summary>
    public class ModuleAlternatorReliability : FailureModule
    {
        // The alternator is tied to an engine
        // The engine module also allows to check when the alternator is active
        ModuleEngines engineModule;
        ModuleAlternator alternatorModule;

        public override string DebugName { get { return "DangItAlternator"; } }
        public override string FailureMessage { get { return "Alternator failure!"; } }
        public override string RepairMessage { get { return "Alternator repaired."; } }
        public override string FailGuiName { get { return "Fail alternator"; } }
        public override string EvaRepairGuiName { get { return "Repair alternator"; } }


        // The alternator is only active when the engine is actually firing
        public override bool PartIsActive()
        {
            return DangIt.EngineIsActive(this.engineModule);
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.alternatorModule = this.GetModule<ModuleAlternator>();
                this.engineModule = this.GetModule<ModuleEngines>();
            }
        }


        protected override void DI_FailBegin()
        {
            return;
        }


        protected override void DI_Disable()
        {
            this.alternatorModule.enabled = false;
        }


        protected override void DI_EvaRepair()
        {
            this.alternatorModule.enabled = true; 
        }

    }
}
