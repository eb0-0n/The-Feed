using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TheFeed.Quiet;
namespace TheFeed.Opening.Editor
{
    public static class GuidanceBuilder
    {
        public const string Scene="Assets/Scenes/TheFeed_Story_Guidance.unity";
        public const string Dir="Assets/TheFeedOpening/Art";
        public const string Output="/Users/efabunmi/Documents/Codex/2026-10-19/i-n-2/outputs/TheFeed-Monday-Morning";
        static void DressAndLight(QuietMorning game)
        {
            var profilePath=Dir+"/Morning_Presentation.asset";
            var volume=game.GetComponent<UnityEngine.Rendering.Volume>();
            if(volume)
            {
                var profile=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(profilePath);
                if(!profile){profile=Object.Instantiate(volume.sharedProfile);AssetDatabase.CreateAsset(profile,profilePath);}
                volume.sharedProfile=profile;
                if(profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var color)){color.postExposure.Override(.65f);color.contrast.Override(6);color.saturation.Override(-4);}
                if(profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out var v))v.intensity.Override(.055f);
                EditorUtility.SetDirty(profile);
            }
            RenderSettings.ambientSkyColor=new Color(.57f,.61f,.63f);
            RenderSettings.ambientEquatorColor=new Color(.42f,.42f,.37f);
            RenderSettings.ambientGroundColor=new Color(.24f,.215f,.18f);
            RenderSettings.fogDensity=.009f;
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type==LightType.Directional){l.color=new Color(1,.88f,.68f);l.intensity=2.15f;l.transform.rotation=Quaternion.Euler(32,165,0);}
                else l.shadows=LightShadows.None;
            }
            game.windowLight.intensity=2.2f;
            var shader=Shader.Find("TheFeed/PhysicalSurfaceUI");
            var material=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Physical_UI.mat");
            if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,Dir+"/Physical_UI.mat");}
            foreach(var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if(!c.transform.IsChildOf(game.rig.playerCamera.transform))
                    foreach(var graphic in c.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))graphic.material=material;
            var notes=GameObject.Find("Paper typography");
            if(notes)notes.GetComponentInChildren<UnityEngine.UI.Text>().text="FIELD STUDY\nMemory + place\n\nNorth Pier, Sunday\nlight / sound / movement\n\nAsk Eva for the photo\nshe took on the steps.\n\nStudio review 10:10";
            var postcard=GameObject.Find("Postcard");
            if(postcard)
            {
                var handwritten=postcard.transform.Find("Handwritten").GetComponent<UnityEngine.UI.Text>();
                handwritten.text="NORTH PIER\nSunday, with Eva.\n\nBring the prints.";handwritten.fontSize=22;
                handwritten.rectTransform.anchoredPosition=new Vector2(0,-90);handwritten.rectTransform.sizeDelta=new Vector2(250,150);
                var existing=postcard.transform.Find("A weekend memory");
                var photo=existing?existing.gameObject:new GameObject("A weekend memory",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));
                photo.transform.SetParent(postcard.transform,false);
                var rect=photo.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(0,85);rect.sizeDelta=new Vector2(260,173);
                var image=photo.GetComponent<UnityEngine.UI.RawImage>();image.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/North_Pier_Memory.png");image.material=material;image.raycastTarget=false;
            }
        }
        public static void Build()
        {
            Directory.CreateDirectory(Output);
            if(!File.Exists(Scene))AssetDatabase.CopyAsset("Assets/Scenes/TheFeed_QuietMorning.unity",Scene);
            EditorSceneManager.OpenScene(Scene);
            var game=Object.FindFirstObjectByType<QuietMorning>();
            var guidance=game.GetComponent<OpeningGuidance>();if(!guidance)guidance=game.gameObject.AddComponent<OpeningGuidance>();guidance.game=game;game.storyMorning=true;
            guidance.storyFont=AssetDatabase.LoadAssetAtPath<Font>(Dir+"/CrimsonText-Regular.ttf");
            DressAndLight(game);
            var phoneCanvas=game.phoneText.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            phoneCanvas.sizeDelta=new Vector2(540,300);
            game.phoneText.rectTransform.anchoredPosition=Vector2.zero;game.phoneText.rectTransform.sizeDelta=new Vector2(480,276);
            game.phoneText.text=OpeningGuidance.EvaMessage;game.phoneText.fontSize=18;game.phoneText.alignment=UnityEngine.TextAnchor.UpperLeft;
            game.laptopText.text="STUDIO / MEMORY + PLACE\n\nNorth_Pier_Study.pdf\nReady to submit\n\nDue Monday, 08:45";
            game.timerText.text="DUE MONDAY  08:45";
            var canvas=game.subtitle.GetComponentInParent<Canvas>();canvas.enabled=false;
            game.rig.GoTo(0);TheFeed.Quiet.Editor.QuietReview.Shot(game.rig.playerCamera,Dir+"/Room_Example.png");
            game.rig.GoTo(1);TheFeed.Quiet.Editor.QuietReview.Shot(game.rig.playerCamera,Dir+"/Phone_Example.png");
            game.rig.GoTo(2);TheFeed.Quiet.Editor.QuietReview.Shot(game.rig.playerCamera,Dir+"/Desk_Example.png");
            game.rig.GoTo(3);TheFeed.Quiet.Editor.QuietReview.Shot(game.rig.playerCamera,Dir+"/Window_Example.png");
            AssetDatabase.Refresh();guidance.deskExample=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Desk_Example.png");guidance.windowExample=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Window_Example.png");guidance.roomExample=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Room_Example.png");guidance.phoneExample=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Phone_Example.png");
            foreach(var name in new[]{"Room_Example.png","Phone_Example.png","Desk_Example.png","Window_Example.png"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(Dir+"/"+name);importer.maxTextureSize=1024;importer.mipmapEnabled=false;importer.SaveAndReimport();
            }
            canvas.enabled=true;
            game.rig.GoTo(0);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("STORY_GUIDANCE_BUILT");
        }
    }
}
