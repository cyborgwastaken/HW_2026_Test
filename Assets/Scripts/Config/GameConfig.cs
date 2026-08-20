using System;

namespace Doofus.Config
{
    // Pure data shape mirroring doofus_diary.json's field names (required for
    // JsonUtility to deserialize it directly). The field initializers below are NOT
    // the game's real values - they're a fallback used only if GameConfigLoader can't
    // read/parse the actual file. GameConfigLoader.cs is what performs the real read at
    // runtime (via File.ReadAllText + JsonUtility.FromJson) and overwrites these with
    // whatever the JSON actually contains.
    [Serializable]
    public class PlayerData
    {
        public float speed = 3f;
    }

    [Serializable]
    public class PulpitData
    {
        public float min_pulpit_destroy_time = 4f;
        public float max_pulpit_destroy_time = 5f;
        public float pulpit_spawn_time = 2.5f;
    }

    [Serializable]
    public class GameConfig
    {
        public PlayerData player_data = new PlayerData();
        public PulpitData pulpit_data = new PulpitData();
    }
}
