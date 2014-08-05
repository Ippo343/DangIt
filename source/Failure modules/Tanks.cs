using System;
using System.IO;
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


        protected List<PartResource> FindLeakables()
        {
            List<PartResource> result;

            // Find the runtime and check the resource blacklist
            DangIt runtime = DangIt.Instance;
            if (runtime != null)
                result = part.Resources.list.FindAll(r => !runtime.LeakBlackList.Contains(r.resourceName));
            else
                result = null;

            // Didn't find the runtime
            if (result == null)
            {
                throw new Exception("Could not find the runtime instance!");
            }

            // Found zero eligible resources (this happens on liquid engines for example)
            if (result.Count == 0)
            {
                this.Log("The part " + this.part.name + " does not contain any leakable!");
                this.Events["Fail"].active = false;
                this.leakName = null;
            }

            return result;
        }



        protected override void DI_Start(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                // Ask the runtime for the resources that can be leaked
                this.leakables = FindLeakables();

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
#if DEBUG
            this.Log("OnLoad: loaded leakName " + ((leakName == null) ? "null" : leakName));
#endif

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
                if (!this.isEnabled) return;

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



        protected override void DI_FailBegin()
        {
            try
            {
                leakables = FindLeakables();
#if DEBUG
                this.Log("FailBegin: scanned for leakables, returned " + ((leakables == null) ? "null" : leakables.Count.ToString()));
#endif

                if ((leakables != null) && (leakables.Count > 0))
                {
                    // Choose a random severity of the leak
                    float TC = UnityEngine.Random.Range(MinTC, MaxTC);
                    this.pole = 1 / TC;
                    this.Log("Chosen TC = " + TC + " (min = " + MinTC + ", max = " + MaxTC + ")");

                    // Pick a random index to leak.
                    int idx = (leakables.Count == 1) ? 0 : UnityEngine.Random.Range(0, leakables.Count);
#if DEBUG
                    this.Log("Chosen index " + idx);
#endif
                    this.leakName = leakables[idx].resourceName;
#if DEBUG
                    this.Log("Chosen resource " + leakName);
#endif

                }
                else
                {
                    leakName = null;
                    throw new Exception("Couldn't find any leakable resources!");
                }
            }
            catch (Exception e)
            {
                OnError(e);
                this.isEnabled = false;
                this.SetFailureState(false);
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
            foreach (string item in DangItRuntime.Instance.LeakBlackList)
            {
                this.Log("Blacklisted: " + item);
            }
            this.Log("Done");
        }
#endif

    }
}
