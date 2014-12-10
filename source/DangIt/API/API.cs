using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public partial class DangIt
    {
        public static int CountFailures(Vessel v)
        {
            return CountFailuresRecursive(v.rootPart);
        }

        private static int CountFailuresRecursive(Part part)
        {
            int counter = 0;

            // Count all the failures on the part
            foreach (FailureModule fm in part.Modules.OfType<FailureModule>())
                if (fm.HasFailed) counter++;
            
            // Count all the failures on the child parts
            foreach (Part child in part.children)
                counter += CountFailuresRecursive(child);

            return counter;
        }
    }
}
