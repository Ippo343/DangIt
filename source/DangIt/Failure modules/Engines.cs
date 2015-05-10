using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public class ModuleEngineReliability : FailureModule
    {
		EngineManager engines;
		ModuleSurfaceFX surfaceFX;

		[KSPField(isPersistant = true, guiActive = false)]
		float oldSurfaceFXMaxDistance = -1f;

        public override string DebugName { get { return "DangItEngines"; } }
        public override string ScreenName { get { return "Engine"; } }
        public override string FailureMessage { get { return "ENGINE FAILURE!"; } }
        public override string RepairMessage { get { return "Engine repaired."; } }
        public override string FailGuiName { get { return "Fail engine"; } }
        public override string EvaRepairGuiName { get { return "Repair engine"; } }
        public override string MaintenanceString { get { return "Clean engine"; } }
		public override string ExtraEditorInfo{ get { return "This part's engine can stop providing thrust if it fails"; } }


        protected override float LambdaMultiplier()
        {
            // Engines are designed to operate at max throttle
            // this introduces a heavy penalty for low throttle values
            float x = this.engines.CurrentThrottle;
            return (5 - x);
        }


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
				// Catch if the part has a ModuleSurfaceFX
				if (this.part.Modules.OfType<ModuleSurfaceFX> ().Any ()) {
					surfaceFX = this.part.Modules.OfType<ModuleSurfaceFX>().Single();
				}
            }
        }


        protected override bool DI_FailBegin()
        {
            // Can always fail
            return true;
        }

        protected override void DI_Disable()
        {
            this.engines.Disable();
			if (this.surfaceFX){ // If we have a ModuleSurfaceFX, cache it's old max distance and set it to -1 to block its firing
				this.oldSurfaceFXMaxDistance = this.surfaceFX.maxDistance;
				this.surfaceFX.maxDistance = -1;
			}
        }

        protected override void DI_EvaRepair()
        {
            this.engines.Enable();
			if (this.surfaceFX) { // Reenable FX
				this.surfaceFX.maxDistance = this.oldSurfaceFXMaxDistance;
			}
        }

    }
}
