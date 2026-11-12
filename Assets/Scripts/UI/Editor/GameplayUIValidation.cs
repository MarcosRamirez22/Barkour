using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class GameplayUIValidation
{
    private const string Flag="BarkourGameplayValidation", Output="Docs/GameplayUIValidation";
    private static int step,index;
    private static double next;
    private static string report;
    private static Camera camera;
    private static Rigidbody2D player;
    private static Vector2 frozen;
    private static int seed;
    private static Keyboard keyboard;
    private static Gamepad pad;
    private static InputSettings originalSettings,testSettings;
    private static bool background;
    private static readonly Vector2Int[] Sizes={new(1280,720),new(1920,1080),new(1920,1200),new(1280,800),new(1024,768),new(2560,1080)};
    static GameplayUIValidation() { EditorApplication.update+=Tick; }
    [MenuItem("Barkour/Validate Gameplay UI")]
    public static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) return;
        for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new Exception("Save the open scene first.");
        step=0; index=0; next=0;
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        SessionState.SetBool(Flag,true); EditorApplication.isPlaying=true;
    }
    private static void Tick()
    {
        if(!SessionState.GetBool(Flag,false) || !EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup<next) return;
        next=EditorApplication.timeSinceStartup+.25;
        try { Step(); } catch(Exception e) { Finish("FAIL: "+e); }
    }
    private static void Step()
    {
        switch(step++)
        {
            case 0:
                report="Gameplay UI runtime validation\n"; Directory.CreateDirectory(Output);
                originalSettings=InputSystem.settings; testSettings=UnityEngine.Object.Instantiate(originalSettings);
                testSettings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                testSettings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; InputSystem.settings=testSettings;
                background=Application.runInBackground; Application.runInBackground=true;
                keyboard=InputSystem.AddDevice<Keyboard>(); pad=InputSystem.AddDevice<Gamepad>();
                camera=Camera.main; player=UnityEngine.Object.FindFirstObjectByType<PlayerMovement>().GetComponent<Rigidbody2D>();
                SetSize(Sizes[0]); next+=1; break;
            case 1: InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D)); break;
            case 2:
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                Check(RunManager.Instance.IsRunning,"Gameplay starts");
                player.position=new Vector2(camera.transform.position.x-camera.orthographicSize*camera.aspect-15,player.position.y);
                player.linearVelocity=Vector2.zero; break;
            case 3:
                Check(UnityEngine.Object.FindFirstObjectByType<FailureSystem>().IsRecovering,"Real off-screen recovery triggers");
                Time.timeScale=0; index=0; break;
            case 4:
                ValidateRecovery(); ScreenCapture.CaptureScreenshot(Output+"/Recovery-"+Screen.width+"x"+Screen.height+".png"); break;
            case 5:
                index++;
                if(index<Sizes.Length) { SetSize(Sizes[index]); step=4; next+=.5; }
                else { SetSize(new Vector2Int(1280,720)); Time.timeScale=1; next+=3.5; }
                break;
            case 6:
                Check(RunManager.Instance.CurrentState==RunManager.RunState.Failed,"Recovery timeout reaches Failed");
                Check(Mathf.Approximately(Time.timeScale,0),"Game Over freezes gameplay");
                frozen=player.position;
                seed=UnityEngine.Object.FindFirstObjectByType<LevelChunkSpawner>().CurrentSeed;
                Check(GameObject.Find("RunSeed").GetComponent<TMP_Text>().text=="SEED  "+seed,"Game Over shows actual played seed");
                var score=UnityEngine.Object.FindObjectsByType<DistanceScore>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single();
                Check(GameObject.Find("RunDistance").GetComponent<TMP_Text>().text=="DISTANCE  "+score.CurrentDistance+" m","Final distance matches score");
                Check(GameObject.Find("RunBest").GetComponent<TMP_Text>().text=="BEST  "+score.BestDistance+" m","Best distance matches score");
                index=0; break;
            case 7:
                Check(Vector2.Distance(frozen,player.position)<.001f,"Player remains frozen");
                foreach(var item in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Selectable>(FindObjectsSortMode.None)) Visible((RectTransform)item.transform);
                Visible((RectTransform)GameObject.Find("GameOverCard").transform);
                ScreenCapture.CaptureScreenshot(Output+"/GameOver-"+Screen.width+"x"+Screen.height+".png"); break;
            case 8:
                index++; if(index<Sizes.Length) { SetSize(Sizes[index]); step=7; next+=.5; }
                else { SetSize(new Vector2Int(1920,1200)); EventSystem.current.SetSelectedGameObject(GameObject.Find("RestartButton")); }
                break;
            case 9: InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.DownArrow)); next=EditorApplication.timeSinceStartup+.1; break;
            case 10:
                Check(EventSystem.current.currentSelectedGameObject.name=="MainMenuButton","Keyboard navigates Retry to Main Menu while paused");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState()); break;
            case 11: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadUp)); next=EditorApplication.timeSinceStartup+.1; break;
            case 12:
                Check(EventSystem.current.currentSelectedGameObject.name=="RestartButton","Controller navigates Main Menu to Retry");
                InputSystem.QueueStateEvent(pad,new GamepadState()); break;
            case 13: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South)); next=EditorApplication.timeSinceStartup+.1; break;
            case 14: InputSystem.QueueStateEvent(pad,new GamepadState()); next+=1.5; break;
            case 15:
                Check(SceneManager.GetActiveScene().name=="Game","Retry loads Game");
                Check(RunManager.Instance.CurrentState==RunManager.RunState.WaitingToStart,"Retry resets run and waits for input");
                Check(Mathf.Approximately(Time.timeScale,1),"Retry restores normal time");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D)); break;
            case 16: InputSystem.QueueStateEvent(keyboard,new KeyboardState()); RunManager.Instance.FailRun(); break;
            case 17:
                Check(Mathf.Approximately(Time.timeScale,0),"Second Game Over freezes correctly");
                // Exercise the button's pointer-click event route without injecting OS mouse input.
                ExecuteEvents.Execute(GameObject.Find("MainMenuButton"),new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
                next+=1.5; break;
            case 18:
                Check(SceneManager.GetActiveScene().name=="MainMenu","Main Menu button fades back to menu");
                Check(Mathf.Approximately(Time.timeScale,1),"Return to menu restores normal time");
                Check(UnityEngine.Object.FindFirstObjectByType<BarkourMainMenu>()!=null,"Main menu remains functional");
                Finish("PASS: All gameplay UI checks completed."); break;
        }
    }
    private static void ValidateRecovery()
    {
        Canvas.ForceUpdateCanvases();
        var t=GameObject.Find("RecoveryCountdown").GetComponent<TMP_Text>();
        Check(t.enabled && t.text.StartsWith("RECOVER"),"Recovery timer visible at "+Screen.width+"x"+Screen.height); Visible(t.rectTransform);
        Visible(GameObject.Find("DistanceText").GetComponent<RectTransform>()); Visible(GameObject.Find("BestDistanceText").GetComponent<RectTransform>());
        var framing=camera.GetComponent<GameplayFraming>();
        Check(Mathf.Abs(camera.orthographicSize*2*camera.aspect-framing.GameplayWidth)<.001f,"Horizontal gameplay span preserved");
        Check(Mathf.Abs(camera.transform.position.y-camera.orthographicSize+10f)<.001f,"Bottom edge preserved; extra view extends upward");
        if(Screen.width==2560) Check(camera.rect.width<1,"Ultrawide uses black side borders");
        if(Screen.width==1024) Check(camera.rect.height<1,"4:3 uses black top/bottom borders");
    }
    private static void Visible(RectTransform r)
    {
        Canvas.ForceUpdateCanvases(); var corners=new Vector3[4]; r.GetWorldCorners(corners);
        Rect bounds=Camera.main.pixelRect;
        Check(corners.All(v=>v.x>=bounds.xMin-.1f&&v.x<=bounds.xMax+.1f&&v.y>=bounds.yMin-.1f&&v.y<=bounds.yMax+.1f),r.name+" inside viewport");
    }
    private static void Check(bool condition,string message) { if(!condition) throw new Exception(message); report+="PASS: "+message+"\n"; }
    private static void Finish(string message)
    {
        SessionState.SetBool(Flag,false); Time.timeScale=1;
        if(keyboard!=null) InputSystem.RemoveDevice(keyboard); if(pad!=null) InputSystem.RemoveDevice(pad);
        if(originalSettings!=null) InputSystem.settings=originalSettings;
        if(testSettings!=null) UnityEngine.Object.DestroyImmediate(testSettings);
        Application.runInBackground=background;
        Directory.CreateDirectory(Output); File.WriteAllText(Output+"/Results.txt",report+message); Debug.Log(message); EditorApplication.isPlaying=false;
    }
    private static void SetSize(Vector2Int size)
    {
        var assembly=typeof(Editor).Assembly; var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        var instance=typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(instance,new object[]{0}); var groupType=group.GetType();
        var sizeType=assembly.GetType("UnityEditor.GameViewSize"); var enumType=assembly.GetType("UnityEditor.GameViewSizeType");
        var entry=Activator.CreateInstance(sizeType,new object[]{Enum.ToObject(enumType,1),size.x,size.y,"Gameplay check "+size.x+"x"+size.y});
        groupType.GetMethod("AddCustomSize").Invoke(group,new[]{entry});
        int count=(int)groupType.GetMethod("GetTotalCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
        view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,count-1); view.Focus();
    }
}

