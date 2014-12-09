using System;
using System.Collections;
using System.Linq;
using System.Text;
using ippo;
using UnityEngine;
using KSP;

namespace ippo
{
	public class ModuleCoolantReliabilityCore : ippo.FailureModule //Renamed so that it dosen't conflict if user has an old version of Entropy
	{
		EngineManager engines;

		public override string DebugName { get { return "Coolant Line"; } }
		public override string ScreenName { get { return "Coolant Line"; } }
		public override string FailureMessage { get { return "Coolant has begun to leak from an engine!"; } }
		public override string RepairMessage { get { return "Patch Coolant Line"; } }
		public override string FailGuiName { get { return "Fail Coolant Line"; } }
		public override string EvaRepairGuiName { get { return "Replace Coolant Line"; } }
		public override string MaintenanceString { get { return "Replace Coolant Line"; } }
		public override string ExtraEditorInfo{ get { return "This part can begin to overheat if it fails"; } }

		public override bool PartIsActive()
		{
			return this.engines.IsActive;
		}

		protected override void DI_Start(StartState state)
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				// An engine might actually be two engine modules (e.g: SABREs)
				this.engines = new EngineManager(this.part);
			}
		}

		protected override bool DI_FailBegin()
		{
			return true;
		}

		protected override void DI_Disable()
		{
			this.engines.engines.ForEach (e => e.heatProduction *= 3);
			this.engines.enginesFX.ForEach (e => e.heatProduction *= 3);
		}

		protected override void DI_EvaRepair(){
			this.engines.engines.ForEach (e => e.heatProduction /= 3);
			this.engines.enginesFX.ForEach (e => e.heatProduction /= 3);
		}

		protected override void DI_Update(){}
	}
}

