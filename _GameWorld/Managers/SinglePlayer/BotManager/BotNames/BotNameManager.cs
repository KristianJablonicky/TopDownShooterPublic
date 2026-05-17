using UnityEngine;

public class BotNameManager : MonoBehaviour
{
    [SerializeField] private BotName[] botNames;
    private int[] index;
    private void Awake()
    {
        index = new int[botNames.Length];
        foreach (var botName in botNames)
        {
            GenericUtilities.ShuffleArray(botName.BotNames);
        }
    }
    

    public string GetBotNameForHero(HeroDatabase hero)
    {
        var i = (int)hero;
        var returnVal = botNames[i].BotNames[index[i]];
        index[i]++;
        return returnVal;
    }
}
