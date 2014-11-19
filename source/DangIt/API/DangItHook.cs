using System;
using System.Linq;
using System.Reflection;

namespace ippo
{
    public static class DangItHook
    {
        private static bool? installed = null;
        private static Type scenarioType = null;

        public static int CountFailures(Vessel v)
        {
            return (int)scenarioType.GetMethod("CountFailures").Invoke(null, new object[] { v });
        }

        public static bool IsReady
        {
            get
            {
                return Installed && (Instance != null);
            }
        }

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


        private static Type FindType()
        {
            return AssemblyLoader.loadedAssemblies
                                 .SelectMany(a => a.assembly.GetTypes())
                                 .SingleOrDefault(t => t.FullName == "ippo.DangIt");
        }
    }

}