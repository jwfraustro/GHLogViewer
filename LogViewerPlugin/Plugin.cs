using System;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace LogViewerPlugin
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            base.Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Plugin.harmony.PatchAll();
        }

        public static Harmony harmony = new Harmony("mod.logviewer");
    }

    [HarmonyPatch(typeof(LogWindow), "Inicializar")]
    class Patch_LogWindow_Inicializar
    {
        static void Postfix(LogWindow __instance, string contentLog)
        {
            if (!string.IsNullOrEmpty(contentLog))
            {
                try
                {
                    // Use reflection to access the private field "pathFile"
                    FieldInfo pathFileField = typeof(LogWindow).GetField("pathFile", BindingFlags.NonPublic | BindingFlags.Instance);
                    string logFilePath = pathFileField?.GetValue(__instance) as string;

                    if (string.IsNullOrEmpty(logFilePath))
                    {
                        logFilePath = "UnknownLog.json";
                    }
                    else
                    {
                        // Extract only the filename (e.g., "/var/system.log" -> "system.log")
                        logFilePath = Path.GetFileName(logFilePath);
                    }

                    // Define the save path inside the BepInEx folder
                    string savePath = Path.Combine("BepInEx", logFilePath);
                    File.WriteAllText(savePath, contentLog);

                    UnityEngine.Debug.Log($"[LogWindowPatch] Saved log content to {savePath}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LogWindowPatch] Failed to save log: {ex}");
                }
            }
        }
    }
}
