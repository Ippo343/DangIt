using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DangIt
{
    [Serializable]
    public class PartModuleException : Exception
    {
        public PartModuleException() { }
        public PartModuleException(string message) : base(message) { }
        public PartModuleException(string message, Exception inner) : base(message, inner) { }
        protected PartModuleException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
