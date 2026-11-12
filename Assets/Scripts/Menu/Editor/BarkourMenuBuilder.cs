using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static partial class BarkourMenuBuilder
{
    private const string Request = "Temp/BarkourMenuBuild.request";
    private static Color Brown = Hex("49372D"), Cream = Hex("FFF0CF"), Sage = Hex("AAC09A"), Blue = Hex("7095A2");
    private static TMP_FontAsset font;
    [InitializeOnLoadMethod]
    private static void Initialize() { EditorApplication.delayCall += RequestedBuild; }
    private static void RequestedBuild()
    {
        if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        try { Build(); File.WriteAllText("Temp/BarkourMenuBuild.result", "PASS: MainMenu built and saved."); }
        catch (Exception e) { File.WriteAllText("Temp/BarkourMenuBuild.result", e.ToString()); Debug.LogException(e); }
    }
    [MenuItem("Barkour/Build Main Menu")]
    public static void Build()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save the open scene before building the menu; no scene was changed.");
        Directory.CreateDirectory("Assets/Menu");
        Directory.CreateDirectory("Docs/MainMenuBackup");
        if (!File.Exists("Docs/MainMenuBackup/MainMenu.unity.txt"))
            File.Copy("Assets/Scenes/MainMenu.unity", "Docs/MainMenuBackup/MainMenu.unity.txt");
        if (!File.Exists("Docs/MainMenuBackup/EditorBuildSettings.asset.txt"))
            File.Copy("ProjectSettings/EditorBuildSettings.asset", "Docs/MainMenuBackup/EditorBuildSettings.asset.txt");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 5;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Hex("B8D1CD");
        var environment = new GameObject("MenuEnvironment").transform;
        var layers = new System.Collections.Generic.List<MenuParallax>();
        layers.Add(Layer(environment, "Clouds", Sprite("Assets/Art/Background/clouds.png"), 3.3f, 6f, .12f, -10, true));
        layers.Add(Layer(environment, "Mountains", Sprite("Assets/Art/Background/mountains.png"), .2f, 8f, .28f, -9, true));
        layers.Add(Layer(environment, "Trees", Sprite("Assets/Art/Background/trees.png"), -1.3f, 6.5f, .55f, -8, true));
        layers.Add(Layer(environment, "Hills", Sprite("Assets/Art/Background/hills.png"), -3f, 5f, .85f, -7, true));
        // Bottom-row grass is a full-width repeat with no exposed side borders.
        var ground = AssetDatabase.LoadAllAssetsAtPath("Assets/Tiles/Ground/Grasslands-Terrain-64.png").OfType<Sprite>()
            .OrderBy(s => s.rect.y).ThenBy(s => s.rect.x).First();
        layers.Add(Layer(environment, "Ground", ground, -4.35f, 1.3f, 2.5f, 20, false));
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Art/Player/Kali_Run.anim");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Menu/MenuKali.controller");
        if (!controller)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Menu/MenuKali.controller");
            var run = controller.layers[0].stateMachine.AddState("Run"); run.motion = clip;
            controller.layers[0].stateMachine.defaultState = run;
        }
        var kali = new GameObject("Kali", typeof(SpriteRenderer), typeof(Animator)); kali.transform.SetParent(environment);
        kali.transform.position = new Vector3(-5.7f, -2.8f, 0); kali.transform.localScale = Vector3.one * .3f;
        var firstFrame = AnimationUtility.GetObjectReferenceCurve(clip, AnimationUtility.GetObjectReferenceCurveBindings(clip)[0])[0].value as Sprite;
        kali.GetComponent<SpriteRenderer>().sprite = firstFrame; kali.GetComponent<SpriteRenderer>().sortingOrder = 30;
        var animator = kali.GetComponent<Animator>(); animator.runtimeAnimatorController = controller;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
        var main = Rect("MainPanel", canvas.transform, Vector2.zero, new Vector2(1920,1080)); Stretch(main);
        var group = main.gameObject.AddComponent<CanvasGroup>();
        var logo = Rect("BarkourLogo", main, new Vector2(0,260), new Vector2(780,210));
        // A collar-tag wordmark with floppy ears and a paw emblem, built from editable UI.
        var shadow = Box("LogoShadow", logo, new Vector2(0,-8), new Vector2(690,125), Brown);
        var plate = Box("CollarTag", logo, Vector2.zero, new Vector2(680,124), Cream);
        Box("EarLeft", logo, new Vector2(-335,40), new Vector2(48,115), Brown).localEulerAngles = new Vector3(0,0,-20);
        Box("EarRight", logo, new Vector2(335,40), new Vector2(48,115), Brown).localEulerAngles = new Vector3(0,0,20);
        Label("Wordmark", logo, "BARKOUR", Vector2.zero, new Vector2(650,130), 94, Brown, FontStyles.Bold);
        Label("Tagline", logo, "RUN  /  JUMP  /  EXPLORE", new Vector2(0,-110), new Vector2(700,45), 25, Brown, FontStyles.Bold);
        Paw(logo, new Vector2(0,100));
        var buttons = Rect("MainButtons", main, new Vector2(0,-70), new Vector2(380,290));
        var layout = buttons.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 20; layout.childControlWidth = true; layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        var play = Button(buttons, "Play", Vector2.zero, new Vector2(380,80), Sage);
        var settings = Button(buttons, "Settings", Vector2.zero, new Vector2(380,80), Cream);
        var quit = Button(buttons, "Quit", Vector2.zero, new Vector2(380,70), Cream);
        Navigate(new UnityEngine.UI.Selectable[] { play, settings, quit });
        Box("FooterBacking", main, new Vector2(0,-300), new Vector2(480,46), Cream);
        Label("Footer", main, "A LITTLE DOG. A BIG ADVENTURE.", new Vector2(0,-300), new Vector2(480,40), 22, Brown);
        var status = Label("Status", main, "", new Vector2(0,-350), new Vector2(1000,45), 20, Brown);
        var settingsRoot = Rect("SettingsPanel", canvas.transform, Vector2.zero, Vector2.zero); Stretch(settingsRoot);
        Box("Dim", settingsRoot, Vector2.zero, Vector2.zero, new Color(.16f,.2f,.19f,.55f), true); Stretch((RectTransform)settingsRoot.GetChild(0));
        var card = Box("SettingsCard", settingsRoot, Vector2.zero, new Vector2(650,600), Cream, true);
        Label("Heading", card, "SOUND SETTINGS", new Vector2(0,230), new Vector2(580,75), 42, Brown, FontStyles.Bold);
        var master = Slider(card, "Master", 115, out var masterValue);
        var music = Slider(card, "Music", 5, out var musicValue);
        var sfx = Slider(card, "SFX", -105, out var sfxValue);
        var back = Button(card, "Back", new Vector2(0,-230), new Vector2(260,65), Sage);
        Navigate(new UnityEngine.UI.Selectable[] { master, music, sfx, back });
        var fadeRoot = Box("FadeOverlay", canvas.transform, Vector2.zero, Vector2.zero, Color.black, true); Stretch(fadeRoot);
        var fade = fadeRoot.gameObject.AddComponent<CanvasGroup>(); fade.alpha = 0; fade.blocksRaycasts = false;
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        var module = events.GetComponent<InputSystemUIInputModule>();
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        module.actionsAsset = actions;
        module.point = InputActionReference.Create(actions.FindAction("UI/Point", true));
        module.leftClick = InputActionReference.Create(actions.FindAction("UI/Click", true));
        module.move = InputActionReference.Create(actions.FindAction("UI/Navigate", true));
        module.submit = InputActionReference.Create(actions.FindAction("UI/Submit", true));
        module.cancel = InputActionReference.Create(actions.FindAction("UI/Cancel", true));
        module.scrollWheel = InputActionReference.Create(actions.FindAction("UI/ScrollWheel", true));
        // Persist references as subassets so scene references survive reopening and builds.
        string refsPath = "Assets/Menu/MenuInputReferences.asset";
        if (!AssetDatabase.LoadMainAssetAtPath(refsPath))
        {
            var refs = new[] { module.point, module.leftClick, module.move, module.submit, module.cancel, module.scrollWheel };
            AssetDatabase.CreateAsset(refs[0], refsPath);
            foreach (var reference in refs.Skip(1)) AssetDatabase.AddObjectToAsset(reference, refsPath);
        }
        var savedRefs = AssetDatabase.LoadAllAssetsAtPath(refsPath).OfType<InputActionReference>().ToArray();
        module.point = savedRefs.First(r=>r.action.name=="Point"); module.leftClick=savedRefs.First(r=>r.action.name=="Click");
        module.move=savedRefs.First(r=>r.action.name=="Navigate"); module.submit=savedRefs.First(r=>r.action.name=="Submit");
        module.cancel=savedRefs.First(r=>r.action.name=="Cancel"); module.scrollWheel=savedRefs.First(r=>r.action.name=="ScrollWheel");
        module.deselectOnBackgroundClick = false;
        var manager = canvas.AddComponent<BarkourMainMenu>();
        manager.mainPanel=group; manager.settingsPanel=settingsRoot.gameObject; manager.fade=fade;
        manager.playButton=play; manager.settingsButton=settings; manager.quitButton=quit; manager.backButton=back;
        manager.masterSlider=master; manager.musicSlider=music; manager.sfxSlider=sfx;
        manager.masterValue=masterValue; manager.musicValue=musicValue; manager.sfxValue=sfxValue; manager.status=status;
        manager.kali=kali.transform; manager.kaliAnimator=animator; manager.menuCamera=camera; manager.layers=layers.ToArray();
        events.GetComponent<EventSystem>().firstSelectedGameObject = play.gameObject;
        settingsRoot.gameObject.SetActive(false);
        var musicSource = new GameObject("MenuMusic_Optional", typeof(AudioSource));
        var audio = musicSource.GetComponent<AudioSource>(); audio.playOnAwake = true; audio.loop = true;
        musicSource.AddComponent<MenuAudioSettings>().music = true;
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
        }.Concat(EditorBuildSettings.scenes.Where(s=>s.path!="Assets/Scenes/MainMenu.unity" && s.path!="Assets/Scenes/Game.unity" && File.Exists(s.path))).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("BARKOUR_MENU_BUILD_PASS: MainMenu saved; Game untouched; MainMenu/Game enabled in build.");
    }
    private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    private static MenuParallax Layer(Transform parent, string name, Sprite sprite, float y, float height, float speed, int order, bool mirrored)
    {
        var root=new GameObject(name).transform; root.SetParent(parent); root.position=new Vector3(0,y,0);
        float scale=height/sprite.bounds.size.y, width=sprite.bounds.size.x*scale;
        int count=Mathf.CeilToInt(40/width)+4;
        for(int i=-count;i<=count;i++)
        {
            var sr=new GameObject(name+"_"+i,typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            sr.transform.SetParent(root,false); sr.transform.localPosition=new Vector3(i*width,0,0);
            sr.transform.localScale=Vector3.one*scale; sr.sprite=sprite; sr.sortingOrder=order;
            // Mirrored pairs meet at identical edge pixels, including non-tileable source art.
            sr.flipX=mirrored && (Math.Abs(i)%2==1);
        }
        var scroll=root.gameObject.AddComponent<MenuParallax>(); scroll.speed=speed; scroll.period=width*(mirrored?2:1); return scroll;
    }
    private static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var c); return c; }
    private static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos; rect.sizeDelta=size; return rect;
    }
    private static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
    private static RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color,bool raycast=false)
    { var r=Rect(name,parent,pos,size); var image=r.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=raycast; return r; }
    private static TMP_Text Label(string name,Transform parent,string text,Vector2 pos,Vector2 size,float fontSize,Color color,FontStyles style=FontStyles.Normal)
    {
        var r=Rect(name,parent,pos,size); var t=r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font=font; t.text=text; t.fontSize=fontSize; t.color=color; t.fontStyle=style;
        t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; return t;
    }
    private static UnityEngine.UI.Button Button(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var r=Box(name+"Button",parent,pos,size,color,true);
        var outline=r.gameObject.AddComponent<UnityEngine.UI.Outline>(); outline.effectColor=Brown; outline.effectDistance=new Vector2(3,-4);
        var b=r.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();
        var c=b.colors; c.highlightedColor=Hex("DFE8C4"); c.selectedColor=Hex("CFDDB5"); c.pressedColor=Blue; b.colors=c;
        r.gameObject.AddComponent<MenuFocus>();
        var element=r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); element.preferredHeight=size.y;
        var t=Label("Label",r,name.ToUpperInvariant(),Vector2.zero,size,32,Brown,FontStyles.Bold); Stretch(t.rectTransform);
        return b;
    }
    private static UnityEngine.UI.Slider Slider(Transform parent,string name,float y,out TMP_Text value)
    {
        var root=Rect(name+"Slider",parent,new Vector2(0,y),new Vector2(510,75));
        Label("Label",root,name.ToUpperInvariant(),new Vector2(-170,26),new Vector2(170,35),23,Brown,FontStyles.Bold);
        value=Label("Value",root,"100%",new Vector2(210,26),new Vector2(90,35),22,Brown);
        var track=Box("Track",root,new Vector2(0,-14),new Vector2(510,14),Brown,true);
        var fillArea=Rect("FillArea",root,new Vector2(0,-14),new Vector2(490,14));
        var fill=Box("Fill",fillArea,Vector2.zero,Vector2.zero,Sage); Stretch(fill);
        var handleArea=Rect("HandleArea",root,new Vector2(0,-14),new Vector2(490,38));
        // Slider stretches the handle vertically across HandleArea; avoid doubling its height.
        var handle=Box("Handle",handleArea,Vector2.zero,new Vector2(28,0),Blue,true);
        var s=root.gameObject.AddComponent<UnityEngine.UI.Slider>(); s.fillRect=fill; s.handleRect=handle;
        s.targetGraphic=handle.GetComponent<UnityEngine.UI.Image>(); s.minValue=0; s.maxValue=1; s.value=1;
        root.gameObject.AddComponent<MenuFocus>(); return s;
    }
    private static void Navigate(UnityEngine.UI.Selectable[] items)
    {
        for(int i=0;i<items.Length;i++) items[i].navigation=new UnityEngine.UI.Navigation {
            mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=items[(i+items.Length-1)%items.Length], selectOnDown=items[(i+1)%items.Length] };
    }
    private static void Paw(Transform parent,Vector2 p)
    {
        Box("PawPad",parent,p,new Vector2(36,25),Brown).localEulerAngles=new Vector3(0,0,8);
        for(int i=0;i<4;i++) Box("Toe"+i,parent,p+new Vector2((i-1.5f)*17,23-Mathf.Abs(i-1.5f)*7),new Vector2(12,16),Brown);
    }
}
