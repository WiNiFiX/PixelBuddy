using System;
using System.Windows.Media;
using Microsoft.Win32;

namespace Helpers
{
    public static class GameDVR
    {
        public static bool IsAppCapturedEnabled
        {
            get
            {
                try
                {
                    const string appCaptureEnabledKey = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\GameDVR";
                    if ((int)Registry.GetValue(appCaptureEnabledKey, "AppCaptureEnabled", -1) == 1)
                    {
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    Log.Write("Failed to find GameDVR-AppCaptureEnabled registry key", Brushes.Red);
                    Log.Write("Reason: " + ex.Message, Brushes.Gray);
                }
                return false;
            }
        }

        public static bool IsGameDVREnabled
        {
            get
            {
                try
                {
                    const string appCaptureEnabledKey = "HKEY_CURRENT_USER\\System\\GameConfigStore";
                    if ((int)Registry.GetValue(appCaptureEnabledKey, "GameDVR_Enabled", -1) == 1)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to find GameDVR-GameConfigStore registry key", Brushes.Gray);
                    Log.Write("Reason: " + ex.Message, Brushes.Gray);
                }
                return false;
            }
        }

        public static void SetGameDVREnabled(int value)
        {
            const string gameDVREnabledKey = "HKEY_CURRENT_USER\\System\\GameConfigStore";
            Registry.SetValue(gameDVREnabledKey, "GameDVR_Enabled", value);
        }

        public static void SetAppCapturedEnabled(int value)
        {
            const string appCaptureEnabledKey =
                "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\GameDVR";
            var appCaptureEnabled = (int) Registry.GetValue(appCaptureEnabledKey, "AppCaptureEnabled", value);
        }
    }
}