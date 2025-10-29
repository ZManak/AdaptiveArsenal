using JetBrains.Annotations;
using ModSettings;
using AdaptiveArsenal.Utilities;

namespace AdaptiveArsenal.Settings
{
    internal class AASettings : ModSettingsBase
    {
        [Section("Adaptive Arsenal Settings")]
        
        [Name("Enable Dev Mode")] 
        [Description("Toggle Dev Mode on or off.")]
        [Choice("Yes", "No")]
        public int DevMode = 0;

        [Name("Swap Weapon Skins")]
        [Description("Keymap weapon skin swapping.")]
        public UnityEngine.KeyCode SwapKey = UnityEngine.KeyCode.T;

        [Name("Projectile Trail Persistence")]
        [Description("How long projectile trails persist in seconds.")]
        [Slider(0f, 30f, 1)]
        public float TrailPersistence = 1f;

        [Name("Ammo Sprite Visibility")]
        [Description("Transparency of the ammo sprites in HUD")]
        [ModSettings.Slider(0f, 1.0f, 1)]
        public float AmmoSpriteAlpha = 1.0f;

        protected override void OnConfirm()
        {
            Logging.Log("Arsenal Settings registered");
        }
}
    internal class ArsenalSettings
    {
        internal static readonly AASettings Options = new AASettings();
        public static void OnLoad()
        {
            Options.AddToModSettings("Adaptive Arsenal");
        }
    }
}
