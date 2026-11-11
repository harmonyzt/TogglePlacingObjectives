using EFT;
using EFT.InputSystem;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace TogglePlacingObjectives.Patches;

[HarmonyPatch(typeof(GamePlayerOwner), nameof(GamePlayerOwner.TranslateCommand))]
internal static class GamePlayerOwnerTranslatePatch
{
    [HarmonyPrefix]
    private static bool Prefix(GamePlayerOwner __instance, ECommand command, ref InputNode.ETranslateResult __result)
    {
        if (!TogglePlant.IsActiveFor(__instance.Player))
        {
            return true;
        }

        if (command == ECommand.BeginInteracting)
        {
            TogglePlant.CancelActive();
            __result = InputNode.ETranslateResult.BlockAll;
            return false;
        }

        if (command == ECommand.EndInteracting)
        {
            TogglePlant.BeginReleaseCancelWindow();
        }

        return true;
    }

    [HarmonyPostfix]
    private static void Postfix(ECommand command)
    {
        if (command == ECommand.EndInteracting)
        {
            TogglePlant.EndReleaseCancelWindow();
        }
    }
}

[HarmonyPatch(typeof(ActionPanel), nameof(ActionPanel.TranslateCommand))]
internal static class ActionPanelTranslatePatch
{
    [HarmonyPrefix]
    private static bool Prefix(ECommand command, ref InputNode.ETranslateResult __result)
    {
        if (command != ECommand.BeginInteracting || !TogglePlant.IsArmed)
        {
            return true;
        }

        TogglePlant.CancelActive();
        __result = InputNode.ETranslateResult.BlockAll;
        return false;
    }
}

[HarmonyPatch(typeof(MoveInputTranslator), nameof(MoveInputTranslator.TranslateCommand))]
internal static class MoveInputTranslatorCommandPatch
{
    [HarmonyPrefix]
    private static bool Prefix(MoveInputTranslator __instance, ECommand command, ref InputNode.ETranslateResult __result)
    {
        if (!TogglePlant.IsActiveFor(__instance._controller))
        {
            return true;
        }

        if (command == ECommand.BeginInteracting)
        {
            TogglePlant.CancelActive();
            __result = InputNode.ETranslateResult.BlockAll;
            return false;
        }

        if (command == ECommand.EndInteracting)
        {
            // Releasing the key must not abort the planting!!!!!!
            __result = InputNode.ETranslateResult.BlockAll;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(MoveInputTranslator), nameof(MoveInputTranslator.TranslateAxes))]
internal static class MoveInputTranslatorAxesPatch
{
    private const float MovementThreshold = 0.25f;

    [HarmonyPrefix]
    private static void Prefix(MoveInputTranslator __instance, ref float[] axes)
    {
        if (!Plugin.CancelOnMovement.Value)
        {
            return;
        }

        if (axes == null || axes.Length < 2 || !TogglePlant.IsActiveFor(__instance._controller))
        {
            return;
        }

        if (Mathf.Abs(axes[0]) <= MovementThreshold && Mathf.Abs(axes[1]) <= MovementThreshold)
        {
            return;
        }

        TogglePlant.CancelActive();
    }
}
