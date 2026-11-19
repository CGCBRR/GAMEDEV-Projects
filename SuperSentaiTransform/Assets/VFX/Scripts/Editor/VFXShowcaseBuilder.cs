using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SuperSentaiVFX.EditorTools
{
    /// <summary>
    /// Builds the whole VFX showcase (textures, materials, prefabs, scene) from scratch.
    /// Menu: Tools > Super Sentai VFX > Build Showcase Scene
    /// </summary>
    public static partial class VFXShowcaseBuilder
    {
        const string RootDir = "Assets/VFX";
        const string TexDir = RootDir + "/Textures";
        const string MatDir = RootDir + "/Materials";
        const string PrefabDir = RootDir + "/Prefabs";
        const string MeshDir = RootDir + "/Meshes";
        const string ScenePath = "Assets/Scenes/VFX_Showcase.unity";

        // Effect anchor positions (world X).
        static readonly Vector3 HenshinPos = new Vector3(-11f, 0f, 0f);
        static readonly Vector3 BendingPos = new Vector3(0f, 0f, 0f);
        static readonly Vector3 BossPos = new Vector3(11f, 0f, 0f);

        // ---- Palette -------------------------------------------------------
        static readonly Color Pink = new Color(1f, 0.28f, 0.72f);
        static readonly Color Rose = new Color(1f, 0.45f, 0.85f);
        static readonly Color Magenta = new Color(0.95f, 0.12f, 0.85f);
        static readonly Color Gold = new Color(1f, 0.78f, 0.28f);
        static readonly Color WarmWhite = new Color(1f, 0.94f, 0.82f);
        static readonly Color Cyan = new Color(0.22f, 0.85f, 1f);
        static readonly Color Ice = new Color(0.62f, 0.85f, 1f);
        static readonly Color Fire = new Color(1f, 0.46f, 0.10f);
        static readonly Color FireDeep = new Color(1f, 0.15f, 0.02f);
        static readonly Color Earth = new Color(0.62f, 0.40f, 0.20f);
        static readonly Color Dust = new Color(0.86f, 0.68f, 0.42f);
        static readonly Color Wind = new Color(0.80f, 0.96f, 1f);
        static readonly Color Crimson = new Color(1f, 0.13f, 0.20f);
        static readonly Color Violet = new Color(0.62f, 0.16f, 1f);
        static readonly Color DeepViolet = new Color(0.28f, 0.05f, 0.62f);

        static Texture2D _disc, _glow, _star, _ring, _smoke, _flame, _flamePuff, _pillar, _streak, _petal, _magic;
        static Mesh _rockMesh;

        [MenuItem("Tools/Super Sentai VFX/Build Showcase Scene")]
        public static void BuildAll()
        {
            Debug.Log("[VFX] Building showcase...");
            EnsureFolders();
            BuildTextures();
            BuildMaterials();
            BuildRockMesh();
            BuildScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VFX] Done. Scene saved to " + ScenePath);
        }

        // ====================================================================
        //  Folders
        // ====================================================================
        static void EnsureFolders()
        {
            EnsureFolder(RootDir);
            EnsureFolder(TexDir);
            EnsureFolder(MatDir);
            EnsureFolder(PrefabDir);
            EnsureFolder(MeshDir);
            EnsureFolder("Assets/Scenes");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        // ====================================================================
        //  Procedural textures
        // ====================================================================
        static void BuildTextures()
        {
            _disc = WriteTexture("T_SoftDisc", 512, 512, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float r = Mathf.Sqrt(x * x + y * y);
                float a = Mathf.Pow(Mathf.Clamp01(1f - r), 1.7f);
                return new Color(1f, 1f, 1f, a);
            });

            _glow = WriteTexture("T_Glow", 512, 512, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float r2 = x * x + y * y;
                float a = Mathf.Exp(-r2 * 5.5f) + Mathf.Exp(-r2 * 40f) * 0.7f;
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            _star = WriteTexture("T_Star4", 512, 512, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float r = Mathf.Sqrt(x * x + y * y);
                float ang = Mathf.Atan2(y, x);
                float rays = Mathf.Pow(Mathf.Abs(Mathf.Cos(2f * ang)), 22f);
                float diag = Mathf.Pow(Mathf.Abs(Mathf.Cos(2f * ang + Mathf.PI * 0.25f)), 90f);
                float a = rays * Mathf.Pow(Mathf.Clamp01(1f - r), 2.1f) * 1.25f;
                a += diag * Mathf.Pow(Mathf.Clamp01(1f - r), 3f) * 0.7f;
                a += Mathf.Exp(-r * r * 46f);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            _ring = WriteTexture("T_Ring", 512, 512, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float r = Mathf.Sqrt(x * x + y * y);
                float band = Mathf.Clamp01(1f - Mathf.Abs(r - 0.75f) / 0.038f);
                float sharp = Mathf.Clamp01(1f - Mathf.Abs(r - 0.75f) / 0.012f);
                float haze = Mathf.Clamp01(1f - Mathf.Abs(r - 0.75f) / 0.16f) * 0.05f;
                float a = Mathf.Pow(band, 1.9f) * 0.5f + sharp * 1.0f + haze;
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            _smoke = WriteTexture("T_Smoke", 512, 512, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float r = Mathf.Sqrt(x * x + y * y);
                float n = Fbm(u * 4.2f + 7.3f, v * 4.2f + 2.1f, 5);
                float n2 = Fbm(u * 9.1f + 1.7f, v * 9.1f + 5.9f, 3);
                float a = Mathf.Clamp01((n * 0.75f + n2 * 0.45f - 0.28f) * 2.1f);
                a *= Mathf.Pow(Mathf.Clamp01(1f - r), 1.35f);
                return new Color(1f, 1f, 1f, a);
            });

            _flame = WriteTexture("T_Flame", 512, 512, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f;
                float t = v;
                float width = Mathf.Lerp(0.82f, 0.06f, Mathf.Pow(t, 0.72f));
                float edge = Mathf.Clamp01(1f - Mathf.Abs(x) / Mathf.Max(0.06f, width));
                float n = Fbm(x * 3.4f + 3.1f, t * 3.8f + 6.2f, 4);
                float a = Mathf.Pow(edge, 1.5f) * (0.55f + 0.7f * n) * Mathf.Pow(1f - t, 0.45f);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            _flamePuff = WriteTexture("T_FlamePuff", 256, 256, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float r = Mathf.Sqrt(x * x + y * y);
                float n = Fbm(u * 4.6f + 3.2f, v * 4.6f + 8.1f, 4);
                float n2 = Fbm(u * 9.2f + 0.7f, v * 9.2f + 4.4f, 3);
                float a = Mathf.Pow(Mathf.Clamp01(1f - r), 1.55f) * (0.4f + 0.85f * n + 0.3f * n2);
                a += Mathf.Exp(-r * r * 28f) * 0.55f;
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            // Bright at the bottom (v = 0), fading up (v = 1) with soft edges.
            _pillar = WriteTexture("T_Pillar", 512, 1024, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f;
                float edge = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x)), 2.6f);
                float vertical = Mathf.Pow(Mathf.Clamp01(1f - v), 1.35f);
                float hot = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x) * 3f), 2.4f) * Mathf.Pow(Mathf.Clamp01(1f - v * 1.45f), 1.2f);
                float a = edge * vertical * 0.62f + hot * 0.68f;
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            _streak = WriteTexture("T_Streak", 256, 256, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float a = Mathf.Exp(-(x * x * 90f + y * y * 3.2f));
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            _petal = WriteTexture("T_Petal", 256, 256, delegate (float u, float v)
            {
                float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
                float w = 0.52f * Mathf.Sqrt(Mathf.Clamp01(1f - Mathf.Abs(y * 0.92f))) + 0.035f;
                float d = Mathf.Abs(x) / w;
                float a = Mathf.Clamp01(1f - d * d * 0.92f) * Mathf.Clamp01(1f - Mathf.Abs(y) * 1.04f);
                float notch = 1f;
                if (y > 0.45f)
                    notch = Mathf.Clamp01(1f - Mathf.Abs(x) / 0.16f);
                a *= notch;
                return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            });

            BuildMagicCircle();
        }

        static void BuildMagicCircle()
        {
            const int size = 1024;
            var px = new Color[size * size];

            Vector2[] pent = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                float a = Mathf.PI * 0.5f + i * Mathf.PI * 2f / 5f;
                pent[i] = new Vector2(Mathf.Cos(a) * 0.56f, Mathf.Sin(a) * 0.56f);
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size, v = (y + 0.5f) / size;
                    float px0 = (u - 0.5f) * 2f, py0 = (v - 0.5f) * 2f;
                    float d = Mathf.Sqrt(px0 * px0 + py0 * py0);
                    float ang = Mathf.Atan2(py0, px0);
                    float alpha = 0f;

                    alpha = Mathf.Max(alpha, Circle(d, 0.985f, 0.008f));
                    alpha = Mathf.Max(alpha, Circle(d, 0.925f, 0.004f));
                    alpha = Mathf.Max(alpha, Circle(d, 0.66f, 0.004f));
                    alpha = Mathf.Max(alpha, Circle(d, 0.30f, 0.007f));
                    alpha = Mathf.Max(alpha, Circle(d, 0.155f, 0.012f));

                    // 48 fine radial ticks in the outer band.
                    float seg = Mathf.Repeat((ang + Mathf.PI) / (Mathf.PI * 2f) * 48f, 1f);
                    if (seg < 0.18f && d > 0.925f && d < 0.985f)
                        alpha = Mathf.Max(alpha, 0.85f);

                    // 12 long ticks.
                    float seg12 = Mathf.Repeat((ang + Mathf.PI) / (Mathf.PI * 2f) * 12f, 1f);
                    if (seg12 < 0.075f && d > 0.80f && d < 0.985f)
                        alpha = Mathf.Max(alpha, 1f);

                    // 12 glyph dots.
                    for (int k = 0; k < 12; k++)
                    {
                        float a2 = Mathf.PI * 2f * k / 12f + Mathf.PI / 12f;
                        float dx = px0 - Mathf.Cos(a2) * 0.78f;
                        float dy = py0 - Mathf.Sin(a2) * 0.78f;
                        float dd = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dd < 0.035f)
                            alpha = Mathf.Max(alpha, Mathf.Clamp01(1f - dd / 0.022f));
                    }

                    // Pentagram.
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 a1 = pent[i];
                        Vector2 a2 = pent[(i + 2) % 5];
                        float dist = SegDist(px0, py0, a1.x, a1.y, a2.x, a2.y);
                        alpha = Mathf.Max(alpha, Mathf.Clamp01(1f - dist / 0.008f));
                    }

                    // Soft magical glow hugging the inner rings.
                    float haze = Mathf.Exp(-Mathf.Pow((d - 0.30f) / 0.10f, 2f)) * 0.10f;
                    alpha = Mathf.Clamp01(alpha + haze + Mathf.Exp(-d * d * 20f) * 0.12f);

                    px[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            SavePng("T_MagicCircle", size, size, px);
            _magic = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/T_MagicCircle.png");
        }

        static float Circle(float d, float radius, float width)
        {
            return Mathf.Clamp01(1f - Mathf.Abs(d - radius) / width);
        }

        static float SegDist(float px, float py, float ax, float ay, float bx, float by)
        {
            float vx = bx - ax, vy = by - ay;
            float wx = px - ax, wy = py - ay;
            float c1 = vx * wx + vy * wy;
            float c2 = vx * vx + vy * vy;
            float t = c2 > 0.00001f ? Mathf.Clamp01(c1 / c2) : 0f;
            float dx = px - (ax + vx * t);
            float dy = py - (ay + vy * t);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        static float Hash(float x, float y)
        {
            float n = Mathf.Sin(x * 127.1f + y * 311.7f) * 43758.5453f;
            return n - Mathf.Floor(n);
        }

        static float VNoise(float x, float y)
        {
            float ix = Mathf.Floor(x), iy = Mathf.Floor(y);
            float fx = x - ix, fy = y - iy;
            float ux = fx * fx * (3f - 2f * fx);
            float uy = fy * fy * (3f - 2f * fy);
            float a = Hash(ix, iy);
            float b = Hash(ix + 1f, iy);
            float c = Hash(ix, iy + 1f);
            float d = Hash(ix + 1f, iy + 1f);
            return Mathf.Lerp(Mathf.Lerp(a, b, ux), Mathf.Lerp(c, d, ux), uy);
        }

        static float Fbm(float x, float y, int octaves)
        {
            float sum = 0f, amp = 0.5f, freq = 1f, norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += VNoise(x * freq, y * freq) * amp;
                norm += amp;
                freq *= 2.03f;
                amp *= 0.5f;
            }
            return sum / Mathf.Max(0.0001f, norm);
        }

        static Texture2D WriteTexture(string name, int width, int height, System.Func<float, float, Color> sampler)
        {
            var px = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    px[y * width + x] = sampler((x + 0.5f) / width, (y + 0.5f) / height);
                }
            }
            SavePng(name, width, height, px);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + name + ".png");
        }

        static void SavePng(string name, int width, int height, Color[] pixels)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            tex.SetPixels(pixels);
            tex.Apply();

            string path = TexDir + "/" + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Default;
                imp.alphaIsTransparency = true;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.mipmapEnabled = true;
                imp.filterMode = FilterMode.Bilinear;
                imp.maxTextureSize = 1024;
                imp.textureCompression = TextureImporterCompression.CompressedHQ;
                imp.SaveAndReimport();
            }
        }

        // ====================================================================
        //  Materials
        // ====================================================================
        static void BuildMaterials()
        {
            // Particle materials: white textures, tint comes from particle colours.
            MakeParticleMat("M_Add_Disc", _disc, Color.white, true);
            MakeParticleMat("M_Add_Glow", _glow, Color.white, true);
            MakeParticleMat("M_Add_Star", _star, Color.white, true);
            MakeParticleMat("M_Add_Ring", _ring, Color.white, true);
            MakeParticleMat("M_Add_Smoke", _smoke, Color.white, true);
            MakeParticleMat("M_Add_Flame", _flame, Color.white, true);
            MakeParticleMat("M_Add_FlamePuff", _flamePuff, Color.white, true);
            MakeParticleMat("M_Add_Pillar", _pillar, Color.white, true);
            MakeParticleMat("M_Add_Streak", _streak, Color.white, true);
            MakeParticleMat("M_Add_Petal", _petal, Color.white, true);
            MakeParticleMat("M_Add_Magic", _magic, Color.white, true);

            // Tinted cards (magic circles, light pillars, floor glows, rings).
            MakeParticleMat("M_Card_MagicPink", _magic, new Color(1.9f, 0.55f, 1.35f), true);
            MakeParticleMat("M_Card_MagicGold", _magic, new Color(2.0f, 1.5f, 0.55f), true);
            MakeParticleMat("M_Card_PillarGold", _pillar, new Color(1.9f, 1.35f, 0.52f), true);
            MakeParticleMat("M_Card_PillarPink", _pillar, new Color(1.7f, 0.55f, 1.2f), true);
            MakeParticleMat("M_Card_PillarCrimson", _pillar, new Color(1.5f, 0.22f, 0.30f), true);
            MakeParticleMat("M_Card_PillarViolet", _pillar, new Color(0.8f, 0.22f, 1.7f), true);
            MakeParticleMat("M_Card_PillarPinkSoft", _pillar, new Color(0.62f, 0.18f, 0.42f), true);
            MakeParticleMat("M_Card_PillarCrimsonSoft", _pillar, new Color(0.60f, 0.08f, 0.12f), true);
            MakeParticleMat("M_Card_PillarVioletSoft", _pillar, new Color(0.34f, 0.08f, 0.68f), true);
            MakeParticleMat("M_Card_PillarCyan", _pillar, new Color(0.18f, 0.75f, 1.55f), true);
            MakeParticleMat("M_Card_PillarWaterDeep", _pillar, new Color(0.10f, 0.50f, 1.30f), true);
            MakeParticleMat("M_Card_PillarAir", _pillar, new Color(0.62f, 0.58f, 1.25f), true);
            MakeParticleMat("M_Glow_Pink", _glow, new Color(1.0f, 0.22f, 0.55f), true);
            MakeParticleMat("M_Glow_Cyan", _glow, new Color(0.20f, 0.65f, 1.0f), true);
            MakeParticleMat("M_Glow_Crimson", _glow, new Color(1.0f, 0.12f, 0.18f), true);
            MakeParticleMat("M_Glow_White", _glow, new Color(1.0f, 0.92f, 1.0f), true);
            MakeParticleMat("M_Glow_Wind", _glow, new Color(0.60f, 0.56f, 1.0f), true);
            MakeParticleMat("M_GlowSoft_Pink", _glow, new Color(1.0f, 0.22f, 0.55f, 0.42f), true);
            MakeParticleMat("M_GlowSoft_Cyan", _glow, new Color(0.20f, 0.65f, 1.0f, 0.38f), true);
            MakeParticleMat("M_GlowSoft_Crimson", _glow, new Color(1.0f, 0.12f, 0.18f, 0.42f), true);
            MakeParticleMat("M_GlowSoft_White", _glow, new Color(0.85f, 0.92f, 1.0f, 0.34f), true);
            MakeParticleMat("M_GlowSoft_Fire", _glow, new Color(1.0f, 0.38f, 0.08f, 0.45f), true);
            MakeParticleMat("M_GlowSoft_Earth", _glow, new Color(0.95f, 0.58f, 0.22f, 0.35f), true);
            MakeParticleMat("M_Ring_Pink", _ring, new Color(1.8f, 0.45f, 1.0f), true);
            MakeParticleMat("M_Ring_Cyan", _ring, new Color(0.35f, 1.1f, 1.4f), true);
            MakeParticleMat("M_Ring_Crimson", _ring, new Color(1.6f, 0.20f, 0.28f), true);
            MakeParticleMat("M_Ring_Violet", _ring, new Color(0.9f, 0.30f, 1.6f), true);
            MakeParticleMat("M_Trail_White", _streak, Color.white, true);
            MakeParticleMat("M_Line_Lightning", _streak, new Color(1.2f, 0.65f, 2.0f), true);

            // Rock + metal surfaces.
            var rockMat = MakeLitMat("M_Rock", new Color(0.38f, 0.25f, 0.15f), 0.1f, 0f);
            rockMat.EnableKeyword("_EMISSION");
            rockMat.SetColor("_EmissionColor", new Color(0.20f, 0.085f, 0.028f));
            EditorUtility.SetDirty(rockMat);
            var shardMat = MakeLitMat("M_Shard", new Color(0.14f, 0.07f, 0.2f), 0.5f, 0.6f);
            shardMat.EnableKeyword("_EMISSION");
            shardMat.SetColor("_EmissionColor", new Color(0.18f, 0.03f, 0.32f));
            EditorUtility.SetDirty(shardMat);
            MakeLitMat("M_Floor", new Color(0.012f, 0.014f, 0.022f), 0.86f, 0.35f);
            MakeLitMat("M_Platform", new Color(0.020f, 0.022f, 0.033f), 0.92f, 0.55f);
        }

        static Material MakeParticleMat(string name, Texture2D tex, Color tint, bool additive)
        {
            string path = MatDir + "/" + name + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", additive ? 2f : 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetFloat("_Cutoff", 0.5f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            mat.SetFloat("_QueueOffset", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.DisableKeyword("_ALPHAMODULATE_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material MakeLitMat(string name, Color color, float smoothness, float metallic)
        {
            string path = MatDir + "/" + name + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material Mat(string name)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/" + name + ".mat");
            if (m == null) Debug.LogError("[VFX] Missing material " + name);
            return m;
        }

        // ====================================================================
        //  Rock mesh
        // ====================================================================
        static void BuildRockMesh()
        {
            float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            Vector3[] ico =
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            int[] faces =
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
                1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
                4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            };

            for (int i = 0; i < ico.Length; i++)
            {
                Vector3 n = ico[i].normalized;
                float noise = Hash(n.x * 4.7f + 3.1f, n.z * 5.3f + n.y * 2.9f + 8.2f);
                float radius = 0.82f + noise * 0.36f;
                ico[i] = n * radius;
                ico[i].y *= 0.82f;
            }

            var verts = new Vector3[faces.Length];
            var normals = new Vector3[faces.Length];
            var uvs = new Vector2[faces.Length];
            var tris = new int[faces.Length];

            for (int i = 0; i < faces.Length; i += 3)
            {
                Vector3 a = ico[faces[i]], b = ico[faces[i + 1]], c = ico[faces[i + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a).normalized;

                verts[i] = a; verts[i + 1] = b; verts[i + 2] = c;
                normals[i] = n; normals[i + 1] = n; normals[i + 2] = n;
                uvs[i] = new Vector2(0f, 0f);
                uvs[i + 1] = new Vector2(1f, 0f);
                uvs[i + 2] = new Vector2(0.5f, 1f);
                tris[i] = i; tris[i + 1] = i + 1; tris[i + 2] = i + 2;
            }

            var mesh = new Mesh { name = "RockLump" };
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();

            string path = MeshDir + "/RockLump.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            _rockMesh = mesh;
        }

        // ====================================================================
        //  Scene assembly
        // ====================================================================
        static void BuildScene()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.isDirty) EditorSceneManager.SaveScene(active);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupEnvironment();
            var cameraGo = BuildStage();

            var henshin = BuildHenshin(HenshinPos);
            var bending = BuildBending(BendingPos);
            var boss = BuildBoss(BossPos);

            SavePrefab(henshin, "01_HenshinTransformation");
            SavePrefab(bending, "02_ElementalBending");
            SavePrefab(boss, "03_FinalBossAura");

            Selection.activeGameObject = cameraGo;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                Debug.LogError("[VFX] Failed to save scene.");

            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                view.LookAt(new Vector3(0f, 3.6f, 0f), Quaternion.Euler(8f, 0f, 0f), 32f, false);
                view.Repaint();
            }
        }

        static void SavePrefab(GameObject root, string fileName)
        {
            string path = PrefabDir + "/" + fileName + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (prefab == null)
                Debug.LogError("[VFX] Failed to save prefab " + path);
        }

        static void SetupEnvironment()
        {
            // Deep void: no skybox, so the bloom and coloured lights own the frame.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.028f, 0.034f, 0.062f);
            RenderSettings.ambientEquatorColor = new Color(0.018f, 0.021f, 0.040f);
            RenderSettings.ambientGroundColor = new Color(0.006f, 0.007f, 0.014f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogColor = new Color(0.004f, 0.0055f, 0.012f);

            RenderSettings.reflectionIntensity = 0.2f;
        }

        static GameObject BuildStage()
        {
            // Floor.
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Stage Floor";
            Object.DestroyImmediate(floor.GetComponent<Collider>());
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(30f, 1f, 30f);
            var floorRenderer = floor.GetComponent<MeshRenderer>();
            floorRenderer.sharedMaterial = Mat("M_Floor");
            floorRenderer.shadowCastingMode = ShadowCastingMode.Off;
            floorRenderer.receiveShadows = false;

            // Three thin platforms to anchor the effects are intentionally omitted:
            // the floor decals sit directly on the stage plane so nothing z-fights.

            // Dim directional fill so the rock chunks read as solids.
            var key = new GameObject("Key Light (Dim Fill)");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(0.40f, 0.52f, 1f);
            keyLight.intensity = 0.32f;
            keyLight.shadows = LightShadows.None;
            key.transform.rotation = Quaternion.Euler(28f, -35f, 0f);
            RenderSettings.sun = keyLight;

            // Camera.
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 7.2f, -28f);
            camGo.transform.rotation = Quaternion.Euler(6.5f, 0f, 0f);

            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 46f;
            cam.nearClipPlane = 0.25f;
            cam.farClipPlane = 250f;
            cam.allowHDR = true;
            cam.allowMSAA = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.004f, 0.0055f, 0.012f);
            camGo.AddComponent<AudioListener>();

            var camData = camGo.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
            camData.renderShadows = true;

            BuildPostVolume();

            return camGo;
        }

        static void BuildPostVolume()
        {
            string profilePath = RootDir + "/VFX_Showcase_VolumeProfile.asset";
            AssetDatabase.DeleteAsset(profilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.84f);
            bloom.intensity.Override(0.92f);
            bloom.scatter.Override(0.66f);
            bloom.tint.Override(new Color(1f, 0.97f, 0.98f));
            bloom.highQualityFiltering.Override(true);

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            var colorAdjust = profile.Add<ColorAdjustments>(true);
            colorAdjust.postExposure.Override(0.05f);
            colorAdjust.contrast.Override(16f);
            colorAdjust.saturation.Override(8f);

            var vignette = profile.Add<Vignette>(true);
            vignette.color.Override(new Color(0f, 0f, 0.02f));
            vignette.intensity.Override(0.42f);
            vignette.smoothness.Override(0.55f);

            var chroma = profile.Add<ChromaticAberration>(true);
            chroma.intensity.Override(0.06f);

            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.12f);
            grain.response.Override(0.8f);

            var volumeGo = new GameObject("Global Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 1f;
            volume.profile = profile;
        }
    }
}
