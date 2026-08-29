using UnityEngine;

namespace KMK.EssenceRun
{
    public enum KmkChapter
    {
        LianeLibre = 0,
        PalmeDHiver = 1,
        RivageCuivre = 2
    }

    public struct ThemePalette
    {
        public KmkChapter Chapter;
        public string DisplayName;
        public string Subtitle;
        public Color Sky;
        public Color Fog;
        public Color Ambient;
        public Color Road;
        public Color Pavement;
        public Color BuildingA;
        public Color BuildingB;
        public Color Accent;
        public Color Secondary;
        public Color Emission;

        public ThemePalette(
            KmkChapter chapter,
            string displayName,
            string subtitle,
            Color sky,
            Color fog,
            Color ambient,
            Color road,
            Color pavement,
            Color buildingA,
            Color buildingB,
            Color accent,
            Color secondary,
            Color emission)
        {
            Chapter = chapter;
            DisplayName = displayName;
            Subtitle = subtitle;
            Sky = sky;
            Fog = fog;
            Ambient = ambient;
            Road = road;
            Pavement = pavement;
            BuildingA = buildingA;
            BuildingB = buildingB;
            Accent = accent;
            Secondary = secondary;
            Emission = emission;
        }
    }

    public static class ThemeLibrary
    {
        private static readonly ThemePalette[] Palettes =
        {
            new ThemePalette(
                KmkChapter.LianeLibre,
                "LIANE LIBRE",
                "L'ORIGINE EN MOUVEMENT",
                new Color(0.025f, 0.026f, 0.021f),
                new Color(0.075f, 0.071f, 0.050f),
                new Color(0.24f, 0.21f, 0.14f),
                new Color(0.055f, 0.054f, 0.045f),
                new Color(0.15f, 0.14f, 0.11f),
                new Color(0.10f, 0.11f, 0.085f),
                new Color(0.17f, 0.15f, 0.10f),
                new Color(0.78f, 0.64f, 0.34f),
                new Color(0.25f, 0.36f, 0.19f),
                new Color(1.00f, 0.77f, 0.34f)),
            new ThemePalette(
                KmkChapter.PalmeDHiver,
                "PALME D'HIVER",
                "LE FROID DEVIENT ÉCLAT",
                new Color(0.020f, 0.030f, 0.045f),
                new Color(0.070f, 0.100f, 0.125f),
                new Color(0.17f, 0.24f, 0.31f),
                new Color(0.045f, 0.055f, 0.065f),
                new Color(0.13f, 0.16f, 0.18f),
                new Color(0.08f, 0.12f, 0.16f),
                new Color(0.14f, 0.18f, 0.22f),
                new Color(0.69f, 0.82f, 0.88f),
                new Color(0.43f, 0.58f, 0.69f),
                new Color(0.58f, 0.90f, 1.00f)),
            new ThemePalette(
                KmkChapter.RivageCuivre,
                "RIVAGE CUIVRÉ",
                "LA VILLE PREND FEU",
                new Color(0.050f, 0.021f, 0.020f),
                new Color(0.125f, 0.065f, 0.050f),
                new Color(0.31f, 0.15f, 0.10f),
                new Color(0.065f, 0.042f, 0.038f),
                new Color(0.17f, 0.10f, 0.075f),
                new Color(0.16f, 0.075f, 0.055f),
                new Color(0.23f, 0.11f, 0.075f),
                new Color(0.76f, 0.37f, 0.18f),
                new Color(0.95f, 0.64f, 0.28f),
                new Color(1.00f, 0.39f, 0.15f))
        };

        public static ThemePalette Get(KmkChapter chapter)
        {
            return Palettes[Mathf.Clamp((int)chapter, 0, Palettes.Length - 1)];
        }

        public static KmkChapter ChapterForDistance(float distance)
        {
            int index = Mathf.FloorToInt(Mathf.Max(0f, distance) / KmkConstants.ChapterDistance) % Palettes.Length;
            return (KmkChapter)index;
        }
    }
}
