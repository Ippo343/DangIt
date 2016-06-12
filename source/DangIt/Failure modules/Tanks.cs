using DangIt.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DangIt
{
    public class ModuleTankReliability : FailureModule
    {
        public override string DebugName { get { return "DangItTank"; } }
        public override string ScreenName { get { return "Tank"; } }
        public override string FailureMessage { get { return "A tank of " + leakName + " is leaking!"; } }
        public override string RepairMessage { get { return "Duct tape applied."; } }
        public override string FailGuiName { get { return "Puncture tank"; } }
        public override string EvaRepairGuiName { get { return "Apply duct tape"; } }
        public override string MaintenanceString { get { return "Repair the insulation"; } }
		public override string ExtraEditorInfo 
		{
				get
				{
					var temp = "This part can leak one of the following resources if it fails: ";
					foreach (PartResource pr in part.Resources.list.FindAll(r => !CDangIt.LeakBlackList.Contains(r.resourceName))) {
					temp += pr.resourceName + ", ";
					};
				return temp.TrimEnd(' ').TrimEnd(',');
				} 
		}

        // The leak is modeled as an exponential function
        // by approximating the differential equation
        // dQ(t) = - pole * Q(t)
        // where Q is the amount of fuel left in the tank
        [KSPField(isPersistant = true, guiActive = false)]
        protected float pole = 0.01f;
        
        // Maximum and minimum values of the time constant
        // The time constant is generated randomly between these two limits
        // and pole = 1 / TC
        [KSPField(isPersistant = true, guiActive = false)]
        public float MaxTC = 60f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float MinTC = 10f;

        // Name of the leaking resource
        [KSPField(isPersistant = true, guiActive = false)]
        public string leakName = null;

        // List of resources that the module will choose from when starting a new leak.
        // This list is created when the module is started by taking all the resources
        // in the part and excluding the ones that have been blacklisted in the configuration file
        protected List<PartResource> leakables;

        // This method is executed once at startup during a coroutine
        // that waits for the runtime component to be available and then triggers
        // this method.
        protected override void DI_RuntimeFetch()
        {
            // At this point CDangIt.Instance is not null: fetch the blacklist
            this.leakables = part.Resources.list.FindAll(r => !CDangIt.LeakBlackList.Contains(r.resourceName));

            // If no leakables are found, just disable the module
            if (leakables.Count == 0)
            {
                this.Log("The part " + this.part.name + " does not contain any leakable resource.");
                this.Events["Fail"].active = false;
                this.leakName = null;
                this.enabled = false; // disable the monobehaviour: this won't be updated
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
            this.pole = CUtils.Parse<float>("pole", 0.01f);
            
            this.leakName = node.GetValue("leakName");
            if (string.IsNullOrEmpty(leakName))
                leakName = null;

            this.Log("OnLoad: loaded leakName " + ((leakName == null) ? "null" : leakName));
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
                   (part.Resources[leakName].amount > 0)))  // ignore empty tanks
                {
                    double amount = pole * part.Resources[leakName].amount * TimeWarp.fixedDeltaTime;

                    // The user can disable the flow from tanks: if he does, RequestResource
                    // won't drain anything.
                    // In that case, we need to subtract directly the amount we want

                    if (part.Resources[leakName].flowState)
                        part.RequestResource(leakName, amount);
                    else 
                    {
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
            // Something has gone very wrong somewhere
            if (leakables == null)
                throw new Exception("The list of leakables is null!");

            // Discard every resource that has already been emptied
            leakables.RemoveAll(r => r.amount == 0);

            if (leakables.Count > 0)
            {
                // Choose a random severity of the leak
                // The lower TC, the faster the leak
                float TC = UnityEngine.Random.Range(MinTC, MaxTC);
                this.pole = 1 / TC;

                this.Log(string.Format("Chosen TC = {0} (min = {1}, max = {2})", TC, MinTC, MaxTC));

                // Pick a random index to leak.
                // Random.Range excludes the upper bound,
				// BUT because list.Count returns the length, not the max index, we DONT need a +1
				// e.g. [1].Count == 1 but MyListWithOneItem[1] == IndexError

                int idx = UnityEngine.Random.Range(0, leakables.Count);
				print ("Selected IDX: " + idx.ToString ());
				print ("Length of leakables: " + this.leakables.Count.ToString ());
				print ("Leakables: " + this.leakables.ToString ());

                this.leakName = leakables[idx].resourceName;

                // Picked a resource, allow failing
                return true;
            }
            else
            {
                leakName = null;
                this.Log("Zero leakable resources found on part " + this.part.partName + ", aborting FailBegin()");

                // Disallow failing
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Blacklisted resources:");

            foreach (string item in CDangIt.LeakBlackList)
                sb.AppendLine(item);

            this.Log(sb.ToString());
        }
#endif
		public override bool DI_ShowInfoInEditor(){
			return part.Resources.list.FindAll(r => !CDangIt.LeakBlackList.Contains(r.resourceName)).Count>0; //Only show if has leakable rescoures
		}
    }
}
