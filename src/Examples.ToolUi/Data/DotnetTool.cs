namespace Examples.ToolUi.Data
{
    public record DotnetTool(string Id, string Version, string Commands, string Manifest)
    {
        public const string ManifestKeyword = "Global";

        public DotnetTool(string id, string version, string commands) : this(id, version, commands, ManifestKeyword)
        {
        }
    }
}