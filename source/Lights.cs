using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Module that causes the light bulbs to "burn out"
    /// </summary>
    public class ModuleLightsReliability : ModuleBaseFailure
    {
        ModuleLight lightModule;


        public override string DebugName { get { return "DangItLights"; } }
        public override string FailureMessage { get { return "A light bulb has burned out."; } }
        public override string RepairMessage { get { return "Bulb replaced."; } }
        public override string FailGuiName { get { return "Fail light bulb"; } }
        public override string EvaRepairGuiName { get { return "Replace light bulb"; } }
        public override bool AgeOnlyWhenActive { get { return true; } }


        public override bool PartIsActive()
        {
            return this.lightModule.isOn;
        }


        public override void DI_OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            this.lightModule = part.Modules.OfType<ModuleLight>().First();

        }


        public override void DI_Fail()
        {
            this.lightModule.isOn = false;
            this.part.Modules.Remove(this.lightModule);

        }


        public override void DI_EvaRepair()
        {
            this.part.Modules.Add(this.lightModule); 

        }

    }
}
