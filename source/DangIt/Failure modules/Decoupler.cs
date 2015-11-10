using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
	public class ModuleDecouplerReliability : FailureModule
	{
		DecouplerManager manager;

		[KSPField(isPersistant = true, guiActive = false)]
		float origPercentage = -1f;

		public override string DebugName { get { return "DangItDecoupler"; } }
		public override string ScreenName { get { return "Decoupler"; } }
		public override string FailureMessage { get { return "EXPLOSIVE BOLT FAILURE!"; } }
		public override string RepairMessage { get { return "Bolts rewired."; } }
		public override string FailGuiName { get { return "Fail decoupler"; } }
		public override string EvaRepairGuiName { get { return "Repair decoupler"; } }
		public override string MaintenanceString { get { return "Replace decoupler"; } }
		public override string ExtraEditorInfo{ get { return "This part's decoupler can silently fail, causing it to have no force upon decouple."; } }

		public override bool PartIsActive()
		{
			return !InputLockManager.lockStack.Keys.Contains("manualStageLock");
		}


		protected override void DI_Start(StartState state)
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				this.manager = new DecouplerManager (this.part);
			}
		}


		protected override bool DI_FailBegin()
		{
			// Can always fail
			return true;
		}

		protected override void DI_Disable()
		{
			this.origPercentage = this.manager.ejectionForcePercent;
			this.manager.ejectionForcePercent = 0;
		}

		protected override void DI_EvaRepair()
		{
			this.manager.ejectionForcePercent = this.origPercentage;
		}

	}
}
