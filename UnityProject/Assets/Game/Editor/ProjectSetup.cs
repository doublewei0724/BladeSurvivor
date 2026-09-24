using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BladeSurvivor {
public static class ProjectSetup {
    [MenuItem("Blade Survivor/Prepare Playable Project")]
    public static void Prepare(){
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before preparing the project.");
        Directory.CreateDirectory("Assets/Game/Resources");Directory.CreateDirectory("Assets/Game/Scenes");
        var balance=AssetDatabase.LoadAssetAtPath<GameBalance>("Assets/Game/Resources/GameBalance.asset");
        if(balance==null){balance=ScriptableObject.CreateInstance<GameBalance>();balance.PopulateDefaults();AssetDatabase.CreateAsset(balance,"Assets/Game/Resources/GameBalance.asset");}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var root=new GameObject("Blade Survivor").AddComponent<GameRoot>();root.Balance=balance;
        EditorSceneManager.SaveScene(scene,"Assets/Game/Scenes/BattlePrototype.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Game/Scenes/BattlePrototype.unity",true)};
    // 可調數值【視窗尺寸】預設 1600×900，允許縮放；UI 基準座標需看 GameUI 的 W／H。
        PlayerSettings.productName="Blade Survivor";PlayerSettings.companyName="Hollow Road Studio";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone,"studio.hollowroad.bladesurvivor");PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"studio.hollowroad.bladesurvivor");
    // 可調數值【背景貼圖匯入】最大邊長 2048、關閉 mipmap；改成更大會增加記憶體。
        var importer=AssetImporter.GetAtPath("Assets/Game/Resources/MenuBackground.png") as TextureImporter;if(importer!=null){importer.textureType=TextureImporterType.Default;importer.maxTextureSize=2048;importer.mipmapEnabled=false;importer.SaveAndReimport();}
        AssetDatabase.SaveAssets();EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scenes/BattlePrototype.unity");Selection.activeGameObject=root.gameObject;
        Debug.Log("BLADE_SURVIVOR_PROJECT_READY");
    }
    [MenuItem("Blade Survivor/Build macOS Playtest")]
    public static void BuildMac(){
        string path=Path.GetFullPath("../Builds/Blade Survivor.app");Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Game/Scenes/BattlePrototype.unity"},locationPathName=path,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
        Debug.Log("BLADE_BUILD "+report.summary.result+" "+report.summary.totalErrors);if(report.summary.totalErrors>0)throw new Exception("macOS build failed");
    }
    [MenuItem("Blade Survivor/Build WebGL Playtest")]
    public static void BuildWebGL(){
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before building WebGL.");
        // GitHub Pages cannot set Unity's compressed response headers. Uncompressed files load directly.
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback=false;
        string projectRoot=Path.GetDirectoryName(Application.dataPath);
        string path=Path.GetFullPath(Path.Combine(projectRoot,"..","build","WebGL"));Directory.CreateDirectory(path);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Game/Scenes/BattlePrototype.unity"},locationPathName=path,target=BuildTarget.WebGL,options=BuildOptions.None});
        Debug.Log("BLADE_WEBGL_BUILD "+report.summary.result+" "+report.summary.totalErrors);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded||report.summary.totalErrors>0||!File.Exists(Path.Combine(path,"index.html")))throw new Exception("WebGL build failed");
        File.WriteAllText(Path.Combine(path,".nojekyll"),string.Empty);
    }
}
}
