using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public class ModuleReactionWheelReliability : FailureModule
    {
        ModuleReactionWheel torqueModule;

        public override string DebugName { get { return "DangItReactionWheel"; } }
        public override string ScreenName { get { return "Reaction wheel"; } }
        public override string FailureMessage { get { return "Reaction wheel failure!"; } }
        public override string RepairMessage { get { return "Reaction wheel repaired."; } }
        public override string FailGuiName { get { return "Fail reaction wheel"; } }
        public override string EvaRepairGuiName { get { return "Fix reaction wheel"; } }
        public override string MaintenanceString { get { return "Lubricate reaction wheel"; } }
		public override string ExtraEditorInfo{ get { return "This part can stop providing torque if it fails"; } }


        public override bool PartIsActive()
        {
            // A reaction wheel is always spinning
            // Unless the user has turned it off, consider it active even if there's no torque
            return (torqueModule.isEnabled &&
                    torqueModule.wheelState == ModuleReactionWheel.WheelState.Active);
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.torqueModule = this.part.Modules.OfType<ModuleReactionWheel>().Single();            
            }
        }


        protected override bool DI_FailBegin()
        {
            return true;
        }


        protected override void DI_Disable()
        {
            this.torqueModule.OnToggle();
            this.torqueModule.isEnabled = false;
            this.torqueModule.Events["OnToggle"].active = false;    // hides the ability to turn it back on from the user
            this.torqueModule.wheelState = ModuleReactionWheel.WheelState.Broken;
        }


        protected override void DI_EvaRepair()
        {
            this.torqueModule.isEnabled = true;
            this.torqueModule.Events["OnToggle"].active = true;
            this.torqueModule.wheelState = ModuleReactionWheel.WheelState.Active;
        }

    }
}