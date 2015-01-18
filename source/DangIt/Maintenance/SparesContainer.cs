using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    /// <summary>
    /// Module that handles the spare parts so that the kerbal can grab them while on EVA.
    /// </summary>
    public class ModuleSparesContainer : PartModule
    {
        private bool eventAdded = false;

        public override void OnStart(PartModule.StartState state)
        {
            // Sync settings with the runtime
            this.StartCoroutine("RuntimeFetch");

            this.Events["TakeParts"].active = true;
        }


        // Coroutine that waits for the runtime to be ready and the syncs with the settings
        IEnumerator RuntimeFetch()
        {
            // Wait for the server to be available
            while (DangIt.Instance == null || !DangIt.Instance.IsReady)
                yield return null;

            this.Events["TakeParts"].unfocusedRange = DangIt.Instance.CurrentSettings.MaxDistance;
            this.Events["DepositParts"].unfocusedRange = DangIt.Instance.CurrentSettings.MaxDistance;
        }


       
        [KSPEvent(active=true, guiActiveUnfocused=true, externalToEVAOnly=true, guiName="Take spares", unfocusedRange = 2f)]
        public void TakeParts()
        {
            Part evaPart = DangIt.FindEVAPart();

            if (evaPart == null)
                throw new Exception("ERROR: couldn't find an active EVA!");
            else
                FillEvaSuit(evaPart, this.part);

            Events["DepositParts"].active = true;

            if (!eventAdded)
            {
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                eventAdded = true;
            }
        }



        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 2f, guiName = "Deposit spares", active = false)]
        public void DepositParts()
        {
            Part evaPart = DangIt.FindEVAPart();

            if (evaPart == null)
                this.Log("ERROR: couldn't find an active EVA!");
            else
                EmptyEvaSuit(evaPart, this.part);

            Events["DepositParts"].active = false;

            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            eventAdded = false;
        }



        protected void EmptyEvaSuit(Part evaPart, Part container)
        {
            this.Log("Emptying the EVA suit from " + evaPart.name + " to " + container.name);

            // Compute how much can be left in the container
            double capacity = container.Resources[Spares.Name].maxAmount - container.Resources[Spares.Name].amount;
            double deposit = Math.Min(evaPart.Resources[Spares.Name].amount, capacity);

            // Add it to the spares container and drain it from the EVA part
            container.RequestResource(Spares.Name, -deposit);
            // Once again, MC2 breaks the RequestResource on evaPart, but with the above checks, decrementing should work just fine instead, I think! -TrypChangeling
            //evaPart.RequestResource(Spares.Name, deposit);
            evaPart.Resources[Spares.Name].amount -= deposit;

            // GUI acknowledge
            try
            {
                DangIt.Broadcast(evaPart.protoModuleCrew[0].name + " has left " + deposit + " spares", false, 1f);
            }
            catch (Exception) // The kerbal reenters before this method is called: in that case, trying to get his name will throw an exception
            {
                DangIt.Broadcast("You left " + deposit + " spares", false, 1f);
            }

            ResourceDisplay.Instance.Refresh();
        }



        protected void FillEvaSuit(Part evaPart, Part container)
        {
            // Check if the EVA part contains the spare parts resource: if not, add a new config node
            if (!evaPart.Resources.Contains(Spares.Name))
            {
                this.Log("The eva part doesn't contain spares, adding the config node"); 

                ConfigNode node = new ConfigNode("RESOURCE");
                node.AddValue("name", Spares.Name);
                node.AddValue("maxAmount", Spares.MaxEvaAmount);
                node.AddValue("amount", 0);
                evaPart.Resources.Add(node);
            }

            // Override maxAmount set by other mods (such as MC2) causing taking of parts to fail -TrypChangeling
            if (evaPart.Resources[Spares.Name].maxAmount < Spares.MaxEvaAmount)
            {
            	evaPart.Resources[Spares.Name].maxAmount = Spares.MaxEvaAmount;
            }


            // Compute how much the kerbal can take
            double desired = Spares.MaxEvaAmount - evaPart.Resources[Spares.Name].amount;
            desired = Math.Min(desired, Spares.MinIncrement);
            double amountTaken = Math.Min(desired, container.Resources[Spares.Name].amount);

            // Take it from the container and add it to the EVA
            container.RequestResource(Spares.Name, amountTaken);
            // RequestResource is being overridden by MC2 for some reason - however, with above checks, simply incrementing the value should work... I think! - TrypChangeling
            // evaPart.RequestResource(Spares.Name, -amountTaken);
            evaPart.Resources[Spares.Name].amount += amountTaken;

            // GUI stuff
            DangIt.Broadcast(evaPart.vessel.GetVesselCrew().First().name + " has taken " + amountTaken + " spares", false, 1f);
            ResourceDisplay.Instance.Refresh();
        }


        /// <summary>
        /// When the kerbal boards a vessel, leave the spare parts in the command pod
        /// </summary>
        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
        {
            this.Log("OnCrewBoardVessel, emptying the EVA suit");

            Part evaPart = action.from;
            Part container = action.to;

            if (evaPart.Resources.Contains(Spares.Name))
                EmptyEvaSuit(evaPart, container);
        }


        public void Log(string msg)
        {
            Vessel v = this.part.vessel;
            StringBuilder sb = new StringBuilder();

            sb.Append("[DangIt]: ");
            sb.Append("SparesContainer");
            sb.Append("[" + this.GetInstanceID() + "]");
            sb.Append(": " + msg);

            Debug.Log(sb.ToString());
        }


    }
}
