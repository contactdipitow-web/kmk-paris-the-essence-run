using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KMK.EssenceRun
{
    public sealed class KmkTrackSegment : MonoBehaviour
    {
        public float StartZ { get; private set; }

        private readonly List<Renderer> _buildingRenderers = new List<Renderer>();
        private readonly List<Renderer> _windowRenderers = new List<Renderer>();
        private readonly List<Renderer> _accentRenderers = new List<Renderer>();
        private readonly List<GameObject> _lianeDecor = new List<GameObject>();
        private readonly List<GameObject> _winterDecor = new List<GameObject>();
        private readonly List<GameObject> _copperDecor = new List<GameObject>();
        private readonly EssenceCollectible[] _collectibles = new EssenceCollectible[20];
        private readonly KmkHazard[] _hazards = new KmkHazard[4];

        private KmkGame _game;
        private Renderer _roadRenderer;
        private Renderer _leftPavement;
        private Renderer _rightPavement;
        private TextMesh _chapterSign;
        private int _permanentSeed;

        public void Initialize(KmkGame game, int seed)
        {
            _game = game;
            _permanentSeed = seed * 911 + 73;
            BuildPermanentGeometry();
            BuildGameplaySlots();
        }

        private void BuildPermanentGeometry()
        {
            ThemePalette palette = ThemeLibrary.Get(KmkChapter.LianeLibre);
            Material road = KmkVisuals.Material(palette.Road, 0.05f, 0.24f, false);
            Material pavement = KmkVisuals.Material(palette.Pavement, 0.02f, 0.32f, false);
            Material accent = KmkVisuals.Material(palette.Accent, 0.65f, 0.72f, true);

            GameObject roadObject = KmkVisuals.Primitive(
                PrimitiveType.Cube,
                "Road",
                transform,
                new Vector3(0f, -0.24f, KmkConstants.SegmentLength * 0.5f),
                new Vector3(8.2f, 0.48f, KmkConstants.SegmentLength),
                road,
                false);
            _roadRenderer = roadObject.GetComponent<Renderer>();

            GameObject leftWalk = KmkVisuals.Primitive(
                PrimitiveType.Cube,
                "Left Pavement",
                transform,
                new Vector3(-5.55f, -0.07f, KmkConstants.SegmentLength * 0.5f),
                new Vector3(2.7f, 0.35f, KmkConstants.SegmentLength),
                pavement,
                false);
            _leftPavement = leftWalk.GetComponent<Renderer>();

            GameObject rightWalk = KmkVisuals.Primitive(
                PrimitiveType.Cube,
                "Right Pavement",
                transform,
                new Vector3(5.55f, -0.07f, KmkConstants.SegmentLength * 0.5f),
                new Vector3(2.7f, 0.35f, KmkConstants.SegmentLength),
                pavement,
                false);
            _rightPavement = rightWalk.GetComponent<Renderer>();

            AddAccentCube("Left Curb", new Vector3(-4.12f, 0.12f, KmkConstants.SegmentLength * 0.5f), new Vector3(0.10f, 0.14f, KmkConstants.SegmentLength), accent);
            AddAccentCube("Right Curb", new Vector3(4.12f, 0.12f, KmkConstants.SegmentLength * 0.5f), new Vector3(0.10f, 0.14f, KmkConstants.SegmentLength), accent);

            for (int z = 2; z < KmkConstants.SegmentLength; z += 4)
            {
                AddAccentCube("Lane Mark L", new Vector3(-1.175f, 0.025f, z), new Vector3(0.055f, 0.025f, 1.35f), accent);
                AddAccentCube("Lane Mark R", new Vector3(1.175f, 0.025f, z), new Vector3(0.055f, 0.025f, 1.35f), accent);
            }

            BuildBuildings(palette);
            BuildStreetFurniture(palette);
            BuildArch(palette);
            BuildThemeDecor(palette);
        }

        private void AddAccentCube(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = KmkVisuals.Primitive(PrimitiveType.Cube, name, transform, position, scale, material, false);
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                _accentRenderers.Add(renderer);
            }
        }

        private void BuildBuildings(ThemePalette palette)
        {
            System.Random random = new System.Random(_permanentSeed);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 6; i++)
                {
                    float z = 2.8f + i * 5.4f;
                    float width = 2.6f + (float)random.NextDouble() * 2.4f;
                    float height = 4.6f + (float)random.NextDouble() * 8.2f;
                    float depth = 3.0f + (float)random.NextDouble() * 2.6f;
                    float x = side * (7.8f + width * 0.22f + (float)random.NextDouble() * 1.2f);
                    Color buildingColor = i % 2 == 0 ? palette.BuildingA : palette.BuildingB;
                    GameObject building = KmkVisuals.Primitive(
                        PrimitiveType.Cube,
                        (side < 0 ? "Left" : "Right") + " Paris Building " + i,
                        transform,
                        new Vector3(x, height * 0.5f - 0.05f, z),
                        new Vector3(width, height, depth),
                        KmkVisuals.Material(buildingColor, 0.02f, 0.28f, false),
                        false);
                    Renderer buildingRenderer = building.GetComponent<Renderer>();
                    _buildingRenderers.Add(buildingRenderer);

                    for (int window = 0; window < 2; window++)
                    {
                        float windowY = 1.4f + window * Mathf.Min(1.65f, height / 4.2f);
                        float faceX = x - side * (width * 0.505f);
                        GameObject windowStrip = KmkVisuals.Primitive(
                            PrimitiveType.Cube,
                            "Window Glow",
                            transform,
                            new Vector3(faceX, windowY, z),
                            new Vector3(0.035f, 0.30f, depth * 0.65f),
                            KmkVisuals.Material(palette.Emission, 0.05f, 0.78f, true),
                            false);
                        Renderer windowRenderer = windowStrip.GetComponent<Renderer>();
                        windowRenderer.shadowCastingMode = ShadowCastingMode.Off;
                        _windowRenderers.Add(windowRenderer);
                    }
                }
            }
        }

        private void BuildStreetFurniture(ThemePalette palette)
        {
            Material pole = KmkVisuals.Material(new Color(0.08f, 0.075f, 0.065f), 0.72f, 0.65f, false);
            Material glow = KmkVisuals.Material(palette.Emission, 0.1f, 0.75f, true);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float z = 5.0f + i * 11.0f;
                    float x = side * 4.75f;
                    KmkVisuals.Primitive(PrimitiveType.Cylinder, "Lamp Post", transform, new Vector3(x, 1.55f, z), new Vector3(0.10f, 1.55f, 0.10f), pole, false);
                    KmkVisuals.Primitive(PrimitiveType.Sphere, "Lamp Glow", transform, new Vector3(x, 3.18f, z), new Vector3(0.32f, 0.32f, 0.32f), glow, false);
                }
            }
        }

        private void BuildArch(ThemePalette palette)
        {
            if (_permanentSeed % 3 != 1)
            {
                return;
            }

            Material dark = KmkVisuals.Material(new Color(0.055f, 0.05f, 0.042f), 0.35f, 0.55f, false);
            Material accent = KmkVisuals.Material(palette.Accent, 0.70f, 0.80f, true);
            float z = 25f;
            KmkVisuals.Primitive(PrimitiveType.Cube, "Arch Left", transform, new Vector3(-4.0f, 2.5f, z), new Vector3(0.5f, 5f, 0.55f), dark, false);
            KmkVisuals.Primitive(PrimitiveType.Cube, "Arch Right", transform, new Vector3(4.0f, 2.5f, z), new Vector3(0.5f, 5f, 0.55f), dark, false);
            GameObject top = KmkVisuals.Primitive(PrimitiveType.Cube, "Arch Top", transform, new Vector3(0f, 4.7f, z), new Vector3(8.5f, 0.55f, 0.65f), accent, false);
            _accentRenderers.Add(top.GetComponent<Renderer>());
            _chapterSign = KmkVisuals.Text3D("Chapter Sign", "KMK PARIS", transform, new Vector3(0f, 4.72f, z - 0.36f), new Vector3(0f, 180f, 0f), 0.017f, Color.black, TextAnchor.MiddleCenter);
        }

        private void BuildThemeDecor(ThemePalette palette)
        {
            Material lianeMaterial = KmkVisuals.Material(new Color(0.20f, 0.36f, 0.13f), 0f, 0.42f, false);
            Material winterMaterial = KmkVisuals.Material(new Color(0.60f, 0.88f, 1.0f), 0.18f, 0.88f, true);
            Material copperMaterial = KmkVisuals.Material(new Color(0.75f, 0.31f, 0.12f), 0.76f, 0.78f, true);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    float z = 7f + i * 17f;
                    float x = side * 5.4f;

                    GameObject liane = KmkVisuals.Primitive(PrimitiveType.Capsule, "Liane", transform, new Vector3(x, 1.4f, z), new Vector3(0.20f, 1.35f, 0.20f), lianeMaterial, false);
                    liane.transform.localRotation = Quaternion.Euler(18f, 0f, side * 24f);
                    _lianeDecor.Add(liane);

                    GameObject crystal = KmkVisuals.Primitive(PrimitiveType.Cube, "Winter Crystal", transform, new Vector3(x, 0.75f, z), new Vector3(0.46f, 1.55f, 0.46f), winterMaterial, false);
                    crystal.transform.localRotation = Quaternion.Euler(0f, 45f, 18f);
                    _winterDecor.Add(crystal);

                    GameObject copper = KmkVisuals.Primitive(PrimitiveType.Cylinder, "Copper Totem", transform, new Vector3(x, 1.0f, z), new Vector3(0.42f, 1.0f, 0.42f), copperMaterial, false);
                    _copperDecor.Add(copper);
                }
            }
        }

        private void BuildGameplaySlots()
        {
            for (int i = 0; i < _collectibles.Length; i++)
            {
                GameObject go = new GameObject("Essence Slot " + i);
                go.transform.SetParent(transform, false);
                EssenceCollectible collectible = go.AddComponent<EssenceCollectible>();
                collectible.Initialize(_game);
                go.SetActive(false);
                _collectibles[i] = collectible;
            }

            for (int i = 0; i < _hazards.Length; i++)
            {
                GameObject go = new GameObject("Hazard Slot " + i);
                go.transform.SetParent(transform, false);
                KmkHazard hazard = go.AddComponent<KmkHazard>();
                hazard.Initialize(_game);
                go.SetActive(false);
                _hazards[i] = hazard;
            }
        }

        public void Configure(float startZ, int segmentNumber, bool safe, ThemePalette palette)
        {
            StartZ = startZ;
            transform.position = new Vector3(0f, 0f, startZ);
            ApplyTheme(palette);

            for (int i = 0; i < _collectibles.Length; i++)
            {
                _collectibles[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < _hazards.Length; i++)
            {
                _hazards[i].gameObject.SetActive(false);
            }

            System.Random random = new System.Random(segmentNumber * 1471 + 311);
            if (safe)
            {
                ConfigureSafePath(random);
                return;
            }

            int pattern = Math.Abs(segmentNumber) % 5;
            switch (pattern)
            {
                case 0:
                    ConfigureSingleBlock(random);
                    break;
                case 1:
                    ConfigureDoubleBlock(random);
                    break;
                case 2:
                    ConfigureJumpMoment(random);
                    break;
                case 3:
                    ConfigureSlideMoment(random);
                    break;
                default:
                    ConfigureZigZag(random);
                    break;
            }
        }

        private void ConfigureSafePath(System.Random random)
        {
            int lane = random.Next(0, 3);
            int slot = 0;
            for (int i = 0; i < 12; i++)
            {
                if (i == 4 || i == 8)
                {
                    lane = Mathf.Clamp(lane + (random.NextDouble() > 0.5 ? 1 : -1), 0, 2);
                }
                SetCollectible(slot++, lane, 4f + i * 2.2f, 1.0f);
            }
        }

        private void ConfigureSingleBlock(System.Random random)
        {
            int blocked = random.Next(0, 3);
            int safe = (blocked + 1 + random.Next(0, 2)) % 3;
            SetHazard(0, HazardKind.Blocker, blocked, 18f);
            SpawnCoinLine(safe, 7f, 8, 2.5f, 1.0f, 0);
        }

        private void ConfigureDoubleBlock(System.Random random)
        {
            int safe = random.Next(0, 3);
            int hazardSlot = 0;
            for (int lane = 0; lane < 3; lane++)
            {
                if (lane != safe)
                {
                    SetHazard(hazardSlot++, HazardKind.Blocker, lane, 20f);
                }
            }
            SpawnCoinLine(safe, 6f, 10, 2.2f, 1.0f, 0);
        }

        private void ConfigureJumpMoment(System.Random random)
        {
            int lane = random.Next(0, 3);
            SetHazard(0, HazardKind.JumpBarrier, lane, 18f);
            SpawnCoinArc(lane, 9f, 9, 2.25f, 0);
            int openLane = (lane + 1) % 3;
            SpawnCoinLine(openLane, 9f, 7, 2.7f, 1.0f, 10);
        }

        private void ConfigureSlideMoment(System.Random random)
        {
            int lane = random.Next(0, 3);
            SetHazard(0, HazardKind.SlideGate, lane, 18f);
            SpawnCoinLine(lane, 8f, 10, 2.05f, 0.55f, 0);
            int openLane = (lane + 2) % 3;
            SpawnCoinLine(openLane, 9f, 7, 2.7f, 1.0f, 11);
        }

        private void ConfigureZigZag(System.Random random)
        {
            int lane = random.Next(0, 3);
            int slot = 0;
            for (int group = 0; group < 4; group++)
            {
                for (int i = 0; i < 4; i++)
                {
                    SetCollectible(slot++, lane, 4f + group * 7.2f + i * 1.35f, 1.0f);
                }
                lane = (lane + (group % 2 == 0 ? 1 : 2)) % 3;
            }
        }

        private void SpawnCoinLine(int lane, float startZ, int count, float spacing, float y, int startSlot)
        {
            for (int i = 0; i < count && startSlot + i < _collectibles.Length; i++)
            {
                SetCollectible(startSlot + i, lane, startZ + i * spacing, y);
            }
        }

        private void SpawnCoinArc(int lane, float startZ, int count, float spacing, int startSlot)
        {
            for (int i = 0; i < count && startSlot + i < _collectibles.Length; i++)
            {
                float t = count <= 1 ? 0f : i / (float)(count - 1);
                float y = 0.9f + Mathf.Sin(t * Mathf.PI) * 1.75f;
                SetCollectible(startSlot + i, lane, startZ + i * spacing, y);
            }
        }

        private void SetCollectible(int slot, int lane, float z, float y)
        {
            if (slot < 0 || slot >= _collectibles.Length)
            {
                return;
            }

            EssenceCollectible collectible = _collectibles[slot];
            collectible.transform.localPosition = new Vector3(KmkConstants.LanePosition(lane), y, z);
            collectible.ResetCollectible();
            collectible.gameObject.SetActive(true);
        }

        private void SetHazard(int slot, HazardKind kind, int lane, float z)
        {
            if (slot < 0 || slot >= _hazards.Length)
            {
                return;
            }

            KmkHazard hazard = _hazards[slot];
            hazard.transform.localPosition = new Vector3(KmkConstants.LanePosition(lane), 0f, z);
            hazard.Configure(kind);
            hazard.gameObject.SetActive(true);
        }

        public void ApplyTheme(ThemePalette palette)
        {
            if (_roadRenderer != null)
            {
                _roadRenderer.sharedMaterial = KmkVisuals.Material(palette.Road, 0.05f, 0.24f, false);
            }
            if (_leftPavement != null)
            {
                _leftPavement.sharedMaterial = KmkVisuals.Material(palette.Pavement, 0.02f, 0.32f, false);
            }
            if (_rightPavement != null)
            {
                _rightPavement.sharedMaterial = KmkVisuals.Material(palette.Pavement, 0.02f, 0.32f, false);
            }

            for (int i = 0; i < _buildingRenderers.Count; i++)
            {
                _buildingRenderers[i].sharedMaterial = KmkVisuals.Material(i % 2 == 0 ? palette.BuildingA : palette.BuildingB, 0.02f, 0.28f, false);
            }
            for (int i = 0; i < _windowRenderers.Count; i++)
            {
                _windowRenderers[i].sharedMaterial = KmkVisuals.Material(palette.Emission, 0.05f, 0.78f, true);
            }
            for (int i = 0; i < _accentRenderers.Count; i++)
            {
                _accentRenderers[i].sharedMaterial = KmkVisuals.Material(palette.Accent, 0.68f, 0.78f, true);
            }

            bool liane = palette.Chapter == KmkChapter.LianeLibre;
            bool winter = palette.Chapter == KmkChapter.PalmeDHiver;
            bool copper = palette.Chapter == KmkChapter.RivageCuivre;
            SetActive(_lianeDecor, liane);
            SetActive(_winterDecor, winter);
            SetActive(_copperDecor, copper);

            if (_chapterSign != null)
            {
                _chapterSign.text = palette.DisplayName;
                _chapterSign.color = palette.Chapter == KmkChapter.PalmeDHiver ? new Color(0.04f, 0.07f, 0.09f) : Color.black;
            }
        }

        private static void SetActive(List<GameObject> objects, bool active)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].SetActive(active);
            }
        }
    }
}
