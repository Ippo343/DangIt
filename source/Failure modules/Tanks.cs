using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    /// <summary>
    /// Module that causes leaks in resource tanks
    /// </summary>
    public class ModuleTankReliability : FailureModule
    {
        public override string DebugName { get { return "DangItTank"; } }
        public override string InspectionName { get { return "Tank"; } }
        public override string FailureMessage { get { return "A tank of " + leakName + " is leaking!"; } }
        public override string RepairMessage { get { return "Duct tape applied."; } }
        public override string FailGuiName { get { return "Puncture tank"; } }
        public override string EvaRepairGuiName { get { return "Apply duct tape"; } }
        public override string MaintenanceString { get { return "Repair the insulation"; } }


        [KSPField(isPersistant = true, guiActive = false)]
        protected float pole = 0.01f;

        // Maximum and minimum values for the decay fraction
        [KSPField(isPersistant = true, guiActive = false)]
        public float MaxTC = 60f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float MinTC = 10f;

        [KSPField(isPersistant = true, guiActive = false)]
        public string leakName = null;

        protected List<PartResource> leakables;


        protected override void DI_RuntimeFetch()
        {
            this.leakables = null;

            this.leakables = part.Resources.list.FindAll(r => !DangIt.Instance.LeakBlackList.Contains(r.resourceName));

            // If no leakables are found, just disable the module
            if (leakables.Count == 0)
            {
                this.Log("The part " + this.part.name + " does not contain any leakable resource.");
                this.Events["Fail"].active = false;
                this.leakName = null;
                this.enabled = false;
            }
        }



        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                // The part was already failed when loaded:
                // check if the resource is still in the tank
                if (this.HasFailed)
                {
                    if (string.IsNullOrEmpty(leakName) || !part.Resources.Contains(leakName))
                    {
                        this.Log("ERROR: the part was started as failed but the leakName isn't valid!"); ;
                        this.SetFailureState(false);
                    }
                }

            }
        }



        protected override void DI_OnLoad(ConfigNode node)
        {
            this.leakName = node.GetValue("leakName");
            if (string.IsNullOrEmpty(leakName)) leakName = null;

            this.Log("OnLoad: loaded leakName " + ((leakName == null) ? "null" : leakName));

            this.pole = DangIt.Parse<float>("leakName", 0.01f);
        }



        protected override void DI_OnSave(ConfigNode node)
        {
            node.SetValue("leakName", (leakName == null) ? string.Empty : leakName);
            node.SetValue("pole", this.pole.ToString());
        }



        protected override void DI_Update()
        {
            try
            {
                if (this.HasFailed && 
                   (!string.IsNullOrEmpty(leakName) && 
                   (part.Resources[leakName].amount > 0)))
                {
                    double amount = pole * part.Resources[leakName].amount * TimeWarp.fixedDeltaTime;

                    // Check if the tank's valve has been closed
                    if (part.Resources[leakName].flowState)
                        part.RequestResource(leakName, amount); // valve open, request as usual
                    else 
                    {   // valve closed, subtract directly
                        part.Resources[leakName].amount -= amount;
                        part.Resources[leakName].amount = Math.Max(part.Resources[leakName].amount, 0);
                    }
                }
            }
            catch (Exception e)
            {
                OnError(e);
                this.isEnabled = false;
                this.SetFailureState(false);
            }
        }



        protected override bool DI_FailBegin()
        {
            if (leakables == null)
                throw new Exception("The list of leakables is null!");

            // Discard every resource that has already been emptied
            leakables.RemoveAll(r => r.amount == 0);

            if (leakables.Count > 0)
            {
                // Choose a random severity of the leak
                float TC = UnityEngine.Random.Range(MinTC, MaxTC);
                this.pole = 1 / TC;
                this.Log("Chosen TC = " + TC + " (min = " + MinTC + ", max = " + MaxTC + ")");

                // Pick a random index to leak.
                int idx = (leakables.Count == 1) ? 0 : UnityEngine.Random.Range(0, leakables.Count);
                this.leakName = leakables[idx].resourceName;

                return true;
            }
            else
            {
                leakName = null;
                this.Log("Zero leakable resources found on part " + this.part.partName + ", aborting FailBegin()");
                return false;
            }
        }



        protected override void DI_Disable()
        {
            // nothing to do for tanks
            return;
        }


        
        protected override void DI_EvaRepair()
        {
            this.leakName = null;
        }


        /*
#if DEBUG
        [KSPEvent(active = true, guiActive = true)]
        public void PrintStatus()
        {
            this.Log("Printing flow modes");
            foreach (PartResource res in this.part.Resources)
            {
                this.Log(res.resourceName + ": " + res.flowMode + ", " + res.flowState);
            }
        }

        [KSPEvent(active = true, guiActive=true)]
        public void PrintBlackList()
        {
            this.Log("Printing blacklist");
            foreach (string item in DangIt.Instance.LeakBlackList)
            {
                this.Log("Blacklisted: " + item);
            }
            this.Log("Done");
        }
#endif
         */

    }
}
