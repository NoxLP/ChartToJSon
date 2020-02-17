using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEntitiesSharedTypesReference.Exceptions
{
    [Serializable]
    public class IncorrectEntityUserControlDataContextTypeException : Exception
    {
        private static readonly string Message = "The Data Context (view model) of an entity user control must implement IEntityViewModel";

        public IncorrectEntityUserControlDataContextTypeException() { }
        public IncorrectEntityUserControlDataContextTypeException(string message) : base(message) { }
        public IncorrectEntityUserControlDataContextTypeException(string message, Exception inner) : base(message, inner) { }
        protected IncorrectEntityUserControlDataContextTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
