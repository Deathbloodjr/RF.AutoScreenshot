using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using AutoScreenshot.Plugins;
using UnityEngine;
using System.Collections;

namespace AutoScreenshot
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, ModName, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public const string ModName = "AutoScreenshot";

        public static Plugin Instance;
        private Harmony _harmony = null;
        public new static ManualLogSource Log;


        public ConfigEntry<bool> ConfigEnabled;
        public ConfigEntry<bool> ConfigScreenshotHighScores;
        public ConfigEntry<bool> ConfigScreenshotNewCrowns;
        public ConfigEntry<bool> ConfigTakeSteamScreenshots;
        public ConfigEntry<string> ConfigScreenshotFolder;



        public override void Load()
        {
            Instance = this;

            Log = base.Log;

            SetupConfig();
            SetupHarmony();
        }

        private void SetupConfig()
        {
            var dataFolder = Path.Combine("BepInEx", "data", ModName);

            ConfigEnabled = Config.Bind("General",
                "Enabled",
                true,
                "Enables the mod.");

            ConfigScreenshotHighScores = Config.Bind("General",
                "ScreenshotHighScores",
                true,
                "Enables screenshoting high scores automatically.");

            ConfigScreenshotNewCrowns = Config.Bind("General",
                "ScreenshotNewCrowns",
                true,
                "Enables screenshoting newly obtained crowns automatically.");

            ConfigTakeSteamScreenshots = Config.Bind("General",
                "TakeSteamScreenshots",
                true,
                "Will enable taking steam screenshots in addition to saving images locally.");

            ConfigScreenshotFolder = Config.Bind("General",
                "ScreenshotFolder",
                Path.Combine(dataFolder, "Screenshots"),
                "Enables the example mods.");
        }

        private void SetupHarmony()
        {
            // Patch methods
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            if (ConfigEnabled.Value)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                result &= PatchFile(typeof(AutoScreenshotPatch));
                if (result)
                {
                    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    Log.LogError($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.");
                    _harmony.UnpatchSelf();
                }
            }
            else
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
            }
        }

        private bool PatchFile(Type type)
        {
            if (_harmony == null)
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            }
            try
            {
                _harmony.PatchAll(type);
#if DEBUG
                Log.LogInfo("File patched: " + type.FullName);
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.LogInfo("Failed to patch file: " + type.FullName);
                Log.LogInfo(e.Message);
                return false;
            }
        }

        public static MonoBehaviour GetMonoBehaviour() => TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
        public void StartCoroutine(IEnumerator enumerator)
        {
            GetMonoBehaviour().StartCoroutine(enumerator);
        }
    }
}
