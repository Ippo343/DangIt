using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DangIt
{
    /// <summary>
    /// Helper class that can manage multiple engine modules in the same part,
    /// both of type ModuleEngines and ModuleEnginesFX.
    /// Use it to abstract an engine module.
    /// </summary>
    public class EngineManager
    {
		public List<ModuleEngines> engines;
		public List<ModuleEnginesFX> enginesFX;

        /// <summary>
        /// Creates an EngineManager for a part.
        /// Automatically finds all the engine modules in the part.
        /// </summary>
        public EngineManager(Part p)
        {
            this.engines = p.Modules.OfType<ModuleEngines>().ToList();
            this.enginesFX = p.Modules.OfType<ModuleEnginesFX>().ToList();
        }

        /// <summary>
        /// True when there is any active engine module on the part.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return engines.Any(e => EngineIsActive(e)) |
                       enginesFX.Any(e => EngineIsActive(e));
            }
        }


        /// <summary>
        /// Returns true if an engine is currently in use.
        /// </summary>
        public static bool EngineIsActive(ModuleEngines engineModule)
        {
            return (engineModule.enabled &&
                    engineModule.EngineIgnited &&
                   (engineModule.currentThrottle > 0));
        }

        public static bool EngineIsActive(ModuleEnginesFX engineModule)
        {
            return (engineModule.enabled &&
                    engineModule.EngineIgnited &&
                   (engineModule.currentThrottle > 0));
        }


        /// <summary>
        /// Returns the maximum throttle of all the managed engines.
        /// </summary>
        public float CurrentThrottle
        {
            get
            {
                float throttle = 0;

                if (engines.Count > 0) throttle = engines.Max(e => e.currentThrottle);
                if (enginesFX.Count > 0) throttle = Math.Max(throttle, enginesFX.Max(e => e.currentThrottle));

                return throttle;
            }
        }


        /// <summary>
        /// Disables all the engine modules.
        /// The engine is shut down, the effects stopped, and the behaviour disabled.
        /// </summary>
        public void Disable()
        {
            engines.ForEach(e => disable(e));
            enginesFX.ForEach(e => disable(e));
        }

        /// <summary>
        /// Enables all the managed engine modules.
        /// </summary>
        public void Enable()
        {
            engines.ForEach(e => enable(e));
            enginesFX.ForEach(e => enable(e));
        }


        private void disable(ModuleEngines m)
        {
            m.Shutdown();
            m.DeactivateRunningFX();
            m.DeactivatePowerFX();
            m.enabled = false;
        }

        private void disable(ModuleEnginesFX m)
        {
            m.Shutdown();
            m.DeactivateLoopingFX();
            m.enabled = false;
        }

        private void enable(ModuleEngines m)
        {
            m.enabled = true;
        }

        private void enable(ModuleEnginesFX m)
        {
            m.enabled = true;
        }

    }
}
