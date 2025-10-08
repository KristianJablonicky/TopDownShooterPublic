public class PlayerScore : IResettable
{
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;

    public void Reset()
    {
        Kills = 0;
        Deaths = 0;
    }
    //public int Assists { get; private set; }
}
