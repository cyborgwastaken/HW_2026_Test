using System;

namespace Doofus.Config
{
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
