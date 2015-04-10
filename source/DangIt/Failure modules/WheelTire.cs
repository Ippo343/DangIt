using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
	public class ModuleWheelTireReliability : FailureModule
	{
		ModuleWheel wheel;

		public override string DebugName { get { return "DangItWheel_Tire"; } }
		public override string ScreenName { get { return "Tire"; } }
		public override string FailureMessage { get { return "A tire popped!"; } }
		public override string RepairMessage { get { return "Tire replaced."; } }
		public override string FailGuiName { get { return "Pop tire"; } }
		public override string EvaRepairGuiName { get { return "Replace tire"; } }
		public override string MaintenanceString { get { return "Clean and Fill Tire"; } }
		public override string ExtraEditorInfo{ get { return "This part's tire can pop if it fails"; } }


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
			if (!this.wheel.damageable) {
				this.enabled = false;
			}
		}


		protected override bool DI_FailBegin()
		{
			return true;
		}

		protected override void DI_Disable()
		{
			//this.wheel.isDamaged = true; //Do we need this?
			this.wheel.wheels [0].damageWheel ();

			Events ["Maintenance"].active = false; //We should repair with the ModuleWheel UI option
			Events ["EvaRepair"].active = false;
			Events ["Fail"].active = false;
		}

		protected override void DI_Update(){
			if (this.HasFailed) {
				if (!this.wheel.isDamaged) {
					this.EvaRepair ();
				}
			}
		}

		protected override void DI_EvaRepair()
		{
			this.wheel.wheels[0].repairWheel ();
		}

	}
}
