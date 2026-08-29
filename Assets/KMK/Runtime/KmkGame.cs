using UnityEngine;
using UnityEngine.Rendering;

namespace KMK.EssenceRun
{
    public enum KmkGameState
    {
        Menu,
        Countdown,
        Playing,
        GameOver
    }

    public sealed class KmkGame : MonoBehaviour
    {
        public static KmkGame Instance { get; private set; }

        public KmkGameState State { get; private set; }
        public RunnerController Player { get; private set; }
        public KmkWorld World { get; private set; }
        public KmkCameraRig CameraRig { get; private set; }
        public ProceduralAudio Audio { get; private set; }
        public PremiumHud Hud { get; private set; }
        public KmkChapter Chapter { get; private set; }
        public int Score { get; private set; }
        public int BestScore { get; private set; }
        public int Essence { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public float Distance { get; private set; }
        public float CountdownValue { get; private set; }
        public bool IsMuted { get; private set; }

        public float CurrentSpeed
        {
            get
            {
                if (State != KmkGameState.Playing)
                {
                    return 0f;
                }

                return Mathf.Min(23f, 10.5f + Distance / 145f);
            }
        }

        private int _scoreBonus;
        private float _comboTimer;
        private Light _sun;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (Object.FindAnyObjectByType<KmkGame>() == null)
            {
                GameObject root = new GameObject("KMK Paris — The Essence Run");
                root.AddComponent<KmkGame>();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.orientation = ScreenOrientation.Portrait;
            Input.multiTouchEnabled = true;

            BestScore = PlayerPrefs.GetInt("KMK_BEST_SCORE", 0);
            Chapter = KmkChapter.LianeLibre;
            State = KmkGameState.Menu;

            CreateLighting();
            CreateSystems();
            ApplyChapter(Chapter, true);
        }

        private void CreateLighting()
        {
            GameObject sunObject = new GameObject("Paris Moonlight");
            sunObject.transform.SetParent(transform, false);
            sunObject.transform.rotation = Quaternion.Euler(43f, -28f, 0f);
            _sun = sunObject.AddComponent<Light>();
            _sun.type = LightType.Directional;
            _sun.intensity = 1.15f;
            _sun.shadows = LightShadows.Soft;
            _sun.shadowStrength = 0.72f;
            _sun.shadowBias = 0.08f;
            _sun.shadowNormalBias = 0.35f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 20f;
            RenderSettings.fogEndDistance = 105f;
        }

        private void CreateSystems()
        {
            GameObject audioObject = new GameObject("Audio Director");
            audioObject.transform.SetParent(transform, false);
            Audio = audioObject.AddComponent<ProceduralAudio>();
            Audio.Initialize(this);

            GameObject playerObject = new GameObject("Mini Tyson Runner");
            playerObject.transform.SetParent(transform, false);
            Player = playerObject.AddComponent<RunnerController>();
            Player.Initialize(this);

            GameObject worldObject = new GameObject("KMK Paris World");
            worldObject.transform.SetParent(transform, false);
            World = worldObject.AddComponent<KmkWorld>();
            World.Initialize(this, Player);

            GameObject cameraObject = new GameObject("KMK Camera Rig");
            cameraObject.transform.SetParent(transform, false);
            CameraRig = cameraObject.AddComponent<KmkCameraRig>();
            CameraRig.Initialize(this, Player);

            GameObject hudObject = new GameObject("Premium HUD");
            hudObject.transform.SetParent(transform, false);
            Hud = hudObject.AddComponent<PremiumHud>();
            Hud.Initialize(this);
        }

        private void Update()
        {
            if (State == KmkGameState.Countdown)
            {
                CountdownValue -= Time.unscaledDeltaTime;
                if (CountdownValue <= 0f)
                {
                    CountdownValue = 0f;
                    State = KmkGameState.Playing;
                    Audio.PlayButton();
                }
            }

            if (State != KmkGameState.Playing)
            {
                return;
            }

            Distance = Mathf.Max(0f, Player.transform.position.z);
            Score = Mathf.FloorToInt(Distance * 6f) + _scoreBonus;

            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    Combo = 0;
                }
            }

            KmkChapter nextChapter = ThemeLibrary.ChapterForDistance(Distance);
            if (nextChapter != Chapter)
            {
                ApplyChapter(nextChapter, false);
            }

            Audio.SetIntensity(Mathf.InverseLerp(10.5f, 23f, CurrentSpeed));
        }

        public void StartRun()
        {
            if (State == KmkGameState.Countdown || State == KmkGameState.Playing)
            {
                return;
            }

            Score = 0;
            Essence = 0;
            Combo = 0;
            MaxCombo = 0;
            Distance = 0f;
            _scoreBonus = 0;
            _comboTimer = 0f;
            Chapter = KmkChapter.LianeLibre;
            CountdownValue = 3.15f;
            State = KmkGameState.Countdown;

            World.ResetWorld();
            Player.ResetRunner();
            CameraRig.ResetRig();
            ApplyChapter(Chapter, true);
            Audio.PlayMusic(Chapter);
            Audio.PlayButton();
        }

        public void CollectEssence(Vector3 worldPosition)
        {
            if (State != KmkGameState.Playing)
            {
                return;
            }

            Essence += 1;
            Combo = Mathf.Clamp(Combo + 1, 1, 25);
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            _comboTimer = 3f;
            _scoreBonus += 75 + Combo * 12;
            Audio.PlayCollect(Combo);
            CameraRig.PunchCollect();
            World.EmitCollect(worldPosition, ThemeLibrary.Get(Chapter).Emission);
        }

        public void EndRun()
        {
            if (State != KmkGameState.Playing)
            {
                return;
            }

            State = KmkGameState.GameOver;
            BestScore = Mathf.Max(BestScore, Score);
            PlayerPrefs.SetInt("KMK_BEST_SCORE", BestScore);
            PlayerPrefs.Save();
            Audio.PlayHit();
            CameraRig.PunchHit();

#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public void ReturnToMenu()
        {
            State = KmkGameState.Menu;
            Score = 0;
            Essence = 0;
            Combo = 0;
            MaxCombo = 0;
            Distance = 0f;
            _scoreBonus = 0;
            Player.ResetRunner();
            World.ResetWorld();
            ApplyChapter(KmkChapter.LianeLibre, true);
            Audio.PlayMenuMusic();
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            Audio.SetMuted(IsMuted);
        }

        public void NotifyLaneChange()
        {
            Audio.PlayLane();
        }

        public void NotifyJump()
        {
            Audio.PlayJump();
        }

        public void NotifySlide()
        {
            Audio.PlaySlide();
        }

        private void ApplyChapter(KmkChapter chapter, bool immediate)
        {
            Chapter = chapter;
            ThemePalette palette = ThemeLibrary.Get(chapter);
            RenderSettings.fogColor = palette.Fog;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = palette.Ambient;
            _sun.color = Color.Lerp(Color.white, palette.Accent, 0.24f);
            _sun.intensity = chapter == KmkChapter.PalmeDHiver ? 1.28f : 1.12f;

            if (World != null)
            {
                World.ApplyTheme(palette);
            }

            if (CameraRig != null)
            {
                CameraRig.ApplyTheme(palette, immediate);
            }

            if (Audio != null && State != KmkGameState.Menu)
            {
                Audio.SetChapter(chapter);
            }
        }
    }
}
