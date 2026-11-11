using EFT;
using HarmonyLib;

namespace TogglePlacingObjectives.Patches;

[HarmonyPatch(typeof(IdlePlayerState), nameof(IdlePlayerState.Plant))]
internal static class IdlePlayerStatePlantPatch
{
    [HarmonyPostfix]
    private static void Postfix(IdlePlayerState __instance, bool multitool, float plantTime)
    {
        TogglePlant.Arm(__instance, multitool, plantTime);
    }
}

[HarmonyPatch(typeof(PlantPlayerState))]
internal static class PlantPlayerStatePatches
{
    [HarmonyPatch(nameof(PlantPlayerState.Cancel))]
    [HarmonyPrefix]
    private static bool CancelPrefix(PlantPlayerState __instance)
    {
        return !TogglePlant.ShouldSuppressCancel(__instance);
    }

    [HarmonyPatch(nameof(PlantPlayerState.Exit))]
    [HarmonyPostfix]
    private static void ExitPostfix(PlantPlayerState __instance)
    {
        TogglePlant.Disarm(__instance);
    }

    [HarmonyPatch(nameof(PlantPlayerState.Plant))]
    [HarmonyPrefix]
    private static bool PlantPrefix(PlantPlayerState __instance)
    {
        if (!TogglePlant.IsActive(__instance))
        {
            return true;
        }

        TogglePlant.CancelActive();
        return false;
    }
}
