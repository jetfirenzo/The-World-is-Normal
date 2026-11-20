using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Bouwt een blockout-supermarkt met een spelercamera (scherm 1) en een publiekscamera (scherm 2).
/// Menu: LifeLine/Bouw Supermarkt (demo). Opnieuw bouwen overschrijft de scene.
/// </summary>
public static class SupermarktBuilder
{
    const string ScenePath = "Assets/Supermarkt_Publiek.unity";
    const string MatDir = "Assets/LifeLine/Supermarkt/Materials";
    const string BuildDir = "Builds/Supermarkt_Publiek";
    const string ExeName = "LifeLine_Publiek.exe";

    static readonly Dictionary<string, Material> M = new();

    [MenuItem("LifeLine/Bouw Supermarkt (demo)")]
    public static void Build()
    {
        Materials();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);

        var sun = new GameObject("Directional Light").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.1f;
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Winkel: 20 x 16 m, 4 m hoog. Geen plafond, zodat de publiekscamera van boven naar binnen kijkt.
        var shop = new GameObject("Supermarkt").transform;
        Box("Vloer", shop, new Vector3(0, -0.05f, 0), new Vector3(20f, 0.1f, 16f), "Vloer");
        Box("Muur_Noord", shop, new Vector3(0, 2f, 8.05f), new Vector3(20.2f, 4f, 0.1f), "Muur");
        Box("Muur_Oost", shop, new Vector3(10.05f, 2f, 0), new Vector3(0.1f, 4f, 16f), "Muur");
        Box("Muur_West", shop, new Vector3(-10.05f, 2f, 0), new Vector3(0.1f, 4f, 16f), "Muur");
        Box("Muur_Zuid_L", shop, new Vector3(-6f, 2f, -8.05f), new Vector3(8.2f, 4f, 0.1f), "Muur");   // ingang in het midden
        Box("Muur_Zuid_R", shop, new Vector3(6f, 2f, -8.05f), new Vector3(8.2f, 4f, 0.1f), "Muur");
        Box("Muur_Zuid_Boven", shop, new Vector3(0, 3.25f, -8.05f), new Vector3(3.8f, 1.5f, 0.1f), "Muur");

        // 5 schappen met gekleurde producten.
        string[] kleuren = { "Rood", "Geel", "Blauw", "Groen", "Oranje", "Paars" };
        for (int s = 0; s < 5; s++)
        {
            float x = -7f + s * 3f;
            var schap = new GameObject($"Schap_{s + 1}").transform;
            schap.SetParent(shop, false);
            schap.localPosition = new Vector3(x, 0, 1.5f);
            Box("Kast", schap, new Vector3(0, 1f, 0), new Vector3(1f, 2f, 7f), "Schap");
            for (int plank = 0; plank < 4; plank++)
            {
                float y = 0.35f + plank * 0.45f;
                for (int side = -1; side <= 1; side += 2)
                    for (int i = 0; i < 12; i++)
                    {
                        var kleur = kleuren[(s + plank + i) % kleuren.Length];
                        var p = Box("Product", schap, new Vector3(side * 0.6f, y + 0.12f, -3.2f + i * 0.55f), new Vector3(0.2f, 0.24f, 0.35f), kleur);
                        Object.DestroyImmediate(p.GetComponent<Collider>());
                    }
            }
        }

        // Koelvak tegen de noordmuur.
        var koel = new GameObject("Koelvak").transform;
        koel.SetParent(shop, false);
        koel.localPosition = new Vector3(0, 0, 7.3f);
        Box("Kast", koel, new Vector3(0, 1f, 0), new Vector3(14f, 2f, 1.2f), "Koel");
        Box("Glas", koel, new Vector3(0, 1.1f, -0.62f), new Vector3(13.6f, 1.6f, 0.04f), "Glas");
        for (int i = 0; i < 20; i++)
        {
            var pak = Box("Melk", koel, new Vector3(-6.5f + i * 0.68f, 0.9f, -0.2f), new Vector3(0.25f, 0.45f, 0.25f), "Wit");
            Object.DestroyImmediate(pak.GetComponent<Collider>());
        }

        // 3 kassa's bij de ingang.
        for (int k = 0; k < 3; k++)
        {
            var kassa = new GameObject($"Kassa_{k + 1}").transform;
            kassa.SetParent(shop, false);
            kassa.localPosition = new Vector3(-5f + k * 2.5f, 0, -5f);
            Box("Band", kassa, new Vector3(0, 0.45f, 0), new Vector3(0.8f, 0.9f, 2.4f), "Kassa");
            Box("Scherm", kassa, new Vector3(0.25f, 1.15f, -0.9f), new Vector3(0.35f, 0.3f, 0.05f), "Zwart");
            Box("Lampje", kassa, new Vector3(0, 1.6f, 1.0f), new Vector3(0.15f, 0.4f, 0.15f), "Groen");
        }

        // Speler: CharacterController + WASD (SimpleWalker), staat net binnen de ingang.
        var speler = new GameObject("Speler");
        speler.transform.position = new Vector3(0f, 0f, -7f);
        var cc = speler.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0f, 0.9f, 0f);

        // Spelercamera: scherm 1, ooghoogte, met de AudioListener.
        var player = new GameObject("Main Camera") { tag = "MainCamera" };
        player.transform.SetParent(speler.transform, false);
        player.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        player.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        var pCam = player.AddComponent<Camera>();
        pCam.fieldOfView = 70f;
        pCam.nearClipPlane = 0.05f;
        pCam.targetDisplay = 0;
        player.AddComponent<AudioListener>();
        speler.AddComponent<SimpleWalker>().cameraTransform = player.transform;

        // Zichtbaar lijf, zodat het publiek op scherm 2 ziet waar de speler loopt.
        var lijf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lijf.name = "Lijf";
        Object.DestroyImmediate(lijf.GetComponent<Collider>());
        lijf.transform.SetParent(speler.transform, false);
        lijf.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        lijf.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
        lijf.GetComponent<Renderer>().sharedMaterial = M["Blauw"];
        lijf.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;

        // Publiekscamera: scherm 2, schuin van boven op het midden.
        var pub = new GameObject("PubliekCamera");
        pub.transform.position = new Vector3(-16f, 14f, -16f);
        pub.transform.LookAt(Vector3.zero);
        var aCam = pub.AddComponent<Camera>();
        aCam.fieldOfView = 55f;
        aCam.targetDisplay = 1;
        var ad = pub.AddComponent<AudienceDisplay>();
        ad.orbit = true;
        ad.orbitCenter = Vector3.zero;
        ad.orbitSpeed = 4f;

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        Debug.Log($"[LifeLine] Supermarkt gebouwd: {ScenePath}");
    }

    /// <summary>Windows build met alleen deze scene, zodat de build direct in de supermarkt start.</summary>
    [MenuItem("LifeLine/Build Supermarkt (Windows)")]
    public static void BuildWindows()
    {
        if (!File.Exists(ScenePath)) Build();
        Directory.CreateDirectory(BuildDir);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = Path.Combine(BuildDir, ExeName),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        // Het project staat op IL2CPP; daarvoor zijn Visual Studio C++ build tools nodig. Deze demo-build
        // gebruikt tijdelijk Mono (geen extra tools nodig) en zet de projectinstelling daarna terug.
        var target = UnityEditor.Build.NamedBuildTarget.Standalone;
        var oldBackend = PlayerSettings.GetScriptingBackend(target);
        PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.Mono2x);
        try
        {
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[LifeLine] Build {report.summary.result}: {report.summary.outputPath} ({report.summary.totalErrors} fouten, {report.summary.totalSize / (1024 * 1024)} MB)");
        }
        finally
        {
            PlayerSettings.SetScriptingBackend(target, oldBackend);
        }
    }

    static void AddToBuildSettings()
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (list.Exists(s => s.path == ScenePath)) return;
        list.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    // ---------------------------------------------------------------- materialen (URP of Built-in)

    static void Materials()
    {
        M.Clear();
        Mat("Vloer", new Color(0.75f, 0.75f, 0.72f));
        Mat("Muur", new Color(0.92f, 0.92f, 0.9f));
        Mat("Schap", new Color(0.45f, 0.47f, 0.5f));
        Mat("Koel", new Color(0.85f, 0.88f, 0.92f));
        Mat("Glas", new Color(0.6f, 0.8f, 1f));
        Mat("Kassa", new Color(0.25f, 0.25f, 0.28f));
        Mat("Zwart", new Color(0.05f, 0.05f, 0.05f));
        Mat("Wit", Color.white);
        Mat("Rood", new Color(0.85f, 0.15f, 0.15f));
        Mat("Geel", new Color(0.95f, 0.8f, 0.1f));
        Mat("Blauw", new Color(0.15f, 0.35f, 0.85f));
        Mat("Groen", new Color(0.2f, 0.7f, 0.25f));
        Mat("Oranje", new Color(0.95f, 0.5f, 0.1f));
        Mat("Paars", new Color(0.55f, 0.25f, 0.7f));
    }

    static void Mat(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder("Assets/LifeLine")) AssetDatabase.CreateFolder("Assets", "LifeLine");
        if (!AssetDatabase.IsValidFolder("Assets/LifeLine/Supermarkt")) AssetDatabase.CreateFolder("Assets/LifeLine", "Supermarkt");
        if (!AssetDatabase.IsValidFolder(MatDir)) AssetDatabase.CreateFolder("Assets/LifeLine/Supermarkt", "Materials");

        string path = $"{MatDir}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            bool urp = GraphicsSettings.currentRenderPipeline != null;
            var shader = Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor(mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color", color);
        EditorUtility.SetDirty(mat);
        M[name] = mat;
    }

    static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, string mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = M[mat];
        return go;
    }
}
