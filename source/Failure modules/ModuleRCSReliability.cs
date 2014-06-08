using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangIt
{
    class ModuleRCSReliability : FailureModule
    {
        ModuleRCS rcsModule;

        public override string DebugName { get { return "DangItRCS"; } }
        public override string FailureMessage { get { return "A thruster has stopped thrusting!"; } }
        public override string RepairMessage { get { return "Thruster back online."; } }
        public override string FailGuiName { get { return "Fail thruster"; } }
        public override string EvaRepairGuiName { get { return "Repair thruster"; } }


        public override bool PartIsActive()
        {
            foreach (float force in rcsModule.thrustForces)
            {
                if (force > 0) return true;
            }

            return false;
        }


        protected override void DI_OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                rcsModule = this.part.Modules.OfType<ModuleRCS>().First();                
            }
        }


        protected override void DI_Fail()
        {
            rcsModule.enabled = false;
        }


        protected override void DI_EvaRepair()
        {
            rcsModule.enabled = true;           
        }

    }
}
