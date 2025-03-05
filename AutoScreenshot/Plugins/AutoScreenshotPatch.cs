using HarmonyLib;
using Scripts.OutGame.SongSelect;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScreenshot.Plugins
{
    internal class AutoScreenshotPatch
    {
        [HarmonyPatch(typeof(ResultPlayer))]
        [HarmonyPatch(nameof(ResultPlayer.ToWaitState))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void ResultPlayer_ToWaitState_Prefix(ResultPlayer __instance)
        {
            TakeScreenshot(__instance);
        }

        [HarmonyPatch(typeof(ResultPlayer))]
        [HarmonyPatch(nameof(ResultPlayer.waitResultDisp))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void ResultPlayer_waitResultDisp_Prefix(ResultPlayer __instance)
        {
            TakeScreenshot(__instance);
        }

        private static void TakeScreenshot(ResultPlayer __instance)
        {
            if (!Screenshot.screenshotTaken)
            {
                var playerResult = __instance.localResults.ensoPlayerResult[__instance.playerNo];

                bool takeScreenshot = false;
                if (Plugin.Instance.ConfigScreenshotEverything.Value)
                {
                    takeScreenshot = true;
                    Logger.Log("Screenshot because ScreenshotEverything == true", LogType.Debug);
                }
                if (Plugin.Instance.ConfigScreenshotHighScores.Value)
                {
                    var prevHighScore = TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.EnsoData.ensoSettings.ensoPlayerSettings[__instance.playerNo].hiScore;
                    if (playerResult.score > prevHighScore)
                    {
                        takeScreenshot = true;
                        Logger.Log("Screenshot because playerResult.score > prevHighScore", LogType.Debug);
                    }
                }
                if (Plugin.Instance.ConfigScreenshotNewCrowns.Value)
                {
                    for (int i = 0; i < playerResult.isNewCrown.Length; i++)
                    {
                        if (playerResult.isNewCrown[i])
                        {
                            takeScreenshot = true;
                            Logger.Log("Screenshot because isNewCrown[" + i + "] == true", LogType.Debug);
                        }
                    }
                }


                if (takeScreenshot)
                {
                    TaikoSingletonMonoBehaviour<SaveIcon>.Instance.Deactive();
                    Screenshot.TakeScreenshot(Plugin.Instance.ConfigScreenshotFolder.Value);
                }
            }
        }

        [HarmonyPatch(typeof(ResultPlayer))]
        [HarmonyPatch(nameof(ResultPlayer.Start))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void ResultPlayer_Start_Prefix(ResultPlayer __instance)
        {
            Screenshot.screenshotTaken = false;
        }
    }
}
