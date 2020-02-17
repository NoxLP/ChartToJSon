using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.Exceptions
{
    [Serializable]
    public class LineDraggerUserControlNotFoundException : Exception
    {
        public LineDraggerUserControlNotFoundException() { }
        public LineDraggerUserControlNotFoundException(string message) : base(message) { }
        public LineDraggerUserControlNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected LineDraggerUserControlNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
