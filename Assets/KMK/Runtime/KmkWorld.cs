using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KMK.EssenceRun
{
    public sealed class KmkWorld : MonoBehaviour
    {
        private readonly List<KmkTrackSegment> _segments = new List<KmkTrackSegment>();

        private KmkGame _game;
        private RunnerController _player;
        private Transform _segmentRoot;
        private ParticleSystem _ambientParticles;
        private Material _ambientMaterial;
        private ThemePalette _palette;
        private int _nextSegmentNumber;

        public void Initialize(KmkGame game, RunnerController player)
        {
            _game = game;
            _player = player;
            _palette = ThemeLibrary.Get(KmkChapter.LianeLibre);

            _ambientMaterial = KmkVisuals.Material(_palette.Emission, 0f, 0.2f, true);
            _ambientParticles = KmkVisuals.CreateAmbientParticles(transform, _ambientMaterial);
            ResetWorld();
        }

        private void Update()
        {
            if (_player == null || _segments.Count == 0)
            {
                return;
            }

            Vector3 playerPosition = _player.transform.position;
            if (_ambientParticles != null)
            {
                _ambientParticles.transform.position = playerPosition + new Vector3(0f, 3.5f, 20f);
            }

            float furthestStart = float.MinValue;
            for (int i = 0; i < _segments.Count; i++)
            {
                furthestStart = Mathf.Max(furthestStart, _segments[i].StartZ);
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                KmkTrackSegment segment = _segments[i];
                if (segment.StartZ + KmkConstants.SegmentLength >= playerPosition.z - 24f)
                {
                    continue;
                }

                furthestStart += KmkConstants.SegmentLength;
                segment.Configure(furthestStart, _nextSegmentNumber, false, _palette);
                _nextSegmentNumber += 1;
            }
        }

        public void ResetWorld()
        {
            if (_segmentRoot != null)
            {
                _segmentRoot.gameObject.SetActive(false);
                Destroy(_segmentRoot.gameObject);
            }

            _segments.Clear();
            GameObject root = new GameObject("Procedural Paris Segments");
            root.transform.SetParent(transform, false);
            _segmentRoot = root.transform;

            _nextSegmentNumber = KmkConstants.SegmentCount - 1;
            for (int i = 0; i < KmkConstants.SegmentCount; i++)
            {
                float startZ = (i - 1) * KmkConstants.SegmentLength;
                int segmentNumber = i - 1;

                GameObject segmentObject = new GameObject("Paris Segment " + segmentNumber.ToString("000"));
                segmentObject.transform.SetParent(_segmentRoot, false);
                KmkTrackSegment segment = segmentObject.AddComponent<KmkTrackSegment>();
                segment.Initialize(_game, i + 101);
                segment.Configure(startZ, segmentNumber, i <= 2, _palette);
                _segments.Add(segment);
            }

            ApplyTheme(_palette);
        }

        public void ApplyTheme(ThemePalette palette)
        {
            _palette = palette;
            for (int i = 0; i < _segments.Count; i++)
            {
                _segments[i].ApplyTheme(palette);
            }

            if (_ambientMaterial != null)
            {
                KmkVisuals.SetColor(_ambientMaterial, palette.Emission);
                if (_ambientMaterial.HasProperty("_EmissionColor"))
                {
                    _ambientMaterial.SetColor("_EmissionColor", palette.Emission * 2.2f);
                }
            }

            if (_ambientParticles != null)
            {
                ParticleSystem.MainModule main = _ambientParticles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(palette.Emission.r, palette.Emission.g, palette.Emission.b, 0.35f),
                    new Color(palette.Secondary.r, palette.Secondary.g, palette.Secondary.b, 0.82f));
            }
        }

        public void EmitCollect(Vector3 worldPosition, Color color)
        {
            GameObject burstObject = new GameObject("Essence Collect Burst");
            burstObject.transform.position = worldPosition;

            ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.duration = 0.28f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 0.72f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.11f);
            main.maxParticles = 28;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = KmkVisuals.Material(color, 0f, 0.25f, true);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            particles.Play();
            Destroy(burstObject, 1.4f);
        }
    }
}
