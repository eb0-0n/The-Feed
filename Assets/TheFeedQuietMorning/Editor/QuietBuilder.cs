using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

namespace TheFeed.Quiet.Editor
{
    public static class QuietBuilder
    {
        public const string Scene="Assets/Scenes/TheFeed_QuietMorning.unity";
        public const string Dir="Assets/TheFeedQuietMorning/Art";
        const string Kit="Assets/Brick Project Studio/Apartment Kit/Common/Textures/Surface/";
        const string Legacy="Assets/Brick Project Studio/_BPS Basic Assets/Common/Textures/Legacy/";
        static Transform root;
        static Font font;
        static Material ink,paper,wood,plaster,metal,linen,leaf,brick,ui;
        static int meshIndex;
        static Color Cream=new Color(.88f,.85f,.76f);
        [MenuItem("The Feed/Quiet Morning/Build Revised Scene")]
        public static void Build()
        {
            if(File.Exists(Scene))throw new Exception("Revised scene already exists. It will not be overwritten.");
            Directory.CreateDirectory(Dir); AssetDatabase.Refresh(); Random.InitState(204);
            AssetDatabase.CopyAsset("Assets/Scenes/TheFeed_Interactions.unity",Scene);
            var scene=EditorSceneManager.OpenScene(Scene);
            root=new GameObject("QUIET MORNING - ART DIRECTION").transform;
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var old=Object.FindFirstObjectByType<FeedExperience>();
            var game=root.gameObject.AddComponent<QuietMorning>(); game.rig=old.rig;
            game.door=old.doorPivot; game.deskLight=old.focusLight; game.windowLight=old.windowLight;
            game.phoneDisplay=old.phoneScreen; game.laptopDisplay=old.laptopScreen;
            var manager=Object.FindFirstObjectByType<XRInteractionManager>();
            game.points=new QuietPoint[4];
            for(int i=0;i<4;i++)
            {
                var previous=old.points[i]; var surfaces=previous.highlights;
                if(previous.label)Object.DestroyImmediate(previous.label.gameObject);
                var go=previous.gameObject; Object.DestroyImmediate(previous);
                var p=go.AddComponent<QuietPoint>(); p.id=i;p.game=game;p.surfaces=surfaces;game.points[i]=p;
            }
            Object.DestroyImmediate(GameObject.Find("The Feed - World Space UI Canvas"));
            Object.DestroyImmediate(old);
            Hide("THE FEED - Morning Display"); Hide("Morning Sky"); Hide("Back Wall - Primitive");
            Hide("Curtain Left");Hide("Curtain Right");Hide("Photo A");Hide("Photo B");
            foreach(var v in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))v.enabled=false;
            Materials(); Architecture(); DressRoom(game); Exterior(); Lighting(game); Interface(game); Audio(game);
            game.rig.reticle.GetComponent<Renderer>().sharedMaterial=Mat("Warm reticle",Cream,true);
            game.rig.viewpoints[0].SetPositionAndRotation(new Vector3(-.15f,1.58f,-1.15f),Quaternion.LookRotation(new Vector3(1.2f,1.7f,3.4f)-new Vector3(-.15f,1.58f,-1.15f)));
            game.rig.viewpoints[1].position+=new Vector3(-.1f,-.16f,.18f);
            game.rig.viewpoints[1].rotation=Quaternion.LookRotation(game.points[0].transform.position-game.rig.viewpoints[1].position);
            game.rig.viewpoints[2].position=game.points[1].transform.position+new Vector3(-.92f,.31f,-.28f);
            game.rig.viewpoints[2].rotation=Quaternion.LookRotation(game.points[1].transform.position-game.rig.viewpoints[2].position);
            game.rig.GoTo(0);
            var scenes=EditorBuildSettings.scenes.Where(s=>s.path!=Scene).ToList();scenes.Insert(0,new EditorBuildSettingsScene(Scene,true));EditorBuildSettings.scenes=scenes.ToArray();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("QUIET_BUILD_COMPLETE");
        }
        static void Hide(string name){var o=GameObject.Find(name);if(o)o.SetActive(false);}
        static Material Mat(string name,Color color,bool unlit=false,string tex=null,string normal=null,Vector2? tiling=null)
        {
            var m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.name=name;
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.18f);
            if(tex!=null)m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex));
            if(normal!=null){m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normal));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.45f);}
            if(tiling.HasValue)m.mainTextureScale=tiling.Value;
            AssetDatabase.CreateAsset(m,Dir+"/"+name+".mat");return m;
        }
        static void Materials()
        {
            ink=Mat("Soft charcoal",new Color(.045f,.05f,.047f));
            paper=Mat("Old paper",new Color(.7f,.67f,.54f));
            wood=Mat("Aged oak",new Color(.47f,.4f,.3f),false,Kit+"Rustic Wood.png",Kit+"Rustic Wood - Normal.png");
            plaster=Mat("Weathered warm plaster",new Color(.58f,.59f,.52f),false,Legacy+"Concrete-texture.png",Legacy+"Concrete-normal.png");
            linen=Mat("Washed linen",new Color(.42f,.4f,.33f),false,Kit+"Fabric_01_A.png",Kit+"Fabric_01_N.png",new Vector2(4,4));
            metal=Mat("Oxidized metal",new Color(.18f,.19f,.16f),false,Kit+"CastIron_A.png",Kit+"CastIron_N.png");metal.SetFloat("_Metallic",.45f);
            leaf=Mat("Muted ivy",new Color(.18f,.24f,.095f));leaf.SetFloat("_Cull",0);
            brick=Mat("Courtyard brick",new Color(.5f,.48f,.4f),false,Legacy+"Brick 02 Mute Texture.png",Legacy+"Brick 02 normal.png");
            ui=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheFeedInteractions/Generated/Readable UI.mat");
            // Desaturate copies, never the user's source materials.
            var copies=new Dictionary<Material,Material>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r is not MeshRenderer)continue;
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var src=mats[i];if(!src || !src.shader.name.Contains("Universal"))continue;
                    if(!copies.TryGetValue(src,out var copy))
                    {
                        copy=new Material(src);copy.name="Room "+copies.Count+" "+src.name;
                        if(copy.HasProperty("_BaseColor"))
                        {var c=copy.GetColor("_BaseColor");float g=c.grayscale;copy.SetColor("_BaseColor",Color.Lerp(c,new Color(g*1.03f,g,g*.9f,c.a),.4f));}
                        if(copy.HasProperty("_Smoothness"))copy.SetFloat("_Smoothness",Mathf.Min(.3f,copy.GetFloat("_Smoothness")));
                        AssetDatabase.CreateAsset(copy,Dir+"/Room_"+copies.Count+".mat");copies[src]=copy;
                    }
                    mats[i]=copy;
                }
                r.sharedMaterials=mats;
            }
        }
        static GameObject Box(string name,Vector3 p,Vector3 size,Material mat,Transform parent=null,bool collision=false,bool worldUV=false)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent?parent:root);
            o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collision)Object.DestroyImmediate(o.GetComponent<Collider>());
            if(worldUV)WorldUV(o);return o;
        }
        static void WorldUV(GameObject o)
        {
            var f=o.GetComponent<MeshFilter>();var m=Object.Instantiate(f.sharedMesh);var uv=m.uv;var n=m.normals;var v=m.vertices;var scale=o.transform.localScale;
            for(int i=0;i<uv.Length;i++)
            {
                var p=Vector3.Scale(v[i],scale);
                uv[i]=Mathf.Abs(n[i].y)>.5f?new Vector2(p.x,p.z):Mathf.Abs(n[i].x)>.5f?new Vector2(p.z,p.y):new Vector2(p.x,p.y);
            }
            m.uv=uv;AssetDatabase.CreateAsset(m,Dir+"/WorldUV_"+(meshIndex++)+".asset");f.sharedMesh=m;
        }
        static GameObject Cylinder(string name,Vector3 p,Vector3 scale,Material material)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name=name;o.transform.SetParent(root);o.transform.position=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(o.GetComponent<Collider>());return o;
        }
        static void SetMat(string name,Material material){var o=GameObject.Find(name);if(o && o.TryGetComponent<Renderer>(out var r))r.sharedMaterial=material;}
        static void Architecture()
        {
            foreach(var name in new[]{"Left Wall - Primitive","Right Wall - Primitive","Front Wall Left - Primitive","Front Wall Right - Primitive","Door Header - Primitive"})
            {var o=GameObject.Find(name);o.GetComponent<Renderer>().sharedMaterial=plaster;WorldUV(o);}
            Box("Back wall left",new Vector3(-2.25f,1.6f,3.5f),new Vector3(4.5f,3.2f,.18f),plaster,null,true,true);
            Box("Back wall right",new Vector3(3.9f,1.6f,3.5f),new Vector3(1.2f,3.2f,.18f),plaster,null,true,true);
            Box("Window below",new Vector3(1.65f,.48f,3.5f),new Vector3(3.3f,.96f,.18f),plaster,null,true,true);
            Box("Window lintel",new Vector3(1.65f,2.94f,3.5f),new Vector3(3.3f,.52f,.18f),plaster,null,true,true);
            Box("Plaster ceiling",new Vector3(0,3.23f,0),new Vector3(9,.12f,7),plaster,null,true,true);
            var floorMat=Mat("Scuffed floorboards",new Color(.46f,.38f,.27f),false,Legacy+"Wood Floor 01.png",Legacy+"Floor_normal.png",new Vector2(1.7f,2.5f));
            var floor=GameObject.Find("Floor - Primitive");floor.GetComponent<Renderer>().sharedMaterial=floorMat;WorldUV(floor);
            foreach(var x in new[]{-4.38f,4.38f})Box("Worn skirting",new Vector3(x,.12f,0),new Vector3(.09f,.24f,7),wood,null,false,true);
            Box("Back skirting",new Vector3(0,.12f,3.38f),new Vector3(9,.24f,.09f),wood,null,false,true);
            Box("Front skirting",new Vector3(-.9f,.12f,-3.38f),new Vector3(7,.24f,.09f),wood,null,false,true);
            foreach(var name in new[]{"Frame Top","Frame Bottom","Frame Left","Frame Right","Frame Center","Door Slab"})SetMat(name,wood);
            Box("Deep window sill",new Vector3(1.65f,1.0f,3.23f),new Vector3(3.45f,.075f,.47f),wood,null,false,true);
            // Fine molding gives the plain primitive door a believable construction.
            var door=GameObject.Find("Door Slab");
            foreach(float y in new[]{.52f,1.48f})
            {
                var panel=Box("Recessed door panel",new Vector3(3.3f,y,-3.316f),new Vector3(1.07f,.74f,.025f),wood);
                panel.transform.SetParent(door.transform,true);
                foreach(float x in new[]{2.735f,3.865f})Box("Panel molding",new Vector3(x,y,-3.287f),new Vector3(.035f,.8f,.035f),wood,door.transform);
            }
            SetMat("Door Handle",metal);
            for(int side=0;side<2;side++)
            {
                float x=side==0?-.15f:3.44f;
                var curtain=new GameObject("Pleated linen curtain");curtain.transform.SetParent(root);curtain.transform.position=new Vector3(x,2.84f,3.08f);
                for(int i=0;i<9;i++)
                {
                    var strip=Box("Linen fold",new Vector3(x+(i-4)*.044f,1.88f,3.06f+Mathf.Sin(i*1.3f)*.045f),new Vector3(.058f,1.88f,.025f),linen,curtain.transform);
                    strip.transform.rotation=Quaternion.Euler(0,Mathf.Sin(i*1.3f)*28,0);
                }
                curtain.AddComponent<QuietMotion>();
            }
            Box("Curtain rail",new Vector3(1.65f,2.86f,3.09f),new Vector3(3.95f,.028f,.028f),metal);
            // Opened doorway has a corridor, not a hole into the sky.
            Box("Hall floor",new Vector3(3.3f,-.05f,-5.4f),new Vector3(2.7f,.1f,4),wood,null,true,true);
            Box("Hall end",new Vector3(3.3f,1.6f,-7.4f),new Vector3(2.7f,3.2f,.12f),plaster,null,true,true);
            foreach(float x in new[]{2.05f,4.58f})Box("Hall wall",new Vector3(x,1.6f,-5.4f),new Vector3(.12f,3.2f,4),plaster,null,true,true);
            Box("Hall ceiling",new Vector3(3.3f,3.2f,-5.4f),new Vector3(2.7f,.1f,4),plaster);
        }
        static void DressRoom(QuietMorning game)
        {
            SetMat("Bag",linen);SetMat("Pocket",wood);SetMat("Book 2",wood);SetMat("Book 1",paper);SetMat("Water Bottle",metal);
            game.phoneOn=Mat("Phone lit",new Color(.025f,.043f,.044f),true);
            game.phoneOff=Mat("Phone quiet",new Color(.018f,.024f,.023f),true);
            game.laptopOff=Mat("Notebook idle",new Color(.065f,.078f,.074f),true);
            game.laptopOn=Mat("Notebook focused",new Color(.075f,.085f,.079f),true);
            game.phoneDisplay.sharedMaterial=game.phoneOn;game.laptopDisplay.sharedMaterial=game.laptopOff;
            var phone=GameObject.Find("Phone - Primitive Detail");SetMat(phone.name,metal);
            var pc=PhysicalCanvas("Phone screen content",game.phoneDisplay.bounds.center+Vector3.up*.004f,Quaternion.Euler(90,phone.transform.eulerAngles.y,0),new Vector2(540,300),.00051f);
            game.phoneText=Text(pc.transform,"Phone message","08:42\n\nEVA\nCoffee before class?\nI'm downstairs.",25,new Vector2(15,0),new Vector2(480,276),TextAnchor.MiddleLeft);
            game.phoneText.color=new Color(.68f,.74f,.68f);
            var screen=game.laptopDisplay.bounds;
            var lc=PhysicalCanvas("Laptop screen content",screen.center+Vector3.left*.012f,Quaternion.Euler(0,90,0),new Vector2(680,510),.00058f);
            game.laptopText=Text(lc.transform,"Notes","MONDAY  /  10:10\n\nVirtual Worlds\nInteraction study\n\nThree ideas. One good morning.",32,new Vector2(0,30),new Vector2(586,390),TextAnchor.UpperLeft);
            game.laptopText.color=new Color(.64f,.69f,.59f);
            game.timerText=Text(lc.transform,"Timer","",39,new Vector2(0,-199),new Vector2(586,60),TextAnchor.MiddleLeft);
            game.timerText.color=new Color(.8f,.73f,.52f);
            // Actual keyboard and trackpad instead of a featureless slab.
            var baseObj=GameObject.Find("Laptop Base");var b=baseObj.GetComponent<Renderer>().bounds;
            var keyMat=Mat("Graphite keys",new Color(.09f,.1f,.09f));
            for(int row=0;row<5;row++)for(int col=0;col<11;col++)Box("Keyboard key",new Vector3(b.center.x+.10f-row*.04f,b.max.y+.004f,b.center.z-.215f+col*.042f),new Vector3(.028f,.008f,.029f),keyMat);
            Box("Trackpad",new Vector3(b.center.x-.13f,b.max.y+.003f,b.center.z),new Vector3(.06f,.004f,.16f),metal);
            float deskY=b.min.y-.02f;
            for(int i=0;i<4;i++)
            {
                var page=Box("Study notes",new Vector3(b.center.x-.18f+Random.Range(-.08f,.08f),deskY+.004f*i,b.center.z+.58f),new Vector3(.29f,.002f,.38f),paper);
                page.transform.rotation=Quaternion.Euler(0,12+i*4,0);
            }
            var note=PhysicalCanvas("Paper typography",new Vector3(b.center.x-.18f,deskY+.014f,b.center.z+.58f),Quaternion.Euler(90,24,0),new Vector2(450,600),.0005f);
            var n=Text(note.transform,"Pencil notes","INTERACTION STUDY\n\n01  notice\n02  choose\n03  respond\n\nWhat does it feel like\nto put the phone down?\n\n---------------------\nReview, Monday 10:10",27,Vector2.zero,new Vector2(408,540),TextAnchor.UpperLeft);n.color=new Color(.2f,.23f,.19f);
            Cylinder("Coffee cup ceramic",new Vector3(b.center.x-.18f,deskY+.078f,b.center.z-.6f),new Vector3(.13f,.075f,.13f),Mat("Glazed mug",new Color(.3f,.28f,.22f)));
            Cylinder("Coffee surface",new Vector3(b.center.x-.18f,deskY+.153f,b.center.z-.6f),new Vector3(.107f,.002f,.107f),Mat("Coffee",new Color(.035f,.019f,.01f)));
            var pen=Box("Pencil",new Vector3(b.center.x-.38f,deskY+.012f,b.center.z+.46f),new Vector3(.012f,.012f,.21f),wood);pen.transform.rotation=Quaternion.Euler(0,-22,0);
            var photo=PhysicalCanvas("Pinned timetable",new Vector3(4.321f,2.02f,-.05f),Quaternion.Euler(0,90,0),new Vector2(370,500),.00097f);
            Panel(photo.transform,"Paper",Vector2.zero,new Vector2(370,500),new Color(.66f,.63f,.5f));
            var calendar=Text(photo.transform,"Timetable","SEPTEMBER\n\nM   T   W   T   F\n\n14  15  16  17  18\n21  22  23  24  25\n28  29  30\n\nMON  10:10\nVirtual Worlds\n\ncoffee with Eva",26,Vector2.zero,new Vector2(306,440),TextAnchor.UpperLeft);calendar.color=new Color(.13f,.16f,.13f);
            var postcard=PhysicalCanvas("Postcard",new Vector3(4.321f,1.82f,.72f),Quaternion.Euler(0,90,0),new Vector2(300,400),.00084f);
            Panel(postcard.transform,"Card",Vector2.zero,new Vector2(300,400),new Color(.5f,.52f,.4f));
            Text(postcard.transform,"Handwritten","GO\nOUTSIDE\n\n-- even if\nit rains.",33,Vector2.zero,new Vector2(250,340),TextAnchor.MiddleCenter).color=new Color(.14f,.18f,.14f);
            // A restrained cork board replaces the giant wall feed.
            var board=Box("Cork memo board",new Vector3(-2.05f,2.26f,3.35f),new Vector3(1.25f,.65f,.055f),wood);
            for(int i=0;i<3;i++)
            {
                var c=PhysicalCanvas("Pinned note "+i,new Vector3(-2.46f+i*.4f,2.28f,3.311f),Quaternion.identity,new Vector2(250,310),.00125f);
                c.transform.Rotate(0,0,new[]{-5,3,-2}[i]);Panel(c.transform,"Paper",Vector2.zero,new Vector2(250,310),new Color(.62f+i*.035f,.6f+i*.025f,.48f));
                Text(c.transform,"Memo",new[]{"MONDAY\n\n10:10\nproject review","CALL HOME\n\nSunday\nDon't forget.","EVA\n\nCoffee?\nDownstairs."}[i],26,Vector2.zero,new Vector2(216,260),TextAnchor.MiddleCenter).color=new Color(.2f,.21f,.17f);
            }
            // Liftable lower sash; this is the fourth interaction's moving object.
            var sash=new GameObject("Working lower window sash");sash.transform.SetParent(root);sash.transform.position=new Vector3(1.65f,1.43f,3.27f);game.sash=sash.transform;
            var glass=Mat("Rain grey glass",new Color(.28f,.35f,.34f,.12f));glass.SetFloat("_Surface",1);glass.SetFloat("_Blend",0);glass.SetFloat("_SrcBlend",5);glass.SetFloat("_DstBlend",10);glass.SetFloat("_ZWrite",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;
            Box("Lower glass",new Vector3(1.65f,1.43f,3.29f),new Vector3(3.06f,.7f,.015f),glass,sash.transform);
            Box("Sash crossbar",new Vector3(1.65f,1.83f,3.25f),new Vector3(3.17f,.055f,.075f),wood,sash.transform);
            Box("Window latch",new Vector3(1.65f,1.79f,3.19f),new Vector3(.13f,.028f,.05f),metal,sash.transform);
            game.points[2].surfaces=new[]{sash.GetComponentsInChildren<Renderer>().Last()};
            // Plant on sill, using leaf geometry rather than green placeholder spheres.
            var terracotta=Mat("Clay pot",new Color(.29f,.16f,.095f));
            Cylinder("Terracotta pot",new Vector3(2.87f,1.12f,3.08f),new Vector3(.23f,.115f,.23f),terracotta);
            Cylinder("Pot soil",new Vector3(2.87f,1.24f,3.08f),new Vector3(.206f,.006f,.206f),ink);
            Leaves("Houseplant",new Vector3(2.87f,1.25f,3.08f),.47f,55,false);
        }
        static void Exterior()
        {
            var pavement=Mat("Wet courtyard stone",new Color(.24f,.26f,.24f),false,Legacy+"Concrete-texture.png",Legacy+"Concrete-normal.png");pavement.SetFloat("_Smoothness",.52f);
            Box("Courtyard",new Vector3(0,-2,11),new Vector3(36,.25f,28),pavement,null,false,true);
            Box("Opposite old apartment",new Vector3(1,3,14),new Vector3(25,13,.55f),brick,null,false,true);
            var window=Mat("Outside dark glass",new Color(.052f,.085f,.087f));window.SetFloat("_Smoothness",.7f);
            var trim=Mat("Weathered facade trim",new Color(.43f,.43f,.36f));
            for(int y=0;y<3;y++)for(int x=0;x<7;x++)
            {
                float px=-10+x*3.5f,py=-.05f+y*3.25f;
                Box("Courtyard window recess",new Vector3(px,py,13.68f),new Vector3(1.46f,1.86f,.16f),ink);
                Box("Courtyard window",new Vector3(px,py,13.57f),new Vector3(1.28f,1.67f,.02f),window);
                foreach(float dx in new[]{-.72f,.72f})Box("Stone jamb",new Vector3(px+dx,py,13.5f),new Vector3(.12f,1.99f,.16f),trim);
                foreach(float dy in new[]{-.94f,.94f})Box("Stone lintel",new Vector3(px,py+dy,13.5f),new Vector3(1.7f,.12f,.24f),trim);
                Box("Window mullion",new Vector3(px,py,13.48f),new Vector3(.045f,1.8f,.055f),metal);
                Box("Window transom",new Vector3(px,py+.13f,13.48f),new Vector3(1.45f,.05f,.06f),metal);
                if((x+y)%3==0)Box("Blind left ajar",new Vector3(px,py+.5f,13.46f),new Vector3(1.26f,.62f,.01f),linen);
            }
            for(int floor=0;floor<3;floor++)
            {
                float y=floor*3.25f-.98f;
                Box("Fire escape platform",new Vector3(4.7f,y,12.9f),new Vector3(3.25f,.1f,1.5f),metal);
                Box("Fire escape handrail",new Vector3(4.7f,y+.95f,12.12f),new Vector3(3.25f,.04f,.04f),metal);
                for(int i=0;i<14;i++)Box("Fire escape baluster",new Vector3(3.15f+i*.24f,y+.45f,12.12f),new Vector3(.025f,.92f,.025f),metal);
            }
            for(int i=0;i<4;i++)Leaves("Courtyard climbing ivy",new Vector3(-3.5f+i*3.6f,-1.8f,13.22f),7.2f,200,true);
            Leaves("Ivy on window edge",new Vector3(3.37f,1.0f,3.58f),2.5f,110,true);
            for(int i=0;i<3;i++)
            {
                var pipe=Cylinder("Drainpipe",new Vector3(-6+i*7,3,13.27f),new Vector3(.13f,6,.13f),metal);
            }
            // Rain streaks remain outside, with geometric depth/parallax.
            var rainMat=Mat("Rain streaks",new Color(.39f,.47f,.47f),true);
            for(int i=0;i<55;i++)
            {
                var r=Box("Rain beyond glass",new Vector3(Random.Range(-3f,6f),Random.Range(.2f,6f),Random.Range(4f,11f)),new Vector3(.004f,Random.Range(.03f,.11f),.004f),rainMat);
                r.transform.rotation=Quaternion.Euler(0,0,-12);
            }
        }
        static void Leaves(string name,Vector3 origin,float height,int count,bool vine)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<count;i++)
            {
                float t=Random.value;float a=Random.value*Mathf.PI*2;
                var center=vine?new Vector3(Mathf.Sin(t*15)*height*.14f+Random.Range(-.3f,.3f),t*height,Random.Range(-.13f,.13f)):
                    new Vector3(Mathf.Cos(a)*.14f,t*height,Mathf.Sin(a)*.14f);
                float size=vine?Random.Range(.055f,.17f):Random.Range(.065f,.13f);
                var q=Quaternion.Euler(Random.Range(-40,40),Random.Range(-60,60),Random.Range(-160,160));int k=vertices.Count;
                Vector3[] shape={new Vector3(0,-.65f,0),new Vector3(-.5f,-.1f,0),new Vector3(-.48f,.35f,0),new Vector3(0,.95f,.12f),new Vector3(.48f,.35f,0),new Vector3(.5f,-.1f,0),new Vector3(0,.1f,-.16f)};
                foreach(var v in shape)vertices.Add(center+q*v*size);
                for(int j=0;j<6;j++){triangles.Add(k+6);triangles.Add(k+j);triangles.Add(k+(j+1)%6);}
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,Dir+"/Leaves_"+(meshIndex++)+".asset");
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.transform.position=origin;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=leaf;
            go.AddComponent<QuietMotion>().amount=.22f;
        }
        static void Lighting(QuietMorning game)
        {
            var sun=GameObject.Find("Morning Sun - Directional Light").GetComponent<Light>();
            sun.transform.rotation=Quaternion.Euler(38,175,0);sun.color=new Color(.86f,.91f,.9f);sun.intensity=1.8f;sun.shadows=LightShadows.Soft;
            game.windowLight.color=new Color(.74f,.83f,.83f);game.windowLight.range=7;game.windowLight.intensity=1.65f;game.windowLight.shadows=LightShadows.Soft;
            game.deskLight.transform.position=new Vector3(3.64f,1.62f,.7f);game.deskLight.color=new Color(1,.73f,.4f);game.deskLight.range=3;game.deskLight.intensity=2.2f;
            PointLight("Bedside warm bulb",new Vector3(-3.67f,1.36f,2.79f),new Color(1,.72f,.4f),1.35f,3.3f);
            PointLight("Hall practical",new Vector3(3.3f,2.75f,-5.1f),new Color(1,.78f,.49f),1.8f,5);
            PointLight("Ceiling soft bounce",new Vector3(-.5f,2.85f,0),new Color(.66f,.7f,.65f),.5f,6);
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.33f,.38f,.38f);RenderSettings.ambientEquatorColor=new Color(.2f,.22f,.20f);RenderSettings.ambientGroundColor=new Color(.12f,.105f,.08f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.018f;RenderSettings.fogColor=new Color(.39f,.45f,.44f);
            var cam=game.rig.playerCamera;cam.backgroundColor=new Color(.43f,.48f,.46f);cam.clearFlags=CameraClearFlags.SolidColor;cam.farClipPlane=65;cam.allowHDR=true;
            var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var volume=root.gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.priority=80;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.6f);color.contrast.Override(12);color.saturation.Override(-12);
            var bloom=profile.Add<Bloom>();bloom.intensity.Override(.12f);bloom.threshold.Override(1.15f);
            var vignette=profile.Add<Vignette>();vignette.intensity.Override(.13f);vignette.smoothness.Override(.45f);
            AssetDatabase.CreateAsset(profile,Dir+"/Quiet grade.asset");volume.sharedProfile=profile;
        }
        static void PointLight(string name,Vector3 pos,Color color,float intensity,float range)
        {
            var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=pos;var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.Soft;
        }
        static GameObject PhysicalCanvas(string name,Vector3 p,Quaternion rotation,Vector2 size,float scale)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(root);go.transform.SetPositionAndRotation(p,rotation);go.transform.localScale=Vector3.one*scale;
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;go.GetComponent<RectTransform>().sizeDelta=size;return go;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size)
        {
            var o=new GameObject(name,typeof(RectTransform));var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        static Image Panel(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
        {
            var im=Rect(parent,name,pos,size).gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return im;
        }
        static Text Text(Transform parent,string name,string content,int size,Vector2 pos,Vector2 dimensions,TextAnchor align)
        {
            var t=Rect(parent,name,pos,dimensions).gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.text=content;t.color=Cream;t.alignment=align;t.raycastTarget=false;t.supportRichText=true;t.lineSpacing=1.12f;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        static Button Button(Transform parent,string label,Vector2 p)
        {
            var im=Panel(parent,label,p,new Vector2(280,54),new Color(.07f,.075f,.064f,.85f));im.raycastTarget=true;
            var b=im.gameObject.AddComponent<Button>();var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.6f,1.55f,1.35f);colors.pressedColor=new Color(.6f,.65f,.5f);b.colors=colors;b.navigation=new Navigation{mode=Navigation.Mode.None};
            Text(im.transform,"Button text",label,19,new Vector2(10,0),new Vector2(260,48),TextAnchor.MiddleLeft);
            Panel(im.transform,"Underline",new Vector2(0,-27),new Vector2(280,1),new Color(.7f,.68f,.55f,.7f));return b;
        }
        static void Interface(QuietMorning game)
        {
            var go=PhysicalCanvas("Quiet interface",Vector3.zero,Quaternion.identity,new Vector2(1200,675),.0023f);
            go.transform.SetParent(game.rig.playerCamera.transform,false);go.transform.localPosition=new Vector3(0,0,2.5f);go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one*.004f;
            go.GetComponent<Canvas>().worldCamera=game.rig.playerCamera;go.GetComponent<Canvas>().sortingOrder=20;go.AddComponent<TrackedDeviceGraphicRaycaster>();
            game.title=Rect(go.transform,"Title screen",new Vector2(-355,0),new Vector2(500,640)).gameObject;
            Panel(game.title.transform,"Title scrim",new Vector2(-10,0),new Vector2(465,640),new Color(.025f,.035f,.03f,.76f));
            Text(game.title.transform,"Location","R O O M   2 0 4     /     0 8 : 4 2",15,new Vector2(0,229),new Vector2(375,38),TextAnchor.MiddleLeft);
            Text(game.title.transform,"Title","THE\nFEED",78,new Vector2(0,93),new Vector2(385,214),TextAnchor.MiddleLeft);
            Text(game.title.transform,"Subheading","A QUIET MORNING",17,new Vector2(0,-49),new Vector2(375,40),TextAnchor.MiddleLeft);
            Text(game.title.transform,"Description","The day can wait a moment.",19,new Vector2(0,-98),new Vector2(375,52),TextAnchor.MiddleLeft);
            game.beginButton=Button(game.title.transform,"BEGIN    →",new Vector2(-46,-187));
            Text(game.title.transform,"Controls","WASD move · middle mouse look\nClick inspect · right-click use",14,new Vector2(0,-267),new Vector2(375,50),TextAnchor.MiddleLeft).color=new Color(.57f,.58f,.5f);
            game.objective=Text(go.transform,"Current intention","",14,new Vector2(-349,284),new Vector2(495,42),TextAnchor.MiddleLeft);game.objective.color=new Color(.73f,.74f,.64f);
            game.prompt=Text(go.transform,"Interaction hint","",16,new Vector2(284,-265),new Vector2(535,70),TextAnchor.MiddleRight);
            var captions=Rect(go.transform,"Subtitles",new Vector2(0,-192),new Vector2(890,66));game.subtitleGroup=captions.gameObject.AddComponent<CanvasGroup>();
            Panel(captions,"Caption backing",Vector2.zero,new Vector2(820,44),new Color(.02f,.028f,.022f,.65f));
            game.subtitle=Text(captions,"Caption","",22,Vector2.zero,new Vector2(840,55),TextAnchor.MiddleCenter);
            game.ending=Rect(go.transform,"End screen",Vector2.zero,new Vector2(1200,675)).gameObject;
            Panel(game.ending.transform,"End scrim",Vector2.zero,new Vector2(1200,675),new Color(.024f,.03f,.026f,.9f));
            Text(game.ending.transform,"End eyebrow","0 9 : 0 7   /   D O W N S T A I R S",15,new Vector2(0,140),new Vector2(760,45),TextAnchor.MiddleCenter);
            Text(game.ending.transform,"End title","Some things can wait.",42,new Vector2(0,40),new Vector2(860,100),TextAnchor.MiddleCenter);
            Text(game.ending.transform,"End note","You made a little room for the day.",19,new Vector2(0,-33),new Vector2(760,50),TextAnchor.MiddleCenter);
            game.againButton=Button(game.ending.transform,"RETURN TO THE ROOM    →",new Vector2(0,-144));game.ending.SetActive(false);
            foreach(var g in go.GetComponentsInChildren<Graphic>(true))g.material=ui;
        }
        static void Audio(QuietMorning game)
        {
            game.ambience=root.gameObject.AddComponent<AudioSource>();game.ambience.loop=true;game.ambience.volume=.14f;game.ambience.clip=AudioAsset("Rain in courtyard",18,0);game.ambience.playOnAwake=false;
            game.effects=root.gameObject.AddComponent<AudioSource>();game.effects.playOnAwake=false;
            game.click=AudioAsset("Soft switch",.1f,1);game.hush=AudioAsset("Phone mute",.36f,2);game.latch=AudioAsset("Door latch and wood",1.1f,3);game.windowSound=AudioAsset("Sash sliding",1.0f,4);game.chime=AudioAsset("Focus tone",.85f,5);
        }
        public static AudioClip AudioAsset(string name,float seconds,int kind)
        {
            const int rate=24000;int count=(int)(seconds*rate);var samples=new float[count];var rng=new System.Random(98+kind);float filtered=0;
            for(int i=0;i<count;i++)
            {
                float t=(float)i/rate;float noise=(float)rng.NextDouble()*2-1;filtered=filtered*.975f+noise*.025f;
                float envelope=Mathf.Min(1,t*25)*Mathf.Min(1,(seconds-t)*12);float sample=0;
                if(kind==0)sample=(noise*.025f+filtered*.55f)*(1+.2f*Mathf.Sin(t*.7f))+.008f*Mathf.Sin(t*61);
                if(kind==1)sample=(noise*.2f+Mathf.Sin(t*650)*.07f)*Mathf.Exp(-t*70);
                if(kind==2)sample=Mathf.Sin(t*(650-t*800)*6.283f)*.05f*Mathf.Exp(-t*8);
                if(kind==3)sample=filtered*.7f*Mathf.Sin(t*11)+noise*.25f*Mathf.Exp(-t*35)+Mathf.Sin(t*115)*.055f*Mathf.Exp(-t*4);
                if(kind==4)sample=filtered*.6f+noise*.05f*Mathf.Sin(t*20);
                if(kind==5)sample=(Mathf.Sin(t*880*6.283f)*.045f+Mathf.Sin(t*1320*6.283f)*.02f)*Mathf.Exp(-t*5);
                if(kind==6)sample=(filtered*.8f+noise*.08f+Mathf.Sin(t*95*6.283f)*.14f)*Mathf.Exp(-t*20);
                samples[i]=sample*envelope;
            }
            string path=Dir+"/"+name+".wav";
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);foreach(float v in samples)writer.Write((short)(Mathf.Clamp(v,-1,1)*32767));
            }
            AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        [MenuItem("The Feed/Quiet Morning/Open Revised Scene")]
        public static void Open(){EditorSceneManager.OpenScene(Scene);}
    }
}
