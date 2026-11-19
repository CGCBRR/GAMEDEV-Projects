using System.Collections.Generic;
using UnityEngine;

namespace SuperSentaiVFX.EditorTools
{
    public static partial class VFXShowcaseBuilder
    {
        // ====================================================================
        //  Effect 1 — Henshin
        // ====================================================================
        static GameObject BuildHenshin(Vector3 position)
        {
            var root = new GameObject("1 - Henshin Transformation");
            root.transform.position = position;

            // ---- Magic circles on the floor -------------------------------
            var circleOuter = MakeQuad("Magic Circle (Outer)", root.transform, new Vector3(0f, 0.035f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 7.6f, 7.6f, Mat("M_Card_MagicPink"));
            circleOuter.AddComponent<Spin>().degreesPerSecond = new Vector3(0f, 0f, 8f);

            var circleInner = MakeQuad("Magic Circle (Inner)", root.transform, new Vector3(0f, 0.045f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 5.2f, 5.2f, Mat("M_Card_MagicGold"));
            circleInner.AddComponent<Spin>().degreesPerSecond = new Vector3(0f, 0f, -15f);

            // ---- Floor light spill + arena ring ---------------------------
            var floorGlow = MakeQuad("Floor Glow", root.transform, new Vector3(0f, 0.02f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 13f, 13f, Mat("M_GlowSoft_Pink"));
            var floorGlowPulse = floorGlow.AddComponent<PulseScale>();
            floorGlowPulse.minScale = new Vector3(12.2f, 12.2f, 1f);
            floorGlowPulse.maxScale = new Vector3(13.8f, 13.8f, 1f);
            floorGlowPulse.cyclesPerSecond = 0.18f;

            var shockA = MakeQuad("Ground Shockwave A", root.transform, new Vector3(0f, 0.06f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Pink"));
            var shockARing = shockA.AddComponent<PulseRing>();
            shockARing.startScale = 1.2f;
            shockARing.endScale = 15f;
            shockARing.duration = 1.9f;
            shockARing.interval = 2.45f;
            shockARing.phase = 0f;
            shockARing.color = new Color(1f, 0.36f, 0.78f, 0.85f);

            var shockB = MakeQuad("Ground Shockwave B", root.transform, new Vector3(0f, 0.07f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Pink"));
            var shockBRing = shockB.AddComponent<PulseRing>();
            shockBRing.startScale = 0.8f;
            shockBRing.endScale = 11f;
            shockBRing.duration = 1.5f;
            shockBRing.interval = 2.45f;
            shockBRing.phase = 1.15f;
            shockBRing.color = new Color(1f, 0.78f, 0.35f, 0.7f);

            // ---- Light pillar --------------------------------------------
            var core = PulseCard("Light Pillar Core", root.transform, new Vector3(0f, 4.7f, 0f), Quaternion.identity,
                1.25f, 9.4f, Mat("M_Card_PillarGold"), 0.9f, 1.12f, 0.2f);
            var corePulse = core.AddComponent<PulseScale>();
            corePulse.minScale = new Vector3(1.12f, 9.1f, 1f);
            corePulse.maxScale = new Vector3(1.42f, 9.7f, 1f);
            corePulse.cyclesPerSecond = 0.45f;

            var halo = MakeQuad("Light Pillar Halo", root.transform, new Vector3(0f, 4.9f, 0.05f), Quaternion.identity,
                2.2f, 10f, Mat("M_Card_PillarPink"));
            var haloPulse = halo.AddComponent<PulseScale>();
            haloPulse.minScale = new Vector3(2.0f, 9.6f, 1f);
            haloPulse.maxScale = new Vector3(2.45f, 10.4f, 1f);
            haloPulse.cyclesPerSecond = 0.28f;

            // Soft volumetric haze around the beam (particles, not a hard card).
            var beamHaze = NewPS("Pillar Haze", root.transform, new Vector3(0f, 0.6f, 0f));
            ConfigureMain(beamHaze, 0.9f, 1.6f, 0.5f, 1.6f, 2.4f, 4.2f, Color.white, 60);
            var beamHazeMain = beamHaze.main;
            beamHazeMain.simulationSpace = ParticleSystemSimulationSpace.World;
            Emission(beamHaze, 11f);
            ShapeCylinder(beamHaze, 1.25f, 0.4f);
            Velocity(beamHaze, 0f, 2.2f, 0f);
            Orbital(beamHaze, 18f, 0f);
            Noise(beamHaze, 0.5f, 0.6f, 0.35f);
            ColorOverLife(beamHaze, LifeGradient(new Color(1f, 0.55f, 0.9f), new Color(1f, 0.35f, 0.78f), new Color(1f, 0.2f, 0.65f), 0.16f, 0f));
            RenderBillboard(beamHaze, Mat("M_Add_Smoke"));

            // ---- Crown glow (removed; beam fades naturally) ---------------

            // ---- Spiral sparkle ribbons -----------------------------------
            HenshinSpiral(root.transform, "Spiral Rose", new Color(1f, 0.35f, 0.82f), 2.35f, 7.2f, 1.6f, 0f, 62f, 0.115f);
            HenshinSpiral(root.transform, "Spiral Gold", new Color(1f, 0.78f, 0.35f), 1.9f, 8.1f, 2.2f, 1.1f, -44f, 0.1f);
            HenshinSpiral(root.transform, "Spiral Cyan", new Color(0.35f, 0.85f, 1f), 2.7f, 6.3f, 1.25f, 2.2f, 78f, 0.09f);

            HenshinRibbon(root.transform, "Ribbon Rose", new Color(1f, 0.32f, 0.75f), 2.35f, 92f, 48f, 0f);
            HenshinRibbon(root.transform, "Ribbon Gold", new Color(1f, 0.78f, 0.32f), 1.95f, 44f, 32f, 0.6f);
            HenshinRibbon(root.transform, "Ribbon Cyan", new Color(0.3f, 0.85f, 1f), 2.7f, 128f, 24f, 1.2f);

            // ---- Rising sparks --------------------------------------------
            var sparks = NewPS("Rising Sparkles", root.transform, new Vector3(0f, 0.4f, 0f));
            ConfigureMain(sparks, 1.1f, 2.3f, 0.3f, 1.3f, 0.07f, 0.22f, Color.white, 600);
            Emission(sparks, 18f);
            ShapeCylinder(sparks, 2.9f, 0.2f);
            Velocity(sparks, 0f, 2.4f, 0f);
            Orbital(sparks, 55f, 0f);
            Noise(sparks, 0.55f, 0.7f, 0.45f);
            ColorOverLife(sparks, LifeGradient(new Color(1f, 0.95f, 0.75f), new Color(1f, 0.7f, 0.35f), new Color(1f, 0.28f, 0.7f)));
            SizeOverLife(sparks, Curve(0.35f, 1f, 0.1f));
            RenderBillboard(sparks, Mat("M_Add_Star"));

            // ---- Star glints ---------------------------------------------
            var glints = NewPS("Star Glints", root.transform, new Vector3(0f, 3.4f, 0f));
            ConfigureMain(glints, 0.7f, 1.4f, 0f, 0.25f, 0.1f, 0.3f, Color.white, 400);
            Emission(glints, 7f);
            ShapeBox(glints, new Vector3(6f, 6.2f, 3f));
            RotateOverLife(glints, -30f, 30f);
            ColorOverLife(glints, SparkleGradient(new Color(1f, 0.98f, 0.85f), new Color(1f, 0.65f, 0.9f)));
            RenderBillboard(glints, Mat("M_Add_Star"));

            // ---- Air pulse rings -----------------------------------------
            PulseRings(root.transform, "Halo Rings Low", new Vector3(0f, 1.4f, 0f), new Color(1f, 0.55f, 0.9f), 3.4f, 1.35f, 1.22f, 0f);
            PulseRings(root.transform, "Halo Rings High", new Vector3(0f, 4.6f, 0f), new Color(1f, 0.85f, 0.5f), 4.6f, 1.5f, 1.22f, 0.61f);

            // ---- Petal burst ---------------------------------------------
            var petals = NewPS("Petal Burst", root.transform, new Vector3(0f, 1.6f, 0f));
            ConfigureMain(petals, 2.4f, 4.2f, 2.2f, 6.5f, 0.16f, 0.36f, Color.white, 300);
            Gravity(petals, 0.35f);
            ShapeSphere(petals, 0.8f);
            Bursts(petals, new[] { 2.1f }, 64);
            RotateOverLife(petals, -140f, 140f);
            ColorOverLife(petals, LifeGradient(new Color(1f, 0.82f, 0.95f), new Color(1f, 0.42f, 0.78f), new Color(0.95f, 0.2f, 0.55f)));
            RenderBillboard(petals, Mat("M_Add_Petal"));

            // ---- Orbiting motes ------------------------------------------
            var motes = NewPS("Orbiting Motes", root.transform, new Vector3(0f, 0.3f, 0f));
            ConfigureMain(motes, 2.2f, 3.4f, 0.1f, 0.5f, 0.04f, 0.12f, Color.white, 300);
            Emission(motes, 12f);
            ShapeCircle(motes, 3.1f, 0.35f);
            Velocity(motes, 0f, 0.55f, 0f);
            Orbital(motes, 150f, 0f);
            ColorOverLife(motes, Fade(new Color(1f, 0.7f, 0.95f), new Color(0.6f, 0.85f, 1f)));
            RenderStretch(motes, Mat("M_Add_Streak"), 0.01f, 0.8f);

            // ---- Lights ---------------------------------------------------
            AddLight(root.transform, "Pink Light", new Vector3(0f, 2.4f, 0f), new Color(1f, 0.42f, 0.78f), 5f, 14f, 0.9f, 0f);
            AddLight(root.transform, "Gold Light", new Vector3(0f, 6f, 0f), new Color(1f, 0.8f, 0.45f), 3f, 11f, 1.3f, 2f);

            return root;
        }

        static void HenshinSpiral(Transform parent, string name, Color color, float radius, float height,
            float turns, float phase, float spinSpeed, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.35f, 0f);

            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = Mat("M_Add_Streak");
            line.widthMultiplier = width;
            line.numCapVertices = 4;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.useWorldSpace = false;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.08f),
                    new GradientAlphaKey(0.85f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                });
            line.colorGradient = gradient;

            var spiral = go.AddComponent<SpiralRibbon>();
            spiral.radius = radius;
            spiral.height = height;
            spiral.turns = turns;
            spiral.phase = phase;

            var spin = go.AddComponent<Spin>();
            spin.degreesPerSecond = new Vector3(0f, spinSpeed, 0f);
        }

        static void HenshinRibbon(Transform parent, string name, Color color, float radius, float orbital, float rate, float phase)
        {
            var ps = NewPS(name, parent, new Vector3(0f, 0.25f, 0f));
            ConfigureMain(ps, 1.7f, 2.8f, 0.2f, 0.9f, 0.1f, 0.18f, Color.white, 450);
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startDelay = phase;
            Emission(ps, rate);
            ShapeCircle(ps, radius, 0.12f);
            Velocity(ps, 0f, 2.5f, 0f);
            Orbital(ps, orbital, 0f);
            Noise(ps, 0.9f, 0.5f, 0.6f);
            ColorOverLife(ps, Fade(color, Color.Lerp(color, Color.white, 0.45f)));
            SizeOverLife(ps, Curve(0.45f, 1f, 0.5f));
            RenderBillboard(ps, Mat("M_Add_Disc"));
            Trails(ps, Mat("M_Add_Streak"), 0.55f, 0.1f, TrailGradient(color));
        }

        // ====================================================================
        //  Effect 2 — Elemental bending
        // ====================================================================
        static GameObject BuildBending(Vector3 position)
        {
            var root = new GameObject("2 - Elemental Bending");
            root.transform.position = position;

            var firePos = new Vector3(-4.6f, 0f, -0.8f);
            var waterPos = new Vector3(4.9f, 0f, -0.8f);
            var earthPos = new Vector3(-3.6f, 0f, 3.4f);
            var airPos = new Vector3(2.4f, 0f, 3.4f);

            // ---- Shared base ---------------------------------------------
            var baseGlow = MakeQuad("Floor Glow", root.transform, new Vector3(0f, 0.02f, 0.4f),
                Quaternion.Euler(90f, 0f, 0f), 13f, 13f, Mat("M_GlowSoft_Cyan"));
            var basePulse = baseGlow.AddComponent<PulseScale>();
            basePulse.minScale = new Vector3(12.3f, 12.3f, 1f);
            basePulse.maxScale = new Vector3(13.7f, 13.7f, 1f);
            basePulse.cyclesPerSecond = 0.22f;

            var coreOuter = MakeQuad("Elemental Nexus (Outer)", root.transform, new Vector3(0f, 5.2f, 0.6f), Quaternion.identity,
                2.5f, 2.5f, Mat("M_Glow_Cyan"));
            var coreOuterPulse = coreOuter.AddComponent<PulseScale>();
            coreOuterPulse.minScale = new Vector3(2.15f, 2.15f, 1f);
            coreOuterPulse.maxScale = new Vector3(2.85f, 2.85f, 1f);
            coreOuterPulse.cyclesPerSecond = 0.55f;

            var nexusHalo = MakeQuad("Elemental Nexus (Halo)", root.transform, new Vector3(0f, 5.2f, 0.62f), Quaternion.identity,
                4.4f, 4.4f, Mat("M_GlowSoft_White"));
            var nexusHaloPulse = nexusHalo.AddComponent<PulseScale>();
            nexusHaloPulse.minScale = new Vector3(4.0f, 4.0f, 1f);
            nexusHaloPulse.maxScale = new Vector3(4.9f, 4.9f, 1f);
            nexusHaloPulse.cyclesPerSecond = 0.3f;

            MakeQuad("Elemental Nexus (Inner)", root.transform, new Vector3(0f, 5.2f, 0.58f), Quaternion.identity,
                0.9f, 0.9f, Mat("M_Glow_White"));

            // Four elemental orbs orbiting the nexus.
            var orbit = new GameObject("Elemental Orbit");
            orbit.transform.SetParent(root.transform, false);
            orbit.transform.localPosition = new Vector3(0f, 5.2f, 0.55f);
            orbit.AddComponent<Spin>().degreesPerSecond = new Vector3(0f, 0f, 34f);
            ElementalOrb(orbit.transform, "Fire Orb", -90f, 2.15f, new Color(1f, 0.5f, 0.12f));
            ElementalOrb(orbit.transform, "Water Orb", 0f, 2.15f, new Color(0.2f, 0.75f, 1f));
            ElementalOrb(orbit.transform, "Earth Orb", 90f, 2.15f, new Color(0.85f, 0.62f, 0.25f));
            ElementalOrb(orbit.transform, "Air Orb", 180f, 2.15f, new Color(0.72f, 0.68f, 1f));

            var coreRing = MakeQuad("Core Ring", root.transform, new Vector3(0f, 0.08f, 0.4f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Cyan"));
            var coreRingPulse = coreRing.AddComponent<PulseRing>();
            coreRingPulse.startScale = 1f;
            coreRingPulse.endScale = 11f;
            coreRingPulse.duration = 2.6f;
            coreRingPulse.interval = 2.6f;
            coreRingPulse.color = new Color(0.35f, 1f, 1.35f, 0.55f);

            PulseRings(root.transform, "Nexus Ring A", new Vector3(0f, 5.2f, 0.6f), new Color(0.5f, 0.95f, 1f), 5.2f, 1.5f, 1.6f, 0f);
            PulseRings(root.transform, "Nexus Ring B", new Vector3(0f, 5.2f, 0.6f), new Color(1f, 0.6f, 0.25f), 7.2f, 1.9f, 2.2f, 0.7f);

            AddLight(root.transform, "Nexus Light", new Vector3(0f, 5f, 0.4f), new Color(0.45f, 0.85f, 1f), 5.5f, 14f, 0.8f, 0.4f);

            // ---- Fire (front left) ---------------------------------------
            FloorGlow(root.transform, "Fire Glow", firePos + new Vector3(0f, 0.03f, 0f), 5.8f, Mat("M_GlowSoft_Fire"));

            var flame = NewPS("Fire - Flame", root.transform, firePos + new Vector3(0f, 0.3f, 0f));
            ConfigureMain(flame, 0.9f, 1.5f, 1.8f, 3.4f, 1.4f, 2.8f, Color.white, 600);
            Emission(flame, 46f);
            ShapeCone(flame, 20f, 0.5f);
            Velocity(flame, 0f, 1.2f, 0f);
            Noise(flame, 1.0f, 1.2f, 0.75f);
            ColorOverLife(flame, LifeGradient(new Color(1f, 0.95f, 0.7f), new Color(1f, 0.44f, 0.09f), new Color(0.62f, 0.05f, 0.015f), 0.62f));
            SizeOverLife(flame, Curve(0.85f, 1.05f, 0.15f));
            RenderBillboard(flame, Mat("M_Add_FlamePuff"));

            var flameTip = NewPS("Fire - Flame Tips", root.transform, firePos + new Vector3(0f, 1.2f, 0f));
            ConfigureMain(flameTip, 0.6f, 1f, 3.2f, 5.2f, 0.9f, 1.8f, Color.white, 350);
            Emission(flameTip, 38f);
            ShapeCone(flameTip, 12f, 0.35f);
            Velocity(flameTip, 0f, 1.1f, 0f);
            Noise(flameTip, 1.25f, 1.5f, 1f);
            ColorOverLife(flameTip, LifeGradient(new Color(1f, 0.98f, 0.68f), new Color(1f, 0.5f, 0.1f), new Color(1f, 0.16f, 0.03f), 0.5f));
            SizeOverLife(flameTip, Curve(0.75f, 0.9f, 0.08f));
            RenderBillboard(flameTip, Mat("M_Add_Flame"));

            var embers = NewPS("Fire - Embers", root.transform, firePos + new Vector3(0f, 0.5f, 0f));
            ConfigureMain(embers, 1.1f, 2.1f, 2.3f, 4.6f, 0.05f, 0.13f, Color.white, 300);
            Gravity(embers, -0.15f);
            Emission(embers, 34f);
            ShapeCone(embers, 24f, 0.35f);
            Noise(embers, 1.45f, 1.6f, 0.9f);
            ColorOverLife(embers, LifeGradient(new Color(1f, 0.92f, 0.45f), new Color(1f, 0.5f, 0.1f), new Color(1f, 0.12f, 0.02f)));
            RenderStretch(embers, Mat("M_Add_Streak"), 0.05f, 1.1f);

            AddLight(root.transform, "Fire Light", firePos + new Vector3(0f, 1.5f, 0f), new Color(1f, 0.42f, 0.1f), 10f, 11f, 1.6f, 1.1f);

            // ---- Water (front right) -------------------------------------
            FloorGlow(root.transform, "Water Glow", waterPos + new Vector3(0f, 0.03f, 0f), 7.6f, Mat("M_GlowSoft_Cyan"));

            var spout = MakeQuad("Water - Spout", root.transform, waterPos + new Vector3(0f, 1.9f, 0.02f), Quaternion.identity,
                1.5f, 3.8f, Mat("M_Card_PillarWaterDeep"));
            var spoutPulse = spout.AddComponent<PulseScale>();
            spoutPulse.minScale = new Vector3(1.3f, 3.45f, 1f);
            spoutPulse.maxScale = new Vector3(1.75f, 4.15f, 1f);
            spoutPulse.cyclesPerSecond = 0.4f;

            var stream = NewPS("Water - Rising Column", root.transform, waterPos + new Vector3(0f, 0.35f, 0f));
            ConfigureMain(stream, 1.2f, 2.1f, 1.4f, 2.4f, 0.16f, 0.32f, Color.white, 600);
            var streamMain = stream.main;
            streamMain.simulationSpace = ParticleSystemSimulationSpace.World;
            Emission(stream, 86f);
            ShapeCircle(stream, 0.55f, 0.25f);
            Velocity(stream, 0f, 1.7f, 0f);
            Orbital(stream, 165f, 0f);
            Noise(stream, 0.32f, 0.6f, 0.4f);
            ColorOverLife(stream, LifeGradient(new Color(0.9f, 1f, 1f), new Color(0.25f, 0.7f, 1f), new Color(0.08f, 0.3f, 0.95f), 0.65f));
            SizeOverLife(stream, Curve(0.55f, 1f, 0.35f));
            RenderBillboard(stream, Mat("M_Add_Disc"));
            Trails(stream, Mat("M_Add_Streak"), 0.75f, 0.14f, TrailGradient(new Color(0.45f, 0.9f, 1f)));

            var whip = NewPS("Water - Whip Arc", root.transform, waterPos + new Vector3(0f, 2f, 0f));
            ConfigureMain(whip, 1f, 1.7f, 0.2f, 0.5f, 0.08f, 0.16f, Color.white, 350);
            Emission(whip, 22f);
            ShapeCircle(whip, 0.7f, 0.2f);
            Velocity(whip, 0f, 0.25f, 0f);
            Orbital(whip, -170f, 0f);
            Noise(whip, 0.25f, 0.9f, 0.5f);
            ColorOverLife(whip, Fade(new Color(0.7f, 0.97f, 1f), new Color(0.15f, 0.55f, 1f)));
            SizeOverLife(whip, Curve(0.6f, 1f, 0.5f));
            RenderBillboard(whip, Mat("M_Add_Disc"));
            Trails(whip, Mat("M_Add_Streak"), 0.65f, 0.12f, TrailGradient(new Color(0.4f, 0.85f, 1f)));

            var droplets = NewPS("Water - Droplets", root.transform, waterPos + new Vector3(0f, 1.1f, 0f));
            ConfigureMain(droplets, 1.2f, 2f, 0.5f, 1.4f, 0.04f, 0.09f, Color.white, 300);
            Gravity(droplets, 0.5f);
            Emission(droplets, 18f);
            ShapeSphere(droplets, 0.3f);
            Orbital(droplets, 95f, 0f);
            ColorOverLife(droplets, Fade(new Color(0.75f, 0.95f, 1f), new Color(0.1f, 0.4f, 1f)));
            RenderStretch(droplets, Mat("M_Add_Streak"), 0.04f, 0.45f);

            var splash = MakeQuad("Water - Splash Ring", root.transform, waterPos + new Vector3(0f, 0.06f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Cyan"));
            var splashRing = splash.AddComponent<PulseRing>();
            splashRing.startScale = 0.7f;
            splashRing.endScale = 6.5f;
            splashRing.duration = 1.4f;
            splashRing.interval = 1.6f;
            splashRing.color = new Color(0.35f, 0.95f, 1.3f, 0.6f);

            AddLight(root.transform, "Water Light", waterPos + new Vector3(0f, 1.6f, 0f), new Color(0.22f, 0.65f, 1f), 8f, 10f, 0.7f, 0.7f);

            // ---- Earth (back left) ----------------------------------------
            FloorGlow(root.transform, "Earth Glow", earthPos + new Vector3(0f, 0.03f, 0f), 7.2f, Mat("M_GlowSoft_Earth"));

            // Static boulder pile anchors the element.
            MakeRock("Boulder A", root.transform, earthPos + new Vector3(-0.9f, 0.3f, 0f), new Vector3(1.4f, 1.15f, 1.3f), new Vector3(12f, 35f, 8f));
            MakeRock("Boulder B", root.transform, earthPos + new Vector3(0.7f, 0.26f, 0.35f), new Vector3(1.15f, 0.95f, 1.05f), new Vector3(-18f, 120f, 22f));
            MakeRock("Boulder C", root.transform, earthPos + new Vector3(0.15f, 0.8f, -0.2f), new Vector3(0.95f, 1.35f, 0.9f), new Vector3(8f, 210f, -14f));
            MakeRock("Boulder D", root.transform, earthPos + new Vector3(-0.35f, 0.22f, 0.85f), new Vector3(0.8f, 0.68f, 0.78f), new Vector3(24f, 300f, 31f));

            var rocks = NewPS("Earth - Rock Orbit", root.transform, earthPos + new Vector3(0f, 2.5f, 0f));
            ConfigureMain(rocks, 4f, 6f, 0f, 0.2f, 0.55f, 1.05f, Color.white, 50);
            Emission(rocks, 10f);
            ShapeCircle(rocks, 2.2f, 0.2f);
            Orbital(rocks, 32f, 0f);
            Random3DRotation(rocks, 0.4f, 1.8f);
            RenderMesh(rocks, Mat("M_Rock"), _rockMesh);

            var pebbles = NewPS("Earth - Pebbles", root.transform, earthPos + new Vector3(0f, 1.2f, 0f));
            ConfigureMain(pebbles, 2.5f, 4f, 0f, 0.15f, 0.12f, 0.26f, Color.white, 90);
            Emission(pebbles, 16f);
            ShapeCircle(pebbles, 1.5f, 0.2f);
            Orbital(pebbles, -70f, 0f);
            Random3DRotation(pebbles, 0.8f, 2.6f);
            RenderMesh(pebbles, Mat("M_Rock"), _rockMesh);

            var dust = NewPS("Earth - Dust", root.transform, earthPos + new Vector3(0f, 0.45f, 0f));
            ConfigureMain(dust, 2f, 3.4f, 0.15f, 0.6f, 1.1f, 2.1f, Color.white, 200);
            Emission(dust, 10f);
            ShapeCircle(dust, 2.2f, 0.5f);
            Velocity(dust, 0f, 0.35f, 0f);
            Orbital(dust, 22f, 0f);
            Noise(dust, 0.35f, 0.5f, 0.3f);
            ColorOverLife(dust, LifeGradient(new Color(0.62f, 0.45f, 0.26f), new Color(0.42f, 0.28f, 0.15f), new Color(0.2f, 0.12f, 0.06f), 0.24f));
            RenderBillboard(dust, Mat("M_Add_Smoke"));

            AddLight(root.transform, "Earth Light", earthPos + new Vector3(0f, 1.4f, 0f), new Color(1f, 0.55f, 0.22f), 9f, 10f, 0.55f, 2.2f);

            // ---- Air (back right) -----------------------------------------
            FloorGlow(root.transform, "Air Glow", airPos + new Vector3(0f, 0.03f, 0f), 7.2f, Mat("M_GlowSoft_White"));

            var funnel = MakeQuad("Air - Funnel", root.transform, airPos + new Vector3(0f, 2.6f, 0.02f), Quaternion.identity,
                2.2f, 5.4f, Mat("M_Card_PillarAir"));
            var funnelPulse = funnel.AddComponent<PulseScale>();
            funnelPulse.minScale = new Vector3(1.95f, 5f, 1f);
            funnelPulse.maxScale = new Vector3(2.5f, 5.85f, 1f);
            funnelPulse.cyclesPerSecond = 0.3f;

            var vortex = NewPS("Air - Vortex Outer", root.transform, airPos + new Vector3(0f, 0.35f, 0f));
            ConfigureMain(vortex, 1.4f, 2.4f, 0.6f, 1.2f, 0.14f, 0.26f, Color.white, 600);
            var vortexMain = vortex.main;
            vortexMain.simulationSpace = ParticleSystemSimulationSpace.World;
            Emission(vortex, 106f);
            ShapeCircle(vortex, 1.9f, 0.25f);
            Velocity(vortex, 0f, 1.3f, 0f);
            Orbital(vortex, 300f, 0f);
            Noise(vortex, 0.5f, 1.1f, 0.8f);
            ColorOverLife(vortex, Fade(new Color(0.88f, 0.9f, 1f), new Color(0.55f, 0.5f, 1f)));
            SizeOverLife(vortex, Curve(0.5f, 1f, 0.4f));
            RenderBillboard(vortex, Mat("M_Add_Disc"));
            Trails(vortex, Mat("M_Add_Streak"), 0.85f, 0.16f, TrailGradient(new Color(0.78f, 0.8f, 1f)));

            var inner = NewPS("Air - Vortex Inner", root.transform, airPos + new Vector3(0f, 0.35f, 0f));
            ConfigureMain(inner, 1.2f, 2f, 0.4f, 0.9f, 0.07f, 0.15f, Color.white, 400);
            var innerMain = inner.main;
            innerMain.simulationSpace = ParticleSystemSimulationSpace.World;
            Emission(inner, 48f);
            ShapeCircle(inner, 0.95f, 0.2f);
            Velocity(inner, 0f, 1.6f, 0f);
            Orbital(inner, -240f, 0f);
            Noise(inner, 0.35f, 0.9f, 0.6f);
            ColorOverLife(inner, Fade(new Color(0.95f, 0.97f, 1f), new Color(0.6f, 0.55f, 1f)));
            RenderBillboard(inner, Mat("M_Add_Disc"));
            Trails(inner, Mat("M_Add_Streak"), 0.6f, 0.08f, TrailGradient(new Color(0.85f, 0.85f, 1f)));

            var wisps = NewPS("Air - Wisps", root.transform, airPos + new Vector3(0f, 2f, 0f));
            ConfigureMain(wisps, 2.2f, 3.4f, 0f, 0.35f, 1.5f, 2.8f, Color.white, 160);
            Emission(wisps, 16f);
            ShapeSphere(wisps, 1.5f);
            Velocity(wisps, 0f, 0.4f, 0f);
            Orbital(wisps, 120f, 0f);
            Noise(wisps, 0.5f, 0.8f, 0.4f);
            ColorOverLife(wisps, LifeGradient(new Color(0.78f, 0.8f, 1f), new Color(0.52f, 0.5f, 1f), new Color(0.3f, 0.28f, 0.85f), 0.2f));
            RenderBillboard(wisps, Mat("M_Add_Smoke"));

            var gust = MakeQuad("Air - Gust Ring", root.transform, airPos + new Vector3(0f, 0.07f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Cyan"));
            var gustRing = gust.AddComponent<PulseRing>();
            gustRing.startScale = 0.6f;
            gustRing.endScale = 7.5f;
            gustRing.duration = 1.7f;
            gustRing.interval = 1.8f;
            gustRing.phase = 0.5f;
            gustRing.color = new Color(0.7f, 0.72f, 1.3f, 0.5f);

            AddLight(root.transform, "Air Light", airPos + new Vector3(0f, 1.8f, 0f), new Color(0.62f, 0.62f, 1f), 6f, 9f, 1.1f, 1.6f);

            // ---- Mixed elemental motes -----------------------------------
            var motes = NewPS("Elemental Motes", root.transform, new Vector3(0f, 3.2f, 0.4f));
            ConfigureMain(motes, 1.6f, 3f, 0.1f, 0.7f, 0.06f, 0.17f, Color.white, 300);
            Emission(motes, 20f);
            ShapeSphere(motes, 3.2f);
            Velocity(motes, 0f, 0.7f, 0f);
            Orbital(motes, 60f, 0f);
            Noise(motes, 0.6f, 0.7f, 0.5f);
            ColorOverLife(motes, Fade(new Color(1f, 0.9f, 0.6f), new Color(0.5f, 0.9f, 1f)));
            RenderBillboard(motes, Mat("M_Add_Star"));

            return root;
        }

        static void ElementalOrb(Transform parent, string name, float angleDeg, float radius, Color color)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            var orb = MakeQuad(name, parent, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f),
                Quaternion.identity, 0.9f, 0.9f, Mat("M_Glow_White"));
            var renderer = orb.GetComponent<MeshRenderer>();
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
        }

        static void MakeRock(string name, Transform parent, Vector3 localPos, Vector3 scale, Vector3 euler)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = _rockMesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = Mat("M_Rock");
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        // ====================================================================
        //  Effect 3 — Final boss aura
        // ====================================================================
        static GameObject BuildBoss(Vector3 position)
        {
            var root = new GameObject("3 - Final Boss Aura");
            root.transform.position = position;

            // ---- Ground energy -------------------------------------------
            var bossGlow = MakeQuad("Floor Glow", root.transform, new Vector3(0f, 0.02f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 12.5f, 12.5f, Mat("M_GlowSoft_Crimson"));
            var bossGlowPulse = bossGlow.AddComponent<PulseScale>();
            bossGlowPulse.minScale = new Vector3(11.6f, 11.6f, 1f);
            bossGlowPulse.maxScale = new Vector3(13.4f, 13.4f, 1f);
            bossGlowPulse.cyclesPerSecond = 0.16f;
            bossGlowPulse.phase = 0.2f;

            var waveA = MakeQuad("Ground Shockwave A", root.transform, new Vector3(0f, 0.06f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Crimson"));
            var waveARing = waveA.AddComponent<PulseRing>();
            waveARing.startScale = 1.6f;
            waveARing.endScale = 14f;
            waveARing.duration = 2.3f;
            waveARing.interval = 2.7f;
            waveARing.phase = 0f;
            waveARing.color = new Color(1f, 0.12f, 0.18f, 0.9f);

            var waveB = MakeQuad("Ground Shockwave B", root.transform, new Vector3(0f, 0.07f, 0f),
                Quaternion.Euler(90f, 0f, 0f), 1f, 1f, Mat("M_Ring_Violet"));
            var waveBRing = waveB.AddComponent<PulseRing>();
            waveBRing.startScale = 1f;
            waveBRing.endScale = 11f;
            waveBRing.duration = 1.8f;
            waveBRing.interval = 3.4f;
            waveBRing.phase = 1.4f;
            waveBRing.color = new Color(0.75f, 0.25f, 1.4f, 0.7f);

            // ---- Aura column ---------------------------------------------
            var core = MakeQuad("Aura Core", root.transform, new Vector3(0f, 5.75f, 0f), Quaternion.identity,
                1.5f, 11.2f, Mat("M_Card_PillarCrimsonSoft"));
            var corePulse = core.AddComponent<PulseScale>();
            corePulse.minScale = new Vector3(1.3f, 10.6f, 1f);
            corePulse.maxScale = new Vector3(1.75f, 11.8f, 1f);
            corePulse.cyclesPerSecond = 0.32f;

            var shell = MakeQuad("Aura Shell", root.transform, new Vector3(0f, 5.8f, 0.06f), Quaternion.identity,
                3.1f, 11.4f, Mat("M_Card_PillarVioletSoft"));
            var shellPulse = shell.AddComponent<PulseScale>();
            shellPulse.minScale = new Vector3(2.8f, 10.8f, 1f);
            shellPulse.maxScale = new Vector3(3.5f, 12f, 1f);
            shellPulse.cyclesPerSecond = 0.22f;

            // ---- Core orbs ----------------------------------------------
            var outerOrb = MakeQuad("Core Orb Outer", root.transform, new Vector3(0f, 4.6f, 0f), Quaternion.identity,
                2.7f, 2.7f, Mat("M_Glow_Crimson"));
            var outerOrbPulse = outerOrb.AddComponent<PulseScale>();
            outerOrbPulse.minScale = new Vector3(2.45f, 2.45f, 1f);
            outerOrbPulse.maxScale = new Vector3(3f, 3f, 1f);
            outerOrbPulse.cyclesPerSecond = 0.5f;
            outerOrbPulse.phase = 0.3f;
            var midOrb = MakeQuad("Core Orb Mid", root.transform, new Vector3(0f, 4.6f, 0.02f), Quaternion.identity,
                1.35f, 1.35f, Mat("M_Glow_White"));
            var midOrbPulse = midOrb.AddComponent<PulseScale>();
            midOrbPulse.minScale = new Vector3(1.2f, 1.2f, 1f);
            midOrbPulse.maxScale = new Vector3(1.55f, 1.55f, 1f);
            midOrbPulse.cyclesPerSecond = 0.8f;
            midOrbPulse.phase = 1.1f;
            var innerOrb = MakeQuad("Core Orb Inner", root.transform, new Vector3(0f, 4.6f, 0.03f), Quaternion.identity,
                0.65f, 0.65f, Mat("M_Glow_Crimson"));
            var innerOrbPulse = innerOrb.AddComponent<PulseScale>();
            innerOrbPulse.minScale = new Vector3(0.56f, 0.56f, 1f);
            innerOrbPulse.maxScale = new Vector3(0.78f, 0.78f, 1f);
            innerOrbPulse.cyclesPerSecond = 1.1f;

            // ---- Aura rings ---------------------------------------------
            var ringA = MakeQuad("Aura Ring A", root.transform, new Vector3(0f, 4.6f, 0f), Quaternion.identity,
                5.6f, 5.6f, Mat("M_Ring_Violet"));
            ringA.AddComponent<Spin>().degreesPerSecond = new Vector3(0f, 0f, 16f);
            var ringA_pulse = ringA.AddComponent<PulseScale>();
            ringA_pulse.minScale = new Vector3(5.3f, 5.3f, 1f);
            ringA_pulse.maxScale = new Vector3(6f, 6f, 1f);
            ringA_pulse.cyclesPerSecond = 0.3f;

            var ringB = MakeQuad("Aura Ring B", root.transform, new Vector3(0f, 4.6f, -0.02f), Quaternion.identity,
                7.2f, 7.2f, Mat("M_Ring_Crimson"));
            ringB.AddComponent<Spin>().degreesPerSecond = new Vector3(0f, 0f, -9f);
            var ringB_pulse = ringB.AddComponent<PulseScale>();
            ringB_pulse.minScale = new Vector3(6.9f, 6.9f, 1f);
            ringB_pulse.maxScale = new Vector3(7.7f, 7.7f, 1f);
            ringB_pulse.cyclesPerSecond = 0.18f;
            ringB_pulse.phase = 0.8f;

            // ---- Rising energy ------------------------------------------
            var rising = NewPS("Rising Energy", root.transform, new Vector3(0f, 0.2f, 0f));
            ConfigureMain(rising, 1.2f, 2.2f, 1f, 2.4f, 0.13f, 0.3f, Color.white, 420);
            var risingMain = rising.main;
            risingMain.simulationSpace = ParticleSystemSimulationSpace.World;
            Emission(rising, 34f);
            ShapeCylinder(rising, 1.9f, 0.2f);
            Velocity(rising, 0f, 3.1f, 0f);
            Orbital(rising, 95f, 0f);
            Noise(rising, 1.1f, 1.2f, 0.9f);
            ColorOverLife(rising, LifeGradient(new Color(1f, 0.35f, 0.3f), new Color(0.8f, 0.12f, 0.5f), new Color(0.45f, 0.1f, 1f)));
            SizeOverLife(rising, Curve(0.5f, 1f, 0.2f));
            RenderStretch(rising, Mat("M_Add_Streak"), 0.055f, 1.25f);
            Trails(rising, Mat("M_Add_Streak"), 0.32f, 0.11f, TrailGradient(new Color(1f, 0.25f, 0.4f)));

            // ---- Swirling embers ----------------------------------------
            var embers = NewPS("Aura Embers", root.transform, new Vector3(0f, 3.2f, 0f));
            ConfigureMain(embers, 2f, 3.6f, 0.1f, 0.6f, 0.08f, 0.26f, Color.white, 450);
            Emission(embers, 30f);
            ShapeSphere(embers, 2.7f);
            Velocity(embers, 0f, 0.85f, 0f);
            Orbital(embers, 115f, 0f);
            Noise(embers, 0.9f, 0.9f, 0.6f);
            ColorOverLife(embers, SparkleGradient(new Color(1f, 0.55f, 0.35f), new Color(1f, 0.2f, 0.45f)));
            RenderBillboard(embers, Mat("M_Add_Star"));

            // ---- Rune motes ---------------------------------------------
            var runes = NewPS("Rune Motes", root.transform, new Vector3(0f, 3.6f, 0f));
            ConfigureMain(runes, 4f, 6f, 0f, 0.2f, 0.28f, 0.62f, Color.white, 90);
            Emission(runes, 7f);
            ShapeCircle(runes, 3.3f, 0.1f);
            Velocity(runes, 0f, 0.5f, 0f);
            Orbital(runes, 46f, 0f);
            RotateOverLife(runes, -45f, 45f);
            ColorOverLife(runes, LifeGradient(new Color(1f, 0.45f, 0.75f), new Color(0.8f, 0.25f, 1f), new Color(0.55f, 0.12f, 1f), 0.55f));
            RenderBillboard(runes, Mat("M_Add_Star"));

            // ---- Smoke / aura haze --------------------------------------
            var haze = NewPS("Aura Haze", root.transform, new Vector3(0f, 1.1f, 0f));
            ConfigureMain(haze, 3.2f, 5.2f, 0.1f, 0.45f, 2.2f, 4.2f, Color.white, 90);
            Emission(haze, 7f);
            ShapeCylinder(haze, 2.1f, 0.4f);
            Velocity(haze, 0f, 0.7f, 0f);
            Orbital(haze, 25f, 0f);
            Noise(haze, 1.2f, 0.55f, 0.5f);
            ColorOverLife(haze, LifeGradient(new Color(0.45f, 0.06f, 0.9f), new Color(0.75f, 0.08f, 0.45f), new Color(0.18f, 0.02f, 0.45f), 0.14f));
            RenderBillboard(haze, Mat("M_Add_Smoke"));

            // ---- Aura veil (soft column volume) --------------------------
            var veil = NewPS("Aura Veil", root.transform, new Vector3(0f, 0.8f, 0f));
            ConfigureMain(veil, 1.2f, 2.2f, 0.7f, 1.6f, 1.8f, 3.4f, Color.white, 90);
            Emission(veil, 9f);
            ShapeCylinder(veil, 1.1f, 0.3f);
            Velocity(veil, 0f, 2.1f, 0f);
            Orbital(veil, 42f, 0f);
            Noise(veil, 0.85f, 0.7f, 0.55f);
            ColorOverLife(veil, LifeGradient(new Color(0.7f, 0.16f, 1f), new Color(0.9f, 0.1f, 0.5f), new Color(0.3f, 0.03f, 0.6f), 0.16f, 0f));
            RenderBillboard(veil, Mat("M_Add_Smoke"));

            // ---- Orbiting glow motes ------------------------------------
            var debris = NewPS("Orbiting Glow Motes", root.transform, new Vector3(0f, 2.8f, 0f));
            ConfigureMain(debris, 4f, 6f, 0f, 0.2f, 0.18f, 0.42f, Color.white, 70);
            Emission(debris, 8f);
            ShapeCircle(debris, 2.6f, 0.25f);
            Velocity(debris, 0f, 0.65f, 0f);
            Orbital(debris, 55f, 0f);
            Noise(debris, 0.7f, 0.8f, 0.5f);
            ColorOverLife(debris, SparkleGradient(new Color(1f, 0.55f, 0.7f), new Color(0.65f, 0.25f, 1f)));
            RenderBillboard(debris, Mat("M_Add_Star"));

            // ---- Lightning arcs -----------------------------------------
            for (int i = 0; i < 4; i++)
            {
                var arcGo = new GameObject("Lightning Arc " + (i + 1));
                arcGo.transform.SetParent(root.transform, false);

                var lr = arcGo.AddComponent<LineRenderer>();
                lr.sharedMaterial = Mat("M_Line_Lightning");
                lr.widthMultiplier = 0.06f;
                lr.numCapVertices = 2;
                lr.alignment = LineAlignment.View;
                lr.textureMode = LineTextureMode.Stretch;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.useWorldSpace = false;

                var arc = arcGo.AddComponent<LightningArc>();
                arc.segments = 30;
                arc.height = 10.5f;
                arc.startRadius = 2.15f;
                arc.endRadius = 0.7f;
                arc.drift = 0.85f;
                arc.twist = 0.65f;
                arc.visibleChance = 0.85f;
                arc.rebuildInterval = new Vector2(0.035f, 0.1f);
            }

            // ---- Explosive pulses ---------------------------------------
            var pulse = NewPS("Shockwave Pulses", root.transform, new Vector3(0f, 3.4f, 0f));
            ConfigureMain(pulse, 1.1f, 1.1f, 0f, 0f, 1f, 1f, Color.white, 40);
            Bursts(pulse, new[] { 0f, 1.5f, 3f, 4.5f }, 1);
            SizeOverLife(pulse, Curve(0.3f, 3.8f, 6.4f));
            ColorOverLife(pulse, LifeGradient(new Color(1f, 0.5f, 0.8f), new Color(0.9f, 0.2f, 0.9f), new Color(0.5f, 0.1f, 1f)));
            RenderBillboard(pulse, Mat("M_Add_Ring"));

            var burst = NewPS("Ember Bursts", root.transform, new Vector3(0f, 3.4f, 0f));
            ConfigureMain(burst, 1.4f, 2.6f, 3.5f, 8f, 0.06f, 0.18f, Color.white, 400);
            Gravity(burst, 0.65f);
            ShapeSphere(burst, 0.4f);
            Bursts(burst, new[] { 0.4f, 1.9f, 3.4f, 4.9f }, 26);
            Noise(burst, 0.9f, 1.2f, 0.7f);
            ColorOverLife(burst, LifeGradient(new Color(1f, 0.8f, 0.45f), new Color(1f, 0.25f, 0.2f), new Color(0.7f, 0.08f, 0.45f)));
            RenderStretch(burst, Mat("M_Add_Streak"), 0.04f, 1.1f);

            // ---- Lights ---------------------------------------------------
            AddLight(root.transform, "Crimson Light", new Vector3(0f, 2.4f, 0f), new Color(1f, 0.16f, 0.2f), 8f, 18f, 0.75f, 0f);
            AddLight(root.transform, "Violet Light", new Vector3(0f, 6.4f, 0f), new Color(0.62f, 0.2f, 1f), 6f, 17f, 1.05f, 1.7f);

            return root;
        }

        // ====================================================================
        //  Shared helpers
        // ====================================================================
        static GameObject MakeQuad(string name, Transform parent, Vector3 localPos, Quaternion localRot,
            float width, float height, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = new Vector3(width, height, 1f);

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        static GameObject PulseCard(string name, Transform parent, Vector3 localPos, Quaternion localRot,
            float width, float height, Material material, float minScale, float maxScale, float phase)
        {
            var go = MakeQuad(name, parent, localPos, localRot, width, height, material);
            return go;
        }

        static GameObject FloorGlow(Transform parent, string name, Vector3 localPos, float size, Material material)
        {
            return MakeQuad(name, parent, localPos, Quaternion.Euler(90f, 0f, 0f), size, size, material);
        }

        static void AddLight(Transform parent, string name, Vector3 localPos, Color color, float intensity, float range, float pulseSpeed, float phase)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;

            var pulse = go.AddComponent<LightPulse>();
            pulse.baseIntensity = intensity;
            pulse.amplitude = 0.5f;
            pulse.speed = pulseSpeed;
            pulse.phase = phase;
        }

        static void PulseRings(Transform parent, string name, Vector3 localPos, Color color, float endSize, float duration, float interval, float phase)
        {
            var ps = NewPS(name, parent, localPos);
            ConfigureMain(ps, duration, duration, 0f, 0f, 1f, 1f, Color.white, 24);
            var em = ps.emission;
            em.rateOverTime = 0f;
            var bursts = new List<ParticleSystem.Burst>();
            for (float t = 0f; t < 5f; t += interval)
                bursts.Add(new ParticleSystem.Burst(phase + t, (short)1));
            em.SetBursts(bursts.ToArray());

            ColourOverLife(ps, color, endSize, duration);
        }

        static void ColourOverLife(ParticleSystem ps, Color color, float endSize, float duration)
        {
            var main = ps.main;
            main.startSize = endSize;
            main.startSpeed = 0f;
            main.startLifetime = duration;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.08f), new Keyframe(0.65f, 0.55f), new Keyframe(1f, 1f)));

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.Lerp(color, Color.white, 0.35f), 0.4f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.35f, 0.55f), new GradientAlphaKey(0f, 1f) });
            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = Mat("M_Add_Ring");
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.None;
            renderer.maxParticleSize = 4f;
        }

        static AnimationCurve Curve(float start, float mid, float end)
        {
            return new AnimationCurve(new Keyframe(0f, start), new Keyframe(0.5f, mid), new Keyframe(1f, end));
        }

        static Gradient Fade(Color color)
        {
            return LifeGradient(color, Color.Lerp(color, Color.white, 0.3f), color);
        }

        static Gradient Fade(Color start, Color end)
        {
            return LifeGradient(start, Color.Lerp(start, end, 0.5f), end);
        }

        static Gradient LifeGradient(Color start, Color mid, Color end, float midAlpha = 1f, float endAlpha = 0f)
        {
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(mid, 0.55f),
                    new GradientColorKey(end, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(midAlpha, 0.45f),
                    new GradientAlphaKey(endAlpha, 1f)
                });
            return g;
        }

        static Gradient SparkleGradient(Color start, Color end)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(start, 0f), new GradientColorKey(Color.Lerp(start, end, 0.55f), 0.6f), new GradientColorKey(end, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0f, 1f) });
            return g;
        }

        static Gradient TrailGradient(Color color)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            return g;
        }

        static ParticleSystem NewPS(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = true;
            main.loop = true;
            main.prewarm = true;
            main.duration = 5f;
            main.startDelay = 0f;
            main.startLifetime = 1f;
            main.startSpeed = 0f;
            main.startSize = 0.2f;
            main.startColor = Color.white;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 500;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = 0f;

            var shape = ps.shape;
            shape.enabled = false;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.5f;
            shape.radiusThickness = 1f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = false;
            var force = ps.forceOverLifetime;
            force.enabled = false;
            var noise = ps.noise;
            noise.enabled = false;
            var col = ps.colorOverLifetime;
            col.enabled = false;
            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = false;
            var rot = ps.rotationOverLifetime;
            rot.enabled = false;
            var trails = ps.trails;
            trails.enabled = false;
            var sub = ps.subEmitters;
            sub.enabled = false;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.None;
            renderer.maxParticleSize = 4f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return ps;
        }

        static void ConfigureMain(ParticleSystem ps, float lifeMin, float lifeMax, float speedMin, float speedMax,
            float sizeMin, float sizeMax, Color color, int maxParticles)
        {
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = color;
            main.maxParticles = maxParticles;
        }

        static void Emission(ParticleSystem ps, float rate)
        {
            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = rate;
        }

        static void Bursts(ParticleSystem ps, float[] times, int countPerBurst)
        {
            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = 0f;
            var bursts = new ParticleSystem.Burst[times.Length];
            for (int i = 0; i < times.Length; i++)
                bursts[i] = new ParticleSystem.Burst(times[i], (short)countPerBurst);
            em.SetBursts(bursts);
        }

        static void ShapeCone(ParticleSystem ps, float angle, float radius)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = radius;
            shape.radiusThickness = 1f;
            shape.rotation = Vector3.zero;
        }

        static void ShapeCircle(ParticleSystem ps, float radius, float thickness)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.radiusThickness = thickness;
            shape.rotation = new Vector3(-90f, 0f, 0f);
        }

        static void ShapeSphere(ParticleSystem ps, float radius)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.radiusThickness = 1f;
        }

        static void ShapeBox(ParticleSystem ps, Vector3 size)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            shape.position = Vector3.zero;
        }

        static void ShapeCylinder(ParticleSystem ps, float radius, float height)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.radiusThickness = 1f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            shape.randomDirectionAmount = 0.15f;
        }

        static void Velocity(ParticleSystem ps, float x, float y, float z)
        {
            var v = ps.velocityOverLifetime;
            v.enabled = true;
            v.space = ParticleSystemSimulationSpace.Local;
            v.x = new ParticleSystem.MinMaxCurve(x);
            v.y = new ParticleSystem.MinMaxCurve(y);
            v.z = new ParticleSystem.MinMaxCurve(z);
        }

        static void Orbital(ParticleSystem ps, float orbitalY, float radial)
        {
            var v = ps.velocityOverLifetime;
            v.enabled = true;
            v.space = ParticleSystemSimulationSpace.Local;
            v.orbitalY = new ParticleSystem.MinMaxCurve(orbitalY);
            if (radial != 0f) v.radial = new ParticleSystem.MinMaxCurve(radial);
        }

        static void Gravity(ParticleSystem ps, float gravity)
        {
            var main = ps.main;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(gravity);
        }

        static void Noise(ParticleSystem ps, float strength, float frequency, float scroll)
        {
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(strength);
            noise.frequency = frequency;
            noise.scrollSpeed = new ParticleSystem.MinMaxCurve(scroll);
            noise.damping = true;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.Medium;
        }

        static void ColorOverLife(ParticleSystem ps, Gradient gradient)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        static void SizeOverLife(ParticleSystem ps, AnimationCurve curve)
        {
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        static void RotateOverLife(ParticleSystem ps, float minDeg, float maxDeg)
        {
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.separateAxes = false;
            rot.z = new ParticleSystem.MinMaxCurve(minDeg * Mathf.Deg2Rad, maxDeg * Mathf.Deg2Rad);
        }

        static void Random3DRotation(ParticleSystem ps, float spinMin, float spinMax)
        {
            var main = ps.main;
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.separateAxes = true;
            rot.x = new ParticleSystem.MinMaxCurve(spinMin * Mathf.Deg2Rad, spinMax * Mathf.Deg2Rad);
            rot.y = new ParticleSystem.MinMaxCurve(spinMin * Mathf.Deg2Rad, spinMax * Mathf.Deg2Rad);
            rot.z = new ParticleSystem.MinMaxCurve(spinMin * Mathf.Deg2Rad, spinMax * Mathf.Deg2Rad);
        }

        static void RenderBillboard(ParticleSystem ps, Material material)
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sharedMaterial = material;
            r.alignment = ParticleSystemRenderSpace.View;
            r.sortMode = ParticleSystemSortMode.None;
            r.maxParticleSize = 4f;
        }

        static void RenderStretch(ParticleSystem ps, Material material, float velocityScale, float lengthScale)
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.sharedMaterial = material;
            r.velocityScale = velocityScale;
            r.lengthScale = lengthScale;
            r.cameraVelocityScale = 0f;
            r.alignment = ParticleSystemRenderSpace.View;
            r.sortMode = ParticleSystemSortMode.None;
            r.maxParticleSize = 4f;
        }

        static void RenderMesh(ParticleSystem ps, Material material, Mesh mesh)
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Mesh;
            r.sharedMaterial = material;
            r.mesh = mesh;
            r.alignment = ParticleSystemRenderSpace.Local;
            r.sortMode = ParticleSystemSortMode.None;
            r.maxParticleSize = 4f;
        }

        static void Trails(ParticleSystem ps, Material material, float lifetime, float width, Gradient color)
        {
            var trails = ps.trails;
            trails.enabled = true;
            trails.mode = ParticleSystemTrailMode.Ribbon;
            trails.ratio = 1f;
            trails.lifetime = lifetime;
            trails.minVertexDistance = 0.05f;
            trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            trails.dieWithParticles = true;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(width);
            trails.colorOverTrail = new ParticleSystem.MinMaxGradient(color);
            trails.worldSpace = false;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.trailMaterial = material;
        }
    }
}
