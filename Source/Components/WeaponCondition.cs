namespace AdaptiveArsenal.Components;

/// <summary>
/// Represents the moisture and temperature state of a weapon
/// </summary>
public enum WeaponState
{
    Dry,
    Wet,
    Freezing,
    Frozen
}

/// <summary>
/// Component that tracks and manages weapon condition including wetness and freezing states
/// </summary>
[RegisterTypeInIl2Cpp(false)]
public class WeaponCondition : MonoBehaviour
{
    private const float WetnessDryingRate = 0.01f; // Wetness reduced per second when conditions allow
    private const float FreezingRate = 0.02f; // Rate at which wet weapons freeze in cold
    private const float ThawingRate = 0.015f; // Rate at which frozen weapons thaw in warmth
    private const float FreezingTemperature = -5f; // Temperature below which wet weapons start to freeze
    private const float DryingTemperature = 10f; // Temperature above which weapons dry faster
    
    private float m_WetnessLevel; // 0.0 = dry, 1.0 = fully wet
    private float m_FreezingLevel; // 0.0 = not frozen, 1.0 = fully frozen
    private WeaponState m_CurrentState = WeaponState.Dry;
    private GunItem? m_GunItem;
    
    public WeaponState CurrentState => m_CurrentState;
    public float WetnessLevel => m_WetnessLevel;
    public float FreezingLevel => m_FreezingLevel;
    public bool IsFrozen => m_CurrentState == WeaponState.Frozen;
    
    private void Awake()
    {
        m_GunItem = GetComponent<GunItem>();
        m_WetnessLevel = 0f;
        m_FreezingLevel = 0f;
    }
    
    private void Update()
    {
        if (m_GunItem == null) return;
        
        UpdateWeaponCondition();
        UpdateWeaponState();
    }
    
    /// <summary>
    /// Makes the weapon wet (called when player falls in water)
    /// </summary>
    public void MakeWet()
    {
        m_WetnessLevel = Mathf.Min(1f, m_WetnessLevel + 0.5f);
    }
    
    /// <summary>
    /// Gets the jamming probability modifier based on weapon condition
    /// </summary>
    public float GetJammingProbabilityModifier()
    {
        // Wet weapons have increased jamming chance
        // Freezing weapons have even higher jamming chance
        // Frozen weapons cannot be used at all
        return m_CurrentState switch
        {
            WeaponState.Wet => 1.5f + (m_WetnessLevel * 0.5f), // 1.5x to 2x jamming chance
            WeaponState.Freezing => 2f + (m_FreezingLevel * 1.5f), // 2x to 3.5x jamming chance
            WeaponState.Frozen => float.PositiveInfinity, // Cannot use
            _ => 1f // Normal jamming chance
        };
    }
    
    private void UpdateWeaponCondition()
    {
        var currentTemp = GameManager.GetWeatherComponent().GetCurrentTemperature();
        // Simplified indoor check - assume indoors if temp is moderate
        var isIndoors = currentTemp > 0 && currentTemp < 25;
        
        // Drying logic
        if (m_WetnessLevel > 0)
        {
            if (isIndoors || currentTemp > DryingTemperature)
            {
                // Dry faster when indoors or in warm weather
                m_WetnessLevel = Mathf.Max(0f, m_WetnessLevel - (WetnessDryingRate * 2f * Time.deltaTime));
            }
            else if (currentTemp > 0)
            {
                // Dry slowly in moderate conditions
                m_WetnessLevel = Mathf.Max(0f, m_WetnessLevel - (WetnessDryingRate * Time.deltaTime));
            }
        }
        
        // Freezing logic for wet weapons
        if (m_WetnessLevel > 0.1f && !isIndoors && currentTemp < FreezingTemperature)
        {
            // Weapon starts to freeze when wet and in cold conditions
            m_FreezingLevel = Mathf.Min(1f, m_FreezingLevel + (FreezingRate * Time.deltaTime));
        }
        else if (m_FreezingLevel > 0 && (isIndoors || currentTemp > 0))
        {
            // Weapon thaws when brought to warmth
            m_FreezingLevel = Mathf.Max(0f, m_FreezingLevel - (ThawingRate * Time.deltaTime));
        }
    }
    
    private void UpdateWeaponState()
    {
        if (m_FreezingLevel >= 0.9f)
        {
            m_CurrentState = WeaponState.Frozen;
        }
        else if (m_FreezingLevel > 0.1f)
        {
            m_CurrentState = WeaponState.Freezing;
        }
        else if (m_WetnessLevel > 0.1f)
        {
            m_CurrentState = WeaponState.Wet;
        }
        else
        {
            m_CurrentState = WeaponState.Dry;
        }
    }
}
