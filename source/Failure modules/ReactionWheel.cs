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
    public class ModuleReactionWheelReliability : FailureModule
    {
        ModuleReactionWheel torqueModule;

        public override string DebugName { get { return "DangItReactionWheel"; } }
        public override string FailureMessage { get { return "Reaction wheel failure!"; } }
        public override string RepairMessage { get { return "Reaction wheel repaired."; } }
        public override string FailGuiName { get { return "Fail reaction wheel"; } }
        public override string EvaRepairGuiName { get { return "Fix reaction wheel"; } }


        public override bool PartIsActive()
        {
            return (torqueModule.PitchTorque > 0 ||
                    torqueModule.RollTorque > 0 ||
                    torqueModule.YawTorque > 0);
        }


        protected override void DI_OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.torqueModule = part.Modules.OfType<ModuleReactionWheel>().First();
                
            }
        }


        protected override void DI_Fail()
        {
            this.torqueModule.enabled = false; 
        }


        protected override void DI_EvaRepair()
        {
            this.torqueModule.enabled = true;
        }

    }
}