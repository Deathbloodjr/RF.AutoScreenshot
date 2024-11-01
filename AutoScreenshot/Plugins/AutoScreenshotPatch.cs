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
                if ((Plugin.Instance.ConfigScreenshotHighScores.Value &&
                        __instance.localResults.ensoPlayerResult[__instance.playerNo].isHiScore) ||
                        (Plugin.Instance.ConfigScreenshotNewCrowns.Value &&
                        (__instance.ensoParam.EnsoResult.ensoPlayerResult[__instance.playerNo].isNewCrown[2] || // Silver
                        __instance.ensoParam.EnsoResult.ensoPlayerResult[__instance.playerNo].isNewCrown[3] || // Gold
                        __instance.ensoParam.EnsoResult.ensoPlayerResult[__instance.playerNo].isNewCrown[4])))   // Rainbow
                {
                    TaikoSingletonMonoBehaviour<SaveIcon>.Instance.Deactive();
                    Screenshot.TakeScreenshot();
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
