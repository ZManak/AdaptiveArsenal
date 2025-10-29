using System.Collections.Generic;
using AdaptiveArsenal.Utilities;
using UnityEngine;

namespace AdaptiveArsenal.Components
{
 internal static class DetachedTrailManager
 {
 private sealed class TrailEntry
 {
 public GameObject GameObject;
 public LineRenderer LineRenderer;
 public Color StartColor;
 public Color EndColor;
 public float Duration;
 public float Timer;
 }

 private static readonly List<TrailEntry> _entries = new();

 public static void RegisterTrail(GameObject trailObject, float duration)
 {
 if (trailObject == null)
 {
 Logging.LogWarning("DetachedTrailManager.RegisterTrail - trailObject is null");
 return;
 }

 var lr = trailObject.GetComponent<LineRenderer>();
 if (lr == null)
 {
 Logging.LogWarning("DetachedTrailManager.RegisterTrail - no LineRenderer found on object {0}", trailObject.name);
 return;
 }

 var entry = new TrailEntry
 {
 GameObject = trailObject,
 LineRenderer = lr,
 StartColor = lr.startColor,
 EndColor = lr.endColor,
 Duration = Mathf.Max(0.01f, duration),
 Timer =0f
 };

 _entries.Add(entry);
 Logging.LogDebug("DetachedTrailManager.RegisterTrail - registered trail {0} for duration {1}", trailObject.name, duration);
 }

 public static void UpdateAll()
 {
 if (_entries.Count ==0) return;

 for (int i = _entries.Count -1; i >=0; i--)
 {
 var e = _entries[i];
 if (e == null || e.GameObject == null || e.LineRenderer == null)
 {
 _entries.RemoveAt(i);
 continue;
 }

 e.Timer += Time.deltaTime;
 float alpha = Mathf.Clamp01(1f - (e.Timer / e.Duration));

 var sc = e.StartColor;
 var ec = e.EndColor;
 e.LineRenderer.startColor = new Color(sc.r, sc.g, sc.b, alpha * sc.a);
 e.LineRenderer.endColor = new Color(ec.r, ec.g, ec.b, alpha * ec.a);

 if (e.Timer >= e.Duration)
 {
 Logging.LogDebug("DetachedTrailManager.UpdateAll - fading complete for {0}, destroying", e.GameObject.name);
 UnityEngine.Object.Destroy(e.LineRenderer);
 UnityEngine.Object.Destroy(e.GameObject);
 _entries.RemoveAt(i);
 }
 }
 }
 }
}
