using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Used to store the previous settings of the control surface,
    /// so that they are reset to the user's state when it is repaired.
    /// </summary>
    struct ControlSurfaceSettings
    {
        public bool ignorePitch;
        public bool ignoreRoll;
        public bool ignoreYaw;
    }


    /// <summary>
    /// Module that causes failures in aerodynamic control surfaces
    /// </summary>
    public class ModuleControlSurfaceReliability : ModuleBaseFailure
    {
        ModuleControlSurface controlSurfaceModule;
        ControlSurfaceSettings originalSettings = new ControlSurfaceSettings();

        public override string DebugName { get { return "DangItControlSurface"; } }
        public override string FailureMessage { get { return "A control surface is stuck!"; } }
        public override string RepairMessage { get { return "Control surface repaired."; } }
        public override string FailGuiName { get { return "Fail control surface"; } }
        public override string EvaRepairGuiName { get { return "Drown in WD40"; } }
        public override bool AgeOnlyWhenActive { get { return false; } }

        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            this.controlSurfaceModule = part.Modules.OfType<ModuleControlSurface>().First();
        }


        public override void DI_Fail()
        {
            // Save the settings before overwriting them,
            // just in the case that the user has already set the control surface to ignore some direction
            this.originalSettings.ignorePitch = this.controlSurfaceModule.ignorePitch;
            this.originalSettings.ignoreRoll = this.controlSurfaceModule.ignoreRoll;
            this.originalSettings.ignoreYaw = this.controlSurfaceModule.ignoreYaw;

            // Make the control surface unresponsive
            this.controlSurfaceModule.ignorePitch = true;
            this.controlSurfaceModule.ignoreRoll = true;
            this.controlSurfaceModule.ignoreYaw = true;

            // Disable the module for good measure
            this.controlSurfaceModule.enabled = false; 
        }



        public override void DI_EvaRepair()
        {
            // Enable the module
            this.controlSurfaceModule.enabled = true;

            // Restore the previous settings
            this.controlSurfaceModule.ignorePitch = this.originalSettings.ignorePitch;
            this.controlSurfaceModule.ignoreRoll = this.originalSettings.ignoreRoll;
            this.controlSurfaceModule.ignoreYaw = this.originalSettings.ignoreYaw; 
        }

    }
}
