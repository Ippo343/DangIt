using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DangIt.Utilities;

namespace DangIt
{
    public class ModuleControlSurfaceReliability : FailureModule
    {
        ModuleControlSurface controlSurface;

        #region Previous control state

        // The user might have set the control surface to ignore some control inputs
        // We need to store the previous state in order not to override it when repairing the part

        [KSPField(isPersistant = true, guiActive = false)]
        protected bool ignorePitch = false;

        [KSPField(isPersistant = true, guiActive = false)]
        protected bool ignoreRoll = false;

        [KSPField(isPersistant = true, guiActive = false)]
        protected bool ignoreYaw = false;

        #endregion
        

        public override string DebugName { get { return "DangItControlSurface"; } }
        public override string ScreenName { get { return "Control surface"; } }
        public override string FailureMessage { get { return "A control surface is stuck!"; } }
        public override string RepairMessage { get { return "Control surface repaired."; } }
        public override string FailGuiName { get { return "Fail control surface"; } }
        public override string EvaRepairGuiName { get { return "Drown in WD40"; } }
        public override string MaintenanceString { get { return "Lubricate hinges"; } }
		public override string ExtraEditorInfo{ get { return "This part's control surfaces can become stuck if it fails"; } }


        public override bool PartIsActive()
        {
            // Control surfaces are considered active when the ship's in atmosphere
            // TODO: should this be tied to the actual deflection?
            return (this.part.vessel.atmDensity > 0);
        }

        protected override float LambdaMultiplier()
        {
            // The thicker the atmosphere, the higher the chance of failure
            return (float)this.part.vessel.atmDensity;
        }


        protected override void DI_OnLoad(ConfigNode node)
        {
            this.ignorePitch = CUtils.Parse<bool>(node.GetValue("ignorePitch"), defaultTo: false);
            this.ignoreRoll = CUtils.Parse<bool>(node.GetValue("ignoreRoll"), defaultTo: false);
            this.ignoreYaw = CUtils.Parse<bool>(node.GetValue("ignoreYaw"), defaultTo: false);
        }

        protected override void DI_OnSave(ConfigNode node)
        {
            node.SetValue("ignorePitch", this.ignorePitch.ToString());
            node.SetValue("ignoreRoll", this.ignoreRoll.ToString());
            node.SetValue("ignoreYaw", this.ignoreYaw.ToString());
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.controlSurface = this.part.Modules.OfType<ModuleControlSurface>().Single();
            }
        }


        protected override bool DI_FailBegin()
        {
            // Can always fail
            return true;
        }

        protected override void DI_Disable()
        {
            // Remember the user's settings
            this.ignorePitch = this.controlSurface.ignorePitch;
            this.ignoreRoll = this.controlSurface.ignoreRoll;
            this.ignoreYaw = this.controlSurface.ignoreYaw;

            // Lock the control surface
            this.controlSurface.ignorePitch = true;
            this.controlSurface.ignoreRoll = true;
            this.controlSurface.ignoreYaw = true;

            // Disable the module for good measure
            this.controlSurface.enabled = false; 
        }

        protected override void DI_EvaRepair()
        {
            // Enable the module
            this.controlSurface.enabled = true;

            // Restore the previous settings
            this.controlSurface.ignorePitch = this.ignorePitch;
            this.controlSurface.ignoreRoll = this.ignoreRoll;
            this.controlSurface.ignoreYaw = this.ignoreYaw; 
        }

    }
}
