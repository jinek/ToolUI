namespace ToolUi.Runner.Data
{
    [Parsable(@"[\a\n](?<str>.+)$")]
    public record RawStringRow(string str);
}