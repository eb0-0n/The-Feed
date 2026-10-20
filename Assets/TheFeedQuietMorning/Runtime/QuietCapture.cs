#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TheFeed.Quiet
{
    public class QuietCapture : MonoBehaviour
    {
        const string Root="/Users/efabunmi/Documents/Codex/2026-09-22/for-this-assignment-you-will-create";
        const int Fps=24;
        QuietMorning game;FeedRig rig;RenderTexture rt;Texture2D image;
        string folder;int frame;bool audioRecording;List<float> samples=new List<float>();
        IEnumerator Start()
        {
            File.WriteAllText(Root+"/work/quiet-final-native-checks.txt","Capture started "+DateTime.UtcNow.ToString("O")+"\n");
            Time.captureFramerate=Fps;game=FindFirstObjectByType<QuietMorning>();rig=game.rig;rig.demoMode=true;
            rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);rt.Create();image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            audioRecording=AudioRenderer.Start();Debug.Log("QUIET_AUDIO_CAPTURE "+audioRecording+" rate="+AudioSettings.outputSampleRate);
            yield return null;yield return null;
            var controllers=rig.GetComponent<QuietControllers>();
            Check(controllers&&controllers.leftModel.gameObject.activeInHierarchy&&controllers.rightModel.gameObject.activeInHierarchy,"Both Meta controller models are visible in the XR simulation.");
            BeginClip("01_Begin");rig.GoTo(0);rig.demoAim=game.beginButton.transform.position+Vector3.up*.5f;
            yield return Frames(3);rig.demoAim=game.beginButton.transform.position;yield return Frames(1);
            rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(3.5f);
            Check(game.Started && game.starts==1,"Custom start Canvas responds to native XR UI click.");EndClip();
            game.Use(3);Check(!game.Finished,"Door remains closed until the three room interactions are complete.");
            yield return PointDemo(0,"02_Phone");yield return PointDemo(1,"03_Focus_Desk");yield return PointDemo(2,"04_Window");
            BeginClip("05_Leave_And_Restart");rig.GoTo(4);rig.demoAim=game.points[3].transform.position;
            yield return Frames(1.5f);rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(2);
            rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;yield return Frames(3.5f);
            Check(game.Finished && game.ending.activeSelf,"Door opens and the end screen appears.");
            rig.demoAim=game.againButton.transform.position;yield return Frames(1.2f);rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(2.5f);
            Check(!game.Started&&!game.Finished&&!game.WindowOpen&&game.title.activeSelf,"Restart restores the title and window state.");
            Check(!game.complete[0]&&!game.complete[1]&&!game.complete[2]&&!game.deskLight.enabled,"Restart clears objectives and resets the lamp.");EndClip();
            string report="THE FEED / A QUIET MORNING\nNative XR event verification, desktop controller simulation\nBoth Meta Touch controller models visible. Not headset footage.\n\n";
            report+="XR Simple Interactables: "+FindObjectsByType<XRSimpleInteractable>(FindObjectsSortMode.None).Length+"\n";
            foreach(var p in game.points){report+=$"Point {p.id+1}: hover {p.hoverIn}, exit {p.hoverOut}, select {p.selects}, activate {p.uses}\n";Check(p.hoverIn>0&&p.hoverOut>0&&p.selects>0&&p.uses>0,"Every XR event received at point "+p.id);}
            report+=$"Start clicks: {game.starts}; restart clicks: {game.restarts}\nAudio captured from Unity: {audioRecording}\nAll checks passed.\nPhysical Quest testing is still required. This verification covers the Editor controller simulation only.\n";
            Directory.CreateDirectory(Root+"/outputs/TheFeed_QuietMorning");File.WriteAllText(Root+"/outputs/TheFeed_QuietMorning/Verification.txt",report);
            Debug.Log("QUIET_CAPTURE_COMPLETE\n"+report);File.AppendAllText(Root+"/work/quiet-final-native-checks.txt","QUIET_CAPTURE_COMPLETE\n"+report);if(audioRecording)AudioRenderer.Stop();Time.captureFramerate=0;rt.Release();
            if(Application.isBatchMode)EditorApplication.Exit(0);else EditorApplication.isPlaying=false;
        }
        IEnumerator PointDemo(int id,string name)
        {
            BeginClip(name);rig.GoTo(id+1);var p=game.points[id];
            rig.demoAim=p.transform.position+Vector3.up*2;yield return Frames(1);
            rig.demoAim=p.transform.position;yield return Frames(1.25f);Check(p.hoverIn>0,"Native hover at "+id);
            rig.demoAim=p.transform.position+Vector3.up*2;yield return Frames(.75f);Check(p.hoverOut>0,"Native hover exit at "+id);
            rig.demoAim=p.transform.position;yield return Frames(.5f);rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(3.5f);
            Check(p.selects>0,"Native select at "+id);rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;rig.demoAim=p.transform.position+Vector3.up*2;yield return Frames(4);
            Check(p.uses>0&&game.complete[id],"Native activation changes state at "+id);
            if(id==0)Check(game.phoneDisplay.sharedMaterial==game.phoneOff,"Phone switches to quiet material.");
            if(id==1)Check(game.deskLight.enabled&&game.elapsedFocus>1,"Desk light and focus countdown are running.");
            if(id==2)Check(game.WindowOpen,"Window sash opens.");
            EndClip();
        }
        void BeginClip(string name)
        {
            folder=Root+"/work/quiet-frames-final/"+name;Directory.CreateDirectory(folder);frame=0;samples.Clear();rig.demoSelect=false;rig.demoActivate=false;
        }
        void EndClip()
        {
            if(audioRecording && samples.Count>0)
            {
                using(var w=new BinaryWriter(File.Create(folder+"/Audio.wav")))
                {int bytes=samples.Count*2,rate=AudioSettings.outputSampleRate;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+bytes);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)2);w.Write(rate);w.Write(rate*4);w.Write((short)4);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(bytes);foreach(float f in samples)w.Write((short)(Mathf.Clamp(f,-1,1)*32767));}
            }
            folder=null;
        }
        IEnumerator Frames(float seconds)
        {
            for(int i=0;i<Mathf.CeilToInt(seconds*Fps);i++)
            {
                yield return null;
                if(audioRecording)
                {
                    int n=AudioRenderer.GetSampleCountForCaptureFrame()*2;
                    if(n>0){using(var buffer=new NativeArray<float>(n,Allocator.Temp)){AudioRenderer.Render(buffer);if(folder!=null)samples.AddRange(buffer.ToArray());}}
                }
                if(folder!=null)
                {
                    var cam=rig.playerCamera;var old=cam.targetTexture;var active=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
                    image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();cam.targetTexture=old;RenderTexture.active=active;
                    File.WriteAllBytes(folder+$"/frame_{frame++:0000}.jpg",image.EncodeToJPG(93));
                }
            }
        }
        void Check(bool pass,string message)
        {
            File.AppendAllText(Root+"/work/quiet-final-native-checks.txt",(pass?"QUIET_PASS ":"QUIET_FAIL ")+message+"\n");
            if(pass){Debug.Log("QUIET_PASS "+message);return;}
            Debug.LogError("QUIET_FAIL "+message);if(Application.isBatchMode)EditorApplication.Exit(1);throw new Exception(message);
        }
    }
}
#endif
