using UnityEngine;

namespace KMK.EssenceRun
{
    public enum HazardKind
    {
        Blocker,
        JumpBarrier,
        SlideGate
    }

    public sealed class KmkHazard : MonoBehaviour
    {
        public HazardKind Kind { get; private set; }

        private KmkGame _game;
        private BoxCollider _trigger;
        private GameObject _blockerVisual;
        private GameObject _jumpVisual;
        private GameObject _slideVisual;

        public void Initialize(KmkGame game)
        {
            _game = game;
            _trigger = gameObject.AddComponent<BoxCollider>();
            _trigger.isTrigger = true;
            BuildVisuals();
        }

        private void BuildVisuals()
        {
            ThemePalette palette = ThemeLibrary.Get(KmkChapter.LianeLibre);
            Material dark = KmkVisuals.Material(new Color(0.055f, 0.045f, 0.038f), 0.24f, 0.38f, false);
            Material gold = KmkVisuals.Material(palette.Accent, 0.75f, 0.82f, true);
            Material copper = KmkVisuals.Material(new Color(0.55f, 0.20f, 0.09f), 0.68f, 0.72f, true);

            _blockerVisual = new GameObject("Noise Blocker Visual");
            _blockerVisual.transform.SetParent(transform, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Black Trunk", _blockerVisual.transform, new Vector3(0f, 0.78f, 0f), new Vector3(1.58f, 1.55f, 1.05f), dark, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Gold Edge Top", _blockerVisual.transform, new Vector3(0f, 1.51f, -0.03f), new Vector3(1.64f, 0.09f, 1.10f), gold, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Gold Edge Bottom", _blockerVisual.transform, new Vector3(0f, 0.06f, -0.03f), new Vector3(1.64f, 0.09f, 1.10f), gold, false);
            KmkVisuals.Text3D("Noise Label", "BRUIT", _blockerVisual.transform, new Vector3(0f, 0.82f, -0.56f), new Vector3(0f, 180f, 0f), 0.013f, palette.Accent, TextAnchor.MiddleCenter);

            _jumpVisual = new GameObject("Jump Barrier Visual");
            _jumpVisual.transform.SetParent(transform, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Jump Bar", _jumpVisual.transform, new Vector3(0f, 0.47f, 0f), new Vector3(1.92f, 0.46f, 0.48f), copper, false);
            KmkVisuals.Primitive(PrimitiveType.Cylinder, "Jump Left Foot", _jumpVisual.transform, new Vector3(-0.78f, 0.24f, 0f), new Vector3(0.13f, 0.30f, 0.13f), dark, false);
            KmkVisuals.Primitive(PrimitiveType.Cylinder, "Jump Right Foot", _jumpVisual.transform, new Vector3(0.78f, 0.24f, 0f), new Vector3(0.13f, 0.30f, 0.13f), dark, false);

            _slideVisual = new GameObject("Slide Gate Visual");
            _slideVisual.transform.SetParent(transform, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Slide Left Post", _slideVisual.transform, new Vector3(-0.83f, 1.25f, 0f), new Vector3(0.18f, 2.5f, 0.42f), dark, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Slide Right Post", _slideVisual.transform, new Vector3(0.83f, 1.25f, 0f), new Vector3(0.18f, 2.5f, 0.42f), dark, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Slide Beam", _slideVisual.transform, new Vector3(0f, 1.76f, 0f), new Vector3(1.88f, 0.58f, 0.52f), gold, false);
            KmkVisuals.Text3D("Slide Label", "KMK", _slideVisual.transform, new Vector3(0f, 1.76f, -0.29f), new Vector3(0f, 180f, 0f), 0.011f, Color.black, TextAnchor.MiddleCenter);
        }

        public void Configure(HazardKind kind)
        {
            Kind = kind;
            _blockerVisual.SetActive(kind == HazardKind.Blocker);
            _jumpVisual.SetActive(kind == HazardKind.JumpBarrier);
            _slideVisual.SetActive(kind == HazardKind.SlideGate);

            switch (kind)
            {
                case HazardKind.Blocker:
                    _trigger.center = new Vector3(0f, 0.85f, 0f);
                    _trigger.size = new Vector3(1.65f, 1.70f, 1.10f);
                    break;
                case HazardKind.JumpBarrier:
                    _trigger.center = new Vector3(0f, 0.46f, 0f);
                    _trigger.size = new Vector3(1.95f, 0.92f, 0.72f);
                    break;
                case HazardKind.SlideGate:
                    _trigger.center = new Vector3(0f, 1.72f, 0f);
                    _trigger.size = new Vector3(1.95f, 0.82f, 0.72f);
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_game == null || _game.State != KmkGameState.Playing)
            {
                return;
            }

            RunnerController runner = other.GetComponent<RunnerController>();
            if (runner == null)
            {
                runner = other.GetComponentInParent<RunnerController>();
            }

            if (runner != null && !runner.Clears(Kind))
            {
                runner.HitObstacle();
            }
        }
    }
}
