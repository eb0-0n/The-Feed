#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TheFeed
{
    public class FeedDemo : MonoBehaviour
    {
        const string Root="/Users/efabunmi/Documents/Codex/2026-09-22/for-this-assignment-you-will-create";
        FeedExperience exp;
        FeedRig rig;
        RenderTexture rt;
        Texture2D image;
        string folder;
        int frame;
        const int Fps=15;
        IEnumerator Start()
        {
            Time.captureFramerate=Fps;
            exp=FindFirstObjectByType<FeedExperience>(); rig=exp.rig; rig.demoMode=true;
            rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32); rt.Create();
            image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            yield return null; yield return null;
            BeginClip("01_Start_Screen"); rig.GoTo(0);
            rig.demoAim=exp.startButton.transform.position+Vector3.up*.5f;
            yield return Frames(1.5f);
            rig.demoAim=exp.startButton.transform.position;
            yield return Frames(1);
            rig.demoSelect=true; yield return Frames(.3f); rig.demoSelect=false; yield return Frames(1.5f);
            Check(exp.Started,"Start Canvas button was clicked through XR UI ray.");
            folder=null;
            exp.Activate(FeedInteractable.Point.Door);
            Check(!exp.Finished,"Door stays locked before three choices.");
            yield return PointDemo(0,1,"02_Phone_Interaction");
            yield return PointDemo(1,2,"03_Laptop_Interaction");
            yield return PointDemo(2,3,"04_Window_Interaction");
            BeginClip("05_Door_End_Restart"); rig.GoTo(4);
            rig.demoAim=exp.points[3].transform.position;
            yield return Frames(1.2f);
            rig.demoSelect=true; yield return Frames(.3f); rig.demoSelect=false; yield return Frames(1);
            rig.demoActivate=true; yield return Frames(.3f); rig.demoActivate=false; yield return Frames(2);
            Check(exp.Finished && exp.endPanel.activeSelf,"Door activates end screen after three choices.");
            rig.demoAim=exp.restartButton.transform.position;
            yield return Frames(1); rig.demoSelect=true; yield return Frames(.3f); rig.demoSelect=false; yield return Frames(1.5f);
            Check(!exp.Started && !exp.Finished && exp.Mood==0 && exp.startPanel.activeSelf,"Restart returns to the start screen.");
            Check(!exp.completed[0]&&!exp.completed[1]&&!exp.completed[2],"Restart clears all three task flags.");
            Check(!exp.focusLight.enabled,"Restart turns off focus light.");
            string report="The Feed - interaction test results\nDesktop XR ray simulation; no headset used.\n\n";
            report+="XR Simple Interactables: "+FindObjectsByType<XRSimpleInteractable>(FindObjectsSortMode.None).Length+"\n";
            foreach(var p in exp.points) report+=$"{p.point}: hover enter {p.hoverEnters}, hover exit {p.hoverExits}, select {p.selections}, activate {p.activations}\n";
            report+=$"Start UI clicks: {exp.startClicks}\nRestart UI clicks: {exp.restartClicks}\nAll assertions passed.\n";
            Directory.CreateDirectory(Root+"/outputs/TheFeed_Interactions");
            File.WriteAllText(Root+"/outputs/TheFeed_Interactions/Verification.txt",report);
            File.WriteAllText(Root+"/work/feed-demo-success.txt",report);
            Debug.Log("FEED_DEMO_COMPLETE\n"+report);
            rt.Release(); Destroy(rt); Destroy(image); Time.captureFramerate=0;
            if(Application.isBatchMode) EditorApplication.Exit(0);
            else EditorApplication.isPlaying=false;
        }
        IEnumerator PointDemo(int index,int view,string name)
        {
            BeginClip(name); rig.GoTo(view);
            var p=exp.points[index];
            rig.demoAim=p.transform.position+Vector3.up*2;
            yield return Frames(1);
            rig.demoAim=p.transform.position; yield return Frames(1.2f);
            Check(p.hoverEnters>0,p.point+" receives native XR hover enter.");
            rig.demoAim=p.transform.position+Vector3.up*2; yield return Frames(.8f);
            Check(p.hoverExits>0,p.point+" receives native XR hover exit.");
            rig.demoAim=p.transform.position; yield return Frames(.6f);
            rig.demoSelect=true; yield return Frames(.3f); rig.demoSelect=false; yield return Frames(1.2f);
            Check(p.selections>0,p.point+" receives native XR select.");
            rig.demoActivate=true; yield return Frames(.3f); rig.demoActivate=false; yield return Frames(1.4f);
            Check(p.activations>0 && exp.completed[index],p.point+" activation updates game state.");
            if(index==2)
            {
                Check(exp.Mood==1,"Window switched to sunset material.");
                rig.demoActivate=true; yield return Frames(.3f); rig.demoActivate=false; yield return Frames(1.3f);
                Check(exp.Mood==2,"Window switched to night material.");
            }
            rig.demoAim=p.transform.position+Vector3.up*2;
            yield return Frames(1.2f);
        }
        void BeginClip(string name)
        {
            folder=Root+"/work/feed-demo-frames/"+name; Directory.CreateDirectory(folder); frame=0;
            rig.demoSelect=false; rig.demoActivate=false;
        }
        IEnumerator Frames(float seconds)
        {
            int count=Mathf.CeilToInt(seconds*Fps);
            for(int i=0;i<count;i++)
            {
                yield return null;
                if(folder!=null) Capture();
            }
        }
        void Capture()
        {
            var cam=exp.playerCamera;
            var oldTarget=cam.targetTexture; var oldActive=RenderTexture.active;
            cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,1280,720),0,0); image.Apply();
            cam.targetTexture=oldTarget; RenderTexture.active=oldActive;
            File.WriteAllBytes(Path.Combine(folder,$"frame_{frame++:0000}.jpg"),image.EncodeToJPG(90));
        }
        void Check(bool result,string message)
        {
            if(result) { Debug.Log("FEED PASS: "+message); return; }
            File.WriteAllText(Root+"/work/feed-demo-failure.txt",message);
            Debug.LogError("FEED FAIL: "+message);
            if(Application.isBatchMode) EditorApplication.Exit(1);
            else EditorApplication.isPlaying=false;
            throw new Exception(message);
        }
    }
}
#endif
