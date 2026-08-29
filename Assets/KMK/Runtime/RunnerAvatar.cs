using UnityEngine;

namespace KMK.EssenceRun
{
    public sealed class RunnerAvatar : MonoBehaviour
    {
        private Transform _visualRoot;
        private Transform _torso;
        private Transform _head;
        private Transform _leftArm;
        private Transform _rightArm;
        private Transform _leftLeg;
        private Transform _rightLeg;
        private Transform _leftForearm;
        private Transform _rightForearm;
        private Vector3 _visualVelocity;
        private float _phase;

        public void Build()
        {
            ThemePalette palette = ThemeLibrary.Get(KmkChapter.LianeLibre);
            Material skin = KmkVisuals.Material(new Color(0.39f, 0.20f, 0.115f), 0f, 0.42f, false);
            Material skinLight = KmkVisuals.Material(new Color(0.50f, 0.28f, 0.17f), 0f, 0.46f, false);
            Material hair = KmkVisuals.Material(new Color(0.018f, 0.015f, 0.014f), 0f, 0.25f, false);
            Material ivory = KmkVisuals.Material(new Color(0.84f, 0.80f, 0.70f), 0.12f, 0.55f, false);
            Material black = KmkVisuals.Material(new Color(0.025f, 0.024f, 0.022f), 0.05f, 0.25f, false);
            Material gold = KmkVisuals.Material(palette.Accent, 0.72f, 0.83f, true);
            Material sole = KmkVisuals.Material(new Color(0.12f, 0.10f, 0.085f), 0f, 0.2f, false);

            _visualRoot = new GameObject("Animated Body").transform;
            _visualRoot.SetParent(transform, false);

            Transform pelvis = KmkVisuals.Primitive(
                PrimitiveType.Capsule,
                "Pelvis",
                _visualRoot,
                new Vector3(0f, 0.92f, 0f),
                new Vector3(0.68f, 0.43f, 0.52f),
                black,
                false).transform;
            pelvis.localRotation = Quaternion.Euler(0f, 0f, 90f);

            _torso = new GameObject("Torso Pivot").transform;
            _torso.SetParent(_visualRoot, false);
            _torso.localPosition = new Vector3(0f, 1.35f, 0f);

            GameObject jacket = KmkVisuals.Primitive(
                PrimitiveType.Capsule,
                "Ivory KMK Jacket",
                _torso,
                new Vector3(0f, 0.22f, 0f),
                new Vector3(0.76f, 0.64f, 0.48f),
                ivory,
                false);
            jacket.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            KmkVisuals.Primitive(
                PrimitiveType.Cube,
                "Jacket Black Panel",
                _torso,
                new Vector3(0f, 0.23f, -0.36f),
                new Vector3(0.54f, 0.44f, 0.035f),
                black,
                false);

            KmkVisuals.Text3D(
                "KMK Back Mark",
                "K",
                _torso,
                new Vector3(0f, 0.23f, -0.385f),
                new Vector3(0f, 180f, 0f),
                0.012f,
                palette.Accent,
                TextAnchor.MiddleCenter);

            KmkVisuals.Primitive(
                PrimitiveType.Cylinder,
                "Gold Collar",
                _torso,
                new Vector3(0f, 0.67f, 0f),
                new Vector3(0.23f, 0.035f, 0.23f),
                gold,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Cylinder,
                "Neck",
                _visualRoot,
                new Vector3(0f, 1.95f, 0f),
                new Vector3(0.25f, 0.18f, 0.25f),
                skin,
                false);

            _head = new GameObject("Head Pivot").transform;
            _head.SetParent(_visualRoot, false);
            _head.localPosition = new Vector3(0f, 2.25f, 0f);

            KmkVisuals.Primitive(
                PrimitiveType.Sphere,
                "Head",
                _head,
                Vector3.zero,
                new Vector3(0.58f, 0.65f, 0.56f),
                skinLight,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Sphere,
                "Short Hair",
                _head,
                new Vector3(0f, 0.25f, -0.015f),
                new Vector3(0.60f, 0.30f, 0.58f),
                hair,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Sphere,
                "Beard",
                _head,
                new Vector3(0f, -0.19f, 0.22f),
                new Vector3(0.48f, 0.25f, 0.20f),
                hair,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Sphere,
                "Left Eye",
                _head,
                new Vector3(-0.12f, 0.035f, 0.29f),
                new Vector3(0.055f, 0.045f, 0.035f),
                hair,
                false);
            KmkVisuals.Primitive(
                PrimitiveType.Sphere,
                "Right Eye",
                _head,
                new Vector3(0.12f, 0.035f, 0.29f),
                new Vector3(0.055f, 0.045f, 0.035f),
                hair,
                false);

            _leftArm = BuildArm("Left Arm", -0.57f, ivory, skin, gold);
            _rightArm = BuildArm("Right Arm", 0.57f, ivory, skin, gold);
            _leftForearm = _leftArm.Find("Forearm Pivot");
            _rightForearm = _rightArm.Find("Forearm Pivot");

            _leftLeg = BuildLeg("Left Leg", -0.25f, black, ivory, sole);
            _rightLeg = BuildLeg("Right Leg", 0.25f, black, ivory, sole);
        }

        private Transform BuildArm(string name, float x, Material jacket, Material skin, Material gold)
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(_visualRoot, false);
            pivot.localPosition = new Vector3(x, 1.72f, 0f);

            KmkVisuals.Primitive(
                PrimitiveType.Capsule,
                "Upper Arm",
                pivot,
                new Vector3(0f, -0.32f, 0f),
                new Vector3(0.25f, 0.42f, 0.25f),
                jacket,
                false);

            Transform forearm = new GameObject("Forearm Pivot").transform;
            forearm.SetParent(pivot, false);
            forearm.localPosition = new Vector3(0f, -0.64f, 0f);

            KmkVisuals.Primitive(
                PrimitiveType.Capsule,
                "Forearm",
                forearm,
                new Vector3(0f, -0.27f, 0f),
                new Vector3(0.21f, 0.36f, 0.21f),
                skin,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Sphere,
                "Hand",
                forearm,
                new Vector3(0f, -0.62f, 0f),
                new Vector3(0.24f, 0.27f, 0.22f),
                skin,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Cylinder,
                "Bracelet",
                forearm,
                new Vector3(0f, -0.48f, 0f),
                new Vector3(0.23f, 0.035f, 0.23f),
                gold,
                false);

            return pivot;
        }

        private Transform BuildLeg(string name, float x, Material trousers, Material shoe, Material sole)
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(_visualRoot, false);
            pivot.localPosition = new Vector3(x, 0.92f, 0f);

            KmkVisuals.Primitive(
                PrimitiveType.Capsule,
                "Thigh",
                pivot,
                new Vector3(0f, -0.39f, 0f),
                new Vector3(0.31f, 0.50f, 0.31f),
                trousers,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Capsule,
                "Shin",
                pivot,
                new Vector3(0f, -0.96f, 0.06f),
                new Vector3(0.26f, 0.43f, 0.26f),
                trousers,
                false);

            KmkVisuals.Primitive(
                PrimitiveType.Cube,
                "Trainer",
                pivot,
                new Vector3(0f, -1.36f, 0.18f),
                new Vector3(0.40f, 0.22f, 0.72f),
                shoe,
                false);
            KmkVisuals.Primitive(
                PrimitiveType.Cube,
                "Trainer Sole",
                pivot,
                new Vector3(0f, -1.48f, 0.20f),
                new Vector3(0.43f, 0.08f, 0.75f),
                sole,
                false);

            return pivot;
        }

        public void Tick(float normalizedSpeed, KmkGameState state, bool airborne, bool sliding)
        {
            if (_visualRoot == null)
            {
                return;
            }

            float runAmount = state == KmkGameState.Playing || state == KmkGameState.Countdown ? 1f : 0.18f;
            _phase += Time.deltaTime * Mathf.Lerp(2.2f, 10.8f, Mathf.Clamp01(normalizedSpeed + 0.15f)) * runAmount;
            float swing = Mathf.Sin(_phase);
            float opposite = Mathf.Sin(_phase + Mathf.PI);
            float stride = state == KmkGameState.Playing ? 38f : 7f;

            _leftArm.localRotation = Quaternion.Euler(opposite * stride, 0f, -5f);
            _rightArm.localRotation = Quaternion.Euler(swing * stride, 0f, 5f);
            _leftForearm.localRotation = Quaternion.Euler(-28f + Mathf.Max(0f, swing) * 18f, 0f, 0f);
            _rightForearm.localRotation = Quaternion.Euler(-28f + Mathf.Max(0f, opposite) * 18f, 0f, 0f);
            _leftLeg.localRotation = Quaternion.Euler(swing * stride, 0f, 0f);
            _rightLeg.localRotation = Quaternion.Euler(opposite * stride, 0f, 0f);

            float bob = state == KmkGameState.Playing ? Mathf.Abs(Mathf.Sin(_phase * 2f)) * 0.055f : Mathf.Sin(Time.time * 2f) * 0.018f;
            Vector3 desiredPosition = new Vector3(0f, bob, 0f);
            Vector3 desiredScale = Vector3.one;
            Quaternion desiredRotation = Quaternion.Euler(airborne ? -12f : 7f, 0f, 0f);

            if (sliding)
            {
                desiredPosition = new Vector3(0f, -0.38f, 0.34f);
                desiredScale = new Vector3(1f, 0.58f, 1f);
                desiredRotation = Quaternion.Euler(62f, 0f, 0f);
            }

            _visualRoot.localPosition = Vector3.SmoothDamp(_visualRoot.localPosition, desiredPosition, ref _visualVelocity, 0.06f);
            _visualRoot.localScale = Vector3.Lerp(_visualRoot.localScale, desiredScale, 1f - Mathf.Exp(-14f * Time.deltaTime));
            _visualRoot.localRotation = Quaternion.Slerp(_visualRoot.localRotation, desiredRotation, 1f - Mathf.Exp(-12f * Time.deltaTime));
            _torso.localRotation = Quaternion.Euler(0f, swing * 2.5f, opposite * 1.6f);
            _head.localRotation = Quaternion.Euler(-2f, -swing * 1.5f, 0f);
        }

        public void ResetPose()
        {
            _phase = 0f;
            _visualVelocity = Vector3.zero;
            if (_visualRoot != null)
            {
                _visualRoot.localPosition = Vector3.zero;
                _visualRoot.localScale = Vector3.one;
                _visualRoot.localRotation = Quaternion.identity;
            }
        }
    }
}
