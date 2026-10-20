using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TheFeed.Editor
{
    public static class FeedBuilder
    {
        public const string ScenePath="Assets/Scenes/TheFeed_Interactions.unity";
        const string Folder="Assets/TheFeedInteractions/Generated";
        static Font font;
        static Color Ink=new Color(.055f,.11f,.13f), Teal=new Color(.12f,.65f,.59f);
        [MenuItem("The Feed/Interactions/Build Assignment Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
            if(File.Exists(ScenePath)) throw new InvalidOperationException("Interaction scene already exists. Open it instead; it will not be overwritten.");
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            if(!AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity",ScenePath)) throw new Exception("Could not copy the source scene.");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach(var tour in UnityEngine.Object.FindObjectsByType<BedroomCameraTour>(FindObjectsSortMode.None)) tour.enabled=false;
            var camera=Camera.main;
            camera.name="XR Player Camera"; camera.nearClipPlane=.03f; camera.fieldOfView=64;
            var root=new GameObject("THE FEED - FIVE INTERACTIONS");
            var manager=new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>(); manager.transform.SetParent(root.transform);
            var exp=root.AddComponent<FeedExperience>(); exp.playerCamera=camera;
            var rigObject=new GameObject("XR Origin - Desktop and Headset"); rigObject.transform.SetParent(root.transform);
            var origin=rigObject.AddComponent<XROrigin>(); origin.Camera=camera;
            var offset=new GameObject("Camera Offset"); offset.transform.SetParent(rigObject.transform,false);
            origin.CameraFloorOffsetObject=offset;
            origin.RequestedTrackingOriginMode=XROrigin.TrackingOriginMode.Floor;
            camera.transform.SetParent(offset.transform,true);
            var rig=rigObject.AddComponent<FeedRig>(); rig.playerCamera=camera; exp.rig=rig;
            rig.desktopRay=Ray("Desktop XR Ray",rigObject.transform,manager);
            rig.leftRay=Ray("Left Controller XR Ray",offset.transform,manager);
            rig.rightRay=Ray("Right Controller XR Ray",offset.transform,manager);
            rig.leftRay.gameObject.SetActive(false); rig.rightRay.gameObject.SetActive(false);
            var reticle=GameObject.CreatePrimitive(PrimitiveType.Sphere); reticle.name="Interaction Reticle"; reticle.transform.SetParent(root.transform);
            UnityEngine.Object.DestroyImmediate(reticle.GetComponent<Collider>());
            reticle.GetComponent<Renderer>().sharedMaterial=Mat("Reticle",Teal,true); rig.reticle=reticle.transform;
            reticle.SetActive(false);
            foreach(var es in UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None)) es.gameObject.SetActive(false);
            var eventObject=new GameObject("XR UI Event System",typeof(EventSystem),typeof(XRUIInputModule)); eventObject.transform.SetParent(root.transform);
            var input=eventObject.GetComponent<XRUIInputModule>(); input.enableMouseInput=false; input.enableTouchInput=false; input.enableGamepadInput=false; input.enableXRInput=true;

            var phone=GameObject.Find("Phone - Primitive Detail");
            phone.transform.localScale=new Vector3(.38f,.045f,.22f);
            var phoneScreen=Cube("Phone Notification Display",phone.transform,new Vector3(0,.56f,0),new Vector3(.85f,.05f,.76f),Mat("Phone Notifications",new Color(.2f,.36f,.75f),true),true);
            exp.phoneScreen=phoneScreen.GetComponent<Renderer>();
            exp.phoneQuietMaterial=Mat("Phone - Quiet",new Color(.11f,.68f,.47f),true);
            var laptop=GameObject.Find("Laptop Display"); exp.laptopScreen=laptop.GetComponent<Renderer>();
            exp.laptopFocusMaterial=Mat("Laptop - Focus",new Color(.85f,.63f,.24f),true);
            exp.windowView=GameObject.Find("Morning Sky").GetComponent<Renderer>();
            exp.windowLight=GameObject.Find("Window Fill - Point Light").GetComponent<Light>();
            exp.windowMaterials=new Material[3];
            for(int i=0;i<3;i++) exp.windowMaterials[i]=WindowMat(i);
            exp.windowView.sharedMaterial=exp.windowMaterials[0];
            var lamp=new GameObject("Focus Desk Light"); lamp.transform.SetParent(root.transform);
            lamp.transform.position=laptop.transform.position+new Vector3(-.25f,.25f,0);
            exp.focusLight=lamp.AddComponent<Light>(); exp.focusLight.type=LightType.Point; exp.focusLight.color=new Color(1,.8f,.43f); exp.focusLight.range=2; exp.focusLight.intensity=2; exp.focusLight.enabled=false;
            var door=GameObject.Find("Door Slab");
            var hinge=new GameObject("Interactive Door Hinge"); hinge.transform.SetParent(door.transform.parent);
            hinge.transform.position=new Vector3(3.97f,0,-3.39f); door.transform.SetParent(hinge.transform,true);
            GameObject.Find("Door Handle").transform.SetParent(hinge.transform,true); exp.doorPivot=hinge.transform;

            exp.points=new[] {
                Point(root,exp,manager,FeedInteractable.Point.Phone,phone.transform.position+Vector3.up*.08f,new Vector3(.52f,.25f,.42f),new[]{phone.GetComponent<Renderer>(),exp.phoneScreen},"PHONE",.25f),
                Point(root,exp,manager,FeedInteractable.Point.Laptop,laptop.transform.position,new Vector3(.13f,.51f,.65f),new[]{exp.laptopScreen},"LAPTOP",.39f),
                Point(root,exp,manager,FeedInteractable.Point.Window,new Vector3(1.65f,1.85f,3.22f),new Vector3(3.08f,1.52f,.12f),new[]{exp.windowView},"WINDOW",.89f),
                Point(root,exp,manager,FeedInteractable.Point.Door,new Vector3(3.3f,1.1f,-3.28f),new Vector3(1.34f,2.2f,.12f),new[]{door.GetComponent<Renderer>()},"LEAVE FOR CLASS",1.23f)
            };
            var stations=new GameObject("Desktop Demo Viewpoints"); stations.transform.SetParent(root.transform);
            rig.viewpoints=new[] {
                View(stations,"1 - Start",new Vector3(.0f,1.6f,-.8f),new Vector3(-1.3f,1.5f,2.4f)),
                View(stations,"2 - Phone",phone.transform.position+new Vector3(.62f,.88f,-1.02f),phone.transform.position),
                View(stations,"3 - Laptop",laptop.transform.position+new Vector3(-1.25f,.45f,-.36f),laptop.transform.position),
                View(stations,"4 - Window",new Vector3(1.45f,1.7f,.7f),new Vector3(1.65f,1.85f,3.2f)),
                View(stations,"5 - Door",new Vector3(2.05f,1.6f,-1.42f),new Vector3(3.3f,1.35f,-3.3f))
            };
            rig.GoTo(0);
            BuildUI(exp);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(),"TheFeed_Interactions_Build.txt"),"Created "+ScenePath+"\n4 XR Simple Interactables; XR Origin; 3 XR Ray Interactors; world-space Canvas; XR UI Input Module.\nOriginal SampleScene preserved.");
            Debug.Log("FEED_BUILD_COMPLETE "+ScenePath);
        }
        static XRRayInteractor Ray(string name,Transform parent,XRInteractionManager manager)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            var ray=go.AddComponent<XRRayInteractor>(); ray.interactionManager=manager;
            ray.enableUIInteraction=true; ray.maxRaycastDistance=12; ray.allowHoveredActivate=true;
            ray.raycastTriggerInteraction=QueryTriggerInteraction.Collide;
            return ray;
        }
        static FeedInteractable Point(GameObject root,FeedExperience exp,XRInteractionManager manager,FeedInteractable.Point type,Vector3 position,Vector3 size,Renderer[] renderers,string title,float labelHeight)
        {
            var go=new GameObject(type+" - XR Simple Interactable"); go.transform.SetParent(root.transform); go.transform.position=position; go.layer=8;
            var collider=go.AddComponent<BoxCollider>(); collider.size=size; collider.isTrigger=true;
            var xr=go.AddComponent<XRSimpleInteractable>(); xr.interactionManager=manager; xr.colliders.Add(collider);
            var p=go.AddComponent<FeedInteractable>(); p.point=type; p.experience=exp; p.highlights=renderers;
            var label=new GameObject(title+" Label",typeof(RectTransform),typeof(Canvas)); label.transform.SetParent(go.transform);
            label.transform.localPosition=new Vector3(0,labelHeight,0); label.transform.localScale=Vector3.one*.002f;
            label.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace; label.GetComponent<RectTransform>().sizeDelta=new Vector2(340,60);
            var text=Text(label.transform,"Label",title,24,Vector2.zero,new Vector2(340,60),TextAnchor.MiddleCenter);
            text.color=Color.white; var outline=text.gameObject.AddComponent<Outline>(); outline.effectColor=Color.black; outline.effectDistance=new Vector2(1,-1);
            p.label=label.transform;
            return p;
        }
        static Transform View(GameObject parent,string name,Vector3 pos,Vector3 target)
        {
            var go=new GameObject(name); go.transform.SetParent(parent.transform); go.transform.position=pos; go.transform.rotation=Quaternion.LookRotation(target-pos); return go.transform;
        }
        static Material Mat(string name,Color color,bool unlit=false)
        {
            var mat=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit")); mat.name=name; mat.color=color;
            AssetDatabase.CreateAsset(mat,Folder+"/"+name+".mat"); return mat;
        }
        static Material WindowMat(int mood)
        {
            var mat=Mat("Window - "+new[]{"Morning","Sunset","Night"}[mood],Color.white,true);
            var tex=new Texture2D(512,256,TextureFormat.RGBA32,false);
            Color[] top={new Color(.2f,.58f,.82f),new Color(.32f,.19f,.48f),new Color(.015f,.035f,.11f)};
            Color[] low={new Color(.82f,.92f,.94f),new Color(1,.58f,.29f),new Color(.12f,.21f,.4f)};
            for(int y=0;y<256;y++) for(int x=0;x<512;x++)
            {
                float u=x/511f,v=y/255f; Color color=Color.Lerp(low[mood],top[mood],v);
                float hill=.16f+.045f*Mathf.Sin(u*16)+.025f*Mathf.Sin(u*31);
                if(v<hill) color=mood==2?new Color(.025f,.08f,.10f):new Color(.13f,.32f,.29f);
                Vector2 sun=new Vector2((u-.74f)*2,v-(mood==1?.35f:.74f));
                if(sun.magnitude<.095f) color=mood==2?new Color(.89f,.93f,.95f):new Color(1,.92f,.67f);
                if(mood==2 && v>.33f && ((x*73+y*151)%1231==0)) color=Color.white;
                tex.SetPixel(x,y,color);
            }
            tex.Apply(); tex.name="Window View "+mood; AssetDatabase.CreateAsset(tex,Folder+"/WindowView"+mood+".asset"); mat.mainTexture=tex; return mat;
        }
        static GameObject Cube(string name,Transform parent,Vector3 pos,Vector3 scale,Material mat,bool local)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent,false);
            go.transform.localPosition=pos; go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=mat;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=pos; return rect;
        }
        static Text Text(Transform parent,string name,string value,int size,Vector2 pos,Vector2 dimensions,TextAnchor align=TextAnchor.UpperLeft)
        {
            var t=Rect(parent,name,pos,dimensions).gameObject.AddComponent<Text>(); t.font=font; t.fontSize=size; t.text=value; t.color=Color.white; t.alignment=align; t.raycastTarget=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Overflow; return t;
        }
        static GameObject Panel(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
        {
            var rect=Rect(parent,name,pos,size); var image=rect.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false; return rect.gameObject;
        }
        static Button Button(Transform parent,string label,Vector2 pos)
        {
            var go=Panel(parent,label+" Button",pos,new Vector2(310,64),Teal);
            var image=go.GetComponent<Image>(); image.raycastTarget=true;
            var button=go.AddComponent<Button>(); var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.66f,1,.86f); colors.pressedColor=new Color(1,.77f,.36f); button.colors=colors;
            button.navigation=new Navigation{mode=Navigation.Mode.None};
            Text(go.transform,"Text",label,24,Vector2.zero,new Vector2(300,56),TextAnchor.MiddleCenter); return button;
        }
        static void BuildUI(FeedExperience exp)
        {
            var go=new GameObject("The Feed - World Space UI Canvas",typeof(RectTransform),typeof(Canvas),typeof(TrackedDeviceGraphicRaycaster));
            go.transform.SetParent(exp.playerCamera.transform,false); go.transform.localPosition=new Vector3(0,0,1.8f); go.transform.localScale=Vector3.one*.0023f;
            var canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace; canvas.worldCamera=exp.playerCamera; canvas.sortingOrder=10;
            go.GetComponent<RectTransform>().sizeDelta=new Vector2(960,600);
            var header=Panel(go.transform,"Morning Header",new Vector2(0,278),new Vector2(910,40),Ink);
            exp.progress=Text(header.transform,"Progress","THE FEED",17,Vector2.zero,new Vector2(870,32),TextAnchor.MiddleLeft);
            var footer=Panel(go.transform,"Interaction Feedback",new Vector2(0,-266),new Vector2(910,72),Ink);
            exp.status=Text(footer.transform,"Status","",17,Vector2.zero,new Vector2(875,63),TextAnchor.MiddleLeft);
            exp.informationPanel=Panel(go.transform,"Information Panel",new Vector2(280,115),new Vector2(350,240),new Color(.055f,.11f,.13f,.94f));
            exp.heading=Text(exp.informationPanel.transform,"Heading","",22,new Vector2(0,84),new Vector2(310,52)); exp.heading.color=new Color(.47f,.92f,.80f);
            exp.body=Text(exp.informationPanel.transform,"Body","",18,new Vector2(0,-16),new Vector2(310,152));
            exp.informationPanel.SetActive(false);
            exp.startPanel=Panel(go.transform,"Start Screen",Vector2.zero,new Vector2(640,396),Ink);
            Text(exp.startPanel.transform,"Title","THE FEED",50,new Vector2(0,110),new Vector2(540,68),TextAnchor.MiddleCenter);
            Text(exp.startPanel.transform,"Intro","Good morning, Maya.\nA new day. Three small choices.\nMake room for the world beyond the screen.",24,new Vector2(0,10),new Vector2(550,140),TextAnchor.MiddleCenter);
            exp.startButton=Button(exp.startPanel.transform,"START MY MORNING",new Vector2(0,-124));
            exp.endPanel=Panel(go.transform,"End Screen",Vector2.zero,new Vector2(660,416),Ink);
            Text(exp.endPanel.transform,"Title","YOU'RE READY",42,new Vector2(0,133),new Vector2(560,65),TextAnchor.MiddleCenter);
            exp.ending=Text(exp.endPanel.transform,"Summary","",23,new Vector2(0,20),new Vector2(570,168),TextAnchor.MiddleCenter);
            exp.restartButton=Button(exp.endPanel.transform,"START AGAIN",new Vector2(0,-139));
            exp.endPanel.SetActive(false);
        }
    }
}
