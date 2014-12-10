using System;
using System.Linq;
using System.Reflection;

namespace ippo
{
    /// <summary>
    /// Reflection wrapper to interact with DangIt
    /// </summary>
    public static class DangItHook
    {      
        #region Check installation and readiness

        /// <summary>
        /// Checks if DangIt is installed in the game.
        /// </summary>
        public static bool Installed
        {
            get
            {
                if (installed == null)
                {
                    scenarioType = FindType();
                    installed = !(scenarioType == null);
                }

                return (bool)installed;
            }
        }

        /// <summary>
        /// Returns the instance of the DangIt ScenarioModule
        /// </summary>
        public static ScenarioModule Instance
        {
            get
            {
                if (Installed)
                {
                    object instance = scenarioType
                                      .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                                      .GetValue(null, null);

                    return (ScenarioModule)instance;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Returns true if DangIt is installed and its instance is not null.
        /// </summary>
        public static bool IsReady
        {
            get
            {
                return Installed && (Instance != null);
            }
        }

        #endregion

        #region Interact with failures

        public static bool HasAnyFailures(this Vessel v)
        {
            return CountFailures(v) > 0;
        }

        public static int CountFailures(this Vessel v)
        {
            return (int)scenarioType.GetMethod("CountFailures").Invoke(null, new object[] { v });
        }

        #endregion

        #region Gears and pulleys

        private static bool? installed = null;
        private static Type scenarioType = null;

        private static Type FindType()
        {
            return AssemblyLoader.loadedAssemblies
                                 .SelectMany(a => a.assembly.GetTypes())
                                 .SingleOrDefault(t => t.FullName == "ippo.DangIt");
        }

        #endregion
    }

}