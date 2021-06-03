
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fluid
{
    public partial class Character : MonoBehaviour
    {
        [SerializeField] private float _moveDuration = 0.2f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0,0,1,1);
        public bool _useCurve = true;
        IEnumerator LerpToDestination(Vector3 targetDestination, float duration)
        {
            _isBusy = true;
            float time = 0;
            Vector3 startPosition = transform.position;

            while (time < duration)
            {
                var delta = time / duration;
                transform.position = Vector3.Lerp(startPosition, targetDestination, _useCurve ? _moveCurve.Evaluate(delta) : delta);
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = targetDestination;
            _isBusy = false;
        }

        IEnumerator LerpInOut(Vector3 targetDestination, float duration)
        {
            var origPos = transform.position;
            yield return LerpToDestination(targetDestination, duration * 0.5f);
            yield return LerpToDestination(origPos, duration * 0.5f);
        }

        IEnumerator LerpIdle(float targetScale, Color targetColor, float duration)
        {
            _isIdle = true;
            while (_isBusy == false)
            {
                var origScale = _renderer.transform.localScale.y;
                var origColor = _renderer.color;
                yield return LerpHeightScaleColor(targetScale, targetColor, duration * 0.5f);
                yield return LerpHeightScaleColor(origScale, origColor, duration * 0.5f);
            }

            _isIdle = false;
        }

        IEnumerator LerpTakeDamage(float targetScale, Color targetColor, float duration)
        {
            var origScale = _renderer.transform.localScale.y;
            var origColor = _renderer.color;
            yield return LerpHeightScaleColor(targetScale, targetColor, duration * 0.5f);
            yield return LerpHeightScaleColor(origScale, origColor, duration * 0.5f);
        }

        IEnumerator LerpHeightScaleColor(float targetScale, Color targetColor, float duration)
        {
            float time = 0;
            float startScale = _renderer.transform.localScale.y;
            Color startColor = _renderer.color;

            while (time < duration && _isBusy == false)
            {
                var delta = time / duration;
                _renderer.transform.localScale = new Vector3(_renderer.transform.localScale.x,Mathf.Lerp(startScale, targetScale, _useCurve ? _scaleCurve.Evaluate(delta) : delta), _renderer.transform.localScale.z);
                _renderer.color = Color.Lerp(startColor, targetColor, Mathf.Lerp(startScale, targetScale, _useCurve ? _scaleCurve.Evaluate(delta) : delta));
                time += Time.deltaTime;
                yield return null;
            }
            _renderer.transform.localScale = new Vector3(_renderer.transform.localScale.x, targetScale, _renderer.transform.localScale.z);
            _renderer.color = targetColor;
        }

        IEnumerator LerpMoveScale(float targetScale, float duration)
        {
            var origScale = _renderer.transform.localScale.x;
            yield return LerpWidthScale(targetScale, duration * 0.5f);
            yield return LerpWidthScale(origScale, duration * 0.5f);
        }

        IEnumerator LerpWidthScale(float targetScale, float duration)
        {
            float time = 0;
            float startScale = _renderer.transform.localScale.x;

            while (time < duration && _isBusy == false)
            {
                var delta = time / duration;
                _renderer.transform.localScale = new Vector3(Mathf.Lerp(startScale, targetScale, _useCurve ? _scaleCurve.Evaluate(delta) : delta), _renderer.transform.localScale.y, _renderer.transform.localScale.z);
                time += Time.deltaTime;
                yield return null;
            }
            _renderer.transform.localScale = new Vector3(targetScale, _renderer.transform.localScale.y, _renderer.transform.localScale.z);
        }
    }
}
