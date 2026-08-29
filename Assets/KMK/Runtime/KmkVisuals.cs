using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KMK.EssenceRun
{
    public static class KmkVisuals
    {
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        public static Material Material(Color color, float metallic, float smoothness, bool emission)
        {
            Color32 c = color;
            string key = c.r + "_" + c.g + "_" + c.b + "_" + c.a + "_" + metallic.ToString("0.00") + "_" + smoothness.ToString("0.00") + "_" + emission;
            Material material;
            if (Materials.TryGetValue(key, out material) && material != null)
            {
                return material;
            }

            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            material = new Material(shader);
            material.name = "KMK_" + key;
            SetColor(material, color);

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (emission)
            {
                Color emissionColor = color * 2.3f;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", emissionColor);
                    material.EnableKeyword("_EMISSION");
                }
            }

            Materials[key] = material;
            return material;
        }

        public static Material Material(Color color)
        {
            return Material(color, 0f, 0.35f, false);
        }

        public static void SetColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        public static GameObject Primitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            if (!keepCollider)
            {
                Collider collider = go.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.Destroy(collider);
                }
            }

            return go;
        }

        public static TextMesh Text3D(
            string name,
            string value,
            Transform parent,
            Vector3 localPosition,
            Vector3 localEuler,
            float characterSize,
            Color color,
            TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localEulerAngles = localEuler;

            TextMesh text = go.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = anchor;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = characterSize;
            text.color = color;
            text.richText = false;

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return text;
        }

        public static ParticleSystem CreateAmbientParticles(Transform parent, Material material)
        {
            GameObject go = new GameObject("Essence Atmosphere");
            go.transform.SetParent(parent, false);
            ParticleSystem particles = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.07f);
            main.maxParticles = 180;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 20f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(12f, 7f, 28f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.z = new ParticleSystem.MinMaxCurve(-1.8f, -0.5f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            particles.Play();
            return particles;
        }
    }
}
