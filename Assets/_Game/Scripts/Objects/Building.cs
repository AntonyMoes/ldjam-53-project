using System;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Objects {
    public class Building : MonoBehaviour {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _opaqueMaterial;
        [SerializeField] private Material _transparentMaterial;

        private const float Transparent = 0.5f;
        private const float Opaque = 1f;
        private const float Duration = 0.3f;

        private static readonly int Opacity = Shader.PropertyToID("_Opacity");

        private Tween _animation;

        public void ToggleDithering(bool enable) {
            _animation?.Kill();
            var current = _renderer.material.HasProperty(Opacity) ? _renderer.material.GetFloat(Opacity) : Opaque;
            var result = enable ? Transparent : Opaque;
            _animation = DOTween.Sequence()
                .InsertCallback(enable ? 0 : Duration,
                    () => _renderer.material = enable ? _transparentMaterial : _opaqueMaterial)
                .Insert(0, DOVirtual
                    .Float(current, result, Duration, val => _renderer.material.SetFloat(Opacity, val))
                    .SetEase(Ease.InOutSine));
        }

        public void ResetDithering() {
            _animation?.Kill();
            _renderer.material = _opaqueMaterial;
        }
    }
}