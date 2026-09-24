using UnityEngine;
using UnityEngine.Rendering;

namespace BladeSurvivor {
public enum GameScreen { MainMenu,Map,Story,Battle,Result,Equipment,Skills,Upgrades,Shop,Settings,Recruit,Endless }
public class GameRoot:MonoBehaviour {
    public static GameRoot Instance {get;private set;}
    public GameBalance Balance;public ProgressionService Progression;public BattleController Battle;public BattleCamera BattleCamera;public CombatEffects Effects;public GameAudio Audio;public GameUI UI;public GestureInput Gestures;
    public GameScreen Screen;public bool Victory;public int SelectedStage;public bool TestMode;
    public IRewardedAdService Ads=new MockRewardedAdService();public IIAPService Store=new MockIAPService();
    // 可調數值【顯示品質】目標 60FPS、垂直同步 1、抗鋸齒 4 倍、陰影距離 55m；手機效能需真機驗證。
    void Awake(){Instance=this;Application.targetFrameRate=60;Application.runInBackground=true;UnityEngine.Screen.orientation=ScreenOrientation.LandscapeLeft;QualitySettings.vSyncCount=1;QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=55;
        if(Balance==null)Balance=Resources.Load<GameBalance>("GameBalance");if(Balance==null){Balance=ScriptableObject.CreateInstance<GameBalance>();Balance.PopulateDefaults();}
        Progression=new ProgressionService(Balance);SelectedStage=Mathf.Min(Progression.Save.selectedStage,Progression.Save.unlockedStage);
        var cam=new GameObject("Battle Camera");cam.tag="MainCamera";cam.AddComponent<Camera>();cam.AddComponent<AudioListener>();BattleCamera=cam.AddComponent<BattleCamera>();
    // 可調數值【主光】RGB(.87,.9,1)、亮度 1.25、旋轉角(48,-35,0)；下方三組 RGB 為天空／水平／地面環境光。
        var light=new GameObject("Moon & ash").AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(.87f,.9f,1);light.intensity=1.25f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(48,-35,0);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.4f,.46f,.56f);RenderSettings.ambientEquatorColor=new Color(.27f,.29f,.33f);RenderSettings.ambientGroundColor=new Color(.14f,.12f,.13f);RenderSettings.fog=false;
        Audio=gameObject.AddComponent<GameAudio>();Effects=gameObject.AddComponent<CombatEffects>();Battle=gameObject.AddComponent<BattleController>();UI=gameObject.AddComponent<GameUI>();UI.Root=this;
        Gestures=gameObject.AddComponent<GestureInput>();Gestures.Root=this;Battle.Initialize(this);Screen=GameScreen.MainMenu;Audio.Volume(Progression.Save.volume);
    }
    public void Navigate(GameScreen screen){if(Screen==GameScreen.Battle&&screen!=GameScreen.Battle)Battle.Leave();Screen=screen;UI.ResetPage();Gestures.ResetGesture();Time.timeScale=1;}
    public void SelectStage(int index){SelectedStage=Mathf.Clamp(index,0,Progression.Save.unlockedStage);Progression.Save.selectedStage=SelectedStage;Progression.Persist();Screen=GameScreen.Story;}
    public void BeginBattle(){Screen=GameScreen.Battle;UI.ResetPage();Gestures.ResetGesture();Battle.StartBattle(SelectedStage);}
    public void ShowResult(bool success){Victory=success;Screen=GameScreen.Result;Gestures.ResetGesture();}
    public void ExitResult(){Battle.Leave();Navigate(GameScreen.Map);}
    void OnApplicationPause(bool pause){if(pause&&Screen==GameScreen.Battle)Battle.Pause(true);if(pause)Progression?.Persist();}
    void OnApplicationQuit(){Progression?.Persist();Time.timeScale=1;}
    void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;}
}
}
