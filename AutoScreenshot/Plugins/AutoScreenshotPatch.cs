using HarmonyLib;
using Scripts.OutGame.SongSelect;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoScreenshot.Plugins
{
    internal class AutoScreenshotPatch
    {
        static bool screenshotSkipped = false;
        static bool screenshotTaken = false;

        [HarmonyPatch(typeof(ResultPlayer))]
        [HarmonyPatch(nameof(ResultPlayer.ToWaitState))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void ResultPlayer_ToWaitState_Prefix(ResultPlayer __instance)
        {
            // This caused the 000000 score issue
            if (!screenshotSkipped)
            {
                Logger.Log("ResultPlayer_ToWaitState_Prefix", LogType.Debug);
                TakeScreenshot(__instance);
            }
            screenshotSkipped = false;
        }


        [HarmonyPatch(typeof(ResultPlayer))]
        [HarmonyPatch(nameof(ResultPlayer.SkipDispResult))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void ResultPlayer_SkipDispResult_Postfix(ResultPlayer __instance)
        {
            screenshotSkipped = true;
            Logger.Log("ResultPlayer_SkipDispResult_Postfix", LogType.Debug);
            Plugin.Instance.StartCoroutine(DelayTakeScreenshot(__instance));
        }

        private static IEnumerator DelayTakeScreenshot(ResultPlayer __instance)
        {
            yield return new WaitForSeconds(0.4f);
            TakeScreenshot(__instance);
        }

        private static void TakeScreenshot(ResultPlayer __instance)
        {
            if (!screenshotTaken)
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
                    // I wonder if this has issues with nijiiro scoring?
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
            screenshotTaken = false;
        }
    }
}
