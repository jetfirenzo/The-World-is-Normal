using TMPro;
using TWIN.Core;
using TWIN.Endings;
using TWIN.Scenes;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace TWIN.EditorTools
{
    /// <summary>
    /// The five ending scenes. Each is loaded additively on top of 07_LastMorning (same world origin), so
    /// rooms "behind the door" sit at z > 2.5, and rooms that replace the bedroom sit at x = +60.
    /// Every ending is driven by a Timeline (Data/Endings/End_*.playable): a Signal track whose markers call
    /// the controller's methods, plus Animation tracks for the moving props. Move the markers / clips in the
    /// Timeline window to retime an ending; no code change needed.
    /// </summary>
    public static partial class GreyboxBuilder
    {
        static void BuildEndings()
        {
            Build_End_GoodMorningSam();
            Build_End_TheNote();
            Build_End_TheFuneral();
            Build_End_WakeUp();
            Build_End_SystemError();
        }

        // ------------------------------------------------------------------ timeline helpers

        /// <summary>Everything needed to author one ending's Timeline from code.</summary>
        class TimelineBuild
        {
            public TimelineAsset timeline;
            public PlayableDirector director;
            public SignalTrack signals;
            public SignalReceiver receiver;
            public EndingController controller;
            public string prefix;

            /// <summary>At <paramref name="time"/> seconds, call the controller's parameterless method <paramref name="method"/>.</summary>
            public void Beat(double time, string method)
            {
                var signalPath = $"{SignalsDir}/{prefix}_{method}.signal";
                var signal = AssetDatabase.LoadAssetAtPath<SignalAsset>(signalPath);
                if (signal == null)
                {
                    signal = ScriptableObject.CreateInstance<SignalAsset>();
                    AssetDatabase.CreateAsset(signal, signalPath);
                }
                var emitter = signals.CreateMarker<SignalEmitter>(time);
                emitter.asset = signal;
                emitter.name = method;

                var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), controller, method);
                var evt = new UnityEvent();
                UnityEventTools.AddPersistentListener(evt, action);
                receiver.AddReaction(signal, evt);
            }

            /// <summary>
            /// An Animation track playing <paramref name="clip"/> from start for duration. The Animator sits on
            /// <paramref name="rig"/> (the animated prop's parent) and the clip animates the child by path. Never
            /// animate the Animator's own root transform: Timeline then applies its track offset (0,0,0 by default)
            /// and the prop jumps to the world origin.
            /// </summary>
            public void Animate(GameObject rig, AnimationClip clip, double start, double duration, string trackName)
            {
                // Never use ?? on Unity objects: a missing component is a "fake null" that ?? treats as non-null.
                var animator = rig.GetComponent<Animator>();
                if (animator == null) animator = rig.AddComponent<Animator>();
                var track = timeline.CreateTrack<AnimationTrack>(null, trackName);
                var tc = track.CreateClip(clip);
                tc.start = start;
                tc.duration = duration;
                tc.displayName = clip.name;
                director.SetGenericBinding(track, animator);
            }
        }

        static TimelineBuild NewTimeline(string endingName, EndingController controller)
        {
            var path = $"{EndingsDir}/{endingName}.playable";
            AssetDatabase.DeleteAsset(path);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = endingName;
            AssetDatabase.CreateAsset(timeline, path);

            var director = controller.GetComponent<PlayableDirector>();
            if (director == null) director = controller.gameObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = false; // EndingController.Start plays it after it found the bridge
            director.extrapolationMode = DirectorWrapMode.Hold;
            controller.director = director;

            var receiver = controller.gameObject.AddComponent<SignalReceiver>();
            var signals = timeline.CreateTrack<SignalTrack>(null, "Beats");
            director.SetGenericBinding(signals, controller.gameObject);

            return new TimelineBuild { timeline = timeline, director = director, signals = signals, receiver = receiver, controller = controller, prefix = endingName };
        }

        /// <summary>A clip that moves the child at <paramref name="path"/> from a to b in its parent's space (ease in/out).</summary>
        static AnimationClip MoveClip(string name, string path, Vector3 from, Vector3 to, float seconds)
        {
            var clip = new AnimationClip { name = name };
            clip.SetCurve(path, typeof(Transform), "localPosition.x", AnimationCurve.EaseInOut(0f, from.x, seconds, to.x));
            clip.SetCurve(path, typeof(Transform), "localPosition.y", AnimationCurve.EaseInOut(0f, from.y, seconds, to.y));
            clip.SetCurve(path, typeof(Transform), "localPosition.z", AnimationCurve.EaseInOut(0f, from.z, seconds, to.z));
            return SaveClip(clip);
        }

        /// <summary>A clip that rotates the child at <paramref name="path"/> through local Euler angles a to b (ease in/out).</summary>
        static AnimationClip RotateClip(string name, string path, Vector3 from, Vector3 to, float seconds)
        {
            var clip = new AnimationClip { name = name };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.EaseInOut(0f, from.x, seconds, to.x));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.EaseInOut(0f, from.y, seconds, to.y));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.EaseInOut(0f, from.z, seconds, to.z));
            return SaveClip(clip);
        }

        /// <summary>An empty parent for an animated prop (holds the Animator), placed at the prop's parent's origin.</summary>
        static GameObject Rig(string name, Transform parent, Vector3? pos = null)
        {
            return Group(name, parent, pos ?? Vector3.zero);
        }

        static AnimationClip SaveClip(AnimationClip clip)
        {
            var path = $"{ClipsDir}/{clip.name}.anim";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        static GameObject StandPoint(Transform parent, Vector3 pos, float yaw, float pitch = 0f)
        {
            return Group("StandPoint", parent, pos, new Vector3(pitch, yaw, 0f));
        }

        /// <summary>The last morning's north wall with a 1 x 2.1 m doorway where the appeared door is (x = -1.2).</summary>
        static GameObject DoorwayWall(Transform parent)
        {
            var g = Group("DoorwayWall", parent);
            const float h = 2.8f, z = 2.55f, t = 0.1f;
            Box("Left", g.transform, new Vector3(-2.15f, h / 2, z), new Vector3(0.9f, h, t), "Grey_Wall");     // x -2.6 .. -1.7
            Box("Right", g.transform, new Vector3(0.95f, h / 2, z), new Vector3(3.3f, h, t), "Grey_Wall");     // x -0.7 .. 2.6
            Box("Lintel", g.transform, new Vector3(-1.2f, 2.45f, z), new Vector3(1.0f, 0.7f, t), "Grey_Wall"); // y 2.1 .. 2.8
            return g;
        }

        static Scene NewEndingScene()
        {
            var scene = NewScene(withLight: false);
            RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.4f);
            return scene;
        }

        // ------------------------------------------------------------------ 1. GOOD MORNING, SAM

        static void Build_End_GoodMorningSam()
        {
            var scene = NewEndingScene();
            var b = BuildBedroom(wrong: false);
            b.root.position = new Vector3(60f, 0f, 0f);
            b.alarmRenderer.sharedMaterial = M["Grey_Prop"];
            var stand = StandPoint(b.root, new Vector3(0.3f, 0f, -0.8f), 20f);

            var ctl = new GameObject("GoodMorningSamController").AddComponent<GoodMorningSamController>();
            ctl.ending = Ending.GoodMorningSam;
            ctl.standPoint = stand.transform;
            ctl.alarmAudio = b.alarmAudio; ctl.alarmRenderer = b.alarmRenderer; ctl.clockText = b.clock;
            ctl.fadeSeconds = 3f;

            var tl = NewTimeline(s_flow.endGoodMorningSam, ctl);
            tl.Beat(0.2, nameof(EndingController.LieDown));
            tl.Beat(1.5, nameof(EndingController.FadeToBlack));          // eyes close, 3 s
            tl.Beat(5.0, nameof(GoodMorningSamController.SayGoodBoy));   // "Good boy, Sam." (2.5 s)
            tl.Beat(10.0, nameof(GoodMorningSamController.AlarmRings));  // 2 s of black and silence, then the alarm, 11:11
            tl.Beat(10.5, nameof(EndingController.FadeFromBlack));       // back in the normal bedroom
            tl.Beat(14.0, nameof(EndingController.ShowRestartPrompt));
            tl.Beat(16.0, nameof(GoodMorningSamController.AlarmStops));
            Save(scene, s_flow.endGoodMorningSam);
        }

        // ------------------------------------------------------------------ 2. THE NOTE

        static void Build_End_TheNote()
        {
            var scene = NewEndingScene();
            var b = BuildBedroom(wrong: false);
            b.root.position = new Vector3(60f, 0f, 0f);
            b.alarmRenderer.sharedMaterial = M["Grey_Prop"];

            // Someone else in the bed: the shared NPC prefab, lying down, face turned to the wall, under a duvet.
            // FigureRig (Animator) -> Hips (the sit-up pivot, animated) -> FigureInBed (feet 0.7 m below the hips).
            var figureRig = Rig("FigureRig", b.root);
            var hips = Group("Hips", figureRig.transform, new Vector3(1.6f, 0.65f, 1.25f), new Vector3(-90f, 180f, 0f)); // lying, head to the pillow (+z)
            var figure = (GameObject)PrefabUtility.InstantiatePrefab(s_childPrefab);
            figure.name = "FigureInBed";
            figure.transform.SetParent(hips.transform, false);
            figure.transform.localPosition = new Vector3(0f, -0.7f, 0f);
            figure.GetComponent<WeepingChild>().enabled = false; // the Timeline animates it; no look-at-player rule
            figure.transform.Find("Face").localRotation = Quaternion.Euler(0f, -90f, 0f);     // face turned away from the player, to the wall
            var duvet = Box("Duvet", b.root, new Vector3(1.6f, 0.75f, 0.9f), new Vector3(1.5f, 0.25f, 1.5f), "Accent_Blue");
            Object.DestroyImmediate(duvet.GetComponent<Collider>());

            // PROP_Note on the nightstand, with a pen beside it, and the hand that will write.
            var note = Box("PROP_Note", b.root, new Vector3(0.5f, 0.615f, 2.0f), new Vector3(0.22f, 0.01f, 0.16f), "White", new Vector3(0, -10f, 0));
            Object.DestroyImmediate(note.GetComponent<Collider>());
            var noteText = Label3D(note.transform, "", new Vector3(0, 0.51f, 0), new Vector3(90f, 0, 0), new Vector2(0.2f, 0.14f), 0.3f);
            noteText.fontStyle = FontStyles.Italic;
            var handRig = Rig("HandRig", b.root);
            var hand = Box("Hand", handRig.transform, new Vector3(0.55f, 0.95f, 1.25f), new Vector3(0.09f, 0.04f, 0.18f), "Face");
            Object.DestroyImmediate(hand.GetComponent<Collider>());
            var pen = Prim(PrimitiveType.Cylinder, "Pen", hand.transform, new Vector3(0.6f, 0f, 0.6f), new Vector3(0.15f, 2.0f, 0.1f), "Grey_Dark", new Vector3(60f, 0, 0));
            Object.DestroyImmediate(pen.GetComponent<Collider>());

            // Standing beside the bed (west side): nightstand ahead-left, the figure in the bed ahead-right.
            var stand = StandPoint(b.root, new Vector3(0.3f, 0f, 0.9f), 35f, 30f);

            var ctl = new GameObject("TheNoteController").AddComponent<TheNoteController>();
            ctl.ending = Ending.TheNote;
            ctl.standPoint = stand.transform;
            ctl.noteText = noteText; ctl.hand = hand.transform; ctl.figureInBed = figure.transform;
            ctl.fadeSeconds = 1f; ctl.writeSeconds = 4f;

            var tl = NewTimeline(s_flow.endTheNote, ctl);
            tl.Beat(0.2, nameof(EndingController.LieDown));
            tl.Beat(1.0, nameof(EndingController.FadeToBlack));                // short fade (1 s)
            tl.Beat(2.5, nameof(TheNoteController.ArriveStanding));           // suddenly standing next to the bed
            tl.Beat(4.5, nameof(TheNoteController.HandStartsMoving));         // slow haptic pulse
            tl.Animate(handRig, MoveClip("TheNote_HandToNote", "Hand", hand.transform.localPosition, new Vector3(0.66f, 0.65f, 1.86f), 2.5f) /* beside the note, not on it */, 4.5, 2.5, "Hand moves on its own");
            tl.Beat(7.2, nameof(TheNoteController.WriteNote));                // letters appear (writeSeconds)
            tl.Beat(11.5, nameof(EndingController.StopHaptics));
            tl.Animate(figureRig, RotateClip("TheNote_FigureSitsUp", "Hips", new Vector3(-90f, 180f, 0f), new Vector3(-20f, 180f, 0f), 2.5f), 11.5, 2.5, "Figure sits up");
            tl.Beat(13.8, nameof(EndingController.CutToBlack));
            tl.Beat(14.5, nameof(EndingController.ShowRestartPrompt));
            Save(scene, s_flow.endTheNote);
        }

        // ------------------------------------------------------------------ 3. THE FUNERAL

        static void Build_End_TheFuneral()
        {
            var scene = NewEndingScene();
            RenderSettings.ambientLight = new Color(0.18f, 0.16f, 0.16f);
            var root = Group("FuneralRoom").transform;
            var doorway = DoorwayWall(root);

            // Small room behind the last morning's north wall: x -4 .. 2, z 2.6 .. 9.6, 3 m high.
            const float t = 0.1f;
            Box("Floor", root, new Vector3(-1f, -t / 2, 6.1f), new Vector3(6f, t, 7f), "Grey_Dark");
            Box("Ceiling", root, new Vector3(-1f, 3f + t / 2, 6.1f), new Vector3(6f, t, 7f), "Grey_Dark");
            Box("Wall_North", root, new Vector3(-1f, 1.5f, 9.65f), new Vector3(6f, 3f, t), "Grey_Wall");
            Box("Wall_East", root, new Vector3(2.05f, 1.5f, 6.1f), new Vector3(t, 3f, 7f), "Grey_Wall");
            Box("Wall_West", root, new Vector3(-4.05f, 1.5f, 6.1f), new Vector3(t, 3f, 7f), "Grey_Wall");
            Box("Wall_South_L", root, new Vector3(-3.3f, 1.5f, 2.65f), new Vector3(1.5f, 3f, t), "Grey_Wall"); // beyond the doorway wall's ends
            Box("Carpet", root, new Vector3(-1f, 0.01f, 6f), new Vector3(1.2f, 0.02f, 6f), "Accent_Red");
            PointLight(root, new Vector3(-1f, 2.7f, 7.5f), 2.5f, 8f, new Color(1f, 0.85f, 0.7f));
            PointLight(root, new Vector3(-1f, 2.7f, 4.5f), 1.2f, 7f, new Color(1f, 0.85f, 0.7f));

            // Rows of chairs.
            var chairs = Group("Chairs", root);
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 4; i++)
                {
                    float x = (i < 2 ? -3.3f + i * 0.6f : 0.2f + (i - 2) * 0.6f);
                    float z = 3.6f + row * 0.9f;
                    Box("Chair", chairs.transform, new Vector3(x, 0.25f, z), new Vector3(0.45f, 0.5f, 0.45f), "Wood");
                    var back = Box("Back", chairs.transform, new Vector3(x, 0.75f, z + 0.2f), new Vector3(0.45f, 0.5f, 0.05f), "Wood");
                    Object.DestroyImmediate(back.GetComponent<Collider>());
                }

            // Open coffin with Sam in it.
            var stand = Box("CoffinStand", root, new Vector3(-1f, 0.35f, 8.2f), new Vector3(0.8f, 0.7f, 2.2f), "Grey_Dark");
            var coffin = Box("Coffin", root, new Vector3(-1f, 0.95f, 8.2f), new Vector3(0.9f, 0.5f, 2.3f), "Wood");
            // Sam in the player's blue (the mother has the NPC face, Sam does not).
            var body = Prim(PrimitiveType.Capsule, "Sam", coffin.transform, new Vector3(0, 0.3f, 0), new Vector3(0.5f, 0.42f, 0.5f), "Accent_Blue", new Vector3(90f, 0, 0));
            Object.DestroyImmediate(body.GetComponent<Collider>());
            var head = Prim(PrimitiveType.Sphere, "Head", coffin.transform, new Vector3(0, 0.45f, -0.35f), new Vector3(0.35f, 0.6f, 0.13f), "Accent_Blue");
            Object.DestroyImmediate(head.GetComponent<Collider>());
            var plate = Box("Nameplate", root, new Vector3(-1f, 0.8f, 6.95f), new Vector3(0.4f, 0.12f, 0.02f), "Grey_Prop");
            Object.DestroyImmediate(plate.GetComponent<Collider>());
            Label3D(plate.transform, s_lines.coffinNameplate, new Vector3(0, 0, -0.6f), Vector3.zero, new Vector2(0.4f, 0.12f), 0.8f, Color.black).fontStyle = FontStyles.Bold;
            for (int i = 0; i < 5; i++) Bouquet(root, new Vector3(-2.2f + i * 0.6f, 0f, 9.2f), 1.2f);
            Bouquet(root, new Vector3(-1.8f, 0f, 7.0f), 1f);
            Bouquet(root, new Vector3(-0.2f, 0f, 7.0f), 1f);

            // His mother: same face as the shopkeeper and the child. Starts facing the coffin, turns to the player.
            var motherRig = Rig("MotherRig", root);
            var mother = (GameObject)PrefabUtility.InstantiatePrefab(s_childPrefab);
            mother.name = "Mother";
            mother.transform.SetParent(motherRig.transform, false);
            mother.transform.localPosition = new Vector3(0.1f, 0f, 8.0f);
            mother.transform.localScale = Vector3.one * 1.3f;
            mother.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            mother.GetComponent<WeepingChild>().enabled = false; // the Timeline turns her, not the look-away rule
            var organ = root.gameObject.AddComponent<AudioSource>();
            organ.playOnAwake = false; organ.spatialBlend = 0f;

            var standPt = StandPoint(root, new Vector3(-0.6f, 0f, 5.3f), 10f, 5f); // a step back, coffin and mother both in view

            var ctl = new GameObject("TheFuneralController").AddComponent<TheFuneralController>();
            ctl.ending = Ending.TheFuneral;
            ctl.standPoint = standPt.transform; ctl.doorwayWall = doorway;
            ctl.organ = organ; ctl.mother = mother.transform;
            ctl.walkSeconds = 4f; ctl.doorSwingSeconds = 1.5f;

            var tl = NewTimeline(s_flow.endTheFuneral, ctl);
            tl.Beat(0.2, nameof(EndingController.OpenDoor));
            tl.Beat(0.2, nameof(TheFuneralController.OrganOn));
            tl.Beat(0.2, nameof(TheFuneralController.HeartbeatStarts));
            tl.Beat(2.0, nameof(EndingController.WalkToStandPoint));       // 4 s, to the coffin
            tl.Beat(6.5, nameof(TheFuneralController.HeartbeatRamps));     // faster as she turns
            tl.Animate(motherRig, RotateClip("TheFuneral_MotherTurns", "Mother", new Vector3(0f, -90f, 0f), new Vector3(0f, -165f, 0f), 4f) /* -165 = facing the stand point */, 6.5, 4.0, "Mother turns");
            tl.Beat(10.8, nameof(TheFuneralController.SayMorningSam));
            tl.Beat(13.5, nameof(EndingController.CutToBlack));            // hold one beat, then black
            tl.Beat(13.5, nameof(EndingController.StopHaptics));
            tl.Beat(14.5, nameof(EndingController.ShowRestartPrompt));
            Save(scene, s_flow.endTheFuneral);
        }

        // ------------------------------------------------------------------ 4. WAKE UP

        static void Build_End_WakeUp()
        {
            var scene = NewEndingScene();
            RenderSettings.ambientLight = Color.white;
            var root = Group("WhiteRoom").transform;
            var doorway = DoorwayWall(root);
            const float t = 0.1f;
            Box("Floor", root, new Vector3(-1f, -t / 2, 7.1f), new Vector3(8f, t, 9f), "White");
            Box("Ceiling", root, new Vector3(-1f, 4f + t / 2, 7.1f), new Vector3(8f, t, 9f), "White");
            Box("Wall_North", root, new Vector3(-1f, 2f, 11.65f), new Vector3(8f, 4f, t), "White");
            Box("Wall_East", root, new Vector3(3.05f, 2f, 7.1f), new Vector3(t, 4f, 9f), "White");
            Box("Wall_West", root, new Vector3(-5.05f, 2f, 7.1f), new Vector3(t, 4f, 9f), "White");
            Box("Wall_South_L", root, new Vector3(-3.8f, 2f, 2.65f), new Vector3(2.5f, 4f, t), "White");
            Box("Wall_South_R", root, new Vector3(2.8f, 2f, 2.65f), new Vector3(0.5f, 4f, t), "White");
            for (int i = 0; i < 3; i++) PointLight(root, new Vector3(-1f, 3.5f, 4f + i * 3f), 6f, 12f, Color.white);

            var standPt = StandPoint(root, new Vector3(-1f, 0f, 6.5f), 0f);

            var ctl = new GameObject("WakeUpController").AddComponent<WakeUpController>();
            ctl.ending = Ending.WakeUp;
            ctl.standPoint = standPt.transform; ctl.doorwayWall = doorway;
            ctl.walkSeconds = 4f; ctl.silenceSeconds = 3f; ctl.whiteSeconds = 3f;

            var tl = NewTimeline(s_flow.endWakeUp, ctl);
            tl.Beat(0.2, nameof(EndingController.StopHaptics));
            tl.Beat(0.2, nameof(EndingController.OpenDoor));                // onto pure white
            tl.Beat(2.0, nameof(EndingController.WalkToStandPoint));
            tl.Beat(6.5, nameof(WakeUpController.FadeToWhiteAndSilence));   // 3 s to silence and white
            tl.Beat(10.0, nameof(WakeUpController.ShowTakeOffHeadset));
            tl.Beat(10.0, nameof(WakeUpController.EnablePassthrough));
            tl.Beat(12.0, nameof(EndingController.ShowRestartPrompt));
            Save(scene, s_flow.endWakeUp);
        }

        // ------------------------------------------------------------------ 5. SYSTEM ERROR (placeholder)

        static void Build_End_SystemError()
        {
            var scene = NewEndingScene();
            var ctl = new GameObject("SystemErrorController").AddComponent<SystemErrorController>();
            ctl.ending = Ending.SystemError;
            ctl.glitchSeconds = 2f;

            var tl = NewTimeline(s_flow.endSystemError, ctl);
            tl.Beat(0.2, nameof(SystemErrorController.Glitch));
            tl.Beat(2.5, nameof(SystemErrorController.ShowSystemError));
            tl.Beat(5.0, nameof(EndingController.ShowRestartPrompt));
            Save(scene, s_flow.endSystemError);
        }
    }
}
