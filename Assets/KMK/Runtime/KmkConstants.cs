using UnityEngine;

namespace KMK.EssenceRun
{
    public static class KmkConstants
    {
        public const string ProductName = "KMK Paris — The Essence Run";
        public const string BundleIdentifier = "com.kmkparis.theessencerun";
        public const float LaneSpacing = 2.35f;
        public const float SegmentLength = 34f;
        public const int SegmentCount = 7;
        public const float ChapterDistance = 700f;
        public const float GroundY = 0f;

        public static readonly float[] LaneX =
        {
            -LaneSpacing,
            0f,
            LaneSpacing
        };

        public static float LanePosition(int lane)
        {
            return LaneX[Mathf.Clamp(lane, 0, LaneX.Length - 1)];
        }
    }
}
