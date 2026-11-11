using System;
using EFT;
using EFT.Interactive;
using EFT.Quests;

namespace TogglePlacingObjectives;

internal static class TogglePlant
{
    private static PlantPlayerState? _activeState;
    private static bool _suppressReleaseCancel;
    private static bool _forceCancel;

    internal static bool IsArmed => _activeState != null && IsLocalPlantState(_activeState);

    internal static bool IsActive(PlantPlayerState state)
        => state != null && ReferenceEquals(_activeState, state) && IsLocalPlantState(state);

    internal static bool IsActiveFor(Player player)
        => player != null
           && _activeState != null
           && ReferenceEquals(player.MovementContext, _activeState.MovementContext)
           && player.CurrentState is PlantPlayerState;

    internal static void Arm(IdlePlayerState idleState, bool multitool, float plantTime)
    {
        if (!Plugin.ToggleEnabled.Value)
        {
            return;
        }

        Player player = GamePlayerOwner.MyPlayer;
        if (player == null || !ReferenceEquals(player.MovementContext, idleState.MovementContext))
        {
            return;
        }

        if (player.CurrentState is not PlantPlayerState plantState)
        {
            return;
        }

        PlaceItemTrigger? zone = player.PlaceItemZone;
        if (zone == null)
        {
            return;
        }

        ConditionLeaveItemAtLocation? condition = FindLeaveItemCondition(player, zone.Id);
        if (condition == null)
        {
            // Not a hide/plant objective:
            // leave the original hold-to-plant
            return;
        }

        _activeState = plantState;
        _suppressReleaseCancel = false;
        _forceCancel = false;

        Plugin.Log.LogDebug(
            $"Armed toggle plant: zone={zone.Id}, time={condition.plantTime:0.##}s, multitool={multitool}.");
    }

    internal static void Disarm(PlantPlayerState state)
    {
        if (!ReferenceEquals(_activeState, state))
        {
            return;
        }

        _activeState = null;
        _suppressReleaseCancel = false;
        _forceCancel = false;
    }

    // True while the key release cancel must be ignored
    internal static bool ShouldSuppressCancel(PlantPlayerState state)
        => IsActive(state) && _suppressReleaseCancel && !_forceCancel;

    internal static void BeginReleaseCancelWindow() => _suppressReleaseCancel = true;

    internal static void EndReleaseCancelWindow() => _suppressReleaseCancel = false;

    internal static void CancelActive()
    {
        PlantPlayerState? state = _activeState;
        if (state == null || !IsActive(state))
        {
            return;
        }

        _forceCancel = true;
        try
        {
            state.Cancel();
        }
        finally
        {
            _forceCancel = false;
            _suppressReleaseCancel = false;
        }
    }

    private static ConditionLeaveItemAtLocation? FindLeaveItemCondition(Player player, string zoneId)
    {
        QuestBook? quests = player.QuestController != null ? player.QuestController.Quests : null;
        if (quests == null)
        {
            return null;
        }

        foreach (ConditionProgressChecker handler in quests.GetConditionHandlersByZone<ConditionZone>(zoneId))
        {
            if (handler.CurrentValue < handler.Condition.value
                && handler.Condition is ConditionLeaveItemAtLocation leaveItem)
            {
                return leaveItem;
            }
        }

        return null;
    }

    private static bool IsLocalPlantState(PlantPlayerState state)
    {
        Player player = GamePlayerOwner.MyPlayer;
        return player != null
               && ReferenceEquals(player.MovementContext, state.MovementContext)
               && player.CurrentState is PlantPlayerState;
    }
}
