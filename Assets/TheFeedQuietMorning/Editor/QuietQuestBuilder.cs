using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace TheFeed.Quiet.Editor
{
    public static class QuietQuestBuilder
    {
        const string Dir="Assets/TheFeedQuietMorning/MetaControllers";
        [Serializable] class Data {public Part[] meshes;public Node[] nodes;}
        [Serializable] class Part {public string name;public float[] positions,normals,uv;public int[] triangles;}
        [Serializable] class Node {public string name;public int mesh;public int[] children;public float[] position,rotation,scale;}
        static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
        [MenuItem("The Feed/Quiet Morning/Configure Meta Controllers")]
        public static void Configure()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(QuietBuilder.Scene);
            var game=UnityEngine.Object.FindFirstObjectByType<QuietMorning>();
            var controls=game.rig.GetComponent<QuietControllers>();if(!controls)controls=game.rig.gameObject.AddComponent<QuietControllers>();controls.rig=game.rig;
            controls.actions=Actions();controls.leftModel=Model("left",game.rig.leftRay.transform);controls.rightModel=Model("right",game.rig.rightRay.transform);
            controls.quest2LeftModel=Model("quest2-left",game.rig.leftRay.transform);controls.quest2RightModel=Model("quest2-right",game.rig.rightRay.transform);
            controls.quest2LeftModel.gameObject.SetActive(false);controls.quest2RightModel.gameObject.SetActive(false);
            controls.leftAim=Aim(game.rig.leftRay.transform);controls.rightAim=Aim(game.rig.rightRay.transform);
            game.rig.leftRay.rayOriginTransform=controls.leftAim;game.rig.rightRay.rayOriginTransform=controls.rightAim;
            var text=game.title.transform.Find("Controls").GetComponent<UnityEngine.UI.Text>();
            text.text="Left stick move · right stick turn\nGrip inspect · Trigger use / click";
            // Keep the saved scene at the start, with real devices driving these objects on Quest.
            QuietReview.Tune();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            File.WriteAllText(QuietReview.Output+"/ControllerSetup.txt","Quest 2 Touch and Quest 3 Touch Plus controller visuals installed, both hands.\nController appearance follows the detected headset; the Editor preview defaults to Quest 3.\nThumbstick, grip and trigger parts animate from their input values.\nGrip inspects; trigger uses objects and clicks UI.\nDesktop preview drives the same XR rays, but is not headset footage.\n");
            Debug.Log("QUEST_CONTROLLERS_CONFIGURED");
        }
        static Transform Aim(Transform parent)
        {
            var t=parent.Find("Meta pointer pose");if(t)return t;
            t=new GameObject("Meta pointer pose").transform;t.SetParent(parent,false);return t;
        }
        static QuietControllerVisual Model(string side,Transform parent)
        {
            string modelName=side.StartsWith("quest2-")?"Meta Quest 2 Touch "+side.Substring(7):"Meta Touch Plus "+side;
            var existing=parent.Find(modelName);if(existing)return existing.GetComponent<QuietControllerVisual>();
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Dir+"/"+side+".json"));
            string matPath=Dir+"/"+side+" controller.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/"+side+".png"));mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Smoothness",.35f);mat.SetFloat("_Cull",0);AssetDatabase.CreateAsset(mat,matPath);}
            Mesh[] meshes=new Mesh[data.meshes.Length];
            for(int n=0;n<meshes.Length;n++)
            {
                string path=Dir+"/"+side+" part "+n+".asset";meshes[n]=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(meshes[n])continue;
                var p=data.meshes[n];var m=new Mesh(){name=side+" "+p.name};var verts=new Vector3[p.positions.Length/3];var norms=new Vector3[verts.Length];var uv=new Vector2[verts.Length];
                for(int i=0;i<verts.Length;i++){verts[i]=new Vector3(p.positions[i*3],p.positions[i*3+1],p.positions[i*3+2]);norms[i]=new Vector3(p.normals[i*3],p.normals[i*3+1],p.normals[i*3+2]);uv[i]=new Vector2(p.uv[i*2],p.uv[i*2+1]);}
                m.vertices=verts;m.normals=norms;m.uv=uv;m.triangles=p.triangles;m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);meshes[n]=m;
            }
            var root=new GameObject(modelName);root.transform.SetParent(parent,false);
            var nodes=new Transform[data.nodes.Length];
            for(int i=0;i<nodes.Length;i++)nodes[i]=new GameObject(data.nodes[i].name).transform;
            for(int i=0;i<nodes.Length;i++)nodes[i].SetParent(root.transform,false);
            for(int i=0;i<nodes.Length;i++)foreach(int child in data.nodes[i].children)nodes[child].SetParent(nodes[i],false);
            for(int i=0;i<nodes.Length;i++)
            {
                var n=data.nodes[i];var t=nodes[i];t.localPosition=V(n.position);t.localScale=V(n.scale);t.localRotation=new Quaternion(n.rotation[0],n.rotation[1],n.rotation[2],n.rotation[3]);
                if(n.mesh<0)continue;t.gameObject.AddComponent<MeshFilter>().sharedMesh=meshes[n.mesh];var renderer=t.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return root.AddComponent<QuietControllerVisual>();
        }
        static InputActionAsset Actions()
        {
            string path=Dir+"/Meta Quest Controls.inputactions";var existing=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);if(existing)return existing;
            var asset=ScriptableObject.CreateInstance<InputActionAsset>();
            foreach(string hand in new[]{"Left","Right"})
            {
                var map=asset.AddActionMap(hand);string prefix="<XRController>{"+hand+"Hand}/";
                Add(map,"Grip",prefix+"grip","Axis");Add(map,"Trigger",prefix+"trigger","Axis");Add(map,"Stick",prefix+"primary2DAxis","Vector2");
                Add(map,"Primary",prefix+"primaryButton","Button");Add(map,"Secondary",prefix+"secondaryButton","Button");
                Add(map,"Position",prefix+"devicePosition","Vector3");Add(map,"Rotation",prefix+"deviceRotation","Quaternion");
                Add(map,"AimPosition",prefix+"pointerPosition","Vector3");Add(map,"AimRotation",prefix+"pointerRotation","Quaternion");
            }
            var head=asset.AddActionMap("Head");Add(head,"Position","<XRHMD>/centerEyePosition","Vector3");Add(head,"Rotation","<XRHMD>/centerEyeRotation","Quaternion");
            File.WriteAllText(path,asset.ToJson());AssetDatabase.ImportAsset(path);UnityEngine.Object.DestroyImmediate(asset);return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        static void Add(InputActionMap map,string name,string binding,string control)
        {map.AddAction(name,InputActionType.Value,binding,expectedControlLayout:control);}
    }
}
