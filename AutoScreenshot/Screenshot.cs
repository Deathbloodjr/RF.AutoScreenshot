using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoScreenshot
{
    public class Screenshot
    {
        internal static bool screenshotTaken = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath">The folder path to save screenshots to. Defaults to AutoScreenshot's config if blank.</param>
        public static void TakeScreenshot(string folderPath = "")
        {
            if (Plugin.Instance.ConfigEnabled.Value)
            {
                var now = DateTime.Now;
                string fileName = now.Year.ToString() + "-"
                                + now.Month.ToString() + "-"
                                + now.Day.ToString() + "-"
                                + now.Hour.ToString() + "-"
                                + now.Minute.ToString() + "-"
                                + now.Second.ToString() + ".png";

                if (folderPath == "")
                {
                    folderPath = Plugin.Instance.ConfigScreenshotFolder.Value;
                }

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(folderPath, fileName)));
                if (Plugin.Instance.ConfigTakeSteamScreenshots.Value)
                {
                    SteamScreenshots.TriggerScreenshot();
                }
                screenshotTaken = true;

                //Plugin.LogInfo("Screenshot saved to: " + Path.Combine(Plugin.Instance.ConfigScreenshotFolder.Value, fileName));
            }
        }
    }
}
