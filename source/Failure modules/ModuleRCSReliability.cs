using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    class ModuleRCSReliability : FailureModule
    {
        ModuleRCS rcsModule;

        public override string DebugName { get { return "DangItRCS"; } }
        public override string InspectionName { get { return "RCS Thruster"; } }
        public override string FailureMessage { get { return "A thruster has stopped thrusting!"; } }
        public override string RepairMessage { get { return "Thruster back online."; } }
        public override string FailGuiName { get { return "Fail thruster"; } }
        public override string EvaRepairGuiName { get { return "Repair thruster"; } }
        public override string MaintenanceString { get { return "Clean thruster"; } }


        public override bool PartIsActive()
        {
            foreach (float force in rcsModule.thrustForces)
            {
                if (force > 0) return true;
            }

            return false;
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                rcsModule = this.part.Modules.OfType<ModuleRCS>().Single();         
            }
        }


        protected override void DI_FailBegin()
        {
            return;
        }

        protected override void DI_Disable()
        {
            rcsModule.enabled = false;
        }


        protected override void DI_EvaRepair()
        {
            rcsModule.enabled = true;           
        }

    }
}
