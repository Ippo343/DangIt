/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrewFiles;

namespace DangIt
{
    [CrewDataGenerator]
    class PerkGenerator : ICrewDataGenerator
    {
        public ConfigNode Generate(ConfigNode kerbalData)
        {
            ConfigNode result = new ConfigNode("PERKS");

            if (UnityEngine.Random.Range(0f, 1f) < 0.5f) result.AddValue("perks", "Electrician");
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f) result.AddValue("perks", "Mechanic");

            return result;
        }

        public bool MustRun(ConfigNode kerbalData)
        {
            return !kerbalData.HasNode("PERKS");
        }
    }
}
*/