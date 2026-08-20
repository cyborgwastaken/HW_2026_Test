using System;

namespace Doofus.Core
{
    // Static event bus decoupling gameplay systems (spawner, scoring, UI, camera) from
    // one another so none of them need direct references to the others.
    public static class GameEvents
    {
        public static event Action OnGameReset;
        public static event Action OnGameStart;
        public static event Action OnPulpitLanded;
        public static event Action<int> OnScoreChanged;
        public static event Action OnPlayerFell;
        public static event Action OnGameOver;

        public static void RaiseGameReset() => OnGameReset?.Invoke();
        public static void RaiseGameStart() => OnGameStart?.Invoke();
        public static void RaisePulpitLanded() => OnPulpitLanded?.Invoke();
        public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
        public static void RaisePlayerFell() => OnPlayerFell?.Invoke();
        public static void RaiseGameOver() => OnGameOver?.Invoke();
    }
}
