using System.Collections.Generic;
using System.IO;
using TMPro;
using TWIN.Core;
using TWIN.Debugging;
using TWIN.Narration;
using TWIN.Player;
using TWIN.Scenes;
using TWIN.Vote;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TWIN.EditorTools
{
    /// <summary>
    /// Builds the whole greybox from code: materials, data assets, prefabs and the ten scenes.
    /// Re-running it overwrites the generated scenes (hand edits in those scenes are lost; edit the
    /// builder or save a copy under another name). Data assets and materials are only created when missing.
    ///
    /// Menu: Tools > TWIN > Build All Greybox Scenes. Headless: GreyboxBuilder.BuildAll().
    /// </summary>
    public static partial class GreyboxBuilder
    {
        public const string Root = "Assets/TWIN";
        public const string ScenesDir = Root + "/Scenes";
        public const string MaterialsDir = Root + "/Materials";
        public const string DataDir = Root + "/Data";
        public const string ForksDir = DataDir + "/Forks";
        public const string PrefabsDir = Root + "/Prefabs";
        public const string ResourcesDir = Root + "/Resources";
        public const string PlayerLayerName = "Player";
        public const string EndingsDir = DataDir + "/Endings";
        public const string SignalsDir = EndingsDir + "/Signals";
        public const string ClipsDir = EndingsDir + "/Clips";

        // Filled by BuildMaterials / BuildData / BuildPrefabs, used by the scene builders.
        static Dictionary<string, Material> M = new();
        static SceneFlow s_flow;
        static NarratorLines s_lines;
        static ForkDefinition[] s_forks;
        static GameObject s_playerRigPrefab, s_childPrefab, s_persistentPrefab;
        static int s_playerLayer;

        [MenuItem("Tools/TWIN/Build All Greybox Scenes")]
        public static void BuildAll()
        {
            EnsureFolders();
            s_playerLayer = EnsureLayer(PlayerLayerName);
            BuildMaterials();
            BuildData();
            BuildPrefabs();
            BuildScenes();
            BuildEndings();
            SetBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TWIN] Greybox build complete.");
        }

        [MenuItem("Tools/TWIN/Open Boot Scene")]
        public static void OpenBoot()
        {
            EditorSceneManager.OpenScene($"{ScenesDir}/00_Boot.unity");
        }

        // ------------------------------------------------------------------ folders / layer

        static void EnsureFolders()
        {
            foreach (var d in new[] { Root, ScenesDir, MaterialsDir, DataDir, ForksDir, PrefabsDir, ResourcesDir, EndingsDir, SignalsDir, ClipsDir })
            {
                if (!AssetDatabase.IsValidFolder(d))
                {
                    var parent = Path.GetDirectoryName(d).Replace('\\', '/');
                    AssetDatabase.CreateFolder(parent, Path.GetFileName(d));
                }
            }
        }

        static int EnsureLayer(string layerName)
        {
            int existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0) return existing;
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (int i = 6; i < layers.arraySize; i++)
            {
                var p = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(p.stringValue))
                {
                    p.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[TWIN] Added layer '{layerName}' at index {i}");
                    return i;
                }
            }
            Debug.LogError("[TWIN] No free layer slot for 'Player'");
            return 0;
        }

        // ------------------------------------------------------------------ materials

        static void BuildMaterials()
        {
            M.Clear();
            Mat("Grey_Floor", new Color(0.35f, 0.35f, 0.35f));
            Mat("Grey_Wall", new Color(0.6f, 0.6f, 0.6f));
            Mat("Grey_Ceiling", new Color(0.75f, 0.75f, 0.75f));
            Mat("Grey_Prop", new Color(0.5f, 0.5f, 0.52f));
            Mat("Grey_Dark", new Color(0.2f, 0.2f, 0.22f));
            Mat("White", new Color(0.95f, 0.95f, 0.95f));
            Mat("Accent_Red", new Color(0.85f, 0.15f, 0.15f));
            Mat("Accent_Blue", new Color(0.2f, 0.4f, 0.9f));
            Mat("Yellow_Backrooms", new Color(0.85f, 0.75f, 0.3f));
            Mat("Yellow_Carpet", new Color(0.55f, 0.45f, 0.2f));
            Mat("Wood", new Color(0.55f, 0.38f, 0.22f));
            Mat("Green", new Color(0.2f, 0.6f, 0.25f));
            Mat("Face", new Color(0.6f, 0.3f, 0.7f)); // the child's and the shopkeeper's "face"
            Mat("Ball", new Color(0.9f, 0.5f, 0.1f));
            Mat("Mirror", Color.white, unlit: true);
            Mat("Black", Color.black, unlit: true);
        }

        static Material Mat(string name, Color color, bool unlit = false)
        {
            string path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
                if (shader == null) throw new System.Exception("URP shaders not found. Is URP the active pipeline?");
                mat = new Material(shader);
                mat.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(mat, path);
            }
            M[name] = mat;
            return mat;
        }

        // ------------------------------------------------------------------ data

        static void BuildData()
        {
            s_flow = LoadOrCreate<SceneFlow>($"{DataDir}/SceneFlow.asset");
            s_lines = LoadOrCreate<NarratorLines>($"{DataDir}/NarratorLines.asset");
            s_forks = new ForkDefinition[GameState.ForkCount];
            s_forks[0] = Fork(0, "Fork1_Note", "The note", "Throw it in the bin", "Put it in your pocket");
            s_forks[1] = Fork(1, "Fork2_Breakfast", "Breakfast", "Look for milk", "Answer the phone");
            s_forks[2] = Fork(2, "Fork3_Shop", "Everything is free today", "Take it and go home", "Look behind the counter");
            s_forks[3] = Fork(3, "Fork4_Door", "A door has appeared", "Go back to bed", "Open the door");
        }

        static ForkDefinition Fork(int index, string file, string title, string normal, string strange)
        {
            string path = $"{ForksDir}/{file}.asset";
            var f = AssetDatabase.LoadAssetAtPath<ForkDefinition>(path);
            if (f != null) return f;
            f = ScriptableObject.CreateInstance<ForkDefinition>();
            f.forkIndex = index; f.title = title; f.normalLabel = normal; f.strangeLabel = strange;
            AssetDatabase.CreateAsset(f, path);
            return f;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) return a;
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        // ------------------------------------------------------------------ prefabs

        static void BuildPrefabs()
        {
            s_playerRigPrefab = SavePrefab(MakePlayerRig(), $"{PrefabsDir}/PlayerRig.prefab");
            s_childPrefab = SavePrefab(MakeChild(), $"{PrefabsDir}/Child.prefab");
            s_persistentPrefab = SavePrefab(MakePersistent(), $"{ResourcesDir}/{SceneBootstrapper.PersistentPrefabResource}.prefab");
        }

        static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject MakePlayerRig()
        {
            var rig = new GameObject("PlayerRig") { tag = "Player", layer = s_playerLayer };
            var cc = rig.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0f, 0.9f, 0f); cc.stepOffset = 0.3f; cc.slopeLimit = 50f;

            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(rig.transform, false);
            offset.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera", layer = s_playerLayer };
            camGo.transform.SetParent(offset.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 70f;
            camGo.AddComponent<AudioListener>();
            var tpd = camGo.AddComponent<TrackedPoseDriver>();
            tpd.positionInput = new InputActionProperty(new InputAction("Position", InputActionType.Value, "<XRHMD>/centerEyePosition"));
            tpd.rotationInput = new InputActionProperty(new InputAction("Rotation", InputActionType.Value, "<XRHMD>/centerEyeRotation"));
            tpd.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;

            var origin = rig.AddComponent<XROrigin>();
            origin.Camera = cam;
            origin.CameraFloorOffsetObject = offset;
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;

            // Visible body so a mirror would have something NOT to reflect.
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.layer = s_playerLayer;
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(rig.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            body.GetComponent<Renderer>().sharedMaterial = M["Accent_Blue"];

            var desk = rig.AddComponent<DesktopRig>();
            desk.playerCamera = cam;
            desk.cameraPivot = offset.transform;
            desk.xrOnly = new Behaviour[] { tpd };
            desk.interactMask = ~(1 << s_playerLayer);
            return rig;
        }

        static GameObject MakeChild()
        {
            var root = new GameObject("Child");
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
            body.GetComponent<Renderer>().sharedMaterial = M["Grey_Dark"];
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Face";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            head.transform.localScale = Vector3.one * 0.4f;
            head.GetComponent<Renderer>().sharedMaterial = M["Face"];
            var eyes = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eyes.name = "Eyes";
            eyes.transform.SetParent(head.transform, false);
            eyes.transform.localPosition = new Vector3(0f, 0.1f, 0.45f);
            eyes.transform.localScale = new Vector3(0.6f, 0.12f, 0.1f);
            eyes.GetComponent<Renderer>().sharedMaterial = M["Black"];
            Object.DestroyImmediate(eyes.GetComponent<Collider>());
            var wc = root.AddComponent<WeepingChild>();
            wc.bodyRenderer = body.GetComponent<Renderer>();
            return root;
        }

        static GameObject MakePersistent()
        {
            var root = new GameObject(SceneBootstrapper.PersistentPrefabResource);
            var gm = root.AddComponent<GameManager>();
            gm.flow = s_flow; gm.lines = s_lines; gm.forks = s_forks;

            var narrator = root.AddComponent<Narrator>();
            var voice = root.AddComponent<AudioSource>();
            voice.playOnAwake = false; voice.spatialBlend = 0f;
            narrator.voice = voice;

            var vote = root.AddComponent<KeyboardVoteProvider>();
            var forkCtl = root.AddComponent<ForkController>();
            forkCtl.providerBehaviour = vote;
            var fader = root.AddComponent<ScreenFader>();
            var dbg = root.AddComponent<DebugOverlay>();
            var haptics = root.AddComponent<TWIN.Haptics.HapticsService>();
            gm.narrator = narrator; gm.forkController = forkCtl; gm.fader = fader; gm.haptics = haptics;

            // Canvas
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var crosshair = UIImage(canvasGo.transform, "Crosshair", new Color(1f, 1f, 1f, 0.8f), Vector2.zero, new Vector2(6, 6));
            crosshair.raycastTarget = false;

            var prompt = UIText(canvasGo.transform, "Prompt", "", 26, TextAlignmentOptions.Center, new Vector2(0, -60), new Vector2(900, 40));
            var subtitle = UIText(canvasGo.transform, "Subtitle", "", 36, TextAlignmentOptions.Bottom, new Vector2(0, 80), new Vector2(1400, 160), new Vector2(0.5f, 0f));
            subtitle.fontStyle = FontStyles.Bold;
            narrator.subtitle = subtitle;

            // Fork panel
            var panel = UIImage(canvasGo.transform, "ForkPanel", new Color(0f, 0f, 0f, 0.75f), new Vector2(0, 60), new Vector2(900, 380));
            var title = UIText(panel.transform, "Title", "Vote", 40, TextAlignmentOptions.Center, new Vector2(0, 130), new Vector2(860, 60));
            var timer = UIText(panel.transform, "Timer", "10", 56, TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(200, 70));
            var tally = UIText(panel.transform, "Tally", "", 22, TextAlignmentOptions.Center, new Vector2(0, -150), new Vector2(860, 40));
            var bNormal = UIButton(panel.transform, "NormalButton", "1  Normal", new Vector2(-210, -40), new Vector2(380, 110), out var lNormal);
            var bStrange = UIButton(panel.transform, "StrangeButton", "2  Strange", new Vector2(210, -40), new Vector2(380, 110), out var lStrange);
            vote.panel = panel.gameObject; vote.titleText = title; vote.timerText = timer; vote.tallyText = tally;
            vote.normalButton = bNormal; vote.strangeButton = bStrange; vote.normalLabel = lNormal; vote.strangeLabel = lStrange;
            panel.gameObject.SetActive(false);

            // Ending texts (hidden until an ending shows them)
            var center = UIText(canvasGo.transform, "CenterText", "", 64, TextAlignmentOptions.Center, Vector2.zero, new Vector2(1400, 300));
            center.fontStyle = FontStyles.Bold;
            center.gameObject.SetActive(false);
            var restart = UIText(canvasGo.transform, "RestartPrompt", "", 30, TextAlignmentOptions.Bottom, new Vector2(0, 30), new Vector2(1200, 50), new Vector2(0.5f, 0f));
            restart.color = new Color(1f, 1f, 1f, 0.85f);
            restart.gameObject.SetActive(false);
            gm.centerText = center; gm.restartPromptText = restart;

            var dbgText = UIText(canvasGo.transform, "DebugText", "", 20, TextAlignmentOptions.TopLeft, new Vector2(10, -10), new Vector2(900, 200), new Vector2(0f, 1f));
            dbgText.color = new Color(0.6f, 1f, 0.6f);
            dbg.text = dbgText;

            var fade = UIImage(canvasGo.transform, "Fader", Color.black, Vector2.zero, Vector2.zero);
            var frt = fade.rectTransform; frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = frt.offsetMax = Vector2.zero;
            fade.raycastTarget = false;
            fader.image = fade;

            // Event system for the buttons
            var es = new GameObject("EventSystem");
            es.transform.SetParent(root.transform, false);
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            return root;
        }

        // ------------------------------------------------------------------ UI helpers

        static TMP_Text UIText(Transform parent, string name, string text, float size, TextAlignmentOptions align, Vector2 pos, Vector2 sizeDelta, Vector2? anchor = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            var a = anchor ?? new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = rt.pivot = a;
            rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.alignment = align; t.color = Color.white; t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        static Image UIImage(Transform parent, string name, Color color, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        static Button UIButton(Transform parent, string name, string label, Vector2 pos, Vector2 sizeDelta, out TMP_Text labelText)
        {
            var img = UIImage(parent, name, new Color(0.25f, 0.25f, 0.3f, 1f), pos, sizeDelta);
            var b = img.gameObject.AddComponent<Button>();
            var colors = b.colors; colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f); colors.pressedColor = new Color(0.6f, 0.6f, 0.7f); b.colors = colors;
            labelText = UIText(img.transform, "Label", label, 32, TextAlignmentOptions.Center, Vector2.zero, sizeDelta - new Vector2(20, 20));
            return b;
        }

        // ------------------------------------------------------------------ build settings

        static void SetBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var n in s_flow.All)
                list.Add(new EditorBuildSettingsScene($"{ScenesDir}/{n}.unity", true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
