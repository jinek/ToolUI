using System;

namespace ToolUi.Runner.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ParsableAttribute : Attribute
    {
        public ParsableAttribute(string regexp)
        {
            Regexp = regexp;
        }

        public string Regexp { get; }
    }
}