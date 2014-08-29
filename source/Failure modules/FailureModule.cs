using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ippo
{
    /// <summary>
    /// Base failure module: handles the aging of the part, causes the random failures
    /// and handles the EVA repair.
    /// </summary>
    public abstract class FailureModule : PartModule, IPartCostModifier
    {

        #region Custom strings

        public abstract string InspectionName { get; }
        public abstract string DebugName { get; }
        public abstract string RepairMessage { get; }
        public abstract string FailureMessage { get; }
        public abstract string FailGuiName { get; }
        public abstract string EvaRepairGuiName { get; }
        public abstract string MaintenanceString { get; }

        public virtual string InspectionMessage()
        {
            if (this.HasFailed)
                return "the part has failed!";

            float ratio = this.Age / this.LifeTimeSecs;

            if (ratio < 0.10)
                return "this part seems to be as good as new";
            else if (ratio < 0.50)
                return "this part is still in good condition";
            else if (ratio < 0.75)
                return "this part is starting to show its age";
            else if (ratio < 1.25)
                return "it looks like it's time to get a new one";
            else if (ratio < 2.00)
                return "it really isn't a good idea to keep using this part";
            else if (ratio < 3)
                return "this part needs replacing soon";
            else
                return "this part appears to be in terrible condition";
        }

        #endregion


        #region Methods to add the specific logic of the module

        protected virtual void DI_Reset() { }
        protected virtual void DI_OnLoad(ConfigNode node) { }
        protected virtual void DI_Start(StartState state) { }
        protected virtual void DI_RuntimeFetch() { }
        protected virtual void DI_Update() { }
        protected abstract bool DI_FailBegin();
        protected abstract void DI_Disable();
        protected abstract void DI_EvaRepair();
        protected virtual void DI_OnSave(ConfigNode node) { }
        public virtual bool PartIsActive() { return true; }
        protected virtual float LambdaMultiplier() { return 1f; } 

        #endregion


        public List<Perk> PerkRequirements = new List<Perk>();


        #region Fields from the cfg file

        [KSPField(isPersistant = true, guiActive = false)]
        public float MTBF = 1000f;                                  // Original Mean Time Between Failures.

        [KSPField(isPersistant = true, guiActive = false)]
        public float LifeTime = 1f;                                 // Time constant of the exponential decay

        [KSPField(isPersistant = true, guiActive = false)]
        public float RepairCost = 5f;                               // Amount of spares needed to repair the part

        [KSPField(isPersistant = true, guiActive = false)]
        public float RepairBonus = 0f;                              // Age discount during a repair

        [KSPField(isPersistant = true, guiActive = false)]
        public float MaintenanceCost = 1f;                          // Amount of spares needed to perform maintenance

        [KSPField(isPersistant = true, guiActive = false)]
        public float MaintenanceBonus = 0.2f;                       // Age discount for preemptive maintenance

        [KSPField(isPersistant = true, guiActive = false)]
        public float InspectionBonus = 60f;                         // Duration of the inspection discount

        [KSPField(isPersistant = true, guiActive = false)]
        public bool Silent = false;


        #endregion


        #region Internal state

        [KSPField(isPersistant = true, guiActive = false)]
        public bool HasInitted = false;

        [KSPField(isPersistant = true, guiActive = false)]
        public float TimeOfLastReset = float.PositiveInfinity;

        [KSPField(isPersistant = true, guiActive = false)]
        public float TimeOfLastInspection = float.NegativeInfinity;

        [KSPField(isPersistant = true, guiActive = false)]
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


        /// <summary>
        /// Istantenous chance of failure.
        /// </summary>
        public float Lambda()
        {
            return LambdaFromMTBF(this.CurrentMTBF)
                    * (1 + TemperatureMultiplier())     // the temperature increases the chance of failure
                    * LambdaMultiplier()                // optional multiplier from the child class
                    * InspectionMultiplier();           // apply inspection bonus
        }


        private float LambdaFromMTBF(float MTBF)
        {
            try
            {
                return (1f / MTBF) / 3600f * TimeWarp.fixedDeltaTime;
            }
            catch (Exception e)
            {
                OnError(e);
                return 0f;
            }
        }

        private float InspectionMultiplier()
        {
            float elapsed = (DangIt.Now() - this.TimeOfLastInspection);
            return Math.Max(0f, Math.Min(elapsed / this.InspectionBonus, 1f));
        }


        #endregion


        /// <summary>
        /// Coroutine that waits for the runtime to be ready
        /// and initializes the results from preference.
        /// </summary>
        IEnumerator RuntimeFetch()
        {
            // Wait for the server to be available
            while (DangIt.Instance == null || !DangIt.Instance.IsReady)
                yield return null;

            this.Events["Fail"].guiActive = DangIt.Instance.CurrentSettings.ManualFailures;
            this.Events["EvaRepair"].unfocusedRange = DangIt.Instance.CurrentSettings.MaxDistance;
            this.Events["Maintenance"].unfocusedRange = DangIt.Instance.CurrentSettings.MaxDistance;

            DI_RuntimeFetch();
        }


        /// <summary>
        /// Resets the failure state and age tracker.
        /// This must be called only at the beginning of the flight to initialize
        /// the age tracking.
        /// Put your reset logic in DI_Reset()
        /// </summary>
        protected void Reset()
        {
            try
            {
                this.Log("Resetting");

                float now = DangIt.Now();

                #region Internal state

                this.Age = 0;
                this.TimeOfLastReset = now;
                this.LastFixedUpdate = now;

                this.TimeOfLastInspection = float.NegativeInfinity;

                this.CurrentMTBF = this.MTBF;
                this.LifeTimeSecs = this.LifeTime * 3600f;
                this.HasFailed = false;
                #endregion

                #region Fail and repair events

                this.Events["Fail"].guiName = this.FailGuiName;

                this.Events["EvaRepair"].guiName = this.EvaRepairGuiName;
                this.Events["EvaRepair"].active = false;

                this.Events["Maintenance"].guiName = this.MaintenanceString;
              
                #endregion

                // Run the custom reset of the subclasses
                this.DI_Reset();

                this.HasInitted = true;
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }



        /// <summary>
        /// Load the values from the config node of the persistence file.
        /// Put your loading logic in DI_OnLoad()
        /// </summary>
        public override void OnLoad(ConfigNode node)
        {
            try
            {
                this.Log("OnLoad. Node:\n" + node.ToString());

                // Load all the internal state variables
                this.HasInitted = DangIt.Parse<bool>(node.GetValue("HasInitted"), false);
                this.Age = DangIt.Parse<float>(node.GetValue("Age"), defaultTo: 0f);
                this.TimeOfLastReset = DangIt.Parse<float>(node.GetValue("TimeOfLastReset"), defaultTo: float.PositiveInfinity);
                this.TimeOfLastInspection = DangIt.Parse<float>(node.GetValue("TimeOfLastInspection"), defaultTo: float.NegativeInfinity);
                this.LastFixedUpdate = DangIt.Parse<float>(node.GetValue("LastFixedUpdate"), defaultTo: 0f);
                this.CurrentMTBF = DangIt.Parse<float>(node.GetValue("CurrentMTBF"), defaultTo: float.PositiveInfinity);
                this.LifeTimeSecs = DangIt.Parse<float>(node.GetValue("LifeTimeSecs"), defaultTo: float.PositiveInfinity);
                this.HasFailed = DangIt.Parse<bool>(node.GetValue("HasFailed"), defaultTo: false);

                // Load the required perks, if any
                this.PerkRequirements = new List<Perk>();
                if (node.HasNode("PERKS"))
                {
                    foreach (string item in node.values)
                    {
                        this.PerkRequirements.Add(Perk.FromString(item));
                    }
                }

                // Run the subclass' custom onload
                this.DI_OnLoad(node);

                // If OnLoad is called during flight, call the start again
                // so that modules can be rescanned
                if (HighLogic.LoadedSceneIsFlight)
                    this.DI_Start(StartState.Flying);

                this.Log("OnLoad complete, age is " + this.Age); 

                base.OnLoad(node);

            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }



        /// <summary>
        /// Saves the internal state of the failure module to the persistence file.
        /// Put your custom save logic in DI_OnSave()
        /// </summary>
        public override void OnSave(ConfigNode node)
        {
            try
            {
                // Save the internal state
                node.SetValue("HasInitted", this.HasInitted.ToString());
                node.SetValue("Age", Age.ToString());
                node.SetValue("TimeOfLastReset", TimeOfLastReset.ToString());
                node.SetValue("TimeOfLastInspection", TimeOfLastInspection.ToString());
                node.SetValue("LastFixedUpdate", LastFixedUpdate.ToString());
                node.SetValue("CurrentMTBF", CurrentMTBF.ToString());
                node.SetValue("LifeTimeSecs", LifeTimeSecs.ToString());
                node.SetValue("HasFailed", HasFailed.ToString());

                // Run the subclass' custom onsave
                this.DI_OnSave(node);

                base.OnSave(node);
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }



        /// <summary>
        /// Module re-start logic. OnStart will be called usually once for each scene,
        /// editor included.
        /// Put your custom start logic in DI_Start(): if you need to act on other part's
        /// variable, this is the place to do it, not DI_Reset()
        /// </summary>
        public override void OnStart(PartModule.StartState state)
        {
            try
            {
                if (HighLogic.LoadedSceneIsFlight) // nothing to do in editor
                {
                    this.Log("Starting in flight: last reset " + TimeOfLastReset + ", now " + DangIt.Now());

                    // Reset the internal state at the beginning of the flight
                    // this condition also catches a revert to launch (+1 second for safety)
                    if (DangIt.Now() < (this.TimeOfLastReset + 1))
                        this.Reset();

                    // If the part was saved when it was failed,
                    // re-run the failure logic to disable it
                    // ONLY THE DISABLING PART IS RUN!
                    if (this.HasFailed)
                        this.DI_Disable();

                    DangIt.ResetShipGlow(this.part.vessel);
                }

                this.DI_Start(state);
                this.StartCoroutine("RuntimeFetch");
            }
            catch (Exception e)
            {
                OnError(e);
            }

        }


        /// <summary>
        /// Update logic on every physics frame update.
        /// Place your custom update logic in DI_Update()
        /// </summary>
        public void FixedUpdate()
        {
            try
            {
                // Only update the module during flight and after the re-initialization has run
                if (HighLogic.LoadedSceneIsFlight && this.HasInitted)
                {
                    float now = DangIt.Now();

                    float dt = now - LastFixedUpdate;

                    // The temperature aging is independent from the use of the part
                    this.Age += dt * this.TemperatureMultiplier();

                    if (!PartIsActive())
                    {
                        this.LastFixedUpdate = now;
                        return;
                    }

                    // If control reaches this point, the part is active: add the elapsed time to the age
                    this.Age += dt;

                    this.CurrentMTBF = this.MTBF * this.ExponentialDecay();

                    // If the part has not already failed, toss the dice
                    if (!this.HasFailed)
                    {
                        if (UnityEngine.Random.Range(0f, 1f) < this.Lambda())
                        {
                            this.Fail();
                        }
                    }

                    // Run custom update logic
                    this.DI_Update();

                    this.LastFixedUpdate = now; 
                }
            }

            catch (Exception e)
            {
                OnError(e);
            }
        }


        private float ExponentialDecay()
        {
            return (float)Math.Exp(-this.Age / this.LifeTimeSecs);
        }


        /// <summary>
        /// Increase the aging rate as the temperature increases.
        /// </summary>
        private float TemperatureMultiplier()
        {
            return 3 * (float)Math.Pow((Math.Max(part.temperature, 0) / part.maxTemp), 5);
        }



        [KSPEvent(active = true, guiActive = false, guiActiveUnfocused = true, unfocusedRange = 2f, externalToEVAOnly = true)]
        public void Maintenance()
        {
            this.Log("Initiating EVA maitenance");

            // Get the EVA part (parts can hold resources)
            Part evaPart = DangIt.FindEVAPart();
            if (evaPart == null)
            {
                DangIt.Broadcast("DangIt ERROR: couldn't find an active EVA!");
                this.Log("ERROR: couldn't find an active EVA!");
                return;
            }

            // Check if he is carrying enough spares
            if (evaPart.Resources.Contains(Spares.Name) && evaPart.Resources[Spares.Name].amount >= this.MaintenanceCost)
            {
                this.Log("Spare parts check: OK! Maintenance allowed allowed");
                DiscountAge(this.MaintenanceBonus);
                DangIt.Broadcast("This should last a little longer now");
            }
            else
            {
                this.Log("Spare parts check: failed! Maintenance NOT allowed");
                DangIt.Broadcast("You need " + this.MaintenanceCost + " spares to maintain this.");
            }

        }


        /// <summary>
        /// Initiates the part's failure.
        /// Put your custom failure code in DI_Fail()
        /// </summary>
        [KSPEvent(guiActive = false)]
        public void Fail()
        {
            try
            {
                this.Log("Initiating Fail()");

                // First, run the custom failure logic
                // The child class can refuse to fail in FailBegin()
                if (!this.DI_FailBegin())
                {
                    this.Log(this.DebugName + " has not agreed to fail, failure aborted!");
                    return;
                }
                else
                {
                    this.Log(this.DebugName + " has agreed to fail, failure allowed.");
                }

                // If control reaches this point, the child class has agreed to fail
                // Disable the part and handle the internal state and notifications

                this.DI_Disable();

                TimeWarp.SetRate(0, true);      // stop instantly
                this.SetFailureState(true);     // Sets the failure state, handles the events, handles the glow

                if (!this.Silent)
                {
                    DangIt.Broadcast(this.FailureMessage);
                    DangIt.PostMessage("Failure!",
                                       this.FailureMessage,
                                       MessageSystemButton.MessageButtonColor.RED,
                                       MessageSystemButton.ButtonIcons.ALERT);

                }

                DangIt.FlightLog(this.FailureMessage);

            }
            catch (Exception e)
            {
                OnError(e);
            }
        }


        /// <summary>
        /// Sets / resets the failure of the part.
        /// Also resets the ship's glow and sets the event's visibility
        /// </summary>
        protected void SetFailureState(bool state)
        {
            try
            {
                this.HasFailed = state;
                DangIt.ResetShipGlow(this.part.vessel);

                Events["Fail"].active = !state;
                Events["EvaRepair"].active = state;
                Events["Maintenance"].active = !state;
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }



        /// <summary>
        /// Initiates the part's EVA repair.
        /// The repair won't be executed if the kerbonaut doesn't have enough spare parts.
        /// Put your custom repair code in DI_Repair()
        /// </summary>
        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 2f, externalToEVAOnly = true)]
        public void EvaRepair()
        {
            try
            {
                this.Log("Initiating EVA repair");

                // Get the EVA part (parts can hold resources)
                Part evaPart = DangIt.FindEVAPart();
                
                if (evaPart == null)
                {
                    DangIt.Broadcast("DangIt ERROR: couldn't find an active EVA!");
                    this.Log("ERROR: couldn't find an active EVA!");
                    return;
                }

                // Check if the kerbal is able to perform the repair
                if ( CheckRepairConditions(evaPart) )
                {
                    this.DI_EvaRepair();
                    this.SetFailureState(false);

                    DangIt.FlightLog(this.RepairMessage);

                    float intelligence = 1 - evaPart.vessel.GetVesselCrew().First().stupidity;
                    float discountedCost = (float)Math.Round( RepairCost * (1 - UnityEngine.Random.Range(0f, intelligence)) );
                    float discount = RepairCost - discountedCost;

                    this.Log("Kerbal's intelligence: " + intelligence + ", discount: " + discount);

                    evaPart.RequestResource(Spares.Name, discountedCost);
                    ResourceDisplay.Instance.Refresh();

                    DangIt.Broadcast(this.RepairMessage, true);

                    this.DiscountAge(this.RepairBonus);

                    if (discount > 0)
                    {
                        DangIt.Broadcast(evaPart.vessel.GetVesselCrew().First().name + " was able to save " + discount + " spare parts");
                    }   
                }

                DangIt.ResetShipGlow(this.part.vessel);

            }
            catch (Exception e)
            {
                OnError(e);
            }

        }


        private bool CheckRepairConditions(Part evaPart)
        {
            bool allow = true;
            string reason = string.Empty;

            if (!evaPart.Resources.Contains(Spares.Name) || evaPart.Resources[Spares.Name].amount < this.RepairCost)
            {
                allow = false;
                reason = "not carrying enough spares";
                DangIt.Broadcast("You need " + this.RepairCost + " spares to repair this.", true);
            }

            if (allow)
                this.Log("Repair allowed!");
            else
                this.Log("Repair NOT allowed. Reason: " + reason);

            return allow;
        }


        /*
        public override string GetInfo()
        {
            try
            {
                // This is the time (in hours) that it takes for the MTBF to drop to 1 hour
                double EOL = Math.Round(Math.Max(-LifeTime * Math.Log(1 / this.MTBF), 0));

                return ("MTBF: " + this.MTBF + " h"
                    + "\nLifetime: " + this.LifeTime + " h"
                    + "\nEOL: " + EOL + " h"
                    + "\nRepair cost: " + this.RepairCost);
            }
            catch (Exception e)
            {
                this.OnError(e);
                return string.Empty;
            }
        }
        */


        private void DiscountAge(float percentage)
        {
            this.Age *= (1 - percentage);
        }

        #region Logging utilities

        public void Log(string msg)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("[DangIt]: ");
                sb.Append(this.DebugName);
                sb.Append("[" + this.GetInstanceID() + "]");
                if (part.vessel != null) sb.Append("[Ship: " + part.vessel.vesselName + "]");
                sb.Append(": " + msg);

                Debug.Log(sb.ToString());
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }



        public void LogException(Exception e)
        {
            this.Log("ERROR: " + e.Message + "\n" + e.StackTrace);
        }


        /// <summary>
        /// Exception handling code: logs the exception message and then disables the module.
        /// Disabled modules are not updated.
        /// </summary>
        protected void OnError(Exception e)
        {
            LogException(e);
            this.enabled = false;   // prevent the module from updating
            return;
        } 

        #endregion


        public float GetModuleCost()
        {
            return (this.ExponentialDecay() - 1) * this.part.partInfo.cost;
        }
    }

}
