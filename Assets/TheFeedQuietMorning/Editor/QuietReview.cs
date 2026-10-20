using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
namespace TheFeed.Quiet.Editor
{
    [InitializeOnLoad]
    public static class QuietReview
    {
        public const string Output="/Users/efabunmi/Documents/Codex/2026-10-19/i-n-2/work/quiet-preview";
        static QuietReview()
        {
            EditorApplication.playModeStateChanged+=state=>{
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("QuietCapture",false))
                {SessionState.SetBool("QuietCapture",false);new GameObject("Revised gameplay verification").AddComponent<QuietCapture>();}
            };
        }
        [MenuItem("The Feed/Quiet Morning/Record and Verify")]
        public static void Record()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            Preview();SessionState.SetBool("QuietCapture",true);EditorApplication.EnterPlaymode();
        }
        public static void Preview()
        {
            EditorSceneManager.OpenScene(QuietBuilder.Scene);Tune();Directory.CreateDirectory(Output);
            var game=Object.FindFirstObjectByType<QuietMorning>();var rig=game.rig;
            game.subtitleGroup.alpha=0;
            for(int i=0;i<5;i++)
            {
                rig.GoTo(i);game.title.SetActive(i==0);game.ending.SetActive(false);
                if(i==0){game.title.SetActive(false);Shot(rig.playerCamera,Output+"/Room.png");game.title.SetActive(true);}
                Shot(rig.playerCamera,Output+"/Preview_"+i+".png");
            }
            Debug.Log("QUIET_PREVIEW_COMPLETE");
        }
        public static void Tune()
        {
            var game=Object.FindFirstObjectByType<QuietMorning>();
            var locomotion=game.rig.GetComponent<QuietLocomotion>();if(!locomotion)locomotion=game.rig.gameObject.AddComponent<QuietLocomotion>();locomotion.rig=game.rig;
            game.footstep=AssetDatabase.LoadAssetAtPath<AudioClip>(QuietBuilder.Dir+"/Soft wood footstep.wav");
            if(!game.footstep)game.footstep=QuietBuilder.AudioAsset("Soft wood footstep",.3f,6);
            GameObject.Find("Quiet interface").transform.localScale=Vector3.one*.004f;
            game.subtitleGroup.alpha=0;
            var postcard=GameObject.Find("Postcard").transform.Find("Handwritten").GetComponent<UnityEngine.UI.Text>();postcard.text="NORTH PIER\n\nSEPTEMBER\n\nwish you\nwere here.\n\n— E.";postcard.fontSize=25;
            var plaster=AssetDatabase.LoadAssetAtPath<Material>(QuietBuilder.Dir+"/Weathered warm plaster.mat");plaster.SetFloat("_BumpScale",.18f);plaster.SetColor("_BaseColor",new Color(.67f,.67f,.59f));EditorUtility.SetDirty(plaster);
            RenderSettings.ambientSkyColor=new Color(.39f,.42f,.41f);RenderSettings.ambientEquatorColor=new Color(.28f,.29f,.26f);RenderSettings.ambientGroundColor=new Color(.18f,.155f,.12f);
            var grade=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(QuietBuilder.Dir+"/Quiet grade.asset");
            if(grade.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var color))color.postExposure.Override(1.0f);EditorUtility.SetDirty(grade);
            AddLight("Courtyard soft sky fill",new Vector3(1,4,9),new Color(.7f,.79f,.78f),4.5f,16);
            AddLight("Door practical fill",new Vector3(3.3f,2.42f,-2.97f),new Color(1,.79f,.53f),1.25f,3.3f);
            if(!GameObject.Find("Door wall light housing"))
            {
                var housing=GameObject.CreatePrimitive(PrimitiveType.Cube);housing.name="Door wall light housing";housing.transform.position=new Vector3(3.3f,2.62f,-3.3f);housing.transform.localScale=new Vector3(.34f,.10f,.11f);Object.DestroyImmediate(housing.GetComponent<Collider>());housing.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(QuietBuilder.Dir+"/Oxidized metal.mat");
            }
            if(!GameObject.Find("Desk reading lamp"))
            {
                var lamp=Object.Instantiate(GameObject.Find("Right Bedside Lamp"));lamp.name="Desk reading lamp";lamp.transform.position=Vector3.zero;
                var bounds=BoundsOf(lamp);lamp.transform.localScale*=.46f/bounds.size.y;bounds=BoundsOf(lamp);
                lamp.transform.position=new Vector3(3.77f-bounds.center.x,.924f-bounds.min.y,1.04f-bounds.center.z);
            }
            game.deskLight.transform.position=new Vector3(3.77f,1.24f,1.04f);game.deskLight.intensity=1.9f;
            game.phoneText.fontSize=33;
            GameObject.Find("Phone screen content").transform.rotation=Quaternion.Euler(90,GameObject.Find("Phone - Primitive Detail").transform.eulerAngles.y,180);
            game.againButton.GetComponentInChildren<UnityEngine.UI.Text>().text="PLAY AGAIN    →";
            game.ending.transform.Find("End scrim").GetComponent<RectTransform>().sizeDelta=new Vector2(1450,850);
            game.rig.viewpoints[1].position=game.points[0].transform.position+new Vector3(.25f,.55f,-.54f);
            game.rig.viewpoints[1].rotation=Quaternion.LookRotation(game.points[0].transform.position-game.rig.viewpoints[1].position);
            game.title.transform.Find("Title").GetComponent<UnityEngine.UI.Text>().fontStyle=FontStyle.Bold;
            string gradientPath=QuietBuilder.Dir+"/Menu shadow gradient.asset";
            var gradient=AssetDatabase.LoadAssetAtPath<Texture2D>(gradientPath);
            if(!gradient)
            {
                gradient=new Texture2D(256,1,TextureFormat.RGBA32,false);gradient.wrapMode=TextureWrapMode.Clamp;
                for(int x=0;x<256;x++)gradient.SetPixel(x,0,new Color(.018f,.024f,.02f,Mathf.SmoothStep(.96f,0,x/255f)));
                gradient.Apply();AssetDatabase.CreateAsset(gradient,gradientPath);
            }
            var scrim=game.title.transform.Find("Title scrim").gameObject;
            var oldImage=scrim.GetComponent<UnityEngine.UI.Image>();if(oldImage)Object.DestroyImmediate(oldImage);
            var raw=scrim.GetComponent<UnityEngine.UI.RawImage>();if(!raw)raw=scrim.AddComponent<UnityEngine.UI.RawImage>();
            raw.texture=gradient;raw.color=Color.white;raw.raycastTarget=false;raw.material=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheFeedInteractions/Generated/Readable UI.mat");
            var rect=scrim.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(-110,0);rect.sizeDelta=new Vector2(850,900);
            var round=RoundedBox();
            foreach(string name in new[]{"Phone - Primitive Detail","Laptop Base","Laptop Screen","Shoe Left","Shoe Right"})
            {
                var detail=GameObject.Find(name);
                if(!detail || !detail.TryGetComponent<MeshFilter>(out var mesh))
                    throw new System.InvalidOperationException("Missing enclosure mesh: "+name);
                mesh.sharedMesh=round;
            }
            foreach(var ray in new[]{game.rig.leftRay,game.rig.rightRay})
            {
                var visual=ray.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
                if(!visual)continue;
                var line=new Gradient();line.SetKeys(new[]{new GradientColorKey(new Color(.78f,.74f,.61f),0),new GradientColorKey(new Color(.95f,.90f,.73f),1)},new[]{new GradientAlphaKey(.18f,0),new GradientAlphaKey(.7f,1)});
                visual.lineWidth=.003f;visual.validColorGradient=line;visual.invalidColorGradient=line;visual.blockedColorGradient=line;
            }
            game.rig.viewpoints[2].position=game.points[1].transform.position+new Vector3(-.92f,.31f,-.28f);
            game.rig.viewpoints[2].rotation=Quaternion.LookRotation(game.points[1].transform.position-game.rig.viewpoints[2].position);
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t.name=="Rain beyond glass" && !t.GetComponent<QuietRain>())t.gameObject.AddComponent<QuietRain>();
                if(t.name=="Panel molding")t.localScale=new Vector3(.035f/1.34f,.8f/2.2f,.035f/.12f);
            }
            game.rig.GoTo(0);EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static Bounds BoundsOf(GameObject o)
        {
            var renderers=o.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);return b;
        }
        static void AddLight(string name,Vector3 p,Color c,float intensity,float range)
        {
            var go=GameObject.Find(name);if(!go)go=new GameObject(name,typeof(Light));go.transform.position=p;var l=go.GetComponent<Light>();l.type=LightType.Point;l.color=c;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }
        static Mesh RoundedBox()
        {
            string path=QuietBuilder.Dir+"/Soft edge enclosure.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
            var vertices=new System.Collections.Generic.List<Vector3>();var normals=new System.Collections.Generic.List<Vector3>();var uv=new System.Collections.Generic.List<Vector2>();var indices=new System.Collections.Generic.List<int>();
            Vector3[] axes={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};float[] c={-.5f,-.42f,.42f,.5f};
            foreach(var n in axes)
            {
                var u=Mathf.Abs(n.y)>.9f?Vector3.right:Vector3.up;var v=Vector3.Cross(n,u);int start=vertices.Count;
                for(int j=0;j<4;j++)for(int i=0;i<4;i++)
                {
                    Vector3 p=n*.5f+u*c[i]+v*c[j];var inner=new Vector3(Mathf.Clamp(p.x,-.42f,.42f),Mathf.Clamp(p.y,-.42f,.42f),Mathf.Clamp(p.z,-.42f,.42f));var normal=(p-inner).normalized;
                    vertices.Add(inner+normal*.08f);normals.Add(normal);uv.Add(new Vector2(c[i]+.5f,c[j]+.5f));
                }
                for(int j=0;j<3;j++)for(int i=0;i<3;i++){int a=start+j*4+i;indices.AddRange(new[]{a,a+1,a+5,a,a+5,a+4});}
            }
            var m=new Mesh();m.SetVertices(vertices);m.SetNormals(normals);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;
        }
        public static void Shot(Camera cam,string path)
        {
            var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);rt.Create();var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            var old=cam.targetTexture;var active=RenderTexture.active;cam.targetTexture=rt;Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());cam.targetTexture=old;RenderTexture.active=active;rt.Release();Object.DestroyImmediate(image);Object.DestroyImmediate(rt);
        }
    }
}
