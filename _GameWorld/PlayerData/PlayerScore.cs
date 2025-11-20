public class PlayerScore : IResettable
{
    public ObservableValue<int> Kills { get; set; } = new(0);
    public ObservableValue<int> Deaths { get; set; } = new(0);
    
    public void Reset()
    {
        Kills.Set(0);
        Deaths.Set(0);
    }
    //public int Assists { get; private set; }
}
