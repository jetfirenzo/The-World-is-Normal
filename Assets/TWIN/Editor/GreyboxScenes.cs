using TMPro;
using TWIN.Core;
using TWIN.Player;
using TWIN.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TWIN.EditorTools
{
    /// <summary>The per-scene greybox layouts. All positions are metres; player spawns face +Z.</summary>
    public static partial class GreyboxBuilder
    {
        static void BuildScenes()
        {
            Build_00_Boot();
            Build_01_Bedroom();
            Build_02_Bathroom();
            Build_03_LivingRoom();
            Build_04_Street();
            Build_05_Shop();
            Build_06A_FrontDoor();
            Build_06B_Backrooms();
            Build_07_LastMorning();
            Build_99_EndOfDemo();
            EditorSceneManager.OpenScene($"{ScenesDir}/{s_flow.boot}.unity");
        }

        // ------------------------------------------------------------------ generic helpers

        static Scene NewScene(bool withLight = true)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);
            if (withLight)
            {
                var light = new GameObject("Directional Light").AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
            new GameObject("SceneBootstrapper").AddComponent<SceneBootstrapper>();
            return scene;
        }

        static void Save(Scene scene, string name)
        {
            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/{name}.unity");
            Debug.Log($"[TWIN] Saved {name}");
        }

        static GameObject Spawn(Vector3 pos, float yaw = 0f)
        {
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(s_playerRigPrefab);
            rig.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            return rig;
        }

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, string mat, Vector3? euler = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            if (euler.HasValue) go.transform.localRotation = Quaternion.Euler(euler.Value);
            go.GetComponent<Renderer>().sharedMaterial = M[mat];
            return go;
        }

        static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 size, string mat, Vector3? euler = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            if (euler.HasValue) go.transform.localRotation = Quaternion.Euler(euler.Value);
            go.GetComponent<Renderer>().sharedMaterial = M[mat];
            return go;
        }

        static GameObject Group(string name, Transform parent = null, Vector3? pos = null, Vector3? euler = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            if (pos.HasValue) go.transform.localPosition = pos.Value;
            if (euler.HasValue) go.transform.localRotation = Quaternion.Euler(euler.Value);
            return go;
        }

        /// <summary>A closed room centred on (0, h/2, 0) under parent. Walls are 0.1 thick, inside the given size.</summary>
        static void Room(Transform parent, float w, float h, float d, string floor = "Grey_Floor", string wall = "Grey_Wall", string ceiling = "Grey_Ceiling", bool withCeiling = true)
        {
            const float t = 0.1f;
            Box("Floor", parent, new Vector3(0, -t / 2, 0), new Vector3(w + 2 * t, t, d + 2 * t), floor);
            if (withCeiling) Box("Ceiling", parent, new Vector3(0, h + t / 2, 0), new Vector3(w + 2 * t, t, d + 2 * t), ceiling);
            Box("Wall_North", parent, new Vector3(0, h / 2, d / 2 + t / 2), new Vector3(w + 2 * t, h, t), wall);
            Box("Wall_South", parent, new Vector3(0, h / 2, -d / 2 - t / 2), new Vector3(w + 2 * t, h, t), wall);
            Box("Wall_East", parent, new Vector3(w / 2 + t / 2, h / 2, 0), new Vector3(t, h, d), wall);
            Box("Wall_West", parent, new Vector3(-w / 2 - t / 2, h / 2, 0), new Vector3(t, h, d), wall);
        }

        static TextMeshPro Label3D(Transform parent, string text, Vector3 pos, Vector3 euler, Vector2 size, float fontSize, Color? color = null)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            // Counter the parent's (often non-uniform) scale so size and fontSize are in world metres.
            // The label may be rotated, so each label axis is divided by the parent scale of the parent axis it maps to
            // (exact for 90-degree rotations, which is all the builder uses).
            if (parent != null)
            {
                var ps = parent.lossyScale;
                var rot = Quaternion.Euler(euler);
                Vector3 Axis(Vector3 a) { var v = rot * a; return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)); }
                float Div(Vector3 w) => Mathf.Max(Vector3.Dot(w, ps), 1e-4f);
                go.transform.localScale = new Vector3(1f / Div(Axis(Vector3.right)), 1f / Div(Axis(Vector3.up)), 1f / Div(Axis(Vector3.forward)));
            }
            var t = go.AddComponent<TextMeshPro>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color ?? Color.black;
            t.rectTransform.sizeDelta = size;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        static Light PointLight(Transform parent, Vector3 pos, float intensity = 2f, float range = 8f, Color? color = null)
        {
            var go = new GameObject("Point Light");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.intensity = intensity; l.range = range; l.color = color ?? Color.white;
            return l;
        }

        /// <summary>A door slab with Interactable + Door. Faces +Z in local space (place it flush with a wall).</summary>
        static Door DoorObj(string name, Transform parent, Vector3 pos, Vector3 euler, string mat = "Wood", string targetScene = "")
        {
            var go = Box(name, parent, pos, new Vector3(1f, 2.1f, 0.12f), mat, euler);
            var i = go.AddComponent<Interactable>();
            var d = go.AddComponent<Door>();
            d.targetScene = targetScene;
            // TMP text (like a Quad) is readable from its local -Z side.
            Label3D(go.transform, "DOOR", new Vector3(0, 0.35f, -0.52f), Vector3.zero, new Vector2(0.9f, 0.3f), 1.5f);
            Label3D(go.transform, "DOOR", new Vector3(0, 0.35f, 0.52f), new Vector3(0, 180, 0), new Vector2(0.9f, 0.3f), 1.5f);
            // Handle
            var h = Box("Handle", go.transform, new Vector3(0.35f, 0f, 0f), new Vector3(0.08f, 0.03f, 1.6f), "Grey_Dark");
            Object.DestroyImmediate(h.GetComponent<Collider>());
            return d;
        }

        static Interactable Interact(GameObject go, string prompt)
        {
            var i = go.GetComponent<Interactable>() ?? go.AddComponent<Interactable>();
            i.prompt = prompt;
            return i;
        }

        // ------------------------------------------------------------------ bedroom (shared by 01, 06A, 07)

        struct BedroomRefs
        {
            public Transform root;
            public GameObject bed, alarm, nightstand, wardrobe, chair;
            public TextMeshPro clock;
            public Door door;
            public Renderer alarmRenderer;
            public AudioSource alarmAudio;
        }

        /// <summary>5 x 2.8 x 5 room. Door on the north wall. wrong = furniture halfway up the walls (scene 7).</summary>
        static BedroomRefs BuildBedroom(bool wrong, bool withDoor = true)
        {
            var r = new BedroomRefs();
            var root = Group("Bedroom");
            r.root = root.transform;
            Room(root.transform, 5f, 2.8f, 5f);
            PointLight(root.transform, new Vector3(0, 2.5f, 0), 1.5f, 9f, new Color(1f, 0.95f, 0.85f));

            var furniture = Group("Furniture", root.transform);
            r.bed = Box("Bed", furniture.transform, new Vector3(1.6f, 0.3f, 1.2f), new Vector3(1.4f, 0.6f, 2.1f), "White");
            Box("Pillow", r.bed.transform, new Vector3(0, 0.55f, 0.38f), new Vector3(0.6f, 0.15f, 0.18f), "Grey_Prop");
            Interact(r.bed, "Bed").available = false;
            r.nightstand = Box("Nightstand", furniture.transform, new Vector3(0.5f, 0.3f, 2.1f), new Vector3(0.5f, 0.6f, 0.5f), "Wood");
            r.wardrobe = Box("Wardrobe", furniture.transform, new Vector3(-2.0f, 1.0f, 1.5f), new Vector3(0.9f, 2.0f, 0.6f), "Wood");
            r.chair = Box("Chair", furniture.transform, new Vector3(-1.8f, 0.45f, -1.5f), new Vector3(0.5f, 0.9f, 0.5f), "Grey_Prop");
            Box("Rug", furniture.transform, new Vector3(0, 0.01f, -0.2f), new Vector3(2f, 0.02f, 1.5f), "Accent_Red");

            r.alarm = Box("AlarmClock", furniture.transform, new Vector3(0.5f, 0.72f, 2.1f), new Vector3(0.3f, 0.22f, 0.15f), "Accent_Red");
            r.alarmRenderer = r.alarm.GetComponent<Renderer>();
            r.alarmAudio = r.alarm.AddComponent<AudioSource>();
            r.alarmAudio.playOnAwake = false; r.alarmAudio.spatialBlend = 1f; r.alarmAudio.minDistance = 1f;
            Interact(r.alarm, "Turn off the alarm");
            r.clock = Label3D(r.alarm.transform, "07:00", new Vector3(0, 0, -0.51f), Vector3.zero, new Vector2(0.4f, 0.2f), 1.0f, Color.white);
            r.clock.fontStyle = FontStyles.Bold;
            r.clock.textWrappingMode = TextWrappingModes.NoWrap;

            var window = Box("Window", root.transform, new Vector3(2.44f, 1.6f, -0.5f), new Vector3(0.02f, 1.2f, 1.6f), "Accent_Blue");
            Object.DestroyImmediate(window.GetComponent<Collider>());

            if (withDoor)
                r.door = DoorObj("BedroomDoor", root.transform, new Vector3(-1.2f, 1.05f, 2.44f), Vector3.zero);

            if (wrong)
            {
                // Furniture has slid halfway up the walls.
                r.bed.transform.localPosition = new Vector3(2.0f, 1.5f, 1.2f);
                r.bed.transform.localRotation = Quaternion.Euler(0, 0, 55f);
                r.wardrobe.transform.localPosition = new Vector3(-2.1f, 1.9f, 1.5f);
                r.wardrobe.transform.localRotation = Quaternion.Euler(0, 0, -40f);
                r.chair.transform.localPosition = new Vector3(-1.9f, 1.6f, -1.5f);
                r.chair.transform.localRotation = Quaternion.Euler(30f, 0, -70f);
                r.nightstand.transform.localPosition = new Vector3(0.5f, 1.3f, 2.2f);
                r.nightstand.transform.localRotation = Quaternion.Euler(-50f, 0, 0);
                r.alarm.transform.localPosition = new Vector3(0.5f, 1.0f, 1.9f);
                r.alarm.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
                var rug = furniture.transform.Find("Rug");
                rug.localPosition = new Vector3(0, 1.2f, -2.4f);
                rug.localRotation = Quaternion.Euler(90f, 0, 0);
            }
            return r;
        }

        // ------------------------------------------------------------------ 00 boot

        static void Build_00_Boot()
        {
            var scene = NewScene(withLight: false);
            var cam = new GameObject("Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.gameObject.AddComponent<AudioListener>();
            var canvas = SceneCanvas("TitleCanvas");
            var title = UIText(canvas.transform, "Title", "", 72, TextAlignmentOptions.Center, Vector2.zero, new Vector2(1400, 500));
            var boot = new GameObject("S0_Boot").AddComponent<S0_Boot>();
            boot.title = title;
            Save(scene, s_flow.boot);
        }

        static Canvas SceneCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        // ------------------------------------------------------------------ 01 bedroom

        static void Build_01_Bedroom()
        {
            var scene = NewScene();
            var b = BuildBedroom(wrong: false);
            Spawn(new Vector3(0.3f, 0f, -0.8f), 20f);
            var s = new GameObject("S1_Bedroom").AddComponent<S1_Bedroom>();
            s.alarm = b.alarm.GetComponent<Interactable>();
            s.alarmRenderer = b.alarmRenderer;
            s.alarmAudio = b.alarmAudio;
            s.clockText = b.clock;
            s.door = b.door;
            Save(scene, s_flow.bedroom);
        }

        // ------------------------------------------------------------------ 02 bathroom

        static void Build_02_Bathroom()
        {
            var scene = NewScene();
            var root = Group("Bathroom").transform;
            Room(root, 4f, 2.8f, 4f, wall: "White");
            PointLight(root, new Vector3(0, 2.5f, 0), 1.5f, 8f);

            var sink = Box("Sink", root, new Vector3(0, 0.45f, 1.7f), new Vector3(1.0f, 0.9f, 0.55f), "Grey_Prop");
            var basin = Box("Basin", sink.transform, new Vector3(0, 0.45f, 0), new Vector3(0.6f, 0.15f, 0.6f), "White");
            Object.DestroyImmediate(basin.GetComponent<Collider>());
            var tap = Box("Tap", root, new Vector3(0, 1.0f, 1.9f), new Vector3(0.05f, 0.2f, 0.05f), "Grey_Dark");

            // Mirror on the north wall above the sink, facing -Z (into the room)
            var mirror = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mirror.name = "Mirror";
            mirror.transform.SetParent(root, false);
            mirror.transform.localPosition = new Vector3(0, 1.65f, 1.94f);
            // A Quad's visible side is its local -Z: unrotated it faces the room; MirrorNoReflection.Normal = -forward.
            mirror.transform.localScale = new Vector3(1.4f, 1.0f, 1f);
            mirror.GetComponent<Renderer>().sharedMaterial = M["Mirror"];
            var frame = Box("MirrorFrame", root, new Vector3(0, 1.65f, 1.97f), new Vector3(1.5f, 1.1f, 0.04f), "Grey_Dark");
            var reflCamGo = new GameObject("ReflectionCamera");
            reflCamGo.transform.SetParent(mirror.transform, false);
            var reflCam = reflCamGo.AddComponent<Camera>();
            reflCam.depth = -10;
            var mnr = mirror.AddComponent<MirrorNoReflection>();
            mnr.reflectionCamera = reflCam;
            mnr.mirrorRenderer = mirror.GetComponent<Renderer>();
            mnr.hiddenLayers = 1 << s_playerLayer;

            // The note on the sink
            var note = Box("Note", root, new Vector3(0.3f, 0.915f, 1.55f), new Vector3(0.22f, 0.01f, 0.16f), "White", new Vector3(0, -15f, 0));
            var noteText = Label3D(note.transform, "", new Vector3(0, 0.51f, 0), new Vector3(90f, 0, 0), new Vector2(0.2f, 0.14f), 0.3f);
            noteText.fontStyle = FontStyles.Italic;
            var noteI = Interact(note, "Read the note");
            noteI.oneShot = true;

            var bin = Prim(PrimitiveType.Cylinder, "Bin", root, new Vector3(1.5f, 0.3f, 1.5f), new Vector3(0.4f, 0.3f, 0.4f), "Grey_Dark");
            var toilet = Box("Toilet", root, new Vector3(-1.5f, 0.25f, 1.5f), new Vector3(0.5f, 0.5f, 0.7f), "White");
            var shower = Box("Shower", root, new Vector3(1.5f, 1.1f, -1.5f), new Vector3(0.9f, 2.2f, 0.9f), "Accent_Blue");

            var door = DoorObj("BathroomDoor", root, new Vector3(-1.94f, 1.05f, -0.5f), new Vector3(0, 90f, 0));

            Spawn(new Vector3(0f, 0f, -1.2f), 0f);
            var s = new GameObject("S2_Bathroom").AddComponent<S2_Bathroom>();
            s.note = noteI; s.noteText = noteText; s.bin = bin.transform; s.door = door;
            Save(scene, s_flow.bathroom);
        }

        // ------------------------------------------------------------------ 03 living room

        static void Build_03_LivingRoom()
        {
            var scene = NewScene();
            var root = Group("LivingRoom").transform;
            Room(root, 6f, 2.8f, 6f);
            PointLight(root, new Vector3(0, 2.5f, 0), 1.8f, 10f, new Color(1f, 0.95f, 0.85f));

            // Table set for one
            var table = Box("Table", root, new Vector3(0, 0.375f, 0.5f), new Vector3(1.2f, 0.75f, 0.9f), "Wood");
            var chair = Box("Chair", root, new Vector3(0, 0.45f, -0.3f), new Vector3(0.45f, 0.9f, 0.45f), "Grey_Prop");
            var bowl = Prim(PrimitiveType.Cylinder, "Bowl", root, new Vector3(0, 0.78f, 0.4f), new Vector3(0.25f, 0.04f, 0.25f), "White");
            Prim(PrimitiveType.Cylinder, "Cornflakes", bowl.transform, new Vector3(0, 0.6f, 0), new Vector3(0.8f, 0.5f, 0.8f), "Ball");
            Box("Spoon", root, new Vector3(0.25f, 0.77f, 0.4f), new Vector3(0.03f, 0.01f, 0.18f), "Grey_Dark");
            Label3D(table.transform, "a table set for one", new Vector3(0, 0.51f, -0.3f), new Vector3(90f, 0, 0), new Vector2(1f, 0.2f), 0.8f, Color.gray);

            // Fridge with a hinged door
            var fridge = Box("Fridge", root, new Vector3(-2.5f, 1.0f, 2.2f), new Vector3(0.9f, 2.0f, 0.8f), "White");
            var hinge = Group("FridgeHinge", root, new Vector3(-2.95f, 1.0f, 1.8f));
            var fdoor = Box("FridgeDoor", hinge.transform, new Vector3(0.45f, 0, 0), new Vector3(0.9f, 2.0f, 0.06f), "Grey_Wall");
            Object.DestroyImmediate(fdoor.GetComponent<Collider>());
            var contents = Group("FridgeContents", fridge.transform);
            for (int i = 0; i < 6; i++)
            {
                var y = -0.35f + (i / 3) * 0.4f;
                var x = -0.3f + (i % 3) * 0.3f;
                var b = Prim(PrimitiveType.Cylinder, "BowlOfCornflakes", contents.transform, new Vector3(x, y, -0.2f), new Vector3(0.22f, 0.04f, 0.22f), "White");
                Object.DestroyImmediate(b.GetComponent<Collider>());
                b.transform.localScale = new Vector3(0.25f, 0.025f, 0.3f);
            }
            Label3D(fridge.transform, "only more bowls of cornflakes", new Vector3(0, 0.1f, -0.52f), Vector3.zero, new Vector2(0.8f, 0.4f), 0.6f, Color.gray);
            var fridgeI = Interact(fridge, "Look for milk");

            // Phone on a side table
            var sideTable = Box("SideTable", root, new Vector3(2.4f, 0.4f, 2.2f), new Vector3(0.6f, 0.8f, 0.6f), "Wood");
            var phone = Box("Phone", root, new Vector3(2.4f, 0.88f, 2.2f), new Vector3(0.25f, 0.15f, 0.2f), "Grey_Dark");
            Box("Handset", phone.transform, new Vector3(0, 0.6f, 0), new Vector3(1.1f, 0.3f, 0.5f), "Black");
            var phoneAudio = phone.AddComponent<AudioSource>();
            phoneAudio.playOnAwake = false; phoneAudio.spatialBlend = 1f; phoneAudio.minDistance = 1.5f;
            var phoneI = Interact(phone, "Answer the phone");

            Box("Sofa", root, new Vector3(1.8f, 0.35f, -1.8f), new Vector3(2.0f, 0.7f, 0.9f), "Accent_Blue");
            Box("TV", root, new Vector3(-2.2f, 1.2f, -2.9f), new Vector3(1.4f, 0.8f, 0.08f), "Black");

            var frontDoor = DoorObj("FrontDoor", root, new Vector3(2.94f, 1.05f, -0.5f), new Vector3(0, -90f, 0));

            Spawn(new Vector3(-1.5f, 0f, -2.0f), 30f);
            var s = new GameObject("S3_LivingRoom").AddComponent<S3_LivingRoom>();
            s.fridge = fridgeI; s.fridgeDoor = hinge.transform; s.fridgeContents = contents; s.phone = phoneI; s.phoneAudio = phoneAudio; s.frontDoor = frontDoor;
            Save(scene, s_flow.livingRoom);
        }

        // ------------------------------------------------------------------ 04 street

        static void Build_04_Street()
        {
            var scene = NewScene();
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);
            // Everything under a root tilted 4 degrees so the street climbs towards +Z.
            var root = Group("StreetRoot", null, Vector3.zero, new Vector3(-4f, 0, 0)).transform;
            const float length = 44f;
            Box("Road", root, new Vector3(0, -0.1f, length / 2 - 4f), new Vector3(6f, 0.2f, length + 8f), "Grey_Dark");
            Box("Pavement_L", root, new Vector3(-4.5f, -0.05f, length / 2 - 4f), new Vector3(3f, 0.3f, length + 8f), "Grey_Floor");
            Box("Pavement_R", root, new Vector3(4.5f, -0.05f, length / 2 - 4f), new Vector3(3f, 0.3f, length + 8f), "Grey_Floor");
            Box("Wall_Start", root, new Vector3(0, 1.5f, -7.5f), new Vector3(12f, 3f, 0.2f), "Grey_Wall");

            // Identical houses, every door number 9.
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 7; i++)
                {
                    float z = i * 6f;
                    var house = Group($"House_{(side < 0 ? "L" : "R")}{i}", root, new Vector3(side * 8f, 0, z));
                    Box("Body", house.transform, new Vector3(0, 1.75f, 0), new Vector3(4.5f, 3.5f, 4.5f), "Grey_Wall");
                    Box("Roof", house.transform, new Vector3(0, 3.9f, 0), new Vector3(4.8f, 0.8f, 4.8f), "Accent_Red");
                    var door = Box("HouseDoor", house.transform, new Vector3(-side * 2.26f, 1.05f, 0), new Vector3(0.08f, 2.1f, 1.0f), "Wood");
                    Label3D(door.transform, "9", new Vector3(-side * 0.6f, 0.3f, 0), new Vector3(0, side * 90f, 0), new Vector2(0.5f, 0.5f), 5f, Color.white).fontStyle = FontStyles.Bold;
                    var win = Box("Window", house.transform, new Vector3(-side * 2.26f, 2.0f, 1.5f), new Vector3(0.04f, 0.9f, 0.9f), "Accent_Blue");
                    Object.DestroyImmediate(win.GetComponent<Collider>());
                }
            }
            Label3D(root, "every door number 9", new Vector3(0, 0.12f, 3f), new Vector3(90f, 0, 0), new Vector2(5f, 1f), 4f, Color.white);

            // Ball: parked, hidden, launched by a trigger halfway.
            var ball = Prim(PrimitiveType.Sphere, "Ball", root, new Vector3(0, -5f, 0), Vector3.one * 0.5f, "Ball");
            var rb = ball.AddComponent<Rigidbody>();
            rb.mass = 0.5f; rb.isKinematic = true;
            var bfb = ball.AddComponent<BallFromBehind>();
            var trig = Box("BallTrigger", root, new Vector3(0, 1f, 12f), new Vector3(12f, 2f, 1f), "Grey_Dark");
            trig.GetComponent<Renderer>().enabled = false;
            trig.GetComponent<Collider>().isTrigger = true;
            var tz = trig.AddComponent<TriggerZone>();

            // The child at the end of the street.
            var child = (GameObject)PrefabUtility.InstantiatePrefab(s_childPrefab);
            child.transform.SetParent(root, false);
            child.transform.localPosition = new Vector3(1.5f, 0.1f, 34f);
            var wc = child.GetComponent<WeepingChild>();
            wc.canMove = true;

            // The shop at the end.
            var shop = Group("Shop", root, new Vector3(0, 0, 42f));
            Box("ShopBody", shop.transform, new Vector3(0, 2f, 2.5f), new Vector3(12f, 4f, 5f), "Grey_Wall");
            Label3D(shop.transform, "SHOP", new Vector3(0, 3.3f, -0.05f), Vector3.zero, new Vector2(4f, 1f), 8f, Color.white).fontStyle = FontStyles.Bold;
            Label3D(root, "nobody is there", new Vector3(0, 1.5f, -7.3f), new Vector3(0, 180f, 0), new Vector2(3f, 0.4f), 1.2f, Color.white);
            var shopDoor = DoorObj("ShopDoor", shop.transform, new Vector3(0, 1.05f, -0.06f), new Vector3(0, 180f, 0));

            // Ground beyond edges so nobody falls off
            Box("Ground", root, new Vector3(0, -0.5f, length / 2), new Vector3(60f, 0.2f, 80f), "Grey_Floor");

            var spawn = Spawn(Vector3.zero, 0f);
            spawn.transform.SetParent(root, false);
            spawn.transform.localPosition = new Vector3(0, 0.3f, -3f);
            spawn.transform.SetParent(null, true);
            spawn.transform.rotation = Quaternion.identity;

            var s = new GameObject("S4_Street").AddComponent<S4_Street>();
            s.ballTrigger = tz; s.ball = bfb; s.child = wc; s.shopDoor = shopDoor;
            Save(scene, s_flow.street);
        }

        // ------------------------------------------------------------------ 05 shop

        static void Build_05_Shop()
        {
            var scene = NewScene();
            var root = Group("Shop").transform;
            const float w = 8f, h = 3.5f, d = 8f, t = 0.1f;
            Box("Floor", root, new Vector3(0, -t / 2, 0), new Vector3(w + 2 * t, t, d + 2 * t), "Grey_Floor");
            Box("Ceiling", root, new Vector3(0, h + t / 2, 0), new Vector3(w + 2 * t, t, d + 2 * t), "Grey_Ceiling");
            Box("Wall_South", root, new Vector3(0, h / 2, -d / 2 - t / 2), new Vector3(w + 2 * t, h, t), "Grey_Wall");
            Box("Wall_East", root, new Vector3(w / 2 + t / 2, h / 2, 0), new Vector3(t, h, d), "Grey_Wall");
            Box("Wall_West", root, new Vector3(-w / 2 - t / 2, h / 2, 0), new Vector3(t, h, d), "Grey_Wall");
            // North wall with a 1.4 m gap behind the counter, filled by a removable panel.
            Box("Wall_North_L", root, new Vector3(-(w / 4 + 0.35f), h / 2, d / 2 + t / 2), new Vector3(w / 2 - 0.7f, h, t), "Grey_Wall");
            Box("Wall_North_R", root, new Vector3(w / 4 + 0.35f, h / 2, d / 2 + t / 2), new Vector3(w / 2 - 0.7f, h, t), "Grey_Wall");
            var panel = Box("CounterPanel", root, new Vector3(0, h / 2, d / 2 + t / 2), new Vector3(1.4f, h, t), "Grey_Wall");
            PointLight(root, new Vector3(0, 3.2f, 0), 2.5f, 12f);
            PointLight(root, new Vector3(-2.5f, 3.2f, 2), 1.5f, 8f);
            PointLight(root, new Vector3(2.5f, 3.2f, 2), 1.5f, 8f);

            // Counter + shopkeeper (same face as the child).
            var counter = Box("Counter", root, new Vector3(0, 0.5f, 2.2f), new Vector3(3f, 1.0f, 0.8f), "Wood");
            var counterI = Interact(counter, "Look behind the counter");
            var keeper = (GameObject)PrefabUtility.InstantiatePrefab(s_childPrefab);
            keeper.name = "Shopkeeper";
            keeper.transform.SetParent(root, false);
            keeper.transform.localPosition = new Vector3(1.0f, 0, 3.1f); // beside the opening, not in front of it
            keeper.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            keeper.GetComponent<WeepingChild>().canMove = false;
            var keeperI = Interact(keeper, "Talk to the shopkeeper");
            Label3D(keeper.transform, "same face as the child", new Vector3(0, 1.9f, 0), new Vector3(0, 180f, 0), new Vector2(1.5f, 0.3f), 1.0f, Color.white);

            // Opening behind the counter: a short yellow corridor stub, hidden until "look behind the counter".
            var opening = Group("BackroomsOpening", root, new Vector3(0, 0, d / 2 + 1.5f));
            Box("Floor", opening.transform, new Vector3(0, -t / 2, 0), new Vector3(1.4f, t, 3f), "Yellow_Carpet");
            Box("Ceiling", opening.transform, new Vector3(0, 2.6f, 0), new Vector3(1.4f, t, 3f), "Yellow_Backrooms");
            Box("Wall_L", opening.transform, new Vector3(-0.75f, 1.3f, 0), new Vector3(t, 2.6f, 3f), "Yellow_Backrooms");
            Box("Wall_R", opening.transform, new Vector3(0.75f, 1.3f, 0), new Vector3(t, 2.6f, 3f), "Yellow_Backrooms");
            PointLight(opening.transform, new Vector3(0, 2.3f, 0), 4f, 6f, new Color(1f, 0.9f, 0.5f));
            var backDoor = DoorObj("BackroomsDoor", opening.transform, new Vector3(0, 1.05f, 1.45f), Vector3.zero, "Yellow_Backrooms");

            // Dressing A: supermarket, every shelf full of milk, every carton a missing-person poster.
            var super = Group("Dressing_Supermarket", root);
            for (int s = 0; s < 3; s++)
            {
                var shelf = Box($"Shelf_{s}", super.transform, new Vector3(-3.5f, 0.5f + s * 0.9f, -1f), new Vector3(0.6f, 0.05f, 4f), "Grey_Prop");
                for (int c = 0; c < 8; c++)
                {
                    var carton = Box("MilkCarton", shelf.transform, new Vector3(0, 3.3f, -0.44f + c * 0.125f), new Vector3(0.35f, 5.5f, 0.055f), "White");
                    Object.DestroyImmediate(carton.GetComponent<Collider>());
                    MissingPoster(carton.transform, new Vector3(0.52f, 0, 0), new Vector3(0, 90f, 0));
                }
            }
            Label3D(super.transform, "SUPERMARKET", new Vector3(0, 3.0f, -3.9f), Vector3.zero, new Vector2(5f, 0.8f), 5f, Color.white);
            var milkItem = Box("MilkOnCounter", super.transform, new Vector3(-0.8f, 1.15f, 2.1f), new Vector3(0.12f, 0.3f, 0.12f), "White");
            MissingPoster(milkItem.transform, new Vector3(0, 0, -0.52f), new Vector3(0, 180f, 0));
            var milkI = Interact(milkItem, "Take the milk and go home");
            Box("Checkout", super.transform, new Vector3(2.5f, 0.45f, 0.5f), new Vector3(0.8f, 0.9f, 1.6f), "Grey_Prop");

            // Dressing B: florist, every bouquet carries the same card.
            var florist = Group("Dressing_Florist", root);
            for (int i = 0; i < 8; i++)
            {
                float x = -3.2f + (i % 4) * 1.0f;
                float z = -2.5f + (i / 4) * 1.5f;
                var stand = Box("Stand", florist.transform, new Vector3(x, 0.4f, z), new Vector3(0.5f, 0.8f, 0.5f), "Wood");
                Bouquet(stand.transform, new Vector3(0, 0.5f, 0), 1f / 0.8f);
            }
            Label3D(florist.transform, "FLORIST", new Vector3(0, 3.0f, -3.9f), Vector3.zero, new Vector2(5f, 0.8f), 5f, Color.white);
            var bouquetItem = Group("BouquetOnCounter", florist.transform, new Vector3(-0.8f, 1.0f, 2.1f));
            Bouquet(bouquetItem.transform, Vector3.zero, 1f);
            var bouquetCol = bouquetItem.AddComponent<CapsuleCollider>();
            bouquetCol.center = new Vector3(0, 0.25f, 0); bouquetCol.height = 0.6f; bouquetCol.radius = 0.15f;
            var bouquetI = Interact(bouquetItem, "Take the flowers and go home");
            Box("Cooler", florist.transform, new Vector3(2.8f, 1.0f, 0.5f), new Vector3(1.2f, 2.0f, 2.0f), "Accent_Blue");

            var frontDoor = DoorObj("FrontDoor", root, new Vector3(0.5f, 1.05f, -3.94f), new Vector3(0, 180f, 0));

            Spawn(new Vector3(0.5f, 0f, -2.8f), 0f);
            var sc = new GameObject("S5_Shop").AddComponent<S5_Shop>();
            sc.supermarketDressing = super; sc.floristDressing = florist; sc.shopkeeper = keeperI;
            sc.itemSupermarket = milkI; sc.itemFlorist = bouquetI; sc.counterBack = counterI; sc.counterPanel = panel;
            sc.backroomsOpening = opening; sc.frontDoor = frontDoor; sc.backroomsDoor = backDoor;
            Save(scene, s_flow.shop);
        }

        /// <summary>
        /// A small "missing person" poster (photo of Sam + text) on the face of a carton.
        /// localPos is in the carton's local units (0.5 = its surface); the poster's local +Z is its front.
        /// </summary>
        static void MissingPoster(Transform parent, Vector3 localPos, Vector3 euler)
        {
            var poster = Group("Poster", parent, localPos, euler);
            var ps = parent.lossyScale; // counter the carton's non-uniform scale so the poster is in metres
            poster.transform.localScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z);
            var face = Prim(PrimitiveType.Quad, "Photo", poster.transform, new Vector3(0, 0.035f, 0.002f), new Vector3(0.06f, 0.07f, 1f), "Face", new Vector3(0, 180f, 0));
            Object.DestroyImmediate(face.GetComponent<Collider>());
            var l = Label3D(poster.transform, s_lines.missingPoster, new Vector3(0, -0.05f, 0.002f), new Vector3(0, 180f, 0), new Vector2(0.2f, 0.08f), 0.2f, Color.black);
            l.fontStyle = FontStyles.Bold;
            Label3D(poster.transform, "SAM", new Vector3(0, 0.09f, 0.002f), new Vector3(0, 180f, 0), new Vector2(0.2f, 0.04f), 0.2f, Color.black);
        }

        static void Bouquet(Transform parent, Vector3 pos, float s)
        {
            var stems = Prim(PrimitiveType.Cylinder, "Stems", parent, pos + new Vector3(0, 0.15f * s, 0), new Vector3(0.08f, 0.15f, 0.08f) * s, "Green");
            Object.DestroyImmediate(stems.GetComponent<Collider>());
            var flowers = Prim(PrimitiveType.Sphere, "Flowers", parent, pos + new Vector3(0, 0.38f * s, 0), Vector3.one * 0.3f * s, "Accent_Red");
            Object.DestroyImmediate(flowers.GetComponent<Collider>());
            var card = Box("Card", parent, pos + new Vector3(0.12f * s, 0.3f * s, -0.12f * s), new Vector3(0.12f, 0.08f, 0.005f) * s, "White", new Vector3(0, -30f, 0));
            Object.DestroyImmediate(card.GetComponent<Collider>());
            Label3D(card.transform, s_lines.bouquetCard, new Vector3(0, 0, -0.6f), Vector3.zero, new Vector2(0.11f * s, 0.07f * s), 0.2f * s, Color.black);
        }

        // ------------------------------------------------------------------ 06A front door -> bedroom

        static void Build_06A_FrontDoor()
        {
            var scene = NewScene();
            var b = BuildBedroom(wrong: false, withDoor: false);
            // The door you just came through, now shut behind you.
            var shut = Box("FrontDoor_Shut", b.root, new Vector3(-1.2f, 1.05f, 2.44f), new Vector3(1f, 2.1f, 0.12f), "Wood");
            Label3D(shut.transform, "the shop's front door", new Vector3(0, 0.3f, -0.52f), Vector3.zero, new Vector2(0.9f, 0.3f), 1.2f);
            b.alarmRenderer.sharedMaterial = M["Grey_Prop"];
            b.alarm.GetComponent<Interactable>().available = false;
            var bedI = b.bed.GetComponent<Interactable>();
            bedI.available = true;
            Spawn(new Vector3(-1.2f, 0f, 1.6f), 180f);
            var s = new GameObject("S6_FrontDoor").AddComponent<S6_FrontDoor>();
            s.clockText = b.clock; s.bed = bedI;
            Save(scene, s_flow.frontDoor);
        }

        // ------------------------------------------------------------------ 06B backrooms

        static void Build_06B_Backrooms()
        {
            var scene = NewScene(withLight: false);
            RenderSettings.ambientLight = new Color(0.22f, 0.19f, 0.09f);
            var root = Group("Backrooms").transform;

            // Corridor path: 24 m along +Z, 14 m along +X, 20 m along +Z, into the hall.
            Corridor(root, new Vector3(0, 0, 12f), new Vector3(0, 0, 0), 24f);
            Corridor(root, new Vector3(7f, 0, 24f), new Vector3(0, 90f, 0), 16f);
            Corridor(root, new Vector3(14f, 0, 34f), new Vector3(0, 0, 0), 20f);
            // Corner floors / caps
            Box("CornerFloor_1", root, new Vector3(0, -0.05f, 24f), new Vector3(2.2f, 0.1f, 2.2f), "Yellow_Carpet");
            Box("CornerCeil_1", root, new Vector3(0, 2.65f, 24f), new Vector3(2.2f, 0.1f, 2.2f), "Yellow_Backrooms");
            Box("CornerWall_1", root, new Vector3(0, 1.3f, 25.05f), new Vector3(2.2f, 2.6f, 0.1f), "Yellow_Backrooms");
            Box("CornerWall_1b", root, new Vector3(-1.05f, 1.3f, 24f), new Vector3(0.1f, 2.6f, 2.2f), "Yellow_Backrooms");
            Box("CornerFloor_2", root, new Vector3(14f, -0.05f, 24f), new Vector3(2.2f, 0.1f, 2.2f), "Yellow_Carpet");
            Box("CornerCeil_2", root, new Vector3(14f, 2.65f, 24f), new Vector3(2.2f, 0.1f, 2.2f), "Yellow_Backrooms");
            Box("CornerWall_2", root, new Vector3(14f, 1.3f, 22.95f), new Vector3(2.2f, 2.6f, 0.1f), "Yellow_Backrooms");
            Box("CornerWall_2b", root, new Vector3(15.05f, 1.3f, 24f), new Vector3(0.1f, 2.6f, 2.2f), "Yellow_Backrooms");
            Box("StartWall", root, new Vector3(0, 1.3f, -0.05f), new Vector3(2.2f, 2.6f, 0.1f), "Yellow_Backrooms");
            Label3D(root, "behind the counter", new Vector3(0, 1.6f, 0.02f), new Vector3(0, 180f, 0), new Vector2(2f, 0.4f), 1.2f, Color.black);

            // The hall: the street seen from behind.
            var hall = Group("StageSet", root, new Vector3(14f, 0, 44f + 15f));
            Box("Floor", hall.transform, new Vector3(0, -0.05f, 0), new Vector3(50f, 0.1f, 30f), "Grey_Dark");
            Box("Wall_N", hall.transform, new Vector3(0, 6f, 15f), new Vector3(50f, 12f, 0.2f), "Black");
            Box("Wall_E", hall.transform, new Vector3(25f, 6f, 0), new Vector3(0.2f, 12f, 30f), "Black");
            Box("Wall_W", hall.transform, new Vector3(-25f, 6f, 0), new Vector3(0.2f, 12f, 30f), "Black");
            Box("Wall_S_L", hall.transform, new Vector3(-13.5f, 6f, -15f), new Vector3(23f, 12f, 0.2f), "Black");
            Box("Wall_S_R", hall.transform, new Vector3(13.5f, 6f, -15f), new Vector3(23f, 12f, 0.2f), "Black");
            Box("Ceiling", hall.transform, new Vector3(0, 12f, 0), new Vector3(50f, 0.2f, 30f), "Black");
            // Flat wooden facades in a row (the houses), seen from the back, held up by beams.
            for (int i = 0; i < 7; i++)
            {
                float x = -18f + i * 6f;
                var fac = Group($"Facade_{i}", hall.transform, new Vector3(x, 0, 8f));
                Box("Panel", fac.transform, new Vector3(0, 1.75f, 0), new Vector3(4.5f, 3.5f, 0.08f), "Wood");
                Box("RoofPanel", fac.transform, new Vector3(0, 3.9f, 0), new Vector3(4.8f, 0.8f, 0.08f), "Wood");
                Label3D(fac.transform, "9", new Vector3(0, 1.0f, 0.05f), new Vector3(0, 180f, 0), new Vector2(0.5f, 0.5f), 4f, new Color(0.3f, 0.2f, 0.1f));
                for (int b = -1; b <= 1; b++)
                {
                    var beam = Box("Beam", fac.transform, new Vector3(b * 1.6f, 1.5f, -1.5f), new Vector3(0.12f, 0.12f, 4.2f), "Grey_Prop", new Vector3(-45f, 0, 0));
                    Object.DestroyImmediate(beam.GetComponent<Collider>());
                }
            }
            Label3D(hall.transform, "the street, seen from behind: a stage set", new Vector3(0, 5f, 7.8f), Vector3.zero, new Vector2(14f, 1f), 4f, Color.white);
            var lights = Group("StageSetLights", hall.transform);
            PointLight(lights.transform, new Vector3(-12f, 8f, 2f), 25f, 45f, new Color(1f, 0.9f, 0.8f));
            PointLight(lights.transform, new Vector3(12f, 8f, 2f), 25f, 45f, new Color(1f, 0.9f, 0.8f));
            PointLight(lights.transform, new Vector3(0f, 8f, 4f), 25f, 45f, new Color(1f, 0.9f, 0.8f));
            PointLight(lights.transform, new Vector3(0f, 3f, -4f), 10f, 30f, new Color(1f, 0.9f, 0.8f));
            // One door leads back to the bedroom.
            var bedroomDoor = DoorObj("BedroomDoor", hall.transform, new Vector3(0, 1.05f, 2.5f), Vector3.zero);
            Label3D(hall.transform, "one door leads back", new Vector3(0, 2.6f, 2.4f), Vector3.zero, new Vector2(3f, 0.4f), 1.5f, Color.white);
            PointLight(hall.transform, new Vector3(0, 2.5f, 1.5f), 1f, 5f, new Color(1f, 0.8f, 0.6f));

            var trig = Box("RevealTrigger", root, new Vector3(14f, 1.3f, 45f), new Vector3(2f, 2.6f, 1f), "Grey_Dark");
            trig.GetComponent<Renderer>().enabled = false;
            trig.GetComponent<Collider>().isTrigger = true;
            var tz = trig.AddComponent<TriggerZone>();

            Spawn(new Vector3(0, 0, 1.5f), 0f);
            var s = new GameObject("S6_Backrooms").AddComponent<S6_Backrooms>();
            s.revealTrigger = tz; s.stageSetLights = lights; s.bedroomDoor = bedroomDoor;
            Save(scene, s_flow.backrooms);
        }

        /// <summary>A 2 m wide, 2.6 m high corridor of the given length, centred at pos, running along local +Z.</summary>
        static void Corridor(Transform parent, Vector3 pos, Vector3 euler, float length)
        {
            var c = Group("Corridor", parent, pos, euler);
            Box("Floor", c.transform, new Vector3(0, -0.05f, 0), new Vector3(2f, 0.1f, length), "Yellow_Carpet");
            Box("Ceiling", c.transform, new Vector3(0, 2.65f, 0), new Vector3(2f, 0.1f, length), "Yellow_Backrooms");
            Box("Wall_L", c.transform, new Vector3(-1.05f, 1.3f, 0), new Vector3(0.1f, 2.6f, length), "Yellow_Backrooms");
            Box("Wall_R", c.transform, new Vector3(1.05f, 1.3f, 0), new Vector3(0.1f, 2.6f, length), "Yellow_Backrooms");
            int n = Mathf.Max(1, Mathf.RoundToInt(length / 6f));
            for (int i = 0; i < n; i++)
            {
                float z = -length / 2 + (i + 0.5f) * (length / n);
                PointLight(c.transform, new Vector3(0, 2.4f, z), 6f, 9f, new Color(1f, 0.9f, 0.55f));
                var fixture = Box("Fixture", c.transform, new Vector3(0, 2.58f, z), new Vector3(0.6f, 0.04f, 0.3f), "White");
                Object.DestroyImmediate(fixture.GetComponent<Collider>());
            }
        }

        // ------------------------------------------------------------------ 07 last morning

        static void Build_07_LastMorning()
        {
            var scene = NewScene();
            var b = BuildBedroom(wrong: true, withDoor: false);
            // The normal bedroom door is gone; the wall is blank except, maybe, the appeared door.
            var appeared = DoorObj("AppearedDoor", b.root, new Vector3(-1.2f, 1.05f, 2.44f), Vector3.zero, "Accent_Red");
            Object.DestroyImmediate(appeared);                       // scene script decides what happens, not Door
            var appearedGo = b.root.Find("AppearedDoor").gameObject;
            var appearedI = appearedGo.GetComponent<Interactable>();
            appearedI.prompt = "Open the door";
            Label3D(appearedGo.transform, "a door that wasn't there", new Vector3(0, 0.75f, -0.52f), Vector3.zero, new Vector2(2.0f, 0.3f), 1.1f, Color.white);

            var handNote = Box("HandNote", null, new Vector3(0, -5f, 0), new Vector3(0.18f, 0.12f, 0.005f), "White");
            Object.DestroyImmediate(handNote.GetComponent<Collider>());
            var handText = Label3D(handNote.transform, "", new Vector3(0, 0, -0.6f), Vector3.zero, new Vector2(0.17f, 0.11f), 0.3f, Color.black);
            handText.fontStyle = FontStyles.Italic;
            handNote.SetActive(false);

            var bedI = b.bed.GetComponent<Interactable>();
            Spawn(new Vector3(0.0f, 0f, -0.8f), 0f);

            // What the ending scenes (loaded additively) need from this one.
            var bridge = new GameObject("LastMorningBridge").AddComponent<LastMorningBridge>();
            var lie = Group("BedLiePoint", b.root, new Vector3(1.0f, -1.0f, 0.9f), new Vector3(0, 60f, 0)); // rig root 1 m under the floor = eyes at mattress height
            bridge.bedLiePoint = lie.transform;
            bridge.appearedDoor = appearedGo.transform;
            bridge.northWall = b.root.Find("Wall_North").gameObject;
            bridge.bedroomRoot = b.root.gameObject;
            bridge.handNote = handNote;

            var s = new GameObject("S7_LastMorning").AddComponent<S7_LastMorning>();
            s.alarm = b.alarm.GetComponent<Interactable>(); s.alarmRenderer = b.alarmRenderer; s.alarmAudio = b.alarmAudio; s.clockText = b.clock;
            s.handNote = handNote; s.handNoteText = handText; s.appearedDoor = appearedGo; s.appearedDoorInteractable = appearedI; s.bed = bedI;
            Save(scene, s_flow.lastMorning);
        }

        // ------------------------------------------------------------------ 99 end of demo

        static void Build_99_EndOfDemo()
        {
            var scene = NewScene(withLight: false);
            var cam = new GameObject("Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            cam.gameObject.AddComponent<AudioListener>();
            var canvas = SceneCanvas("EndCanvas");
            var text = UIText(canvas.transform, "Text", "", 40, TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(1400, 700));
            var button = UIButton(canvas.transform, "RestartButton", "Restart", new Vector2(0, -380), new Vector2(300, 80), out _);
            var s = new GameObject("EndOfDemoScreen").AddComponent<EndOfDemoScreen>();
            s.text = text; s.restartButton = button;
            Save(scene, s_flow.endOfDemo);
        }
    }
}
