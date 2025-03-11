using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using AutoScreenshot.Plugins;
using UnityEngine;
using System.Collections;
using SaveProfileManager.Plugins;
using System.Reflection;

namespace AutoScreenshot
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, ModName, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public const string ModName = "AutoScreenshot";

        public static Plugin Instance;
        private Harmony _harmony = null;
        public new static ManualLogSource Log;

        public static PluginSaveDataInterface plugin;

        public ConfigEntry<bool> ConfigEnabled;
        public ConfigEntry<bool> ConfigScreenshotHighScores;
        public ConfigEntry<bool> ConfigScreenshotNewCrowns;
        public ConfigEntry<bool> ConfigScreenshotEverything;
        public ConfigEntry<bool> ConfigTakeSteamScreenshots;
        public ConfigEntry<string> ConfigScreenshotFolder;



        public override void Load()
        {
            Instance = this;

            Log = base.Log;

            SetupConfig(Config, Path.Combine("BepInEx", "data", ModName));
            SetupHarmony();

            var isSaveManagerLoaded = IsSaveManagerLoaded();
            if (isSaveManagerLoaded)
            {
                AddToSaveManager();
            }
        }

        private void SetupConfig(ConfigFile config, string saveFolder)
        {
            string dataFolder = Path.Combine("BepInEx", "data", ModName);

            ConfigEnabled = config.Bind("General",
                "Enabled",
                true,
                "Enables the mod.");

            ConfigScreenshotHighScores = config.Bind("General",
                "ScreenshotHighScores",
                true,
                "Screenshots high scores.");

            ConfigScreenshotNewCrowns = config.Bind("General",
                "ScreenshotNewCrowns",
                true,
                "Screenshots newly obtained crowns.");

            ConfigScreenshotEverything = config.Bind("General",
                "ScreenshotEverything",
                false,
                "Screenshots every score, regardless of if it's a high score or a new crown.");

            ConfigTakeSteamScreenshots = config.Bind("General",
                "TakeSteamScreenshots",
                false,
                "Will enable taking steam screenshots in addition to saving images locally.");

            ConfigScreenshotFolder = config.Bind("General",
                "ScreenshotFolder",
                Path.Combine(saveFolder, "Screenshots"),
                "Folder location to place screenshots in.");
        }

        private void SetupHarmony()
        {
            // Patch methods
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            LoadPlugin();
        }

        public static void LoadPlugin()
        {
            if (Instance.ConfigEnabled.Value)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                result &= Instance.PatchFile(typeof(AutoScreenshotPatch));
                if (result)
                {
                    Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.", LogType.Error);
                    // Unload this instance of Harmony
                    // I hope this works the way I think it does
                    Instance._harmony.UnpatchSelf();
                }
            }
            else
            {
                UnloadPlugin();
                Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
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
                Logger.Log("File patched: " + type.FullName);
#endif
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Failed to patch file: " + type.FullName);
                Logger.Log(e.Message);
                return false;
            }
        }

        public static void UnloadPlugin()
        {
            Instance._harmony.UnpatchSelf();
            Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} has been unpatched.");
        }

        public void AddToSaveManager()
        {
            plugin = new PluginSaveDataInterface(MyPluginInfo.PLUGIN_GUID);
            plugin.AssignLoadFunction(LoadPlugin);
            plugin.AssignUnloadFunction(UnloadPlugin);
            //plugin.AssignReloadSaveFunction(ReloadPlugin);
            plugin.AssignConfigSetupFunction(SetupConfig);
            plugin.AddToManager();
            Logger.Log("Plugin added to SaveDataManager");
        }

        private bool IsSaveManagerLoaded()
        {
            try
            {
                Assembly loadedAssembly = Assembly.Load("com.DB.RF.SaveProfileManager");
                return loadedAssembly != null;
            }
            catch
            {
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
