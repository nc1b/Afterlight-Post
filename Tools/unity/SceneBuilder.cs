using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SceneBuilder
{
    const string Tiles = "Assets/Sprites/Tiles/";
    const string UI = "Assets/Sprites/UI/";
    const string Bg = "Assets/Sprites/Backgrounds/";
    const string Prefabs = "Assets/Prefabs/";
    const string ScenePath = "Assets/Scenes/Main.unity";

    static Texture2D fontTex;
    static readonly Color Cream = new Color32(0xe9, 0xdf, 0xc8, 255);
    static readonly Color Amber = new Color32(0xff, 0xcf, 0x5c, 255);
    static readonly Color Dim = new Color32(0x7d, 0x8c, 0xb3, 255);

    public static void BuildAll()
    {
        ImportSettings();
        MakePrefabs();
        MakeScene();
        ProjectSetup();
        AssetDatabase.SaveAssets();
        Debug.Log("SCENEBUILDER DONE");
    }

    public static void BuildWindows()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.IndexOf(args, "-out");
        string target = args[flag + 1];
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(target)));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = target,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("build failed: " + report.summary.result);
        Debug.Log("PLAYER BUILD OK " + report.summary.totalSize);
    }

    static void ImportSettings()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites", "Assets/Fonts" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            bool font = path.StartsWith("Assets/Fonts");
            ti.textureType = font ? TextureImporterType.Default : TextureImporterType.Sprite;
            if (!font)
            {
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = 16;
                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                ti.SetTextureSettings(settings);
                if (path.EndsWith("panel.png")) ti.spriteBorder = new Vector4(5, 5, 5, 5);
            }
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.SaveAndReimport();
        }
        var music = (AudioImporter)AssetImporter.GetAtPath("Assets/Audio/night_shift.wav");
        var sample = music.defaultSampleSettings;
        sample.loadType = AudioClipLoadType.CompressedInMemory;
        sample.compressionFormat = AudioCompressionFormat.Vorbis;
        sample.quality = 0.6f;
        music.defaultSampleSettings = sample;
        music.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    static Sprite Spr(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) throw new Exception("missing sprite " + path);
        return s;
    }

    static AudioClip Clip(string name)
    {
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + name + ".wav");
        if (c == null) throw new Exception("missing clip " + name);
        return c;
    }

    static SpriteRenderer AddSprite(GameObject go, string path, int order)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr(path);
        sr.sortingOrder = order;
        return sr;
    }

    static GameObject Child(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static GameObject SavePrefab(GameObject go)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + go.name + ".prefab");
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject floorP, waterP, wallP, bellP, mirrorP, hingeP, lampP, gateP, beamP, postieP;

    static void MakePrefabs()
    {
        Directory.CreateDirectory("Assets/Prefabs");

        var go = new GameObject("Floor");
        AddSprite(go, Tiles + "floor_a.png", 0);
        floorP = SavePrefab(go);

        go = new GameObject("Water");
        var wsr = AddSprite(go, Tiles + "water_0.png", 0);
        var wt = go.AddComponent<WaterTile>();
        wt.sr = wsr;
        wt.frames = new[] { Spr(Tiles + "water_0.png"), Spr(Tiles + "water_1.png") };
        waterP = SavePrefab(go);

        go = new GameObject("Wall");
        AddSprite(go, Tiles + "wall_a.png", 1);
        wallP = SavePrefab(go);

        go = new GameObject("Bell");
        AddSprite(go, Tiles + "bell.png", 3);
        go.AddComponent<BellView>();
        bellP = SavePrefab(go);

        go = new GameObject("Mirror");
        AddSprite(go, Tiles + "mirror.png", 3);
        go.AddComponent<MirrorView>();
        mirrorP = SavePrefab(go);

        go = new GameObject("Striped Mirror");
        AddSprite(go, Tiles + "mirror_hinge.png", 3);
        go.AddComponent<MirrorView>();
        hingeP = SavePrefab(go);

        go = new GameObject("Lamp");
        AddSprite(go, Tiles + "lamp_post.png", 3);
        var lamp = go.AddComponent<LampView>();
        lamp.glass = AddSprite(Child(go, "Glass"), Tiles + "lamp_glass.png", 4);
        var glowGo = Child(go, "Glow");
        glowGo.transform.localPosition = new Vector3(0, 0.1875f, 0);
        lamp.glow = AddSprite(glowGo, Tiles + "glow.png", 2);
        lamp.glow.enabled = false;
        lampP = SavePrefab(go);

        go = new GameObject("Gate");
        var gate = go.AddComponent<GateView>();
        gate.body = AddSprite(go, Tiles + "gate_closed.png", 3);
        gate.closedSprite = Spr(Tiles + "gate_closed.png");
        gate.openSprite = Spr(Tiles + "gate_open.png");
        gateP = SavePrefab(go);

        go = new GameObject("Beam");
        AddSprite(go, Tiles + "beam.png", 6);
        beamP = SavePrefab(go);

        go = new GameObject("Postie");
        var player = go.AddComponent<Player>();
        AddSprite(Child(go, "Shadow"), Tiles + "shadow.png", 4);
        var lantern = Child(go, "Lantern Glow");
        lantern.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        AddSprite(lantern, Tiles + "glow.png", 2).color = new Color(1f, 0.81f, 0.36f, 0.3f);
        player.body = AddSprite(Child(go, "Body"), Tiles + "postie_front.png", 5);
        player.front = Spr(Tiles + "postie_front.png");
        player.frontStep = Spr(Tiles + "postie_front_step.png");
        player.back = Spr(Tiles + "postie_back.png");
        player.backStep = Spr(Tiles + "postie_back_step.png");
        postieP = SavePrefab(go);
    }

    static RectTransform TopLeft(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static GameObject Panel(GameObject parent, string name)
    {
        var go = Child(parent, name);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static Image Img(GameObject parent, string name, string sprite, float x, float y, float w, float h)
    {
        var go = Child(parent, name);
        TopLeft(go, x, y, w, h);
        var img = go.AddComponent<Image>();
        img.sprite = Spr(sprite);
        img.raycastTarget = false;
        return img;
    }

    static PixelText Label(GameObject parent, string name, string text, float y, TextAnchor anchor, Color colour, int scale = 1, float x = 0, float w = 320)
    {
        var go = Child(parent, name);
        TopLeft(go, x, y, w, 9 * scale * Mathf.Max(1, text.Split('\n').Length));
        var pt = go.AddComponent<PixelText>();
        pt.fontTexture = fontTex;
        pt.scale = scale;
        pt.anchor = anchor;
        pt.color = colour;
        pt.raycastTarget = false;
        pt.Text = text;
        return pt;
    }

    static void MakeScene()
    {
        fontTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Fonts/postfont.png");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(0x19, 0x1c, 0x30, 255);
        var shake = camGo.AddComponent<CameraShake>();

        var boardGo = new GameObject("Board");
        var view = boardGo.AddComponent<BoardView>();
        view.floorPrefab = floorP; view.waterPrefab = waterP; view.wallPrefab = wallP; view.bellPrefab = bellP;
        view.mirrorPrefab = mirrorP; view.hingePrefab = hingeP; view.lampPrefab = lampP; view.gatePrefab = gateP; view.beamPrefab = beamP;
        view.floorSprites = new[] { Spr(Tiles + "floor_a.png"), Spr(Tiles + "floor_b.png") };
        view.wallSprites = new[] { Spr(Tiles + "wall_a.png"), Spr(Tiles + "wall_b.png") };
        view.counterSprite = Spr(Tiles + "counter.png");
        view.parcelsSprite = Spr(Tiles + "parcels.png");
        view.windowDark = Spr(Tiles + "window_dark.png");
        view.windowLit = Spr(Tiles + "window_lit.png");
        view.slipSprite = Spr(Tiles + "return_slip.png");
        view.scratchSprite = Spr(Tiles + "return_arrow.png");
        view.mailboxDark = Spr(Tiles + "mailbox_dark.png");
        view.mailboxOpen = Spr(Tiles + "mailbox_open.png");
        view.tiles = Child(boardGo, "Tiles").transform;
        view.beams = Child(boardGo, "Beams").transform;
        view.shake = shake;
        view.outSpeed = 21f;
        view.backSpeed = 16f;
        view.boardYOffset = 0.4f;
        view.lampColours = new Color[]
        {
            new Color32(0xff, 0xcf, 0x5c, 255), new Color32(0x6f, 0xe3, 0xd6, 255),
            new Color32(0xff, 0x8f, 0xb3, 255), new Color32(0xa6, 0xe8, 0x6b, 255)
        };

        var pulseGo = Child(boardGo, "Pulse");
        view.pulse = AddSprite(pulseGo, Tiles + "pulse.png", 8);
        var pulseGlow = Child(pulseGo, "Glow");
        pulseGlow.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        view.pulseGlow = AddSprite(pulseGlow, Tiles + "glow.png", 7);
        pulseGo.SetActive(false);

        var sparkMat = new Material(Shader.Find("Sprites/Default"));
        sparkMat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Tiles + "spark.png");
        AssetDatabase.CreateAsset(sparkMat, "Assets/Sprites/Spark.mat");
        var sparksGo = Child(boardGo, "Sparks");
        var ps = sparksGo.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.125f, 0.1875f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 0.8f;
        limit.dampen = 0.12f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
        col.color = grad;
        var psr = sparksGo.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = sparkMat;
        psr.sortingOrder = 9;
        view.sparks = ps;

        var postie = (GameObject)PrefabUtility.InstantiatePrefab(postieP);

        var gameGo = new GameObject("Game");
        var game = gameGo.AddComponent<Game>();
        var sfx = gameGo.AddComponent<Sfx>();
        var srcGo = Child(gameGo, "Sfx Source");
        sfx.source = srcGo.AddComponent<AudioSource>();
        sfx.source.playOnAwake = false;
        var musicGo = Child(gameGo, "Music");
        sfx.music = musicGo.AddComponent<AudioSource>();
        sfx.music.clip = Clip("night_shift");
        sfx.music.loop = true;
        sfx.music.playOnAwake = true;
        sfx.music.volume = 0.55f;
        sfx.step = Clip("step"); sfx.send = Clip("send"); sfx.tick = Clip("tick"); sfx.bell = Clip("bell");
        sfx.thud = Clip("thud"); sfx.lamp = Clip("lamp"); sfx.flip = Clip("flip"); sfx.gate = Clip("gate");
        sfx.undo = Clip("undo"); sfx.ui = Clip("ui"); sfx.empty = Clip("empty"); sfx.clear = Clip("clear");

        game.levels = new[]
        {
            AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Levels/01 First Light.txt"),
            AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Levels/02 Round the Corner.txt"),
            AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Levels/10 Home Signal.txt")
        };
        game.view = view;
        game.player = postie.GetComponent<Player>();

        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(320, 180);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1;
        scaler.referencePixelsPerUnit = 16;
        var hud = canvasGo.AddComponent<Hud>();
        game.hud = hud;

        var frame = Child(canvasGo, "Frame");
        var frt = frame.AddComponent<RectTransform>();
        frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0.5f, 0.5f);
        frt.sizeDelta = new Vector2(320, 180);

        var play = Panel(frame, "Play");
        hud.playPanel = play;
        hud.levelName = Label(play, "Level Name", "01/03  RECEIVING", 4, TextAnchor.UpperLeft, Cream, 1, 6, 200);
        hud.pips = new Image[8];
        for (int i = 0; i < 8; i++)
            hud.pips[i] = Img(play, "Light " + (i + 1), UI + "pip_full.png", 320 - 6 - (8 - i) * 9, 2, 7, 9);
        hud.pipFull = Spr(UI + "pip_full.png");
        hud.pipEmpty = Spr(UI + "pip_empty.png");
        hud.tip = Label(play, "Tip", "TIP LINE ONE\nTIP LINE TWO", 152, TextAnchor.UpperCenter, Cream);

        Label(play, "Shots", "SHOTS", 4, TextAnchor.UpperLeft, Dim, 1, 240, 36);
        Label(play, "Keys", "WASD MOVE   ARROWS SHOOT   Z UNDO   R RESTART", 171, TextAnchor.UpperCenter, Cream);

        var clear = Img(play, "Clear Box", UI + "panel.png", 40, 151, 240, 18);
        clear.type = Image.Type.Sliced;
        hud.clearBox = clear.gameObject;
        hud.clearPrompt = Label(clear.gameObject, "Prompt", "ROOM LIT!   SPACE - NEXT ROOM", 6, TextAnchor.UpperCenter, Amber, 1, 0, 240);
        clear.gameObject.SetActive(false);

        var outBox = Img(play, "Out Box", UI + "panel.png", 35, 151, 250, 18);
        outBox.type = Image.Type.Sliced;
        hud.outBox = outBox.gameObject;
        Label(outBox.gameObject, "Prompt", "OUT OF LIGHT!   Z - UNDO   R - RESTART", 6, TextAnchor.UpperCenter, new Color32(0xff, 0x8f, 0x7a, 255), 1, 0, 250);
        outBox.gameObject.SetActive(false);

        var letter = Img(play, "Letter", UI + "panel.png", 42, 57, 236, 67);
        letter.type = Image.Type.Sliced;
        hud.letterBox = letter.gameObject;
        Label(hud.letterBox, "Heading", "UNDELIVERED LETTER", 8, TextAnchor.UpperCenter, Amber, 1, 0, 236);
        Label(hud.letterBox, "Message", "IF YOU FIND THIS,\nLEAVE A LIGHT ON FOR ME.\n- THE LAST POSTIE",
            22, TextAnchor.UpperCenter, Cream, 1, 0, 236);
        Label(hud.letterBox, "Close", "SPACE - FOLD LETTER", 54, TextAnchor.UpperCenter, Amber, 1, 0, 236);
        hud.letterBox.SetActive(false);

        var title = Panel(frame, "Title");
        hud.titlePanel = title;
        Img(title, "Night", Bg + "town_dark.png", 0, 0, 320, 180);
        Img(title, "Logo", UI + "logo.png", 14, 12, 244, 74);
        Label(title, "Tagline", "BOUNCE BACK", 94, TextAnchor.UpperLeft, Cream, 1, 16);
        Label(title, "Press Start", "SPACE - START SHIFT", 118, TextAnchor.UpperLeft, Amber, 1, 16);
        Label(title, "Keys", "M - SOUND    F11 - FULL SCREEN    ESC - QUIT", 167, TextAnchor.UpperCenter, Dim);

        var end = Panel(frame, "Ending");
        hud.endPanel = end;
        Img(end, "Morning", Bg + "town_lit.png", 0, 0, 320, 180);
        Label(end, "Line 1", "THE POST OFFICE\nIS OPEN.", 12, TextAnchor.UpperLeft, Cream, 2, 14);
        Label(end, "Line 2", "FOUR LAMPS BACK ON.\nMAIL GOES OUT AT DAWN.", 56, TextAnchor.UpperLeft, Amber, 1, 14);
        hud.recoveredLetter = Label(end, "Recovered letter", "ONE LOST LETTER FOUND.", 93, TextAnchor.UpperLeft, Amber, 1, 14);
        Label(end, "Keys", "SPACE - BACK TO TITLE", 167, TextAnchor.UpperCenter, Dim);

        var fadeGo = Panel(canvasGo, "Fade");
        var fade = fadeGo.AddComponent<Image>();
        fade.color = new Color(0.02f, 0.04f, 0.08f, 0);
        fade.raycastTarget = false;
        fade.enabled = false;
        hud.fade = fade;

        play.SetActive(false);
        end.SetActive(false);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    static void ProjectSetup()
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "Return Office";
        PlayerSettings.productName = "Afterlight Post";
        PlayerSettings.bundleVersion = "1.1.0";
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 });
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(UI + "icon.png");
        var sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Application);
        var icons = new Texture2D[sizes.Length];
        for (int i = 0; i < icons.Length; i++) icons[i] = icon;
        PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Application);
    }
}
