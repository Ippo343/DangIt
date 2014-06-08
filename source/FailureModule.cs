using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DangIt
{
    /// <summary>
    /// Base failure module: handles the aging of the part, causes the random failures
    /// and handles the EVA repair.
    /// </summary>
    public abstract class FailureModule : PartModule
    {

        #region Properties that return the custom strings

        public abstract string DebugName { get; }
        public abstract string RepairMessage { get; }
        public abstract string FailureMessage { get; }
        public abstract string FailGuiName { get; }
        public abstract string EvaRepairGuiName { get; }

        #endregion


        #region Methods to add the specific logic of the module

        protected virtual void DI_Reset() { }
        protected virtual void DI_OnLoad(ConfigNode node) { }
        protected virtual void DI_OnStart(StartState state) { }
        protected virtual void DI_Update() { }
        protected abstract void DI_Fail();
        protected abstract void DI_EvaRepair();
        protected virtual void DI_OnSave(ConfigNode node) { }
        public virtual bool PartIsActive() { return true; }
        protected virtual float LambdaMultiplier() { return 1f; } 

        #endregion


        #region Fields from the cfg file

        [KSPField(isPersistant = true, guiActive = false)]
        public float MTBF = 1000f;                                  // Original Mean Time Between Failures.

        [KSPField(isPersistant = true, guiActive = false)]
        public float LifeTime = 1f;                                 // Time constant of the exponential decay

        [KSPField(isPersistant = true, guiActive = false)]
        public float RepairCost = 5f;                               // Amount of spares needed to repair the part

        [KSPField(isPersistant = true, guiActive = false)]
        public bool Silent = false;


        #endregion


        #region Internal state

        [KSPField(isPersistant = true, guiActive = false)]
        public bool HasInitted = false;

        [KSPField(isPersistant = true, guiActive = false)]
        public float TimeOfLastReset = float.PositiveInfinity;

        [KSPField(isPersistant = true, guiActive = DangIt.DEBUG)]
        public float Age = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float LastFixedUpdate = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float CurrentMTBF = float.PositiveInfinity;

        [KSPField(isPersistant = true, guiActive = false)]
        public float LifeTimeSecs = float.PositiveInfinity;

        [KSPField(isPersistant = true, guiActive = false)]
        public bool HasFailed = false;

        #endregion


        #region Lambda

        public float Lambda
        {
            get { return LambdaFromMTBF() * LambdaMultiplier(); }
        }

        private float LambdaFromMTBF()
        {
            try
            {
                return (1f / this.CurrentMTBF) / 3600f * TimeWarp.fixedDeltaTime;
            }
            catch (Exception e)
            {
                ExceptionBoilerPlate(e);
                return 0f;
            }
        } 

        #endregion



        protected void Reset()
        {
            try
            {
                this.Log("Resetting");

                float now = DangIt.Now();

                #region Internal state

                this.Age = 0;
                this.TimeOfLastReset = now + 1; // + 1 second for safety
                this.LastFixedUpdate = now;

                this.CurrentMTBF = this.MTBF;
                this.LifeTimeSecs = this.LifeTime * 3600f;
                this.HasFailed = false;
                #endregion

                #region Fail and repair events
                this.Events["Fail"].guiName = this.FailGuiName;
                this.Events["EvaRepair"].guiName = this.EvaRepairGuiName;
                this.Events["EvaRepair"].active = false;
              
                #endregion

                // Run the custom reset of the subclasses
                this.DI_Reset();

                this.HasInitted = true;
            }
            catch (Exception e)
            {
                ExceptionBoilerPlate(e);
            }
        }



        public override void OnLoad(ConfigNode node)
        {
#if DEBUG
            this.Log("OnLoad");
            this.Log(node.ToString());
#endif

            this.HasInitted = DangIt.Parse<bool>(node.GetValue("HasInitted"), false);
            this.Age = DangIt.Parse<float>(node.GetValue("Age"), defaultTo: 0f);
            this.TimeOfLastReset = DangIt.Parse<float>(node.GetValue("TimeOfLastReset"), defaultTo: float.PositiveInfinity);
            this.LastFixedUpdate = DangIt.Parse<float>(node.GetValue("LastFixedUpdate"), defaultTo: 0f);
            this.CurrentMTBF = DangIt.Parse<float>(node.GetValue("CurrentMTBF"), defaultTo: float.PositiveInfinity);
            this.LifeTimeSecs = DangIt.Parse<float>(node.GetValue("LifeTimeSecs"), defaultTo: float.PositiveInfinity);
            this.HasFailed = DangIt.Parse<bool>(node.GetValue("HasFailed"), defaultTo: false);

            // Run the subclass' custom onload
            this.DI_OnLoad(node);

            this.Log("OnLoad complete, age is " + this.Age);

            base.OnLoad(node);
        }



        public override void OnSave(ConfigNode node)
        {
            node.SetValue("HasInitted", this.HasInitted.ToString());
            node.SetValue("Age", Age.ToString());
            node.SetValue("TimeOfLastReset", TimeOfLastReset.ToString());
            node.SetValue("LastFixedUpdate", LastFixedUpdate.ToString());
            node.SetValue("CurrentMTBF", CurrentMTBF.ToString());
            node.SetValue("LifeTimeSecs", LifeTimeSecs.ToString());
            node.SetValue("HasFailed", HasFailed.ToString());

            // Run the subclass' custom onsave
            this.DI_OnSave(node);

            base.OnSave(node);
        }




        public override void OnStart(PartModule.StartState state)
        {
            try
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
#if DEBUG
                    this.Log("Starting in flight: last reset " + TimeOfLastReset + ", now " + DangIt.Now());
#endif
                    // Reset the internal state at the beginning of the flight (catching a revert to launch)
                    if (DangIt.Now() < TimeOfLastReset)
                        this.Reset();

                    DangIt.ResetShipGlow(this.part.vessel);

                    if (this.HasFailed)
                        this.Fail();
                }                        

                this.DI_OnStart(state);

            }
            catch (Exception e)
            {
                ExceptionBoilerPlate(e);
            }

        }



        public void FixedUpdate()
        {
            try
            {
                if (this.HasInitted)
                {
                    float now = DangIt.Now();

                    if (!PartIsActive())
                    {
                        this.LastFixedUpdate = now;
                        return;
                    }

                    this.Age += now - LastFixedUpdate;

                    this.CurrentMTBF = this.MTBF * (float)Math.Exp(-this.Age / this.LifeTimeSecs);

                    // If the part has not already failed, toss the dice
                    if (!this.HasFailed)
                    {
                        if (UnityEngine.Random.Range(0f, 1f) < this.Lambda)
                        {
                            this.Fail();
                        }
                    }

                    this.DI_Update();

                    this.LastFixedUpdate = now; 
                }
            }

            catch (Exception e)
            {
                ExceptionBoilerPlate(e);
            }
        }



        [KSPEvent(guiActive = DangIt.EnableGuiFailure)]
        public void Fail()
        {
            try
            {
                this.Log("FAIL!");
                this.SetFailureState(true);
                this.DI_Fail();

                if (!this.Silent)
                    DangIt.Broadcast(this.FailureMessage);

            }
            catch (Exception e)
            {
                ExceptionBoilerPlate(e);
            }
        }



        protected void SetFailureState(bool state)
        {
            this.HasFailed = state;
            DangIt.ResetShipGlow(this.part.vessel);

            Events["Fail"].active = ((state) ? false : DangIt.EnableGuiFailure);
            Events["EvaRepair"].active = state;
        }




        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = DangIt.EvaRepairDistance, externalToEVAOnly = true)]
        public void EvaRepair()
        {
            try
            {
                this.Log("Initiating EVA repair");

                // Get the EVA kerbal
                Part evaPart = DangIt.FindEVA();
                if (evaPart == null)
                {
                    DangIt.Broadcast("DangIt ERROR: couldn't find an active EVA!");
                    this.Log("ERROR: couldn't find an active EVA!");
                    return;
                }


                // Check if he is carrying enough spares
                if (evaPart.Resources.Contains(DangIt.Spares.Name) && evaPart.Resources[DangIt.Spares.Name].amount >= this.RepairCost)
                {
                    this.Log("Spare parts check: OK! Repair allowed");

                    this.DI_EvaRepair();
                    this.SetFailureState(false);

                    float intelligence = 1 - evaPart.protoModuleCrew[0].stupidity;
                    float discountedCost = (float)Math.Round( RepairCost * (1 - UnityEngine.Random.Range(0f, intelligence)) );
                    float discount = RepairCost - discountedCost;

                    this.Log("Kerbal's intelligence: " + intelligence + ", discount: " + discount);

                    evaPart.RequestResource(DangIt.Spares.Name, discountedCost);
                    ResourceDisplay.Instance.Refresh();

                    DangIt.Broadcast(this.RepairMessage);

                    if (discount > 0)
                    {
                        DangIt.Broadcast(evaPart.name + " was able to save " + discount + " spare parts");
                    }
                    
                }
                else
                {
                    this.Log("Spare parts check: failed! Repair NOT allowed");
                    DangIt.Broadcast("You need " + this.RepairCost + " spares to repair this.");
                }

                DangIt.ResetShipGlow(this.part.vessel);

            }
            catch (Exception e)
            {
                ExceptionBoilerPlate(e);
            }

        }




        public override string GetInfo()
        {
            // This is the time (in hours) that it takes for the MTBF to drop to 1 hour
            double EOL = Math.Round(Math.Max(-LifeTime * Math.Log(1 / this.MTBF), 0));

            return ("MTBF: " + this.MTBF + " h"
                + "\nLifetime: " + this.LifeTime + " h"
                + "\nEOL: " + EOL + " h"
                + "\nRepair cost: " + this.RepairCost);
        }



        #region Logging utilities

        public void Log(string msg)
        {
            Vessel v = this.part.vessel;
            StringBuilder sb = new StringBuilder();

            sb.Append("[DangIt]: ");
            sb.Append(this.DebugName);
            sb.Append("[" + this.GetInstanceID() + "]");
            if (part.vessel != null) sb.Append("[Ship: " + part.vessel.vesselName + "]");
            sb.Append(": " + msg);

            Debug.Log(sb.ToString());
        }

        public void LogException(Exception e)
        {
            this.Log("ERROR: " + e.Message + "; " + e.StackTrace);
        }

        private void ExceptionBoilerPlate(Exception e)
        {
            LogException(e);
            return;
        } 

        #endregion

    }
}
