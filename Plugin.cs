using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace TogglePlacingObjectives;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.SPT.core", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    internal const string PluginGuid = "com.local.toggleplacingobjectives";
    internal const string PluginName = "Toggle Placing Objectives";
    internal const string PluginVersion = "1.0.0";

    internal static ManualLogSource Log { get; private set; } = null!;
    internal static ConfigEntry<bool> ToggleEnabled { get; private set; } = null!;
    internal static ConfigEntry<bool> CancelOnMovement { get; private set; } = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Log = Logger;

        ToggleEnabled = Config.Bind(
            "General",
            "Enable toggle planting",
            true,
            "Press the interact key once to start hiding/planting a quest item. The timer keeps running after the key is released. Press interact again to cancel.");

        CancelOnMovement = Config.Bind(
            "General",
            "Cancel when moving",
            true,
            "Abort an active plant as soon as movement input is detected. Vanilla blocks movement during the plant; with this enabled, moving first cancels the plant and then moves normally.");

        _harmony = new Harmony(PluginGuid);

        try
        {
            _harmony.PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo($"{PluginName} {PluginVersion} loaded (toggle={ToggleEnabled.Value}, cancelOnMovement={CancelOnMovement.Value}).");
        }
        catch (Exception ex)
        {
            _harmony.UnpatchSelf();
            Log.LogError($"{PluginName} could not patch this game build. Expected SPT 4.1.6 / EFT 0.16.9.40743.\n{ex}");
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
    }
}
