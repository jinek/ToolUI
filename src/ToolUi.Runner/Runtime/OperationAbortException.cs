using System;
using System.Runtime.Serialization;

namespace ToolUi.Runner.Runtime
{
    [Serializable]
    public class OperationAbortException : ApplicationException
    {
        internal OperationAbortException()
        {
        }

        protected OperationAbortException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}