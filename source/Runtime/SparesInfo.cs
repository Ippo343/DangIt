using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
    /// <summary>
    /// Constants related to the spare parts resource
    /// </summary>
    public static class Spares
    {
        /// <summary>
        /// Amount of Spare Parts that is taken each time the button is pressed
        /// </summary>
        public static readonly double Increment = 1f;

        /// <summary>
        /// Maximum amount that a kerbal can carry
        /// </summary>
        public static readonly double MaxEvaAmount = 10f;

        /// <summary>
        /// Resource name as a string
        /// </summary>
        public static readonly string Name = "SpareParts";
    }

}
