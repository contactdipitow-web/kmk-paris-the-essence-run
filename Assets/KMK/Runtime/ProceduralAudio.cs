using UnityEngine;

namespace KMK.EssenceRun
{
    public sealed class ProceduralAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const int LoopBeats = 16;

        private KmkGame _game;
        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _sfx;
        private AudioLowPassFilter _filterA;
        private AudioLowPassFilter _filterB;
        private AudioClip[] _chapterClips;
        private AudioClip _buttonClip;
        private AudioClip _laneClip;
        private AudioClip _jumpClip;
        private AudioClip _slideClip;
        private AudioClip _collectClip;
        private AudioClip _hitClip;
        private bool _activeA = true;
        private bool _crossfading;
        private bool _muted;
        private float _crossfade;
        private float _intensity;
        private KmkChapter _chapter;

        public void Initialize(KmkGame game)
        {
            _game = game;
            _chapterClips = new[]
            {
                BuildMusicLoop(KmkChapter.LianeLibre),
                BuildMusicLoop(KmkChapter.PalmeDHiver),
                BuildMusicLoop(KmkChapter.RivageCuivre)
            };

            _buttonClip = BuildButtonClip();
            _laneClip = BuildLaneClip();
            _jumpClip = BuildJumpClip();
            _slideClip = BuildSlideClip();
            _collectClip = BuildCollectClip();
            _hitClip = BuildHitClip();

            _musicA = CreateMusicSource("Music A", out _filterA);
            _musicB = CreateMusicSource("Music B", out _filterB);
            _sfx = CreateSfxSource();

            _chapter = KmkChapter.LianeLibre;
            _musicA.clip = _chapterClips[(int)_chapter];
            _musicA.volume = 0.22f;
            _musicA.Play();
            _musicB.volume = 0f;
        }

        private void Update()
        {
            if (_musicA == null || _musicB == null)
            {
                return;
            }

            float master = _muted ? 0f : Mathf.Lerp(0.22f, 0.43f, _intensity);
            float cutoff = Mathf.Lerp(4600f, 17000f, _intensity);
            _filterA.cutoffFrequency = Mathf.Lerp(_filterA.cutoffFrequency, cutoff, 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
            _filterB.cutoffFrequency = Mathf.Lerp(_filterB.cutoffFrequency, cutoff, 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));

            if (_crossfading)
            {
                _crossfade = Mathf.MoveTowards(_crossfade, 1f, Time.unscaledDeltaTime / 1.35f);
                float outgoing = Mathf.Cos(_crossfade * Mathf.PI * 0.5f) * master;
                float incoming = Mathf.Sin(_crossfade * Mathf.PI * 0.5f) * master;

                if (_activeA)
                {
                    _musicB.volume = outgoing;
                    _musicA.volume = incoming;
                }
                else
                {
                    _musicA.volume = outgoing;
                    _musicB.volume = incoming;
                }

                if (_crossfade >= 1f)
                {
                    _crossfading = false;
                    AudioSource oldSource = _activeA ? _musicB : _musicA;
                    oldSource.Stop();
                    oldSource.volume = 0f;
                }
            }
            else
            {
                AudioSource active = _activeA ? _musicA : _musicB;
                AudioSource inactive = _activeA ? _musicB : _musicA;
                active.volume = Mathf.Lerp(active.volume, master, 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
                inactive.volume = Mathf.Lerp(inactive.volume, 0f, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
            }

            float pitch = Mathf.Lerp(0.985f, 1.035f, _intensity);
            _musicA.pitch = pitch;
            _musicB.pitch = pitch;
        }

        public void SetIntensity(float value)
        {
            _intensity = Mathf.Clamp01(value);
        }

        public void PlayMusic(KmkChapter chapter)
        {
            SwitchChapter(chapter, true);
            SetIntensity(0.22f);
        }

        public void PlayMenuMusic()
        {
            SwitchChapter(KmkChapter.LianeLibre, false);
            SetIntensity(0f);
        }

        public void SetChapter(KmkChapter chapter)
        {
            SwitchChapter(chapter, true);
        }

        public void SetMuted(bool muted)
        {
            _muted = muted;
            if (_sfx != null)
            {
                _sfx.mute = muted;
            }
        }

        public void PlayButton()
        {
            PlayOneShot(_buttonClip, 0.72f, 1f);
        }

        public void PlayLane()
        {
            PlayOneShot(_laneClip, 0.58f, Random.Range(0.96f, 1.06f));
        }

        public void PlayJump()
        {
            PlayOneShot(_jumpClip, 0.76f, 1f);
        }

        public void PlaySlide()
        {
            PlayOneShot(_slideClip, 0.67f, Random.Range(0.95f, 1.03f));
        }

        public void PlayCollect(int combo)
        {
            float pitch = Mathf.Clamp(0.98f + combo * 0.018f, 0.98f, 1.34f);
            PlayOneShot(_collectClip, 0.82f, pitch);
        }

        public void PlayHit()
        {
            PlayOneShot(_hitClip, 0.96f, 1f);
        }

        private void SwitchChapter(KmkChapter chapter, bool preservePosition)
        {
            if (_chapterClips == null)
            {
                return;
            }

            AudioSource current = _activeA ? _musicA : _musicB;
            if (current != null && current.isPlaying && _chapter == chapter)
            {
                return;
            }

            AudioSource incoming = _activeA ? _musicB : _musicA;
            int currentSamples = current != null && current.clip != null ? current.timeSamples : 0;
            float normalized = current != null && current.clip != null && current.clip.samples > 0
                ? currentSamples / (float)current.clip.samples
                : 0f;

            incoming.clip = _chapterClips[(int)chapter];
            incoming.pitch = current != null ? current.pitch : 1f;
            incoming.volume = 0f;
            incoming.Play();
            if (preservePosition && incoming.clip.samples > 0)
            {
                incoming.timeSamples = Mathf.Clamp(Mathf.FloorToInt(normalized * incoming.clip.samples), 0, incoming.clip.samples - 1);
            }

            _activeA = !_activeA;
            _chapter = chapter;
            _crossfade = 0f;
            _crossfading = true;
        }

        private AudioSource CreateMusicSource(string name, out AudioLowPassFilter filter)
        {
            GameObject sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.priority = 10;

            filter = sourceObject.AddComponent<AudioLowPassFilter>();
            filter.cutoffFrequency = 5200f;
            filter.lowpassResonanceQ = 1.05f;
            return source;
        }

        private AudioSource CreateSfxSource()
        {
            GameObject sourceObject = new GameObject("SFX");
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = 0.86f;
            source.priority = 0;
            return source;
        }

        private void PlayOneShot(AudioClip clip, float volume, float pitch)
        {
            if (_sfx == null || clip == null || _muted)
            {
                return;
            }

            _sfx.pitch = pitch;
            _sfx.PlayOneShot(clip, volume);
        }

        private static AudioClip BuildMusicLoop(KmkChapter chapter)
        {
            float bpm = chapter == KmkChapter.PalmeDHiver
                ? 118f
                : chapter == KmkChapter.RivageCuivre ? 121f : 112f;
            float secondsPerBeat = 60f / bpm;
            int sampleCount = Mathf.CeilToInt(secondsPerBeat * LoopBeats * SampleRate);
            float[] stereo = new float[sampleCount * 2];

            int root = chapter == KmkChapter.LianeLibre ? 45 : chapter == KmkChapter.PalmeDHiver ? 42 : 43;
            int[] degrees = chapter == KmkChapter.PalmeDHiver
                ? new[] { 0, 0, 3, 7, -2, -2, 5, 7, 0, 0, 3, 10, -2, -2, 5, 7 }
                : chapter == KmkChapter.RivageCuivre
                    ? new[] { 0, 0, 5, 7, -2, -2, 3, 7, 0, 0, 5, 10, -2, -2, 3, 7 }
                    : new[] { 0, 0, 3, 7, -2, -2, 5, 7, 0, 0, 3, 10, -2, -2, 5, 7 };

            float brightness = chapter == KmkChapter.PalmeDHiver ? 1.18f : chapter == KmkChapter.RivageCuivre ? 1.08f : 1f;
            float percussion = chapter == KmkChapter.RivageCuivre ? 1.18f : 1f;

            for (int sample = 0; sample < sampleCount; sample++)
            {
                float time = sample / (float)SampleRate;
                float beatPosition = time / secondsPerBeat;
                int beatIndex = Mathf.FloorToInt(beatPosition) % LoopBeats;
                float beatPhase = beatPosition - Mathf.Floor(beatPosition);
                float halfBeatPhase = Mathf.Repeat(beatPosition * 2f, 1f);
                int bar = beatIndex / 4;

                float kickEnvelope = Mathf.Exp(-beatPhase * 15f);
                float kickFrequency = Mathf.Lerp(94f, 45f, Mathf.Clamp01(beatPhase * 3.2f));
                float kickAccent = beatIndex % 4 == 0 ? 1f : 0.70f;
                float kick = Mathf.Sin(2f * Mathf.PI * kickFrequency * time) * kickEnvelope * 0.43f * kickAccent * percussion;

                float clap = 0f;
                if (beatIndex % 4 == 1 || beatIndex % 4 == 3)
                {
                    float clapEnvelope = Mathf.Exp(-beatPhase * 24f);
                    clap = Noise(sample * 17 + 41) * clapEnvelope * 0.14f * percussion;
                    clap += Mathf.Sin(2f * Mathf.PI * 185f * time) * clapEnvelope * 0.045f;
                }

                float hatEnvelope = Mathf.Exp(-halfBeatPhase * 34f);
                float hat = Noise(sample * 31 + 7) * hatEnvelope * 0.058f * brightness;

                float bassFrequency = MidiToFrequency(root + degrees[beatIndex]);
                float bassEnvelope = Mathf.SmoothStep(1f, 0.18f, beatPhase) * Mathf.Clamp01(beatPhase * 8f);
                float bass = (
                    Mathf.Sin(2f * Mathf.PI * bassFrequency * time) * 0.78f +
                    Mathf.Sin(2f * Mathf.PI * bassFrequency * 2f * time) * 0.18f) * bassEnvelope * 0.20f;

                int chordRoot = root + 12 + (bar % 2 == 0 ? 0 : -2);
                int third = chapter == KmkChapter.PalmeDHiver ? 3 : chapter == KmkChapter.RivageCuivre ? 5 : 3;
                int[] chord = { chordRoot, chordRoot + third, chordRoot + 7 };
                float pad = 0f;
                for (int note = 0; note < chord.Length; note++)
                {
                    float frequency = MidiToFrequency(chord[note]);
                    float pulse = 0.54f + Mathf.Sin(time * 0.40f + note * 1.8f) * 0.12f;
                    pad += Mathf.Sin(2f * Mathf.PI * frequency * time + note * 0.7f) * pulse;
                    pad += Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * time + note) * 0.18f;
                }
                pad *= 0.032f * brightness;

                int sparkleNote = root + 24 + ((beatIndex * 5) % 12);
                float sparkleEnvelope = beatPhase < 0.58f ? Mathf.Exp(-beatPhase * 7.5f) : 0f;
                float sparkleFrequency = MidiToFrequency(sparkleNote);
                float sparkle = Mathf.Sin(2f * Mathf.PI * sparkleFrequency * time) * sparkleEnvelope * 0.032f * brightness;

                float mono = SoftClip((kick + clap + hat + bass + pad + sparkle) * 1.20f);
                float pan = Mathf.Sin(2f * Mathf.PI * 0.13f * time) * 0.055f;
                float shimmerLeft = Mathf.Sin(2f * Mathf.PI * sparkleFrequency * 1.004f * time) * sparkleEnvelope * 0.011f;
                float shimmerRight = Mathf.Sin(2f * Mathf.PI * sparkleFrequency * 0.996f * time) * sparkleEnvelope * 0.011f;

                stereo[sample * 2] = Mathf.Clamp(mono * (1f - pan) + shimmerLeft, -0.95f, 0.95f);
                stereo[sample * 2 + 1] = Mathf.Clamp(mono * (1f + pan) + shimmerRight, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("KMK Original — " + ThemeLibrary.Get(chapter).DisplayName, sampleCount, 2, SampleRate, false);
            clip.SetData(stereo, 0);
            return clip;
        }

        private static AudioClip BuildButtonClip()
        {
            return BuildMonoClip("KMK UI", 0.14f, delegate(float time, float normalized, int sample)
            {
                float envelope = Mathf.Exp(-normalized * 7f);
                float frequency = Mathf.Lerp(560f, 300f, normalized);
                return Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * 0.34f;
            });
        }

        private static AudioClip BuildLaneClip()
        {
            return BuildMonoClip("KMK Lane", 0.12f, delegate(float time, float normalized, int sample)
            {
                float envelope = Mathf.Exp(-normalized * 8f);
                return Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(410f, 250f, normalized) * time) * envelope * 0.31f;
            });
        }

        private static AudioClip BuildJumpClip()
        {
            return BuildMonoClip("KMK Jump", 0.30f, delegate(float time, float normalized, int sample)
            {
                float envelope = Mathf.Sin(Mathf.PI * normalized) * Mathf.Exp(-normalized * 1.8f);
                float frequency = Mathf.Lerp(220f, 780f, Mathf.SmoothStep(0f, 1f, normalized));
                return (Mathf.Sin(2f * Mathf.PI * frequency * time) + Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * time) * 0.32f) * envelope * 0.30f;
            });
        }

        private static AudioClip BuildSlideClip()
        {
            return BuildMonoClip("KMK Slide", 0.34f, delegate(float time, float normalized, int sample)
            {
                float envelope = Mathf.Exp(-normalized * 3.8f);
                float noise = Noise(sample * 5 + 19) * (1f - normalized * 0.55f);
                float tone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(180f, 72f, normalized) * time);
                return (noise * 0.18f + tone * 0.16f) * envelope;
            });
        }

        private static AudioClip BuildCollectClip()
        {
            return BuildMonoClip("KMK Essence", 0.43f, delegate(float time, float normalized, int sample)
            {
                float envelope = Mathf.Exp(-normalized * 4.2f);
                float first = Mathf.Sin(2f * Mathf.PI * 880f * time);
                float second = Mathf.Sin(2f * Mathf.PI * 1320f * time + normalized * 2f);
                float bell = Mathf.Sin(2f * Mathf.PI * 1760f * time) * 0.28f;
                return (first * 0.42f + second * 0.28f + bell) * envelope * 0.42f;
            });
        }

        private static AudioClip BuildHitClip()
        {
            return BuildMonoClip("KMK Impact", 0.60f, delegate(float time, float normalized, int sample)
            {
                float envelope = Mathf.Exp(-normalized * 4.8f);
                float low = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(112f, 40f, normalized) * time) * 0.56f;
                float noise = Noise(sample * 11 + 3) * 0.32f;
                return (low + noise) * envelope * 0.66f;
            });
        }

        private delegate float SampleGenerator(float time, float normalizedTime, int sampleIndex);

        private static AudioClip BuildMonoClip(string name, float duration, SampleGenerator generator)
        {
            int sampleCount = Mathf.Max(2, Mathf.CeilToInt(duration * SampleRate));
            float[] samples = new float[sampleCount];
            for (int sample = 0; sample < sampleCount; sample++)
            {
                float time = sample / (float)SampleRate;
                float normalized = sample / (float)(sampleCount - 1);
                samples[sample] = Mathf.Clamp(generator(time, normalized, sample), -0.98f, 0.98f);
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float MidiToFrequency(int midiNote)
        {
            return 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
        }

        private static float SoftClip(float value)
        {
            return value / (1f + Mathf.Abs(value));
        }

        private static float Noise(int seed)
        {
            unchecked
            {
                uint value = (uint)seed;
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                return (value / (float)uint.MaxValue) * 2f - 1f;
            }
        }
    }
}
