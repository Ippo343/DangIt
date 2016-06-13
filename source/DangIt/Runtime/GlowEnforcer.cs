using DangIt.Utilities;
using UnityEngine;
using System.Linq;


namespace DangIt
{
    /// <summary>
    /// It appears that with 1.1 something has been done to how the vessels' glow is handled.
    /// When you switch vessels, the glow is reset on everything except the active vessel.
    /// Unfortunately there seems to be an event that is fired just before switching but not one
    /// that is fired after that, so at this moment my only solution is to refresh everything always.
    /// 
    /// TODO: find a better way to do this.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DangItGlowEnforcer : MonoBehaviour
    {
        void Update()
        {
            foreach (Vessel v in FlightGlobals.Vessels.Where(v => v.loaded))
            {
                CUtils.ResetShipGlow(v);
            }
        }
    }
}
