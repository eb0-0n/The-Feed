using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
namespace TheFeed.Editor
{
    [InitializeOnLoad]
    public static class FeedDemoLauncher
    {
        static FeedDemoLauncher()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("FeedDemo",false))
                {
                    SessionState.SetBool("FeedDemo",false);
                    new GameObject("Interaction Recording and Verification").AddComponent<FeedDemo>();
                }
            };
        }
        [MenuItem("The Feed/Interactions/Record and Verify Five Demos")]
        public static void Record()
        {
            EditorSceneManager.OpenScene(FeedBuilder.ScenePath);
            FinalizeScene();
            SessionState.SetBool("FeedDemo",true);
            EditorApplication.EnterPlaymode();
        }
        [MenuItem("The Feed/Interactions/Open Playable Scene")]
        public static void Open()
        {
            EditorSceneManager.OpenScene(FeedBuilder.ScenePath);
            var type=System.Type.GetType("UnityEditor.GameView,UnityEditor");
            if(type!=null) EditorWindow.GetWindow(type).Show();
        }
        static void FinalizeScene()
        {
            var rig=Object.FindFirstObjectByType<FeedRig>();
            const string uiMaterialPath="Assets/TheFeedInteractions/Generated/Readable UI.mat";
            var uiMaterial=AssetDatabase.LoadAssetAtPath<Material>(uiMaterialPath);
            if(!uiMaterial)
            {
                var shader=Shader.Find("TheFeed/ReadableWorldUI");
                if(!shader)throw new System.Exception("Readable UI shader is missing.");
                uiMaterial=new Material(shader);
                AssetDatabase.CreateAsset(uiMaterial,uiMaterialPath);
            }
            var canvas=GameObject.Find("The Feed - World Space UI Canvas");
            foreach(var graphic in canvas.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)) graphic.material=uiMaterial;
            var experience=Object.FindFirstObjectByType<FeedExperience>();
            foreach(var material in experience.windowMaterials)
            {
                material.mainTextureScale=new Vector2(1,-1);
                material.mainTextureOffset=new Vector2(0,1);
                EditorUtility.SetDirty(material);
            }
            foreach(var ray in new[]{rig.desktopRay,rig.leftRay,rig.rightRay})
            {
                ray.selectInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                ray.activateInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                ray.uiPressInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                if(ray==rig.desktopRay) continue;
                var line=ray.GetComponent<LineRenderer>();
                if(!line)line=ray.gameObject.AddComponent<LineRenderer>();
                line.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheFeedInteractions/Generated/Reticle.mat");
                var visual=ray.GetComponent<XRInteractorLineVisual>();
                if(!visual)visual=ray.gameObject.AddComponent<XRInteractorLineVisual>();
                visual.lineWidth=.007f;
            }
            var scenes=EditorBuildSettings.scenes.Where(s=>s.path!=FeedBuilder.ScenePath).ToList();
            scenes.Insert(0,new EditorBuildSettingsScene(FeedBuilder.ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
    }
}
