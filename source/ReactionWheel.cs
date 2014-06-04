using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{

    /// <summary>
    /// Module that causes failures in reaction wheels
    /// </summary>
    public class ModuleReactionWheelReliability : ModuleBaseFailure
    {
        ModuleReactionWheel torqueModule;

        public override string DebugName { get { return "DangItReactionWheel"; } }
        public override string FailureMessage { get { return "Reaction wheel failure!"; } }
        public override string RepairMessage { get { return "Reaction wheel repaired."; } }
        public override string FailGuiName { get { return "Fail reaction wheel"; } }
        public override string EvaRepairGuiName { get { return "Fix reaction wheel"; } }
        public override bool AgeOnlyWhenActive { get { return true; } }


        public override bool PartIsActive()
        {
            return (torqueModule.PitchTorque > 0 ||
                    torqueModule.RollTorque > 0 ||
                    torqueModule.YawTorque > 0);
        }


        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            this.torqueModule = part.Modules.OfType<ModuleReactionWheel>().First();
        }


        public override void DI_Fail()
        {
            this.torqueModule.enabled = false; 
        }


        public override void DI_EvaRepair()
        {
            this.torqueModule.enabled = true;
        }

    }
}