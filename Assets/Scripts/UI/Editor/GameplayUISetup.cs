using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class GameplayUISetup
{
    private static Color Brown = Hex("49372D"), Cream = Hex("FFF0CF"), Sage = Hex("AAC09A"), Blue = Hex("7095A2");
    private static TMP_FontAsset font;
    [MenuItem("Barkour/Apply Gameplay UI")]
    public static void Apply()
    {
        for (int i=0;i<SceneManager.sceneCount;i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save the open scene before applying gameplay UI.");
        Directory.CreateDirectory("Docs/GameplayUIBackup");
        if (!File.Exists("Docs/GameplayUIBackup/Game.unity.txt")) File.Copy("Assets/Scenes/Game.unity", "Docs/GameplayUIBackup/Game.unity.txt");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        var objects=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
        Transform Find(string name) => objects.Single(t=>t.name==name);
        var camera=Find("Main Camera").GetComponent<Camera>();
        var canvas=Find("Canvas").GetComponent<Canvas>();
        var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        var recovery=Find("RecoveryCountdown").GetComponent<TMP_Text>();
        if (!File.Exists("Docs/GameplayUIBackup/Baseline.txt"))
            File.WriteAllText("Docs/GameplayUIBackup/Baseline.txt",
                "Recovery baseline: Canvas scale mode="+scaler.uiScaleMode+"; anchor="+recovery.rectTransform.anchorMin+
                "; position="+recovery.rectTransform.anchoredPosition+"; size="+recovery.rectTransform.sizeDelta+
                ". At 720px height its center is 360+429=789, above the screen. At 1080px height it is 540+429=969, inside.\n"+
                "Camera half-height="+camera.orthographicSize+"; Full HD horizontal span="+(camera.orthographicSize*2*16/9)+".\n");
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.worldCamera=null;
        scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=0;
        var viewport=canvas.transform.Find("GameplayViewport") as RectTransform;
        if (!viewport) viewport=Rect("GameplayViewport",canvas.transform,Vector2.zero,Vector2.zero);
        Stretch(viewport);
        var score=Find("ScoreHUD"); score.SetParent(viewport,false); Stretch((RectTransform)score);
        recovery.transform.SetParent(viewport,false);
        Anchor(recovery.rectTransform,new Vector2(.5f,1),new Vector2(0,-40),new Vector2(520,100));
        recovery.fontSize=46; recovery.fontStyle=FontStyles.Bold; recovery.color=Cream;
        recovery.alignment=TextAlignmentOptions.Center; recovery.raycastTarget=false;
        recovery.outlineWidth=.18f; recovery.outlineColor=Brown;
        var distance=Find("DistanceText").GetComponent<TMP_Text>();
        var best=Find("BestDistanceText").GetComponent<TMP_Text>();
        Anchor(distance.rectTransform,new Vector2(.5f,0),new Vector2(0,26),new Vector2(420,42));
        Anchor(best.rectTransform,new Vector2(.5f,0),new Vector2(0,69),new Vector2(420,36));
        StyleText(distance,32,Brown); StyleText(best,24,Brown);
        var scoreBacking=Box("ScoreBacking",score,new Vector2(0,24),new Vector2(450,86),Cream);
        Anchor(scoreBacking,new Vector2(.5f,0),new Vector2(0,24),new Vector2(450,86)); scoreBacking.SetAsFirstSibling();
        var panel=Find("GameOverPanel"); panel.SetParent(viewport,false); Stretch((RectTransform)panel);
        var dim=panel.GetComponent<UnityEngine.UI.Image>(); dim.sprite=null; dim.color=new Color(.12f,.16f,.15f,.72f); dim.raycastTarget=true;
        var card=Box("GameOverCard",panel,Vector2.zero,new Vector2(700,760),Cream,true);
        var border=Ensure<UnityEngine.UI.Outline>(card.gameObject);
        border.effectColor=Brown; border.effectDistance=new Vector2(4,-6);
        Label("Brand",card,"B A R K O U R",new Vector2(0,305),new Vector2(600,45),24,Blue);
        var title=Find("GameOver").GetComponent<TMP_Text>(); title.transform.SetParent(card,false);
        Place(title.rectTransform,new Vector2(0,235),new Vector2(620,85)); StyleText(title,60,Brown); title.fontStyle=FontStyles.Bold; title.text="GOOD RUN, KALI!";
        Label("Subtitle",card,"EVERY ADVENTURE STARTS WITH ANOTHER TRY.",new Vector2(0,173),new Vector2(620,45),19,Brown);
        var distanceLabel=Label("RunDistance",card,"DISTANCE  0 m",new Vector2(0,105),new Vector2(600,50),36,Brown);
        var bestLabel=Label("RunBest",card,"BEST  0 m",new Vector2(0,53),new Vector2(600,42),27,Brown);
        var seedLabel=Label("RunSeed",card,"SEED  0",new Vector2(0,2),new Vector2(600,40),24,Blue);
        var retry=Find("RestartButton").GetComponent<UnityEngine.UI.Button>(); retry.transform.SetParent(card,false);
        Place((RectTransform)retry.transform,new Vector2(0,-85),new Vector2(400,76)); StyleButton(retry,Sage);
        retry.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();
        var retryLabel=retry.GetComponentInChildren<TMP_Text>(true); StyleText(retryLabel,32,Brown); retryLabel.text="RETRY"; Stretch(retryLabel.rectTransform);
        var menu=Button("MainMenuButton",card,"MAIN MENU",new Vector2(0,-184),Cream);
        var quit=Button("QuitButton",card,"QUIT",new Vector2(0,-283),Cream);
        Navigate(new UnityEngine.UI.Selectable[]{retry,menu,quit});
        var status=Label("GameOverStatus",card,"",new Vector2(0,-345),new Vector2(640,34),19,Brown);
        var fade=Box("GameplayFade",viewport,Vector2.zero,Vector2.zero,Color.black,true); Stretch(fade);
        var fadeGroup=Ensure<CanvasGroup>(fade.gameObject); fadeGroup.alpha=0; fadeGroup.blocksRaycasts=false;
        var manager=Find("GameOverUIManager").GetComponent<GameOverUI>();
        Set(manager,"gameOverPanel",panel.gameObject); Set(manager,"scoreHud",score.gameObject);
        Set(manager,"distanceScore",score.GetComponent<DistanceScore>()); Set(manager,"levelSpawner",Find("LevelGenerator").GetComponent<LevelChunkSpawner>());
        Set(manager,"distanceLabel",distanceLabel); Set(manager,"bestLabel",bestLabel); Set(manager,"seedLabel",seedLabel); Set(manager,"statusLabel",status);
        Set(manager,"retryButton",retry); Set(manager,"menuButton",menu); Set(manager,"quitButton",quit); Set(manager,"fadeOverlay",fadeGroup);
        var framing=Ensure<GameplayFraming>(camera.gameObject);
        var fs=new SerializedObject(framing); fs.FindProperty("referenceHalfHeight").floatValue=camera.orthographicSize; fs.ApplyModifiedPropertiesWithoutUndo();
        Set(framing,"hudViewport",viewport); Set(framing,"canvasScaler",scaler);
        foreach(var side in new[]{"left","right","bottom","top"})
        {
            var bar=Box(side+"BlackBar",canvas.transform,Vector2.zero,Vector2.zero,Color.black,true);
            Set(framing,side+"Bar",bar); bar.SetAsFirstSibling();
        }
        var module=Find("EventSystem").GetComponent<InputSystemUIInputModule>();
        var refs=AssetDatabase.LoadAllAssetsAtPath("Assets/Menu/MenuInputReferences.asset").OfType<InputActionReference>().ToArray();
        module.actionsAsset=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        module.point=refs.First(r=>r.action.name=="Point"); module.leftClick=refs.First(r=>r.action.name=="Click");
        module.move=refs.First(r=>r.action.name=="Navigate"); module.submit=refs.First(r=>r.action.name=="Submit");
        module.cancel=refs.First(r=>r.action.name=="Cancel"); module.scrollWheel=refs.First(r=>r.action.name=="ScrollWheel");
        module.deselectOnBackgroundClick=false;
        panel.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Debug.Log("GAMEPLAY_UI_APPLY_PASS");
    }
    private static T Ensure<T>(GameObject go) where T : Component { var c=go.GetComponent<T>(); return c ? c : go.AddComponent<T>(); }
    private static Color Hex(string h) { ColorUtility.TryParseHtmlString("#"+h,out var c); return c; }
    private static void Set(UnityEngine.Object target,string property,UnityEngine.Object value)
    { var s=new SerializedObject(target); s.FindProperty(property).objectReferenceValue=value; s.ApplyModifiedPropertiesWithoutUndo(); }
    private static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var r=parent.Find(name) as RectTransform;
        if(!r) { r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); }
        Place(r,position,size); return r;
    }
    private static void Place(RectTransform r,Vector2 p,Vector2 size) { Anchor(r,new Vector2(.5f,.5f),p,size); }
    private static void Anchor(RectTransform r,Vector2 anchor,Vector2 p,Vector2 size)
    { r.localScale=Vector3.one; r.localRotation=Quaternion.identity; r.anchorMin=r.anchorMax=r.pivot=anchor; r.anchoredPosition=p; r.sizeDelta=size; }
    private static void Stretch(RectTransform r) { r.localScale=Vector3.one; r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
    private static RectTransform Box(string name,Transform parent,Vector2 p,Vector2 size,Color c,bool raycast=false)
    { var r=Rect(name,parent,p,size); var i=Ensure<UnityEngine.UI.Image>(r.gameObject); i.sprite=null; i.color=c; i.raycastTarget=raycast; return r; }
    private static void StyleText(TMP_Text t,float size,Color c)
    { t.font=font; t.fontSize=size; t.enableAutoSizing=false; t.color=c; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; }
    private static TMP_Text Label(string name,Transform parent,string text,Vector2 p,Vector2 size,float fontSize,Color c)
    { var r=Rect(name,parent,p,size); var t=Ensure<TextMeshProUGUI>(r.gameObject); StyleText(t,fontSize,c); t.text=text; return t; }
    private static void StyleButton(UnityEngine.UI.Button b,Color color)
    {
        var i=b.GetComponent<UnityEngine.UI.Image>(); i.sprite=null; i.color=color; i.raycastTarget=true; b.targetGraphic=i;
        var c=b.colors; c.normalColor=Color.white; c.highlightedColor=Hex("DFE8C4"); c.selectedColor=Hex("CFDDB5"); c.pressedColor=Blue; b.colors=c;
        if(!b.GetComponent<MenuFocus>()) b.gameObject.AddComponent<MenuFocus>();
        var o=Ensure<UnityEngine.UI.Outline>(b.gameObject); o.effectColor=Brown; o.effectDistance=new Vector2(3,-4);
    }
    private static UnityEngine.UI.Button Button(string name,Transform parent,string label,Vector2 p,Color c)
    {
        var r=Box(name,parent,p,new Vector2(400,76),c,true); var b=Ensure<UnityEngine.UI.Button>(r.gameObject);
        StyleButton(b,c); var t=Label("Label",r,label,Vector2.zero,new Vector2(400,76),32,Brown); Stretch(t.rectTransform); return b;
    }
    private static void Navigate(UnityEngine.UI.Selectable[] items)
    { for(int i=0;i<items.Length;i++) items[i].navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.Explicit,selectOnUp=items[(i+items.Length-1)%items.Length],selectOnDown=items[(i+1)%items.Length]}; }
}


