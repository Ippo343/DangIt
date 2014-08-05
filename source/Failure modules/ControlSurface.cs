using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    /// <summary>
    /// Module that causes failures in aerodynamic control surfaces
    /// </summary>
    public class ModuleControlSurfaceReliability : FailureModule
    {
        ModuleControlSurface controlSurfaceModule;

        [KSPField(isPersistant = true, guiActive = false)]
        protected bool ignorePitch = false;

        [KSPField(isPersistant = true, guiActive = false)]
        protected bool ignoreRoll = false;

        [KSPField(isPersistant = true, guiActive = false)]
        protected bool ignoreYaw = false;
        

        public override string DebugName { get { return "DangItControlSurface"; } }
        public override string InspectionName { get { return "Control surface"; } }
        public override string FailureMessage { get { return "A control surface is stuck!"; } }
        public override string RepairMessage { get { return "Control surface repaired."; } }
        public override string FailGuiName { get { return "Fail control surface"; } }
        public override string EvaRepairGuiName { get { return "Drown in WD40"; } }
        public override string MaintenanceString { get { return "Lubricate hinges"; } }


        public override bool PartIsActive()
        {
            return (this.part.vessel.atmDensity > 0);
        }



        protected override void DI_OnLoad(ConfigNode node)
        {
            this.ignorePitch = DangIt.Parse<bool>(node.GetValue("ignorePitch"), defaultTo: false);
            this.ignoreRoll = DangIt.Parse<bool>(node.GetValue("ignoreRoll"), defaultTo: false);
            this.ignoreYaw = DangIt.Parse<bool>(node.GetValue("ignoreYaw"), defaultTo: false);
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
                this.controlSurfaceModule = this.part.Modules.OfType<ModuleControlSurface>().Single();
            }
        }


        protected override void DI_FailBegin()
        {
            return;
        }

        protected override void DI_Disable()
        {
            // Save the settings before overwriting them,
            // just in the case that the user has already set the control surface to ignore some direction
            this.ignorePitch = this.controlSurfaceModule.ignorePitch;
            this.ignoreRoll = this.controlSurfaceModule.ignoreRoll;
            this.ignoreYaw = this.controlSurfaceModule.ignoreYaw;

            // Make the control surface unresponsive
            this.controlSurfaceModule.ignorePitch = true;
            this.controlSurfaceModule.ignoreRoll = true;
            this.controlSurfaceModule.ignoreYaw = true;

            // Disable the module for good measure
            this.controlSurfaceModule.enabled = false; 
        }



        protected override void DI_EvaRepair()
        {
            // Enable the module
            this.controlSurfaceModule.enabled = true;

            // Restore the previous settings
            this.controlSurfaceModule.ignorePitch = this.ignorePitch;
            this.controlSurfaceModule.ignoreRoll = this.ignoreRoll;
            this.controlSurfaceModule.ignoreYaw = this.ignoreYaw; 
        }

    }
}
