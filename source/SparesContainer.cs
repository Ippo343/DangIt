using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DangIt
{
    public class ModuleSparesContainer : PartModule
    {
        private bool eventAdded = false;

        public override void OnStart(PartModule.StartState state)
        {
            Debug.Log("DangIt: Spares Container [" + this.GetInstanceID() + "]: OnStart, state is " + state);
            
            this.Events["TakeParts"].active = true;
        }

       
        [KSPEvent(active=true, guiActiveUnfocused=true, externalToEVAOnly=true, guiName="Take spares", unfocusedRange=DangIt.EvaRepairDistance)]
        public void TakeParts()
        {
            Part evaPart = DangIt.FindEVAPart();

            if (evaPart == null)
                this.Log("ERROR: couldn't find an active EVA!");
            else
                FillEvaSuit(evaPart, this.part);

            Events["DepositParts"].active = true;

            if (!eventAdded)
            {
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                eventAdded = true;
            }
        }



        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = DangIt.EvaRepairDistance, guiName = "Deposit spares", active = false)]
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


        /*
        [KSPEvent(guiActiveUnfocused = false, unfocusedRange = DangIt.EvaRepairDistance, guiName = "Show perks", active = false)]
        public void ShowPerks()
        {
            try
            {
                ICrewFilesServer server = CrewFilesManager.Server;

                if (server == null) throw new Exception("server is null!");

                ProtoCrewMember kerbal = DangIt.FindEVAPart().vessel.GetVesselCrew().First();
                ConfigNode kerbalFile = server.GetKerbalFile(kerbal);

                if (kerbalFile == null) throw new Exception("kerbalFile is null!");

                ConfigNode perksNode = kerbalFile.GetNode(PerkGenerator.NodeName);

                if (perksNode == null) throw new Exception("perksNode is null!");

                this.Log(kerbal.name + " has " + perksNode.CountNodes + " perks");
            }
            catch (Exception e)
            {
                this.Log(e.Message);
                return;
            }

        }
        */


        protected void EmptyEvaSuit(Part evaPart, Part container)
        {
            this.Log("Emptying the EVA suit from " + evaPart.name + " to " + container.name);

            // Compute how much can be left in the container
            double capacity = container.Resources[DangIt.Spares.Name].maxAmount - container.Resources[DangIt.Spares.Name].amount;
            double deposit = Math.Min(evaPart.Resources[DangIt.Spares.Name].amount, capacity);

            // Add it to the spares container and drain it from the EVA part
            container.RequestResource(DangIt.Spares.Name, -deposit);
            evaPart.RequestResource(DangIt.Spares.Name, deposit);

            // GUI acknowledge
            try
            {
                DangIt.Broadcast(evaPart.protoModuleCrew[0].name + " has left " + deposit + " spares", false, 1f);
            }
            catch (Exception)
            {
                DangIt.Broadcast("You left " + deposit + " spares", false, 1f);
            }

            ResourceDisplay.Instance.Refresh();
        }



        protected void FillEvaSuit(Part evaPart, Part container)
        {
            // Check if the EVA part contains the spare parts resource: if not, add a new config node
            if (!evaPart.Resources.Contains(DangIt.Spares.Name))
            {
                this.Log("The eva part doesn't contain spares, adding the config node"); 

                ConfigNode node = new ConfigNode("RESOURCE");
                node.AddValue("name", DangIt.Spares.Name);
                node.AddValue("maxAmount", DangIt.Spares.MaxEvaAmount);
                node.AddValue("amount", 0);
                evaPart.Resources.Add(node);
            }


            // Compute how much the kerbal can take
            double desired = Math.Min(DangIt.Spares.MaxEvaAmount - evaPart.Resources[DangIt.Spares.Name].amount, DangIt.Spares.Increment);
            double amountTaken = Math.Min(desired, container.Resources[DangIt.Spares.Name].amount);

            // Take it from the container and add it to the EVA
            container.RequestResource(DangIt.Spares.Name, amountTaken);
            evaPart.RequestResource(DangIt.Spares.Name, -amountTaken);

            // GUI stuff
            DangIt.Broadcast(evaPart.vessel.GetVesselCrew().First().name + " has taken " + amountTaken + " spares", false, 1f);
            ResourceDisplay.Instance.Refresh();
        }


        /// <summary>
        /// When the kerbal boards a vessel, leave the spare parts in the command pod
        /// </summary>
        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
        {
#if DEBUG
            this.Log("OnCrewBoardVessel, emptying the EVA suit");
#endif
            Part evaPart = action.from;
            Part container = action.to;

            if (evaPart.Resources.Contains(DangIt.Spares.Name))
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
