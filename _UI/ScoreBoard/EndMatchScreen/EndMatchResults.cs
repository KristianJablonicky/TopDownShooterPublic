public class EndMatchResults
{
    public static EndMatchResults Instance { get; private set; } = new();
    public PlayerEntryData[] PlayerEntries { get; set; }
}
