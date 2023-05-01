using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class SoundController : SingletonBehaviour<SoundController> {
        [SerializeField] private GameObject _sounds;
        [SerializeField] private AudioSource _music;

        [SerializeField] private AudioClip[] _clips;

        private readonly List<AudioSource> _soundSources = new List<AudioSource>();
        private Tween _musicTween;

        public AudioSource PlaySound(string soundName, float volume = 1f, float pitch = 1f) {
            var source = _soundSources.FirstOrDefault(ss => !ss.isPlaying);
            if (source == null) {
                source = _sounds.AddComponent<AudioSource>();
                _soundSources.Add(source);
            }

            source.DOKill();
            source.clip = _clips.First(clip => clip.name == soundName);
            source.Play();
            source.volume = volume;
            source.pitch = pitch;
            source.loop = false;

            return source;
        }

        public AudioSource PlayMusic(string musicName, float volume = 1f) {
            _musicTween?.Kill();
            const float fadeDuration = 0.3f;
            if (_music.isPlaying) {
                _musicTween = DOTween.Sequence()
                    .Append(_music.DOFade(0f, fadeDuration))
                    .AppendCallback(SetNew)
                    .Append(_music.DOFade(volume, fadeDuration));
            } else {
                SetNew();
                _music.DOFade(volume, fadeDuration);
            }

            return _music;

            void SetNew() {
                _music.Stop();
                _music.clip = _clips.First(clip => clip.name == musicName);
                _music.loop = true;
                _music.Play();
            }
        }

        private void OnDestroy() {
            _musicTween?.Kill();
        }
    }
}
