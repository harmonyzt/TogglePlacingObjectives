using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace TogglePlacingObjectives;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
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
            "Press the interact key once to start hiding/planting a quest item. Press interact again to cancel.");

        CancelOnMovement = Config.Bind(
            "General",
            "Cancel when moving",
            true,
            "Abort planting as soon as you move");

        _harmony = new Harmony(PluginGuid);

        try
        {
            _harmony.PatchAll(typeof(Plugin).Assembly);
        }
        catch (Exception ex)
        {
            _harmony.UnpatchSelf();
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
    }
}
