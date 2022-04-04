namespace ToolUi.Runner.Data
{
    [Parsable(@"^(?<Id>\S+)\s+(?<Version>\S+)\s+(?<Commands>\S+)(\s+(?<Manifest>\S+))?\s*?$")]
    public record ToolRow(string Id, string Version, string Commands, string Manifest)
    {
        public const string GlobalManifestKey = "Global";
        public string Manifest { get; } = Manifest is null or "" ? GlobalManifestKey : Manifest;
    }
}