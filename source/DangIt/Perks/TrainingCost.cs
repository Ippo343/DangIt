using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    public struct TrainingCost
    {
        public SkillLevel Level;
        public float Science;
        public float Funds;

        public TrainingCost(SkillLevel level, float science, float funds)
        {
            this.Level = level;
            this.Science = science;
            this.Funds = funds;
        }
    }
}
