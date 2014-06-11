using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Module that causes leaks in resource tanks
    /// </summary>
    public class ModuleTankReliability : FailureModule
    {
        public override string DebugName { get { return "DangItTank"; } }
        public override string FailureMessage { get { return "A tank of " + leakables[leakIndex].resourceName + " is leaking!"; } }
        public override string RepairMessage { get { return "Duct tape applied."; } }
        public override string FailGuiName { get { return "Puncture tank"; } }
        public override string EvaRepairGuiName { get { return "Apply duct tape"; } }


        [KSPField(isPersistant = true, guiActive = false)]
        protected float pole = 0.01f;

        // Maximum and minimum values for the decay fraction
        [KSPField(isPersistant = true, guiActive = false)]
        public float MaxTC = 60f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float MinTC = 10f;

        [KSPField(isPersistant = true, guiActive = false)]
        public int leakIndex = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public string leakName = "";

        protected List<PartResource> leakables;

        /// <summary>
        /// Blacklist of resources that will be ignored by the module.
        /// </summary>
        protected List<string> blackList = new List<string>();


        protected override void DI_OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                // Get the leakable resources
                leakables = part.Resources.list.FindAll(r => !blackList.Contains(r.resourceName));
                if (leakables.Count < 1)
                {
                    throw new Exception("No leakables found!");
                }


                if (this.HasFailed)
                {
                    if (string.IsNullOrEmpty(leakName))
                    {
                        this.Log("ERROR: the part has failed but there is no valid resource name leak!");
                        this.SetFailureState(false);
                    }
                    else
                    {
                        leakIndex = part.Resources.list.FindIndex(r => r.resourceName == leakName);
                    }
                }

            }
        }



        protected override void DI_OnLoad(ConfigNode node)
        {
            this.leakName = node.GetValue("leakName");
            this.pole = DangIt.Parse<float>("leakName", 0.01f);

            this.blackList = node.GetValues("ignore").ToList<string>();

#if DEBUG
            foreach (string s in blackList)
            {
                this.Log("Blacklisted: " + s);
            }
#endif
        }



        protected override void DI_OnSave(ConfigNode node)
        {
            node.SetValue("leakName", this.leakName);
            node.SetValue("pole", this.pole.ToString());
        }



        protected override void DI_Update()
        {
            try
            {
                if (!this.isEnabled) return;

                if (this.HasFailed && (leakIndex >= 0) && (leakables[leakIndex].amount > 0))
                {
                    double amount = pole * leakables[leakIndex].amount * TimeWarp.fixedDeltaTime;
                    part.RequestResource(leakables[leakIndex].resourceName, amount);
                }
            }
            catch (Exception)
            {
                this.isEnabled = false;
                throw;
            }
        }



        protected override void DI_Fail()
        {
            // Choose a random severity of the leak
            float TC = UnityEngine.Random.Range(MinTC, MaxTC);
            this.pole = 1 / TC;
            this.Log("Chosen TC = " + TC + " (min = " + MinTC + ", max = " + MaxTC + ")");

            if (leakables.Count > 0)
            {
                // Pick a random index to leak.
                this.leakIndex = UnityEngine.Random.Range(0, leakables.Count);
                this.leakName = leakables[leakIndex].resourceName; 
            }
            else
            {
                leakIndex = -1;
                leakName = "";
                this.SetFailureState(false);
            }
        }


        
        protected override void DI_EvaRepair()
        {
            this.leakIndex = -1;
            this.leakName = "";

            return;
        }

    }
}
