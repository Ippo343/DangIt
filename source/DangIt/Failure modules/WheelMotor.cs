using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
	public class ModuleWheelMotorReliability : FailureModule
	{
		ModuleWheel wheel;

		public override string DebugName { get { return "DangItWheel_Motor"; } }
		public override string ScreenName { get { return "Motor"; } }
		public override string FailureMessage { get { return "A motor burnt out!"; } }
		public override string RepairMessage { get { return "Motor replaced."; } }
		public override string FailGuiName { get { return "Burn out motor"; } }
		public override string EvaRepairGuiName { get { return "Replace motor"; } }
		public override string MaintenanceString { get { return "Clean Motor"; } }
		public override string ExtraEditorInfo{ get { return "This part's motor can burn out if it fails"; } }


		public override bool PartIsActive()
		{
			return this.part.GroundContact;
		}


		protected override void DI_Start(StartState state)
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				this.wheel = this.part.Modules.OfType<ModuleWheel>().Single();
			}
			if (!this.wheel.hasMotor) {
				this.enabled = false;
			}
		}


		protected override bool DI_FailBegin()
		{
			return true;
		}

		protected override void DI_Disable(){
			this.wheel.motorEnabled = false;
			foreach (BaseEvent e in this.wheel.Events) {
				this.Log ("[WheelMotor] e.guiName = "+e.guiName);
				if (e.guiName.EndsWith ("Motor")) {
					e.active = false;
				}
			}
		}

		protected override void DI_EvaRepair(){
			this.wheel.motorEnabled = true;
			foreach (BaseEvent e in this.wheel.Events) {

				if (e.guiName == "Disable Motor") {
					e.active = true;
				}
			}
		}

	}
}
