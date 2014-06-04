using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangIt
{
    class ModuleRCSReliability : ModuleBaseFailure
    {
        ModuleRCS rcsModule;

        public override string DebugName { get { return "DangItRCS"; } }
        public override string FailureMessage { get { return "A thruster has stopped thrusting!"; } }
        public override string RepairMessage { get { return "Thruster back online."; } }
        public override string FailGuiName { get { return "Fail thruster"; } }
        public override string EvaRepairGuiName { get { return "Repair thruster"; } }
        public override bool AgeOnlyWhenActive { get { return true; } }


        public override bool PartIsActive()
        {
            foreach (float force in rcsModule.thrustForces)
            {
                if (force > 0) return true;
            }

            return false;
        }


        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            rcsModule = this.part.Modules.OfType<ModuleRCS>().First();
        }

        public override void DI_Fail()
        {
            rcsModule.enabled = false;
        }


        public override void DI_EvaRepair()
        {
            rcsModule.enabled = true;           
        }

    }
}
