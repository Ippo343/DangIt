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
    public abstract class ModuleBaseFailure : PartModule
    {

        #region Properties and methods to customize the subclass

        public abstract string DebugName { get; }                   // The name that is shown in the debug logs
        public abstract string FailureMessage { get; }              // Message shown on the screen when a failure occurs
        public abstract string RepairMessage { get; }               // Message shown on the screen when the part is repaired
        public abstract string FailGuiName { get; }                 // Text fot the Fail event (only shown in DEBUG builds)
        public abstract string EvaRepairGuiName { get; }            // Text fot the EVA repair event
        public abstract bool AgeOnlyWhenActive { get; }             // Chooses if the part ages always or only during use

        public virtual void DI_Init() { }                           // Custom initialization code for pre-launch
        public abstract void DI_OnStart(StartState state);          // Executed on every OnStart (if the module is enabled)
        public abstract void DI_Fail();                             // Specify the failure logic.
        public abstract void DI_EvaRepair();                        // Specify the repair behaviour
        public virtual void DI_Update() { }                         // Custom update code, called at every update interval

        public virtual bool PartIsActive() { return true; }         // If AgeOnlyWhenActive, the part ages only when this returns true
        public virtual float LambdaMultiplier() { return 1f; }      // Istantaneous multiplier for the failure chance

        #endregion


        #region Fields from the cfg file - DON'T MODIFY THESE VALUES IN YOUR CODE!

        [KSPField(isPersistant = true, guiActive = false)]
        public float MTBF = 1000f;                                  // Original Mean Time Between Failures.

        [KSPField(isPersistant = true, guiActive = false)]
        public float LifeTime = 1f;                                 // Time constant of the exponential decay

        [KSPField(isPersistant = true, guiActive = false)]
        public float RepairCost = 5f;                               // Amount of spares needed to repair the part

        [KSPField(isPersistant = true, guiActive = false)]
        public float UpdateInterval = 0.5f;                         // Interval between updates

        #endregion


        #region Timekeeping and aging

        [KSPField(isPersistant = true, guiActive = false)]
        public bool needsInit = true;

        [KSPField(isPersistant = true, guiActive = false)]
        public float timeInitted = float.PositiveInfinity;   

        [KSPField(isPersistant = true, guiActive = false)]
        public float initialMTBF = 1000f;                        

        [KSPField(isPersistant = true, guiActive = false)]
        public float currMTBF = 1000f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float lifeTimeSecs = 3600f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float Age = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float timerStart = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float lastFixedUpdate = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;

        #endregion



        public override void OnStart(PartModule.StartState state)
        {
#if TRACE
            this.Log("Entering OnStart: state is " + state + ", enabled is " + this.enabled);
#endif
            try
            {
                if (!this.enabled) return;

                // Run the custom OnStart method
                this.DI_OnStart(state);

                // One time initialization in flight
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (Planetarium.GetUniversalTime() < timeInitted)
                    {
                        this.needsInit = true;
                        this.timeInitted = (float)Planetarium.GetUniversalTime() + 10;
                    }

                    if (!this.needsInit) return;

                    this.Log("OnStart, performing initialization");

                    this.currMTBF = this.initialMTBF = this.MTBF;
                    this.lifeTimeSecs = this.LifeTime * 3600f;

                    float now = (float)Planetarium.GetUniversalTime();
                    this.Age = 0;
                    this.lastFixedUpdate = this.timerStart = now;
                    this.hasFailed = false;

                    // Set the GUI name of the Fail and EvaRepair events
                    this.Events["Fail"].guiName = this.FailGuiName;
                    this.Events["EvaRepair"].guiName = this.EvaRepairGuiName;

                    // Run the initialization of the subclass
                    this.DI_Init();

                    this.needsInit = false;
                }
                else
                {
                    this.Log("OnStart, not initializing: age = " + this.Age);

                    // If we are not in editor, check if the part was failed when it was saved
                    if (state != StartState.Editor)
                    {
                        if (this.hasFailed) // saved as failed: cripple the part
                        {
                            DangIt.ResetShipGlow(this.part.vessel);
                            this.Fail(); // subclass' failure code
                        }
                    }
                }

            }
            catch (Exception e)
            {
                LogException(e);
                this.Log("Module disabling after an exception!");
                this.enabled = false;
                return;
            }

        }





        public override void OnFixedUpdate()
        {
            try
            {
                if (!this.enabled) return;

                float now = (float)Planetarium.GetUniversalTime();
                if (!((now - this.lastFixedUpdate) > this.UpdateInterval)) // the update isn't due yet
                    return;

                // This part only ages when it is active, but the part is inactive: do nothing
                if (this.AgeOnlyWhenActive && !this.PartIsActive())
                {
                    // The part was "updated", it was just not in use
                    this.lastFixedUpdate = now;
                    return;
                }


                // At this point, the update is due: update the part's age
                if (this.AgeOnlyWhenActive) this.Age += now - this.lastFixedUpdate;
                else this.Age = now - this.timerStart;

                // Save the time of this update
                this.lastFixedUpdate = now;


                // Update the failure rate and the MTBF;
                this.currMTBF = this.initialMTBF * (float)Math.Exp(-this.Age / this.lifeTimeSecs);  // The MTBF decays exponentially

                // If the part has not already failed, toss the dice
                if (!this.hasFailed)
                {
                    if (UnityEngine.Random.Range(0f, 1f) < this.Lambda)
                    {
                        this.Fail();
                    }
                }

                // Subclass' update code
                this.DI_Update();

            }

            catch (Exception e)
            {
                LogException(e);
                this.Log("Module disabling after an exception!");
                this.enabled = false;
                return;
            }


        }



        [KSPEvent(guiActive = DangIt.EnableGuiFailure, guiName = "DEBUG Fail")]
        public void Fail()
        {
            try
            {
#if TRACE
                this.Log("Entering Fail(): enabled is " + this.enabled);
#endif
                if (!this.enabled) return;

                this.Log("FAIL!");

                // Mark the module as failed
                this.hasFailed = true;
                DangIt.ResetShipGlow(this.part.vessel);

                // Switch the events
                Events["Fail"].active = false;
                Events["EvaRepair"].active = true;

                // Show the message on screen
                DangIt.Broadcast(this.FailureMessage);

                // Run the custom failure code
                this.DI_Fail();

                return;
            }
            catch (Exception e)
            {
                LogException(e);
                this.Log("Module disabling after an exception!");
                this.enabled = false;
                return;
            }
        }




        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = DangIt.EvaRepairDistance, guiName = "EVA Repair", 
            active = false, externalToEVAOnly = true)]
        public void EvaRepair()
        {
            try
            {
#if TRACE
                this.Log("Entering EvaRepair(): enabled is " + this.enabled.ToString());
#endif
                if (!this.enabled) return;

                this.Log("Initiating EVA repair");

                // Get the EVA kerbal
                Part evaPart = DangIt.FindEVA();
                if (evaPart == null)
                {
#if DEBUG
                    DangIt.Broadcast("DangIt ERROR: couldn't find an active EVA!");
#endif
                    this.Log("ERROR: couldn't find an active EVA!");
                    return;
                }


                // Check if he is carrying enough spares
                if (evaPart.Resources.Contains(DangIt.Spares.Name) && evaPart.Resources[DangIt.Spares.Name].amount >= this.RepairCost)
                {
#if TRACE
                    this.Log("Spare parts check: OK! Repair allowed");
#endif
                    // Restore the part
                    this.hasFailed = false;

                    // Re-switch the events
                    Events["Fail"].active = DangIt.EnableGuiFailure;
                    Events["EvaRepair"].active = false;

                    DangIt.Broadcast(this.RepairMessage);

                    evaPart.Resources[DangIt.Spares.Name].amount -= this.RepairCost;
                    ResourceDisplay.Instance.Refresh();

                    // Custom repair code
                    this.DI_EvaRepair();
                }
                else
                {
#if TRACE
                    this.Log("Spare parts check: failed! Repair NOT allowed");
#endif
                    DangIt.Broadcast("You need " + this.RepairCost + " spares to repair this.");
                }

                DangIt.ResetShipGlow(this.part.vessel);

            }

            catch (Exception e)
            {
                LogException(e);
                this.Log("Module disabling after an exception!");
                this.enabled = false;
                return;
            }

        }







        private float Lambda
        {
            get { return LambdaFromMTBF() * LambdaMultiplier(); }
        }



        private float LambdaFromMTBF() 
        {
            try
            {
                return (1f / this.currMTBF) / 3600f * this.UpdateInterval;
            }
            catch (Exception e)
            {
                this.LogException(e);
                return 0f;
            }
        }


        public override string GetInfo()
        {
#if TRACE
            this.Log("GetInfo");
#endif
            // This is the time (in hours) that it takes for the MTBF to drop to 1 hour
            double EOL = Math.Round(Math.Max(-LifeTime * Math.Log(1 / initialMTBF), 0));

            return ("MTBF: " + this.MTBF + " h"
                + "\nLifetime: " + this.LifeTime + " h"
                + "\nEOL: " + EOL + " h"
                + "\nRepair cost: " + this.RepairCost);
        }


        public void Log(string msg)
        {
            Vessel v = this.part.vessel;
            StringBuilder sb = new StringBuilder();

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

    }
}
