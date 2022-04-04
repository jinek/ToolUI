namespace ToolUi.Runner.Data
{
    [Parsable(@"^-+\n(?<Id>\S+)\nLatest Version:\s(?<Version>\S+)\nAuthors:\s(?<Authors>.+)\n.+?\nDownloads:\s(?<Downloads>\S+)\nVerified: (?<Verified>\S+)\nDescription: (?<Description>.+?)\nVersions:\s+(^\s+(?<Versions>\S+?)\sDownloads: \d+\n)+")]
    public record SearchRow(string Id, string Version, string Authors, string Downloads, string Verified,string Description, string[] Versions)
    {
        public string Version { get; set; } = Version;
    }
}