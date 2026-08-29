using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KMK.EssenceRun
{
    public sealed class PremiumHud : MonoBehaviour
    {
        private static readonly Color Ivory = new Color(0.95f, 0.92f, 0.84f);
        private static readonly Color Gold = new Color(0.80f, 0.65f, 0.32f);
        private static readonly Color Copper = new Color(0.88f, 0.40f, 0.18f);
        private static readonly Color Ink = new Color(0.016f, 0.014f, 0.012f);
        private static readonly Color Muted = new Color(0.57f, 0.54f, 0.48f);

        private KmkGame _game;
        private Font _font;
        private RectTransform _safeRoot;
        private GameObject _menuRoot;
        private GameObject _hudRoot;
        private GameObject _countdownRoot;
        private GameObject _gameOverRoot;
        private Text _menuBest;
        private Text _score;
        private Text _essence;
        private Text _combo;
        private Text _chapter;
        private Text _speed;
        private Text _countdown;
        private Text _finalScore;
        private Text _finalDetail;
        private Text _record;
        private Text _muteLabel;
        private RectTransform _essenceCard;
        private RectTransform _menuTitle;
        private Rect _lastSafeArea;
        private KmkGameState _lastState = (KmkGameState)(-1);
        private float _essencePulse;
        private int _lastEssence;

        public void Initialize(KmkGame game)
        {
            _game = game;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            BuildCanvas();
            BuildMenu();
            BuildHud();
            BuildCountdown();
            BuildGameOver();
            ApplySafeArea(true);
            RefreshState(true);
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            ApplySafeArea(false);
            RefreshState(false);
            UpdateHudValues();
            AnimateUi();
        }

        private void RefreshState(bool force)
        {
            if (!force && _lastState == _game.State)
            {
                return;
            }

            _lastState = _game.State;
            _menuRoot.SetActive(_game.State == KmkGameState.Menu);
            _hudRoot.SetActive(_game.State != KmkGameState.Menu);
            _countdownRoot.SetActive(_game.State == KmkGameState.Countdown);
            _gameOverRoot.SetActive(_game.State == KmkGameState.GameOver);

            if (_game.State == KmkGameState.Menu)
            {
                _menuBest.text = _game.BestScore > 0
                    ? "MEILLEUR RUN  " + _game.BestScore.ToString("000000")
                    : "PREMIER RUN À PARIS";
            }
            else if (_game.State == KmkGameState.GameOver)
            {
                _finalScore.text = _game.Score.ToString("000000");
                _finalDetail.text = _game.Essence.ToString("00") + " ESSENCES  •  COMBO MAX " + Mathf.Max(1, _game.MaxCombo).ToString("00");
                _record.text = _game.Score >= _game.BestScore && _game.Score > 0
                    ? "NOUVEAU RECORD"
                    : "MEILLEUR  " + _game.BestScore.ToString("000000");
            }
        }

        private void UpdateHudValues()
        {
            _score.text = _game.Score.ToString("000000");
            _essence.text = _game.Essence.ToString("00");
            _combo.text = _game.Combo > 1 ? "COMBO  x" + _game.Combo.ToString("00") : "COMBO  —";
            ThemePalette palette = ThemeLibrary.Get(_game.Chapter);
            _chapter.text = palette.DisplayName + "\n" + palette.Subtitle;
            _chapter.color = palette.Accent;
            _speed.text = _game.State == KmkGameState.Playing
                ? _game.CurrentSpeed.ToString("00.0") + " M/S"
                : "PRÊT";

            if (_game.State == KmkGameState.Countdown)
            {
                float value = _game.CountdownValue;
                _countdown.text = value > 1f ? Mathf.CeilToInt(value).ToString() : "GO";
            }

            _muteLabel.text = _game.IsMuted ? "SON OFF" : "SON ON";

            if (_game.Essence != _lastEssence)
            {
                _lastEssence = _game.Essence;
                _essencePulse = 0.28f;
            }
        }

        private void AnimateUi()
        {
            if (_menuRoot.activeSelf && _menuTitle != null)
            {
                float offset = Mathf.Sin(Time.unscaledTime * 1.15f) * 5f;
                _menuTitle.anchoredPosition = new Vector2(0f, 250f + offset);
            }

            if (_essencePulse > 0f)
            {
                _essencePulse -= Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(_essencePulse / 0.28f);
                float scale = 1f + Mathf.Sin((1f - normalized) * Mathf.PI) * 0.18f;
                _essenceCard.localScale = Vector3.one * scale;
            }
            else
            {
                _essenceCard.localScale = Vector3.Lerp(_essenceCard.localScale, Vector3.one, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            }

            if (_countdownRoot.activeSelf)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.04f;
                _countdown.rectTransform.localScale = Vector3.one * pulse;
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("KMK Premium Interface");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject safeObject = CreateUiObject("Safe Area", canvasObject.transform);
            _safeRoot = safeObject.GetComponent<RectTransform>();
            Stretch(_safeRoot);
        }

        private void BuildMenu()
        {
            _menuRoot = CreateUiObject("Main Menu", _safeRoot);
            Stretch(_menuRoot.GetComponent<RectTransform>());
            Image veil = _menuRoot.AddComponent<Image>();
            veil.color = new Color(0.004f, 0.004f, 0.003f, 0.66f);
            veil.raycastTarget = true;

            Image topGlow = CreateImage("Copper Atmosphere", _menuRoot.transform, new Color(0.45f, 0.23f, 0.08f, 0.18f));
            RectTransform glowRect = topGlow.rectTransform;
            glowRect.anchorMin = new Vector2(0f, 0.58f);
            glowRect.anchorMax = new Vector2(1f, 1f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;
            topGlow.raycastTarget = false;

            Text brand = CreateText("Brand", _menuRoot.transform, "KMK  PARIS", 34, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(brand.rectTransform, new Vector2(0.5f, 1f), new Vector2(820f, 70f), new Vector2(0f, -118f));

            Text eyebrow = CreateText("Eyebrow", _menuRoot.transform, "PARIS  •  UNE COURSE  •  UNE ESSENCE", 22, Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(940f, 54f), new Vector2(0f, -198f));

            Text title = CreateText("Title", _menuRoot.transform, "THE\nESSENCE RUN", 108, Ivory, TextAnchor.MiddleCenter, FontStyle.Bold);
            _menuTitle = title.rectTransform;
            SetRect(_menuTitle, new Vector2(0.5f, 0.5f), new Vector2(950f, 310f), new Vector2(0f, 250f));
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 76;
            title.resizeTextMaxSize = 112;
            title.lineSpacing = 0.82f;

            Text subtitle = CreateText("Subtitle", _menuRoot.transform, "ATTRAPE L’ESSENCE.  ÉVITE LE BRUIT.\nCOURS PLUS LOIN QUE LA NUIT.", 28, new Color(0.76f, 0.72f, 0.64f), TextAnchor.MiddleCenter, FontStyle.Normal);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(900f, 120f), new Vector2(0f, 36f));
            subtitle.lineSpacing = 1.24f;

            BuildThemeChips();

            Button play = CreateButton("Play", _menuRoot.transform, "LANCER LE RUN", Gold, Ink, new Vector2(760f, 132f), new Vector2(0f, -350f));
            play.onClick.AddListener(delegate
            {
                _game.Audio.PlayButton();
                _game.StartRun();
            });

            _menuBest = CreateText("Best", _menuRoot.transform, "PREMIER RUN À PARIS", 25, Copper, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(_menuBest.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(820f, 60f), new Vector2(0f, -465f));

            Text controls = CreateText("Controls", _menuRoot.transform, "SWIPE  ←  →   •   ↑  SAUT   •   ↓  GLISSADE", 22, new Color(0.52f, 0.49f, 0.43f), TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(950f, 70f), new Vector2(0f, 96f));

            Text signature = CreateText("Signature", _menuRoot.transform, "UNE BRISE D’ÉLÉGANCE.", 20, Gold, TextAnchor.MiddleCenter, FontStyle.Italic);
            SetRect(signature.rectTransform, new Vector2(0.5f, 0f), new Vector2(720f, 50f), new Vector2(0f, 42f));
        }

        private void BuildThemeChips()
        {
            string[] labels = { "LIANE LIBRE", "PALME D’HIVER", "RIVAGE CUIVRÉ" };
            Color[] foreground =
            {
                Gold,
                new Color(0.68f, 0.85f, 0.91f),
                new Color(0.94f, 0.51f, 0.26f)
            };
            Color[] background =
            {
                new Color(0.72f, 0.60f, 0.31f, 0.18f),
                new Color(0.43f, 0.69f, 0.78f, 0.18f),
                new Color(0.82f, 0.34f, 0.14f, 0.18f)
            };

            for (int i = 0; i < labels.Length; i++)
            {
                GameObject chip = CreateUiObject("Theme " + labels[i], _menuRoot.transform);
                RectTransform rect = chip.GetComponent<RectTransform>();
                SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(280f, 64f), new Vector2((i - 1) * 300f, -102f));
                Image image = chip.AddComponent<Image>();
                image.color = background[i];
                image.raycastTarget = false;
                Text label = CreateText("Label", chip.transform, labels[i], 20, foreground[i], TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(label.rectTransform);
            }
        }

        private void BuildHud()
        {
            _hudRoot = CreateUiObject("Gameplay HUD", _safeRoot);
            Stretch(_hudRoot.GetComponent<RectTransform>());

            Image topVeil = CreateImage("Top Veil", _hudRoot.transform, new Color(0.006f, 0.005f, 0.004f, 0.72f));
            RectTransform topRect = topVeil.rectTransform;
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 260f);
            topRect.anchoredPosition = Vector2.zero;
            topVeil.raycastTarget = false;

            RectTransform unused;
            CreateHudCard("Score", new Vector2(0f, -96f), new Vector2(0f, 1f), new Vector2(350f, 124f), "SCORE", out _score, out unused);
            CreateHudCard("Essence", new Vector2(0f, -96f), new Vector2(0.5f, 1f), new Vector2(300f, 124f), "ESSENCE", out _essence, out _essenceCard);
            CreateHudCard("Speed", new Vector2(0f, -96f), new Vector2(1f, 1f), new Vector2(300f, 124f), "VITESSE", out _speed, out unused);

            _combo = CreateText("Combo", _hudRoot.transform, "COMBO  —", 21, Ivory, TextAnchor.MiddleLeft, FontStyle.Bold);
            SetRect(_combo.rectTransform, new Vector2(0f, 1f), new Vector2(380f, 50f), new Vector2(38f, -192f));

            _chapter = CreateText("Chapter", _hudRoot.transform, "LIANE LIBRE\nL’ORIGINE EN MOUVEMENT", 20, Gold, TextAnchor.MiddleRight, FontStyle.Bold);
            SetRect(_chapter.rectTransform, new Vector2(1f, 1f), new Vector2(560f, 78f), new Vector2(-38f, -201f));
            _chapter.lineSpacing = 0.95f;

            Text hint = CreateText("Hint", _hudRoot.transform, "←  SWIPE  →      ↑  SAUT      ↓  GLISSE", 20, new Color(0.73f, 0.69f, 0.61f, 0.82f), TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(940f, 62f), new Vector2(0f, 42f));

            Button mute = CreateButton("Mute", _hudRoot.transform, "SON ON", new Color(0.04f, 0.035f, 0.028f, 0.86f), Gold, new Vector2(180f, 64f), new Vector2(0f, 116f));
            RectTransform muteRect = mute.GetComponent<RectTransform>();
            muteRect.anchorMin = new Vector2(1f, 0f);
            muteRect.anchorMax = new Vector2(1f, 0f);
            muteRect.pivot = new Vector2(1f, 0f);
            muteRect.anchoredPosition = new Vector2(-34f, 112f);
            _muteLabel = mute.GetComponentInChildren<Text>();
            mute.onClick.AddListener(delegate
            {
                _game.ToggleMute();
                if (!_game.IsMuted)
                {
                    _game.Audio.PlayButton();
                }
            });
        }

        private void BuildCountdown()
        {
            _countdownRoot = CreateUiObject("Countdown", _safeRoot);
            Stretch(_countdownRoot.GetComponent<RectTransform>());
            Image veil = _countdownRoot.AddComponent<Image>();
            veil.color = new Color(0.006f, 0.005f, 0.004f, 0.34f);
            veil.raycastTarget = false;

            _countdown = CreateText("Countdown Value", _countdownRoot.transform, "3", 220, Ivory, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(_countdown.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(600f, 300f), new Vector2(0f, 80f));

            Text ready = CreateText("Ready", _countdownRoot.transform, "PARIS EST À TOI", 28, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(ready.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(700f, 70f), new Vector2(0f, -100f));
        }

        private void BuildGameOver()
        {
            _gameOverRoot = CreateUiObject("Game Over", _safeRoot);
            Stretch(_gameOverRoot.GetComponent<RectTransform>());
            Image veil = _gameOverRoot.AddComponent<Image>();
            veil.color = new Color(0.004f, 0.004f, 0.003f, 0.80f);
            veil.raycastTarget = true;

            GameObject card = CreateUiObject("Result Card", _gameOverRoot.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            SetRect(cardRect, new Vector2(0.5f, 0.5f), new Vector2(900f, 950f), new Vector2(0f, -10f));
            Image cardImage = card.AddComponent<Image>();
            cardImage.color = new Color(0.038f, 0.032f, 0.025f, 0.97f);

            Text eyebrow = CreateText("Eyebrow", card.transform, "RUN TERMINÉ", 24, Copper, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(760f, 70f), new Vector2(0f, -95f));

            _finalScore = CreateText("Final Score", card.transform, "000000", 118, Ivory, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(_finalScore.rectTransform, new Vector2(0.5f, 1f), new Vector2(840f, 190f), new Vector2(0f, -240f));

            Text points = CreateText("Points", card.transform, "POINTS", 23, Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(points.rectTransform, new Vector2(0.5f, 1f), new Vector2(500f, 50f), new Vector2(0f, -350f));

            _finalDetail = CreateText("Details", card.transform, "00 ESSENCES  •  COMBO MAX 00", 25, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(_finalDetail.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(780f, 70f), new Vector2(0f, 120f));

            _record = CreateText("Record", card.transform, "MEILLEUR  000000", 24, new Color(0.74f, 0.70f, 0.62f), TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(_record.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(760f, 60f), new Vector2(0f, 48f));

            Button replay = CreateButton("Replay", card.transform, "REJOUER", Gold, Ink, new Vector2(690f, 126f), new Vector2(0f, -150f));
            replay.onClick.AddListener(delegate
            {
                _game.Audio.PlayButton();
                _game.StartRun();
            });

            Button menu = CreateButton("Menu", card.transform, "RETOUR AU MENU", new Color(0.11f, 0.095f, 0.078f), Ivory, new Vector2(690f, 106f), new Vector2(0f, -300f));
            menu.onClick.AddListener(delegate
            {
                _game.Audio.PlayButton();
                _game.ReturnToMenu();
            });

            Text signature = CreateText("Signature", card.transform, "UNE BRISE D’ÉLÉGANCE.", 20, Gold, TextAnchor.MiddleCenter, FontStyle.Italic);
            SetRect(signature.rectTransform, new Vector2(0.5f, 0f), new Vector2(720f, 50f), new Vector2(0f, 50f));
        }

        private void CreateHudCard(string name, Vector2 position, Vector2 anchor, Vector2 size, string label, out Text value, out RectTransform cardRect)
        {
            GameObject card = CreateUiObject(name + " Card", _hudRoot.transform);
            cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = anchor;
            cardRect.anchorMax = anchor;
            cardRect.pivot = anchor;
            cardRect.sizeDelta = size;
            cardRect.anchoredPosition = position;

            Image image = card.AddComponent<Image>();
            image.color = new Color(0.030f, 0.027f, 0.022f, 0.86f);
            image.raycastTarget = false;

            Text labelText = CreateText("Label", card.transform, label, 19, Muted, TextAnchor.UpperCenter, FontStyle.Bold);
            Stretch(labelText.rectTransform);
            labelText.rectTransform.offsetMin = new Vector2(8f, 56f);
            labelText.rectTransform.offsetMax = new Vector2(-8f, -12f);

            value = CreateText("Value", card.transform, label == "SCORE" ? "000000" : "00", label == "SCORE" ? 38 : 41, label == "ESSENCE" ? Gold : Ivory, TextAnchor.LowerCenter, FontStyle.Bold);
            Stretch(value.rectTransform);
            value.rectTransform.offsetMin = new Vector2(8f, 12f);
            value.rectTransform.offsetMax = new Vector2(-8f, -38f);
        }

        private Button CreateButton(string name, Transform parent, string label, Color background, Color foreground, Vector2 size, Vector2 position)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 0.5f), size, position);

            Image image = buttonObject.AddComponent<Image>();
            image.color = background;

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.86f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.70f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            Text text = CreateText("Label", buttonObject.transform, label, 30, foreground, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(text.rectTransform);
            return button;
        }

        private Text CreateText(string name, Transform parent, string value, int fontSize, Color color, TextAnchor alignment, FontStyle style)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            Stretch(image.rectTransform);
            return image;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject instance = new GameObject(name, typeof(RectTransform));
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private void ApplySafeArea(bool force)
        {
            Rect safeArea = Screen.safeArea;
            if (!force && safeArea == _lastSafeArea)
            {
                return;
            }

            _lastSafeArea = safeArea;
            Vector2 minimum = safeArea.position;
            Vector2 maximum = safeArea.position + safeArea.size;
            minimum.x /= Screen.width;
            minimum.y /= Screen.height;
            maximum.x /= Screen.width;
            maximum.y /= Screen.height;
            _safeRoot.anchorMin = minimum;
            _safeRoot.anchorMax = maximum;
            _safeRoot.offsetMin = Vector2.zero;
            _safeRoot.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }
    }
}
