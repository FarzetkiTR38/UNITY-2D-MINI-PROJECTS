namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Pure in-memory compatibility checks for expanding saved five-level campaigns to fifty levels.</summary>
    public static class CampaignProgressVerification
    {
        /// <summary>Verifies legacy progress, migration idempotence, best stars and the final catalog boundary.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Verify Expanded Campaign Progress")]
        public static void Run()
        {
            var player = JsonUtility.FromJson<PlayerData>("{\"currentLevel\":41,\"highestLevel\":42,\"tipCount\":7,\"freezeCount\":9," +
                "\"levelStars\":[{\"level\":1,\"stars\":2}]}");
            player.selectedGameMode = GameModeId.ColorShift;
            var defenseStars = player.levelStars;
            var shapes = JsonUtility.FromJson<ModeProgress>("{\"_currentLevel\":3,\"_highestLevel\":5,\"_stars\":[" +
                "{\"level\":1,\"stars\":1},{\"level\":2,\"stars\":2},{\"level\":3,\"stars\":3}," +
                "{\"level\":4,\"stars\":2},{\"level\":5,\"stars\":3}]}");
            player.shapesProgress = shapes;
            var colorShift = player.GetModeProgress(GameModeId.ColorShift);
            colorShift.Complete(1, 2, 5);
            int aggregate = player.GetTotalStars(), bestTotal = shapes.TotalStars;
            Require(shapes.HighestLevel == 5 && shapes.GetStars(5) == 3, "old completed MVP is capped at five");
            Require(shapes.ExpandCatalog(50) && shapes.HighestLevel == 6, "completed MVP immediately unlocks six");
            Require(shapes.CurrentLevel == 3 && shapes.TotalStars == bestTotal && player.GetTotalStars() == aggregate,
                "migration preserves selection, stars and leaderboard total");
            Require(!shapes.ExpandCatalog(50), "migration is idempotent");
            Require(colorShift.HighestLevel == 2 && colorShift.TotalStars == 2, "modes migrate independently");
            shapes.Complete(2, 3, 50); shapes.Complete(2, 1, 50);
            Require(shapes.GetStars(2) == 3 && shapes.TotalStars == bestTotal + 1, "only a better result increases stars once");
            shapes.Complete(6, 2, 50);
            Require(shapes.HighestLevel == 7 && shapes.CurrentLevel == 3, "six is playable without replaying five");
            var roundTrip = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(player));
            Require(roundTrip.shapesProgress.HighestLevel == 7 && roundTrip.shapesProgress.CurrentLevel == 3 &&
                roundTrip.shapesProgress.GetStars(2) == 3, "expanded progress survives serialization");
            Require(player.currentLevel == 41 && player.highestLevel == 42 && player.tipCount == 7 && player.freezeCount == 9 &&
                ReferenceEquals(player.levelStars, defenseStars) && player.GetStarsForLevel(1) == 2 &&
                player.selectedGameMode == GameModeId.ColorShift, "Defense fields, inventory and selected mode stay untouched");
            CheckGaps(); CheckBoundaries();
            Debug.Log("[ArrowSwarm] PASS: five-to-fifty migration, selection, best stars, independent modes, holes, " +
                "legacy saves, serialization and final-level boundary. In-memory data only; no DataManager or save writes.");
        }

        private static void CheckGaps()
        {
            var holes = JsonUtility.FromJson<ModeProgress>("{\"_currentLevel\":4,\"_highestLevel\":5,\"_stars\":[" +
                "{\"level\":1,\"stars\":3},{\"level\":2,\"stars\":2},{\"level\":4,\"stars\":3},{\"level\":5,\"stars\":3}]}");
            holes.ExpandCatalog(50);
            Require(holes.HighestLevel == 5 && holes.CurrentLevel == 4, "gaps do not invent completion of the missing level");
            var sparse = JsonUtility.FromJson<ModeProgress>("{\"_highestLevel\":1,\"_stars\":[" +
                "{\"level\":1,\"stars\":3},{\"level\":3,\"stars\":3},{\"level\":50,\"stars\":3}]}");
            sparse.ExpandCatalog(50);
            Require(sparse.HighestLevel == 2 && sparse.GetStars(50) == 3, "isolated later records cannot skip locked levels");
            var empty = JsonUtility.FromJson<ModeProgress>("{\"_currentLevel\":0,\"_highestLevel\":0,\"_stars\":null}");
            empty.ExpandCatalog(50);
            Require(empty.CurrentLevel == 1 && empty.HighestLevel == 1 && empty.TotalStars == 0, "empty legacy records start safely at one");
            Require(!empty.ExpandCatalog(0), "unavailable catalogs leave progress unchanged");
            var legacy = JsonUtility.FromJson<PlayerData>("{\"currentLevel\":12,\"highestLevel\":13}");
            legacy.GetModeProgress(GameModeId.Shapes).ExpandCatalog(50);
            Require(legacy.GetModeProgress(GameModeId.Shapes).HighestLevel == 1 && legacy.currentLevel == 12 && legacy.highestLevel == 13,
                "saves predating puzzle modes preserve Defense and initialize the new mode");
        }

        private static void CheckBoundaries()
        {
            var complete = new ModeProgress();
            for (int level = 1; level <= 50; level++) complete.Complete(level, 3, 50);
            complete.Select(50); complete.ExpandCatalog(50); complete.Complete(51, 3, 50);
            Require(complete.HighestLevel == 50 && complete.CurrentLevel == 50 && complete.TotalStars == 150 &&
                complete.GetStars(51) == 0, "finishing fifty never unlocks fifty-one or grants extra stars");
        }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Campaign progress verification: " + message); }
    }
}
