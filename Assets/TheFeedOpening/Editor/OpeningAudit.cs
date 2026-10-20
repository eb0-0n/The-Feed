using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace TheFeed.Opening.Editor
{
    public static class OpeningAudit
    {
        public static readonly string Root="/Users/efabunmi/Documents/Codex/2026-10-19/i-n-2";
        [Serializable] public class Item {public string name,path,position,size;public int triangles;}
        [Serializable] public class Report {public Item[] scene,prefabs;public string[] lights;public int triangles,renderers,colliders;}
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/TheFeed_QuietMorning.unity");
            var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var items=new List<Item>();int tris=0;
            foreach(var r in all)
            {
                var f=r.GetComponent<MeshFilter>();int t=f&&f.sharedMesh?f.sharedMesh.triangles.Length/3:0;tris+=t;
                items.Add(new Item{name=r.name,path=AssetDatabase.GetAssetPath(f?f.sharedMesh:null),position=r.bounds.center.ToString("F2"),size=r.bounds.size.ToString("F2"),triangles=t});
            }
            var prefabs=new List<Item>();
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Brick Project Studio/Apartment Kit"}))
            {
                string p=AssetDatabase.GUIDToAssetPath(guid);
                if(!System.Text.RegularExpressions.Regex.IsMatch(p,"Bed_Apt|Chair_Apt|Table_Computer|Dresser|Mirror|DrinkingGlass|WallShelf|Rug_Apt|Table_End|ToothbrushCup|Sofa_Apt"))continue;
                var go=AssetDatabase.LoadAssetAtPath<GameObject>(p);var rs=go.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;Bounds b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
                int t=go.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh?f.sharedMesh.triangles.Length/3:0);
                prefabs.Add(new Item{name=go.name,path=p,size=b.size.ToString("F3"),triangles=t});
            }
            var report=new Report{scene=items.ToArray(),prefabs=prefabs.ToArray(),lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Select(l=>l.name+" "+l.type+" "+l.intensity+" "+l.shadows).ToArray(),triangles=tris,renderers=all.Length,colliders=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length};
            File.WriteAllText(Root+"/work/opening-audit.json",JsonUtility.ToJson(report,true));
            var game=UnityEngine.Object.FindFirstObjectByType<TheFeed.Quiet.QuietMorning>();
            game.title.SetActive(false);game.ending.SetActive(false);game.subtitleGroup.alpha=0;game.prompt.text="";game.objective.text="";
            for(int i=0;i<5;i++){game.rig.GoTo(i);TheFeed.Quiet.Editor.QuietReview.Shot(game.rig.playerCamera,Root+"/outputs/TheFeed-Opening-Redesign/Before_"+i+".png");}
            Debug.Log("OPENING_AUDIT_COMPLETE");
        }
    }
}
