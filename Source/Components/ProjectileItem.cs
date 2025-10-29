using AdaptiveArsenal.Utilities;
using AdaptiveArsenal.Settings;
using Il2CppTLD.Stats;


namespace AdaptiveArsenal.Components;

[RegisterTypeInIl2Cpp(false)]
public class ProjectileItem : MonoBehaviour
{
    private AmmoItem m_AmmoItem;
    private GunType m_GunType;
    private LineRenderer m_LineRenderer;
    private Rigidbody m_Rigidbody;
    
    private const float Damage =100f;
    private const float MaxRange =500f;
    private const float MinDamage =10f;
    private const float ScaleMultiplier =0.5f;
    private static readonly int[] RevolverEffectiveRange = new int[] { 40, 50, 60, 80, 100 };
    
    private bool LineRendererStartFadeOut;
    private float LineRendererFadeDuration = ArsenalSettings.Options.TrailPersistence;
    private const float LineRendererMaxLength =200f;
    private const float TrajectoryUpdateInterval =0.05f;
    private float LastTrajectoryUpdateTime;
    private float LineRendererFadeTimer;
    private const int InitialTrajectoryCapacity =100;
    private int CurrentTrajectoryIndex;
    private Vector3 InitialPosition;
    private List<Vector3> TrajectoryPoints;
    
    private static readonly Dictionary<string, int> GunMuzzleVelocities = new()
 {
 {"GEAR_Rifle_Barbs",800},
 {"GEAR_Rifle_Curators",1000},
 {"GEAR_Rifle_Traders",1100},
 {"GEAR_Rifle_Vaughns",700},
 {"GEAR_Rifle",800},
 {"GEAR_RevolverFancy",600},
 {"GEAR_RevolverGreen",400},
 {"GEAR_RevolverStubNosed",300},
 {"GEAR_Revolver",400}
 };
 
 private void Awake()
 {
 Logging.LogDebug("ProjectileItem.Awake() start - GameObject: {0}", name);

 m_Rigidbody = GetComponent<Rigidbody>();
 m_AmmoItem = GetComponent<AmmoItem>();

 TrajectoryPoints = new List<Vector3>(InitialTrajectoryCapacity);
 
 // If essential components are missing, disable this behaviour to avoid null refs
 if (m_Rigidbody == null || m_AmmoItem == null)
 {
 Logging.LogError("ProjectileItem.Awake - missing component: Rigidbody={0}, AmmoItem={1}", m_Rigidbody == null, m_AmmoItem == null);
 enabled = false;
 return;
 }

 ConfigureComponents();
 enabled = false;

 Logging.LogDebug("ProjectileItem.Awake() complete - configured LineRenderer={0}", m_LineRenderer != null);
 }

 public static float CalculateAccuracy(GunItem gunItem, bool isHipFire, bool isStanding, bool isMoving)
 {
 var baseAccuracy = gunItem.m_GunType switch
 {
 GunType.Rifle => GameManager.GetSkillRifle()?.GetEffectiveRange() ?? gunItem.m_AccuracyRange,
 GunType.Revolver => GetEffectiveRevolverRange() + gunItem.m_AccuracyRange,
 _ => gunItem.m_AccuracyRange
 };

 var accuracyMultiplier =1.2f;
 
 if (isHipFire) accuracyMultiplier *=0.7f;
 if (isStanding) accuracyMultiplier *=0.8f;
 if (isMoving) accuracyMultiplier *=0.9f;
 
 return baseAccuracy * accuracyMultiplier;
 }
 
 private static float CalculateDamageByDistance(float distance) => Mathf.Lerp(Damage, MinDamage, Mathf.Clamp01(distance / MaxRange));

 private void ConfigureComponents()
 {
 Logging.LogDebug("ConfigureComponents start - GameObject: {0}", name);

 if (m_Rigidbody == null)
 {
 Logging.LogWarning("ConfigureComponents - Rigidbody is null, aborting configuration.");
 return;
 }

 // safe assignment for gun type
 m_GunType = m_AmmoItem != null ? m_AmmoItem.m_AmmoForGunType : default;

 var fxArrowTrailMaterial = MaterialSwapper.GetLineRendererMaterialFromGearItemPrefab("GEAR_Arrow", "LineRenderer");
 if (fxArrowTrailMaterial == null)
 {
 Logging.LogWarning("ConfigureComponents - fxArrowTrailMaterial not found for GEAR_Arrow");
 return;
 }

 GameObject lineRendererObject = new("LineRenderer") { transform = { parent = transform } };

 m_LineRenderer = lineRendererObject.AddComponent<LineRenderer>();
 if (m_LineRenderer == null)
 {
 Logging.LogWarning("ConfigureComponents - AddComponent<LineRenderer>() returned null");
 return;
 }

 m_LineRenderer.material = fxArrowTrailMaterial;
 m_LineRenderer.startWidth =0.4f;
 m_LineRenderer.endWidth =0.1f;

 Gradient gradient = new();
 gradient.SetKeys(
 Settings.ArsenalSettings.Options.DevMode ==1
 ? new GradientColorKey[] { new(Color.green,0.0f), new(Color.red,1.0f) }
 : new GradientColorKey[] { new(Color.yellow,0.0f), new(Color.white,1.0f) },
 new GradientAlphaKey[] { new(0.3f,0.0f), new(1.0f,1.0f) });
 m_LineRenderer.colorGradient = gradient;
 m_LineRenderer.useWorldSpace = true;
 m_LineRenderer.positionCount =0;

 Logging.LogDebug("ConfigureComponents complete - LineRenderer created on {0}", lineRendererObject.name);
 }

 private void Fire()
 {
 Logging.LogDebug("Fire() called on ProjectileItem - GameObject: {0}", name);

 if (m_Rigidbody == null)
 {
 Logging.LogError("Fire - Rigidbody is null, aborting fire.");
 enabled = false;
 return;
 }

 enabled = true;

 StatsManager.IncrementValue(m_GunType == GunType.Rifle ? StatID.RifleShot : StatID.RevolverShot);

 Utils.SetIsKinematic(m_Rigidbody, false);
 transform.parent = null;

 m_Rigidbody.velocity = Vector3.zero;
 m_Rigidbody.mass =0.02f;
 m_Rigidbody.drag =0.1f;
 m_Rigidbody.angularDrag =0.1f;

 if (m_LineRenderer == null)
 {
 Logging.LogWarning("Fire - LineRenderer missing, attempting to reconfigure.");
 // Try to reconfigure if missing
 ConfigureComponents();
 if (m_LineRenderer == null)
 {
 Logging.LogWarning("Fire - LineRenderer still missing after reconfigure. Visual trail disabled for this projectile.");
 }
 }

 if (Settings.ArsenalSettings.Options.DevMode ==0)
 {
 if (m_LineRenderer != null)
 {
 m_LineRenderer.startColor = new Color(0f,1f,0f,1f); // Match gradient: opaque red at start
 m_LineRenderer.endColor = new Color(1f,0f,0f,1f);
 }
 }
 else
 {
 if (m_LineRenderer != null)
 {
 m_LineRenderer.startColor = new Color(1f,1f,1f,0.9f);
 m_LineRenderer.endColor = new Color(1f,1f,0f,0.1f);
 }
 }

 var playerManager = GameManager.GetPlayerManagerComponent();
 string itemName = playerManager != null && playerManager.m_ItemInHands != null ? playerManager.m_ItemInHands.name : string.Empty;
 var muzzleVelocity = transform.forward * (GetMuzzleVelocity(itemName) * ScaleMultiplier);

 Logging.LogDebug("Fire - muzzle velocity: {0}, itemInHands: {1}", muzzleVelocity, string.IsNullOrEmpty(itemName) ? "(none)" : itemName);

 m_Rigidbody.AddForce(muzzleVelocity, ForceMode.VelocityChange);

 InitialPosition = transform.position;
 if (TrajectoryPoints == null) TrajectoryPoints = new List<Vector3>(InitialTrajectoryCapacity);
 TrajectoryPoints.Add(InitialPosition);

 if (m_LineRenderer != null)
 {
 m_LineRenderer.positionCount =1;
 m_LineRenderer.SetPosition(0, InitialPosition);
 }
 
 var cougar = GameManager.GetCougarManager();
 cougar?.MaybeAimNearMiss(muzzleVelocity.normalized);

 Logging.LogDebug("Fire complete - Projectile launched from {0}", InitialPosition);
 }

 private static float GetEffectiveRevolverRange() => RevolverEffectiveRange.Length >0 ? RevolverEffectiveRange[Mathf.Clamp(GameManager.GetSkillsManager().GetSkill(SkillType.Revolver).GetCurrentTierNumber(),0, RevolverEffectiveRange.Length -1)] :0f;

 private static int GetMuzzleVelocity(string gearItemName)
 {
 if (string.IsNullOrEmpty(gearItemName))
 {
 Logging.LogDebug("GetMuzzleVelocity - gearItemName empty, returning default0");
 return 0;
 }

 foreach (var kv in GunMuzzleVelocities)
 {
 if (gearItemName.Contains(kv.Key))
 {
 Logging.LogDebug("GetMuzzleVelocity - matched {0} => {1}", kv.Key, kv.Value);
 return kv.Value;
 }
 }

 Logging.LogDebug("GetMuzzleVelocity - no match for '{0}', returning default0", gearItemName);
 return 0;
 }

 private void TryInflictDamage(GameObject victim, string collider)
 {
 if (victim == null)
 {
 Logging.LogWarning("TryInflictDamage - victim is null");
 return;
 }

 var baseAi = victim.layer switch
 {
16 => victim.GetComponent<BaseAi>(),
27 => victim.transform.GetComponentInParent<BaseAi>(),
 _ => null
 };
 
 if (baseAi == null)
 {
 Logging.LogDebug("TryInflictDamage - no BaseAi found for victim {0}", victim.name);
 return;
 }

 var localizedDamage = victim.GetComponent<LocalizedDamage>();
 if (localizedDamage == null)
 {
 Logging.LogWarning("TryInflictDamage - LocalizedDamage missing on victim {0}", victim.name);
 return;
 }

 var weaponSource = m_GunType.ToWeaponSource();
 baseAi.MaybeFleeOrAttackFromProjectileHit(weaponSource);
 var bleedOutMinutes = localizedDamage.GetBleedOutMinutes(weaponSource);
 var damageScaleFactor = localizedDamage.GetDamageScale(weaponSource);

 var distance = Vector3.Distance(InitialPosition, transform.position);
 var distanceBasedDamage = CalculateDamageByDistance(distance);

 var damage = distanceBasedDamage * damageScaleFactor;

 if (!baseAi.m_IgnoreCriticalHits && localizedDamage.RollChanceToKill(WeaponSource.Rifle)) damage = float.PositiveInfinity;

 Logging.LogDebug("TryInflictDamage - applying damage {0} to {1} (distance {2}, scale {3})", damage, victim.name, distance, damageScaleFactor);

 if (baseAi.GetAiMode() != AiMode.Dead)
 {
 var statId = m_GunType == GunType.Rifle ? StatID.SuccessfulHits_Rifle : StatID.SuccessfulHits_Revolver;
 var skillType = m_GunType == GunType.Rifle ? SkillType.Rifle : SkillType.Revolver;
 StatsManager.IncrementValue(statId);
 GameManager.GetSkillsManager()?.IncrementPointsAndNotify(skillType,1, SkillsManager.PointAssignmentMode.AssignOnlyInSandbox);
 }

 baseAi.SetupDamageForAnim(transform.position, GameManager.GetPlayerTransform()?.position ?? transform.position, localizedDamage);
 baseAi.ApplyDamage(damage, bleedOutMinutes, DamageSource.Player, collider);
 }

 private void OnCollisionEnter(Collision collision)
 {
 if (collision == null || collision.gameObject == null)
 {
 Logging.LogWarning("OnCollisionEnter - collision or collision.gameObject is null");
 return;
 }

 TryInflictDamage(collision.gameObject, collision.contacts.Length >0 ? collision.contacts[0].thisCollider.name : collision.gameObject.name);

 Logging.LogDebug("OnCollisionEnter - hit object {0} (layer {1}) at position {2}", collision.gameObject.name, collision.gameObject.layer, collision.contacts.Length >0 ? collision.contacts[0].point.ToString() : "(no contact point)");

 SpawnImpactEffects(collision, transform);

 if (m_Rigidbody != null)
 {
 m_Rigidbody.velocity = Vector3.zero;
 m_Rigidbody.isKinematic = true;
 }

 // Start fading the line renderer over the configured duration
 LineRendererStartFadeOut = true;
 LineRendererFadeTimer =0f;

 // Attach a fader component to the line renderer GameObject so the trail persists and fades
 if (m_LineRenderer != null && m_LineRenderer.gameObject != null)
 {
 // Detach so it persists after this projectile GameObject is destroyed
 var trailObject = m_LineRenderer.gameObject;
 trailObject.transform.parent = null;

 // Register with DetachedTrailManager to handle fading without requiring a managed component on the detached object
 DetachedTrailManager.RegisterTrail(trailObject, LineRendererFadeDuration);
 Logging.LogDebug("OnCollisionEnter - registered detached trail with DetachedTrailManager for duration {0}", LineRendererFadeDuration);
 }

 Destroy(gameObject);
 }

 internal static void SpawnAndFire(GameObject prefab, Vector3 startPos, Quaternion startRot)
 {
 if (prefab == null)
 {
 Logging.LogWarning("SpawnAndFire called with null prefab");
 return;
 }

 var gameObject = Instantiate(prefab, startPos, startRot);
 if (gameObject == null)
 {
 Logging.LogError("SpawnAndFire - Instantiate returned null for prefab {0}", prefab.name);
 return;
 }

 Logging.LogDebug("SpawnAndFire - instantiated prefab {0} at {1}", prefab.name, startPos);

 gameObject.name = prefab.name;
 gameObject.transform.parent = null;
 var proj = gameObject.GetComponent<ProjectileItem>();
 if (proj == null)
 {
 Logging.LogWarning("SpawnAndFire - instantiated object does not contain ProjectileItem component: {0}", gameObject.name);
 return;
 }
 proj.Fire();
 }

 private static void SpawnImpactEffects(Collision collision, Transform transform)
 {
 if (collision == null || collision.collider == null || collision.collider.gameObject == null) return;

 var materialTagForObjectAtPosition = Utils.GetMaterialTagForObjectAtPosition(collision.collider.gameObject, collision.gameObject.transform.position);
 Logging.LogDebug("SpawnImpactEffects - materialTag: {0}", materialTagForObjectAtPosition);

 var impactEffectTypeBasedOnMaterial = vp_Bullet.GetImpactEffectTypeBasedOnMaterial(materialTagForObjectAtPosition);
 var bulletImpactEffectManager = GameManager.GetEffectPoolManager();
 if (bulletImpactEffectManager == null) return;

 var pool = bulletImpactEffectManager.GetBulletImpactEffectPool();
 pool?.SpawnUntilParticlesDone(
 impactEffectTypeBasedOnMaterial == BulletImpactEffectType.BulletImpactEffect_Untagged
 ? BulletImpactEffectType.BulletImpactEffect_Stone
 : impactEffectTypeBasedOnMaterial, transform.position, transform.rotation);

 var materialEffectType = ImpactDecals.MapBulletImpactEffectTypeToMaterialEffectType(impactEffectTypeBasedOnMaterial);
 GameManager.GetDynamicDecalsManager()?.AddImpactDecal(ProjectileType.Bullet, materialEffectType, collision.gameObject.transform.position, transform.forward);

 if (collision.collider != null && collision.collider.gameObject != null)
 {
 GameAudioManager.SetMaterialSwitch(materialTagForObjectAtPosition, collision.collider.gameObject);
 var soundEmitterFromGameObject = GameAudioManager.GetSoundEmitterFromGameObject(collision.collider.gameObject);
 AkSoundEngine.PostEvent("Play_BulletImpacts", soundEmitterFromGameObject);
 GameAudioManager.SetAudioSourceTransform(collision.collider.gameObject, collision.collider.gameObject.transform);
 }

 Logging.LogDebug("SpawnImpactEffects complete for object {0}", collision.collider.gameObject.name);
 }

 private void Update()
 {
 if (m_LineRenderer == null) return;
 if (!LineRendererStartFadeOut)
 {
 if (Time.time - LastTrajectoryUpdateTime < TrajectoryUpdateInterval) return;

 LastTrajectoryUpdateTime = Time.time;
 if (TrajectoryPoints == null) TrajectoryPoints = new List<Vector3>(InitialTrajectoryCapacity);
 TrajectoryPoints.Add(transform.position);

 Logging.LogDebug("Update - added trajectory point, total points: {0}", TrajectoryPoints.Count);
 
 var totalLength =0f;
 var removeCount =0;
 for (var i = TrajectoryPoints.Count -1; i >0; i--)
 {
 totalLength += Vector3.Distance(TrajectoryPoints[i], TrajectoryPoints[i -1]);
 if (!(totalLength > LineRendererMaxLength)) continue;
 removeCount = i;
 break;
 }

 if (removeCount >0)
 {
 TrajectoryPoints.RemoveRange(0, removeCount);
 Logging.LogDebug("Update - trimmed {0} trajectory points to maintain max length", removeCount);
 }

 m_LineRenderer.positionCount = TrajectoryPoints.Count;
 m_LineRenderer.SetPositions(TrajectoryPoints.ToArray());
 }
 else
 {
 LineRendererFadeTimer += Time.deltaTime;
 var alpha = Mathf.Clamp01(1.0f - (LineRendererFadeTimer / LineRendererFadeDuration));
 var startColor = m_LineRenderer.startColor;
 var endColor = m_LineRenderer.endColor;
 m_LineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha * startColor.a);
 m_LineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha * endColor.a);

 if (LineRendererFadeTimer >= LineRendererFadeDuration)
 {
 Logging.LogDebug("Update - LineRenderer fade complete, destroying trail and projectile");
 Destroy(m_LineRenderer);
 Destroy(gameObject);
 }
 }
 }
}