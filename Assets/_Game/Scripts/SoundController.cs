using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class SoundController : SingletonBehaviour<SoundController> {
        [SerializeField] private GameObject _sounds;
        [SerializeField] private AudioSource _music;

        [SerializeField] private AudioClip[] _clips;

        private readonly List<AudioSource> _soundSources = new List<AudioSource>();

        public AudioSource PlaySound(string soundName, float volume = 1f, float pitch = 1f) {
            var source = _soundSources.FirstOrDefault(ss => !ss.isPlaying);
            if (source == null) {
                source = _sounds.AddComponent<AudioSource>();
                _soundSources.Add(source);
            }

            source.clip = _clips.First(clip => clip.name == soundName);
            source.Play();
            source.volume = volume;
            source.pitch = pitch;

            return source;
        }

        public AudioSource PlayMusic(string musicName, float volume = 1f) {
            _music.clip = _clips.First(clip => clip.name == musicName);
            _music.Play();
            _music.volume = volume;

            return _music;
        }
    }
}
