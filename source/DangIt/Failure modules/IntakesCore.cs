using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ippo;
using UnityEngine;
using KSP;

namespace ippo
{
	public class ModuleIntakeReliabilityCore : ippo.FailureModule //Renamed so that it dosen't conflict if user has an old version of Entropy
	{
		ModuleResourceIntake intake;

		public override string DebugName { get { return "Intake"; } }
		public override string ScreenName { get { return "Intake"; } }
		public override string FailureMessage { get { return "An intake has become clogged"; } }
		public override string RepairMessage { get { return "You have cleared the intake"; } }
		public override string FailGuiName { get { return "Fail intake"; } }
		public override string EvaRepairGuiName { get { return "Repair intake"; } }
		public override string MaintenanceString { get { return "Clean intake"; } }
		public override string ExtraEditorInfo{ get { return "This part's intakes can clog if it fails"; } }

		public override bool PartIsActive()
		{
			// A intake is active if its not landed and in atmosphere
			return !part.vessel.LandedOrSplashed & part.vessel.atmDensity>0 & intake.intakeEnabled;
		}

		protected override void DI_Start(StartState state)
		{
			intake = this.part.Modules.OfType<ModuleResourceIntake>().Single();
		}

		protected override bool DI_FailBegin()
		{
			return true;
		}

		protected override void DI_Disable()
		{
			intake.enabled = false;
		}


		protected override void DI_EvaRepair()
		{
			intake.enabled = true;           
		}
	}
}

