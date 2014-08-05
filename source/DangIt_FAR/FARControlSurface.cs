using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ferram4;
using System.Reflection;
using ippo;

namespace DangIt_FAR
{
    /// <summary>
    /// Module that causes failures in aerodynamic control surfaces
    /// </summary>
    public class ModuleFARControlSurfaceReliability : FailureModule
    {
        FARControllableSurface controlSurfaceModule;

        [KSPField(isPersistant = true, guiActive = false)]
        public bool wasFlap = true;

        FieldInfo AoAOffset;
        FieldInfo AoAFromFlap;
        

        public override string DebugName { get { return "DangItFARControlSurface"; } }
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
            this.wasFlap = DangIt.Parse<bool>(node.GetValue("wasFlap"), defaultTo: true);
        }


        protected override void DI_OnSave(ConfigNode node)
        {
            node.SetValue("wasFlap", this.wasFlap.ToString());
        }


        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.controlSurfaceModule = this.part.Modules.OfType<FARControllableSurface>().Single();
                this.wasFlap = controlSurfaceModule.isFlap;

                this.AoAFromFlap = typeof(FARControllableSurface).GetField("AoAFromFlap", BindingFlags.NonPublic);
                this.AoAOffset = typeof(FARControllableSurface).GetField("AoAOffset", BindingFlags.NonPublic);

                if (AoAOffset == null || AoAFromFlap == null)
                {
                    throw new Exception("Could not get the field info from FAR!");
                }

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
            this.wasFlap = this.controlSurfaceModule.isFlap;

            // Make the control surface unresponsive
            //AoAFromFlap.SetValue(this.controlSurfaceModule, AoAOffset.GetValue(this.controlSurfaceModule));
            this.controlSurfaceModule.isFlap = false;

            // Disable the module for good measure
            this.controlSurfaceModule.enabled = false; 
        }



        protected override void DI_EvaRepair()
        {
            // Enable the module
            this.controlSurfaceModule.enabled = true;

            // Re-allow control as a flap
            this.controlSurfaceModule.isFlap = true;
        }

    }


}
