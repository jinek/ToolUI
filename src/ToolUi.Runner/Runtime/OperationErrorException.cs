using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ToolUi.Runner.Runtime
{
    [Serializable]
    public class OperationErrorException : ApplicationException
    {
        public int ExitCode { get; }

        public OperationErrorException(string message, int exitCode) : base(message)
        {
            ExitCode = exitCode;
        }

        protected OperationErrorException([NotNull] SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}