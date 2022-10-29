namespace ToolUi.Runner.Data
{
    [Parsable(@"^(?<str>.+)$")]
    public record RawStringRow(string str);
}