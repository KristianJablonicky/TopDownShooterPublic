public class PlayerData
{
    public string Name { get; private set; }
    public bool Owner { get; private set; }
    public CharacterMediator Mediator { get; private set; }
    public TeamData Team { get; private set; }
    public PlayerScore PlayerScore { get; private set; } = new PlayerScore();
    public PlayerData(CharacterMediator mediator, TeamData team, string name, bool owner)
    {
        Mediator = mediator;
        Team = team;
        Owner = owner;

        if (name == string.Empty) name = "Player" + mediator.PlayerId.ToString();
        Name = name;
    }
}

public enum Team
{
    Orange,
    Cyan
}
