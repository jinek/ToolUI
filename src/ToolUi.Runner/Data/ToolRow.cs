namespace ToolUi.Runner.Data
{
    public record ToolRow(string Id, string Version, string Commands, string Manifest)
    {
        public const string GlobalManifestKey = "Global";

        public ToolRow(string id, string version, string commands) : this(id, version, commands, GlobalManifestKey)
        {
        }
    }
}