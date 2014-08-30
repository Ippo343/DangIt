using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    class EngineManager
    {
        List<ModuleEngines> engines;
        List<ModuleEnginesFX> enginesFX;

        public EngineManager(Part p)
        {
            this.engines = p.Modules.OfType<ModuleEngines>().ToList();
            this.enginesFX = p.Modules.OfType<ModuleEnginesFX>().ToList();
        }

        public bool IsActive
        {
            get
            {
                return engines.Any(e => ippo.DangIt.EngineIsActive(e)) |
                       enginesFX.Any(e => ippo.DangIt.EngineIsActive(e));
            }
        }

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


        public void Disable()
        {
            engines.ForEach(e => disable(e));
            enginesFX.ForEach(e => disable(e));
        }

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
