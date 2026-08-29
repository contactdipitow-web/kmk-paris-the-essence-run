using UnityEngine;
using UnityEngine.Rendering;

namespace KMK.EssenceRun
{
    public sealed class EssenceCollectible : MonoBehaviour
    {
        private KmkGame _game;
        private Transform _visual;
        private Vector3 _baseLocalPosition;
        private bool _collected;
        private float _phase;

        public void Initialize(KmkGame game)
        {
            _game = game;
            SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.58f;

            _visual = new GameObject("KMK Essence Bottle").transform;
            _visual.SetParent(transform, false);
            BuildBottle(_visual);
            _phase = Random.value * Mathf.PI * 2f;
        }

        private void BuildBottle(Transform root)
        {
            ThemePalette palette = ThemeLibrary.Get(KmkChapter.LianeLibre);
            Material glass = KmkVisuals.Material(new Color(0.88f, 0.72f, 0.34f), 0.22f, 0.88f, true);
            Material cap = KmkVisuals.Material(palette.Accent, 0.78f, 0.92f, true);
            Material label = KmkVisuals.Material(new Color(0.05f, 0.047f, 0.04f), 0.06f, 0.52f, false);

            GameObject body = KmkVisuals.Primitive(PrimitiveType.Cube, "Bottle Body", root, new Vector3(0f, 0f, 0f), new Vector3(0.52f, 0.72f, 0.30f), glass, false);
            body.transform.localRotation = Quaternion.Euler(0f, 8f, 0f);
            KmkVisuals.Primitive(PrimitiveType.Cylinder, "Bottle Neck", root, new Vector3(0f, 0.43f, 0f), new Vector3(0.16f, 0.14f, 0.16f), glass, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Bottle Cap", root, new Vector3(0f, 0.62f, 0f), new Vector3(0.31f, 0.18f, 0.27f), cap, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Bottle Label", root, new Vector3(0f, -0.02f, -0.165f), new Vector3(0.36f, 0.26f, 0.025f), label, false);
            KmkVisuals.Text3D("Bottle KMK", "KMK", root, new Vector3(0f, -0.01f, -0.184f), new Vector3(0f, 180f, 0f), 0.0062f, palette.Accent, TextAnchor.MiddleCenter);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        public void ResetCollectible()
        {
            _collected = false;
            _baseLocalPosition = transform.localPosition;
            if (_visual != null)
            {
                _visual.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            if (_visual == null || _collected)
            {
                return;
            }

            float t = Time.time * 2.6f + _phase;
            _visual.localPosition = new Vector3(0f, Mathf.Sin(t) * 0.12f, 0f);
            _visual.localRotation = Quaternion.Euler(0f, t * 48f, Mathf.Sin(t * 0.7f) * 4f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || _game == null || _game.State != KmkGameState.Playing)
            {
                return;
            }

            RunnerController runner = other.GetComponent<RunnerController>();
            if (runner == null)
            {
                runner = other.GetComponentInParent<RunnerController>();
            }

            if (runner == null)
            {
                return;
            }

            _collected = true;
            Vector3 position = transform.position;
            _game.CollectEssence(position);
            gameObject.SetActive(false);
        }
    }
}
