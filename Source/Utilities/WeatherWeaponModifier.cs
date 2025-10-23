namespace AdaptiveArsenal.Utilities;

/// <summary>
/// Provides weather-based modifiers for weapon statistics
/// </summary>
internal static class WeatherWeaponModifier
{
    private const float BadWeatherAccuracyPenalty = 0.90f; // 10% accuracy reduction in bad weather
    private const float WindAccuracyPenalty = 0.95f; // 5% accuracy reduction in high wind
    
    private const float ColdDamagePenalty = 0.95f; // 5% damage reduction in extreme cold
    private const float WetDamagePenalty = 0.90f; // 10% damage reduction when weapon is wet
    
    private const float ColdDeteriorationIncrease = 1.15f; // 15% faster deterioration in cold
    private const float WetDeteriorationIncrease = 1.25f; // 25% faster deterioration when wet
    
    /// <summary>
    /// Calculate accuracy modifier based on weather conditions
    /// </summary>
    public static float GetAccuracyModifier()
    {
        var weather = GameManager.GetWeatherComponent();
        if (weather == null) return 1f;
        
        var modifier = 1f;
        
        // Check for precipitation or poor visibility
        var temperature = weather.GetCurrentTemperature();
        
        // Snow/freezing conditions affect accuracy
        if (temperature < 0f)
        {
            modifier *= BadWeatherAccuracyPenalty;
        }
        
        // Check for wind - using temperature as a proxy since exact wind methods might vary
        // This is a simplified approach
        if (temperature < -15f)
        {
            modifier *= WindAccuracyPenalty;
        }
        
        AdaptiveArsenal.Utilities.Logging.LogDebug("Accuracy Modifier: {0}", modifier);
        return modifier;
    }
    
    /// <summary>
    /// Calculate damage (lethality) modifier based on weather and weapon condition
    /// </summary>
    public static float GetDamageModifier(Components.WeaponCondition? weaponCondition)
    {
        var weather = GameManager.GetWeatherComponent();
        if (weather == null) return 1f;
        
        var modifier = 1f;
        
        // Extreme cold can affect gunpowder efficiency
        var temperature = weather.GetCurrentTemperature();
        if (temperature < -20f)
        {
            modifier *= ColdDamagePenalty;
        }
        
        // Wet weapons have reduced effectiveness
        if (weaponCondition != null && weaponCondition.CurrentState != Components.WeaponState.Dry)
        {
            modifier *= WetDamagePenalty;
        }
        
        AdaptiveArsenal.Utilities.Logging.LogDebug("Damage Modifier: {0}", modifier);
        return modifier;
    }
    
    /// <summary>
    /// Calculate deterioration rate modifier based on weather and weapon condition
    /// </summary>
    public static float GetDeteriorationModifier(Components.WeaponCondition? weaponCondition)
    {
        var weather = GameManager.GetWeatherComponent();
        if (weather == null) return 1f;
        
        var modifier = 1f;
        
        // Cold weather increases wear
        var temperature = weather.GetCurrentTemperature();
        if (temperature < -10f)
        {
            modifier *= ColdDeteriorationIncrease;
        }
        
        // Wet weapons deteriorate faster
        if (weaponCondition != null && weaponCondition.CurrentState != Components.WeaponState.Dry)
        {
            modifier *= WetDeteriorationIncrease;
        }
        
        AdaptiveArsenal.Utilities.Logging.LogDebug("Deterioration Modifier: {0}", modifier);
        return modifier;
    }
}
