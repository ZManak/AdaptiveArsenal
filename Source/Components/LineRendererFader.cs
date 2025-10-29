using AdaptiveArsenal.Utilities;
using UnityEngine;

namespace AdaptiveArsenal.Components
{
    [RegisterTypeInIl2Cpp(true)]
    public class LineRendererFader : MonoBehaviour
    {
        public float Duration = 5f;
        private float _timer;
        private LineRenderer _lineRenderer;
        private Color _startColor;
        private Color _endColor;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                Logging.LogWarning("LineRendererFader.Awake - no LineRenderer found, destroying fader object");
                Destroy(gameObject);
                return;
            }

            _startColor = _lineRenderer.startColor;
            _endColor = _lineRenderer.endColor;
            _timer = 0f;

            Logging.LogDebug("LineRendererFader.Awake - started fade of LineRenderer on {0} with duration {1}", gameObject.name, Duration);
        }

        private void Update()
        {
            if (_lineRenderer == null)
            {
                Destroy(gameObject);
                return;
            }

            _timer += Time.deltaTime;
            var alpha = Mathf.Clamp01(1f - (_timer / Duration));
            _lineRenderer.startColor = new Color(_startColor.r, _startColor.g, _startColor.b, alpha * _startColor.a);
            _lineRenderer.endColor = new Color(_endColor.r, _endColor.g, _endColor.b, alpha * _endColor.a);

            if (_timer >= Duration)
            {
                Logging.LogDebug("LineRendererFader.Update - fade complete, destroying fader {0}", gameObject.name);
                Destroy(gameObject);
            }
        }
    }
}
