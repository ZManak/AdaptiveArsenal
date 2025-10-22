using AdaptiveArsenal.Components;

namespace AdaptiveArsenal.Patches;

/// <summary>
/// Patches for player-related events that affect weapon condition
/// </summary>
internal static class PlayerManagerPatches
{
    private static float lastWetnessCheckTime = 0f;
    private const float WetnessCheckInterval = 1f; // Check every second
    
    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update))]
    private static class DetectWaterState
    {
        private static void Postfix(PlayerManager __instance)
        {
            // Periodically check if player is in freezing water
            if (Time.time - lastWetnessCheckTime < WetnessCheckInterval) return;
            lastWetnessCheckTime = Time.time;
            
            // Check if player is experiencing freezing effects (likely in water)
            var freezingComponent = GameManager.GetFreezingComponent();
            if (freezingComponent != null && freezingComponent.IsFreezing())
            {
                var temperature = GameManager.GetWeatherComponent().GetCurrentTemperature();
                // If it's freezing and temperature is reasonable, likely in water
                if (temperature > -20f)
                {
                    WetInventoryWeapons();
                }
            }
        }
    }
    
    private static void WetInventoryWeapons()
    {
        // Wet the weapon currently in hands
        var itemInHands = GameManager.GetPlayerManagerComponent().m_ItemInHands;
        if (itemInHands != null && itemInHands.m_GunItem != null)
        {
            var weaponCondition = itemInHands.gameObject.GetComponent<WeaponCondition>();
            if (weaponCondition != null)
            {
                weaponCondition.MakeWet();
            }
        }
    }
}

/// <summary>
/// Patches for GunItem to integrate weapon condition system
/// </summary>
internal static class GunItemPatches
{
    [HarmonyPatch(typeof(GunItem), nameof(GunItem.Awake))]
    private static class AttachWeaponConditionComponent
    {
        private static void Postfix(GunItem __instance)
        {
            // Attach WeaponCondition component to all gun items
            if (__instance.m_GunType != GunType.FlareGun)
            {
                _ = __instance.GetComponent<WeaponCondition>() ?? __instance.gameObject.AddComponent<WeaponCondition>();
            }
        }
    }
}
