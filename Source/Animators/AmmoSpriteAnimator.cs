using AdaptiveArsenal.Utilities;
using System.Collections;
using System.Collections.Generic;
using AdaptiveArsenal.Settings;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AdaptiveArsenal.Animators;

[RegisterTypeInIl2Cpp(false)]
public class AmmoSpriteAnimator : MonoBehaviour
{
    // Find all ammo sprites in EquipItemPopup and use them for casing ejection animation
    private GameObject ammoPrefab;
    private readonly List<GameObject> activeAmmoSprites = new List<GameObject>();

    [HideFromIl2Cpp]
    internal IEnumerator CasingEjectionAnimation(Vector3 startPosition, Transform parentTransform, GunType gunType)
    {
        if (ammoPrefab == null)
        {
            Logging.LogWarning("CasingEjectionAnimation - ammoPrefab is null, aborting.");
            yield break;
        }

        var casingObject = Instantiate(ammoPrefab, parentTransform);
        if (casingObject == null)
        {
            Logging.LogWarning("CasingEjectionAnimation - Instantiate returned null");
            yield break;
        }

        activeAmmoSprites.Add(casingObject);
        casingObject.transform.localPosition = startPosition;
        casingObject.SetActive(true);

        var sprite = casingObject.GetComponent<UISprite>();
        if (sprite != null)
        {
            sprite.depth = 30;
            sprite.spriteName = gunType switch
            {
                GunType.Rifle => "ico_ammo_rifle",
                GunType.Revolver => "ico_ammo_revolver",
                _ => ""
            };
            sprite.color = Color.yellow; // fallback / visual color
            sprite.alpha = Settings.ArsenalSettings.Options.AmmoSpriteAlpha;
        }

        var end = new Vector3(startPosition.x + 100f, startPosition.y - 150f, startPosition.z);
        var controlPoint = new Vector3(startPosition.x + 50f, startPosition.y + 100f, startPosition.z);

        var duration = 1f;
        var elapsedTime = 0f;
        var rotationSpeed = 360f;

        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;

            casingObject.transform.localPosition = MathUtils.QuadraticBezier(startPosition, controlPoint, end, t);
            casingObject.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        activeAmmoSprites.Remove(casingObject);
        if (casingObject != null) Destroy(casingObject);
    }

    private void Start()
    {
        var popup = GetComponent<EquipItemPopup>();
        if (popup == null)
        {
            Logging.LogWarning("AmmoSpriteAnimator.Start - EquipItemPopup component not found; creating fallback prefab.");
            CreateFallbackAmmoPrefab();
            return;
        }

        var ammoSprites = popup.m_ListAmmoSprites;
        if (ammoSprites == null || ammoSprites.Length == 0)
        {
            Logging.LogWarning("AmmoSpriteAnimator.Start - m_ListAmmoSprites is null or empty; creating fallback prefab.");
            CreateFallbackAmmoPrefab();
            return;
        }

        // apply alpha if available
        foreach (var s in ammoSprites)
        {
            if (s == null) continue;
            s.alpha = Settings.ArsenalSettings.Options.AmmoSpriteAlpha;
        }

        // prefer index 1 when present, otherwise index 0
        var index = ammoSprites.Length > 1 ? 1 : 0;
        if (ammoSprites[index] != null)
        {
            ammoPrefab = ammoSprites[index].gameObject;
        }
        else
        {
            CreateFallbackAmmoPrefab();
        }
    }

    private void CreateFallbackAmmoPrefab()
    {
        var go = new GameObject("AA_AmmoSpritePrefab");
        var sprite = go.AddComponent<UISprite>();
        sprite.spriteName = ""; // leave empty so user can supply atlas if desired
        sprite.color = Color.yellow;
        sprite.alpha = Settings.ArsenalSettings.Options.AmmoSpriteAlpha;
        sprite.depth = 30;
        go.SetActive(false);
        ammoPrefab = go;
        Logging.LogDebug("AmmoSpriteAnimator - created fallback ammo prefab with yellow color");
    }

    private void OnDisable()
    {
        foreach (var sprite in activeAmmoSprites)
        {
            if (sprite != null) Destroy(sprite);
        }
        activeAmmoSprites.Clear();
    }
}