using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Repeatable Editor smoke test. Injected devices exercise the actual Input System module.
[InitializeOnLoad]
public static class BarkourMenuValidation
{
    private static int step;
    private static double next;
    private static Gamepad pad;
    private static Keyboard keyboard;
    private static BarkourMainMenu menu;
    private static float master, music, sfx;
    private static Vector3 layerStart;
    private static string report;
    private static InputSettings originalInputSettings;
    private static InputSettings testInputSettings;
    private static bool originalBackground;
    private const string Output = "Docs/MenuValidation";
    static BarkourMenuValidation() { EditorApplication.update += Tick; }
    [MenuItem("Barkour/Validate Main Menu")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.WriteAllText("Temp/BarkourMenuTest.request", "Validate menu");
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying && File.Exists("Temp/BarkourMenuTest.request"))
        {
            File.Delete("Temp/BarkourMenuTest.request");
            step=0; next=0;
            SessionState.SetBool("BarkourMenuTest", true);
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            EditorApplication.isPlaying=true; return;
        }
        if (!SessionState.GetBool("BarkourMenuTest",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (EditorApplication.timeSinceStartup<next) return;
        next=EditorApplication.timeSinceStartup+.4;
        try { Step(); } catch(Exception e) { Finish("FAIL: "+e); }
    }
    private static void Step()
    {
        switch(step++)
        {
            case 0:
                Directory.CreateDirectory(Output); report="Barkour Unity runtime smoke checks\n";
                menu=UnityEngine.Object.FindFirstObjectByType<BarkourMainMenu>();
                Check(menu!=null,"Menu loaded");
                master=MenuAudioSettings.Master; music=MenuAudioSettings.Music; sfx=MenuAudioSettings.Sfx;
                originalInputSettings=InputSystem.settings;
                testInputSettings=UnityEngine.Object.Instantiate(originalInputSettings);
                testInputSettings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                testInputSettings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings=testInputSettings;
                originalBackground=Application.runInBackground; Application.runInBackground=true;
                keyboard=InputSystem.AddDevice<Keyboard>(); pad=InputSystem.AddDevice<Gamepad>();
                SetSize(1920,1080); layerStart=menu.layers[0].transform.position; next+=2; break;
            case 1:
                Check(menu.layers[0].transform.position!=layerStart,"Independent parallax moves");
                Check(UnityEngine.Object.FindFirstObjectByType<RunManager>()==null,"No gameplay manager in MainMenu");
                Check(menu.kaliAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime>0,"Kali run animation advances");
                Bounds(); ScreenCapture.CaptureScreenshot(Output+"/MainMenu-1920x1080.png");
                EventSystem.current.SetSelectedGameObject(menu.playButton.gameObject);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.DownArrow)); break;
            case 2:
                Check(EventSystem.current.currentSelectedGameObject==menu.settingsButton.gameObject,"Keyboard navigation Play to Settings");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState()); break;
            case 3: InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter)); break;
            case 4:
                Check(menu.settingsPanel.activeSelf,"Keyboard submit opens Settings");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState()); break;
            case 5:
                Bounds(); ScreenCapture.CaptureScreenshot(Output+"/Settings-1920x1080.png");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftArrow)); break;
            case 6:
                Check(menu.masterSlider.value<master,"Keyboard adjusts master slider");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                menu.masterSlider.value=.63f; menu.musicSlider.value=.42f; menu.sfxSlider.value=.31f;
                Check(Mathf.Approximately(AudioListener.volume,.63f),"Master volume applied to listener");
                Check(UnityEngine.Object.FindObjectsByType<MenuAudioSettings>(FindObjectsSortMode.None).Where(a=>a.music)
                    .All(a=>Mathf.Approximately(a.GetComponent<AudioSource>().volume,.42f)),"Music volume applied independently");
                menu.CloseSettings();
                Check(Mathf.Approximately(PlayerPrefs.GetFloat("Barkour.Audio.Sfx"),.31f),"Volume values persisted");
                SetSize(1920,1200); next+=1; break;
            case 7:
                Bounds(); ScreenCapture.CaptureScreenshot(Output+"/MainMenu-1920x1200.png");
                step=70; break;
            case 70:
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South)); step=8; break;
            case 8:
                Check(menu.settingsPanel.activeSelf,"Controller submit opens Settings");
                InputSystem.QueueStateEvent(pad,new GamepadState()); break;
            case 9: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown)); break;
            case 10:
                Check(EventSystem.current.currentSelectedGameObject==menu.musicSlider.gameObject,"Controller navigates to Music");
                InputSystem.QueueStateEvent(pad,new GamepadState()); break;
            case 11: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadLeft)); break;
            case 12:
                Check(menu.musicSlider.value<.42f,"Controller adjusts music slider");
                InputSystem.QueueStateEvent(pad,new GamepadState()); Bounds();
                ScreenCapture.CaptureScreenshot(Output+"/Settings-1920x1200.png"); break;
            case 13: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East)); break;
            case 14:
                Check(!menu.settingsPanel.activeSelf,"Controller cancel returns to menu");
                InputSystem.QueueStateEvent(pad,new GamepadState());
                SceneManager.LoadScene("MainMenu"); next+=1; break;
            case 15:
                menu=UnityEngine.Object.FindFirstObjectByType<BarkourMainMenu>();
                Check(Mathf.Approximately(menu.masterSlider.value,.63f),"Volume survives scene reload");
                menu.Play(); menu.Play(); next+=3; break;
            case 16:
                Check(SceneManager.GetActiveScene().name=="Game","Play transition loads Game, duplicate Play ignored");
                Check(RunManager.Instance!=null && RunManager.Instance.CurrentState==RunManager.RunState.WaitingToStart,"New Game waits for first input");
                Check(UnityEngine.Object.FindFirstObjectByType<BarkourMainMenu>()==null,"Menu removed after loading Game");
                Check(UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).All(s=>s.GetComponent<MenuAudioSettings>()!=null),"Game audio receives persisted SFX settings");
                ScreenCapture.CaptureScreenshot(Output+"/Game-1920x1200.png");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space)); break;
            case 17:
                Check(RunManager.Instance.IsRunning,"Game starts from keyboard input");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState()); Finish("PASS: All runtime smoke checks completed."); break;
        }
    }
    private static void Bounds()
    {
        Canvas.ForceUpdateCanvases();
        foreach(var selectable in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Selectable>(FindObjectsSortMode.None))
        {
            var corners=new Vector3[4]; ((RectTransform)selectable.transform).GetWorldCorners(corners);
            Check(corners.All(p=>p.x>=0 && p.y>=0 && p.x<=Screen.width && p.y<=Screen.height),selectable.name+" visible at "+Screen.width+"x"+Screen.height);
        }
    }
    private static void Check(bool ok,string message) { if(!ok) throw new Exception(message); report+="PASS: "+message+"\n"; }
    private static void Finish(string message)
    {
        SessionState.SetBool("BarkourMenuTest",false);
        MenuAudioSettings.Set("Master",master); MenuAudioSettings.Set("Music",music); MenuAudioSettings.Set("Sfx",sfx); MenuAudioSettings.Save();
        if(pad!=null) InputSystem.RemoveDevice(pad); if(keyboard!=null) InputSystem.RemoveDevice(keyboard);
        if(originalInputSettings!=null) InputSystem.settings=originalInputSettings;
        if(testInputSettings!=null) UnityEngine.Object.DestroyImmediate(testInputSettings);
        Application.runInBackground=originalBackground;
        Directory.CreateDirectory(Output); File.WriteAllText(Output+"/Results.txt",report+message);
        Debug.Log(message); EditorApplication.isPlaying=false;
    }
    private static void SetSize(int width,int height)
    {
        var assembly=typeof(Editor).Assembly;
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        var instance=typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(instance,new object[]{0});
        var groupType=group.GetType();
        var sizeType=assembly.GetType("UnityEditor.GameViewSize");
        var enumType=assembly.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,new object[]{Enum.ToObject(enumType,1),width,height,"Barkour check "+width+"x"+height});
        groupType.GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int total=(int)groupType.GetMethod("GetTotalCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
        view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,total-1);
        view.Focus();
    }
}
