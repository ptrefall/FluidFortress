using System.Collections;
using UnityEngine;

namespace Fluid
{
    public class IntroUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _pulsingText;
        [SerializeField] private AnimationCurve _pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private void Start()
        {
            StartCoroutine(Pulse(2.0f));
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator Pulse(float duration)
        {
            while (gameObject.activeSelf)
            {
                yield return LerpAlpha(0.2f, duration * 0.5f);
                yield return LerpAlpha(1f, duration * 0.5f);
            }
        }

        IEnumerator LerpAlpha(float targetAlpha, float duration)
        {
            float time = 0;
            var startAlpha = _pulsingText.alpha;

            while (time < duration)
            {
                var delta = time / duration;
                _pulsingText.alpha = Mathf.Lerp(startAlpha, targetAlpha, _pulseCurve.Evaluate(delta));
                time += Time.deltaTime;
                yield return null;
            }

            _pulsingText.alpha = targetAlpha;
        }
    }
}