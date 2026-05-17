using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotSpawner : NpcSpawner
{
    [SerializeField] private float startingDifficulty = 0.5f;
    [SerializeField] private BotNameManager nameManager;
    public void FillLobbyWithBots()
    {
        var manager = CharacterManager.Instance;
        CurrentDifficultyValue = startingDifficulty;
        var unpickedHeroes = new List<HeroDatabase>
        {
            HeroDatabase.Recruit,
            HeroDatabase.BabaYaga,
            HeroDatabase.Dracula,
            HeroDatabase.Djinn
        };

        foreach (var mediator in manager.Mediators.Values)
        {
            var hero = mediator.Toolkit.DatabaseEntry;
            if (unpickedHeroes.Contains(hero))
            {
                unpickedHeroes.Remove(hero);
            }
        }

        var unpickedHeroesArray = unpickedHeroes.ToArray();
        GenericUtilities.ShuffleArray(unpickedHeroesArray);

        var spawnCount = Constants.maxPlayerCount - manager.Mediators.Count;

        for (int i = 0; i < spawnCount; i++)
        {
            if (i < unpickedHeroesArray.Length)
            {
                var hero = spawnConfig.NpcPrefabs
                    .Where(m => m.Toolkit.DatabaseEntry == unpickedHeroesArray[i])
                    .First();
                SpawnNpc(spawnConfig.SpawnPoints[i], hero);
            }
            else
            {
                SpawnNpc(spawnConfig.SpawnPoints[i], spawnConfig.RandomNpc);
            }
        }
    }
    protected override void OnSpawn(CharacterMediator mediator)
    {
        mediator.Killed += (m, _) => Adjust(m, true);
        mediator.ScoredAKill += (_, m) => Adjust(m, false);

        var name = nameManager.GetBotNameForHero(mediator.Toolkit.DatabaseEntry);
        mediator.NetworkInput.SetName(name);
    }

    private void Adjust(CharacterMediator mediator, bool increase)
    {
        var step = difficultyConfig.DifficultyStep;
        step *= increase ? 1f : -1f;
        mediator.AiDecisions.ShootingHandler.AdjustDifficulty(step);
    }
}
