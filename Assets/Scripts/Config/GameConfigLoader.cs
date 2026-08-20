using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Doofus.Config
{
    // Reads Doofus's Diary (StreamingAssets/doofus_diary.json) and exposes the parsed values.
    // Falls back to sane defaults if the file is missing, unreachable, or malformed so the
    // game is always playable even when the config can't be read.
    public class GameConfigLoader : MonoBehaviour
    {
        private const string FileName = "doofus_diary.json";

        public static GameConfigLoader Instance { get; private set; }
        public GameConfig Config { get; private set; }
        public bool IsLoaded { get; private set; }

        public static event Action<GameConfig> OnConfigReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(LoadConfig());
        }

        private IEnumerator LoadConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);
            GameConfig loaded = null;

            if (path.Contains("://"))
            {
                using UnityWebRequest request = UnityWebRequest.Get(path);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    loaded = TryParse(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogWarning($"[GameConfigLoader] Failed to fetch config from '{path}': {request.error}. Using defaults.");
                }
            }
            else
            {
                try
                {
                    loaded = TryParse(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[GameConfigLoader] Failed to read config at '{path}': {e.Message}. Using defaults.");
                }
            }

            Config = loaded ?? new GameConfig();
            IsLoaded = true;
            OnConfigReady?.Invoke(Config);
        }

        private GameConfig TryParse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[GameConfigLoader] Config file was empty. Using defaults.");
                return null;
            }

            try
            {
                GameConfig parsed = JsonUtility.FromJson<GameConfig>(json);
                if (parsed == null)
                {
                    Debug.LogWarning("[GameConfigLoader] Config parsed to null. Using defaults.");
                    return null;
                }

                parsed.player_data ??= new PlayerData();
                parsed.pulpit_data ??= new PulpitData();
                return parsed;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameConfigLoader] Malformed config JSON: {e.Message}. Using defaults.");
                return null;
            }
        }
    }
}
