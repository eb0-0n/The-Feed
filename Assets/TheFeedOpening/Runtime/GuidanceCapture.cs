#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using TheFeed.Quiet;
namespace TheFeed.Opening
{
    public class GuidanceCapture : MonoBehaviour
    {
        const string Root="/Users/efabunmi/Documents/Codex/2026-10-19/i-n-2/work/story-guidance-capture";
        const string Shots="/Users/efabunmi/Documents/Codex/2026-10-19/i-n-2/outputs/TheFeed-Monday-Morning";
        const int Fps=24;
        OpeningGuidance guide;QuietMorning game;FeedRig rig;RenderTexture rt;Texture2D image;
        string folder;int frame;bool audioRecording;List<float> samples=new List<float>();
        IEnumerator Start()
        {
            Directory.CreateDirectory(Root);Directory.CreateDirectory(Shots);File.WriteAllText(Root+"/Checks.txt","Native XR input checks for the story guidance revision\n");
            Time.captureFramerate=Fps;yield return null;yield return null;
            guide=FindFirstObjectByType<OpeningGuidance>();game=guide.game;rig=game.rig;rig.demoMode=true;
            rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);rt.Create();image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            audioRecording=AudioRenderer.Start();Check(audioRecording,"Unity audioRecording recording active.");
            BeginClip("01_Wake_And_Begin");rig.GoTo(0);rig.demoAim=guide.closeButton.transform.position+Vector3.up*.5f;yield return Frames(3);
            CheckCardFits();Shot("01_First_Time_Card");yield return Click(guide.closeButton.transform.position);yield return Frames(4);
            Check(game.Started&&game.starts==1&&!guide.CardOpen,"Start card closes through native XR UI and starts the experience.");EndClip();
            BeginClip("02_Evas_Message");rig.GoTo(1);rig.demoAim=game.points[0].transform.position+Vector3.up*1.5f;yield return Frames(.8f);
            rig.demoAim=game.points[0].transform.position;yield return Frames(2.5f);
            Check(guide.CardOpen&&guide.cardKind==1,"Phone story/instruction card appears at first encounter.");CheckCardFits();Shot("02_Phone_Instruction_Card");
            yield return Click(guide.closeButton.transform.position);yield return Frames(.5f);
            Check(!guide.CardOpen,"Phone card closes through native XR UI.");
            rig.demoAim=game.points[0].transform.position;yield return Frames(1);
            Shot("03_Context_Prompt");
            rig.demoAim=game.points[0].transform.position+Vector3.up;yield return Frames(.6f);rig.demoAim=game.points[0].transform.position;yield return Frames(.6f);
            Check(!guide.CardOpen,"Phone help does not repeat on a second hover.");
            rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(3);
            rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;yield return Frames(4);
            Check(game.complete[0]&&game.points[0].selects>0&&game.points[0].uses>0,"Phone native select and activation work after dismissing help.");Check(game.repliesSent==1&&!game.ReplyPending&&game.phoneText.text.Contains("Grab us a table?")&&game.phoneText.text.Contains("Delivered"),"Maya's reply asks Eva to get a table and is delivered once.");
            rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;yield return Frames(.5f);
            Check(game.repliesSent==1,"A second trigger does not send the reply twice.");
            Canvas.ForceUpdateCanvases();Check(game.phoneText.preferredHeight<=game.phoneText.rectTransform.rect.height+1,"The complete reply and delivery receipt fit inside the phone screen.");
            Shot("04_Phone_Response");EndClip();
            yield return Point(1,"03_Last_Minute_Submission","05_Desk_Response");yield return Point(2,"04_Courtyard_Window","06_Window_Response");
            BeginClip("05_Leave_And_Return");rig.GoTo(4);rig.demoAim=game.points[3].transform.position;yield return Frames(1);
            rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(3);
            rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;yield return Frames(4);
            Check(game.Finished&&guide.CardOpen&&guide.cardKind==3,"Door opens and the ending card appears.");CheckCardFits();Shot("07_End_Of_Morning");
            yield return Click(guide.closeButton.transform.position);yield return Frames(2);
            Check(!game.Started&&!game.Finished&&guide.CardOpen&&guide.cardKind==0,"Return restores the opening and resets the story guide.");
            Check(!game.complete[0]&&!game.complete[1]&&!game.complete[2]&&!game.WindowOpen&&game.repliesSent==0&&game.assignmentsSubmitted==0&&!game.ReplyPending&&!game.SubmissionPending,"Restart resets phone, desk, and window state.");EndClip();
            File.AppendAllText(Root+"/Checks.txt","ALL_CHECKS_PASSED\nRecorded in desktop XR controller simulation, not on a physical headset.\n");
            AudioRenderer.Stop();Time.captureFramerate=0;rt.Release();Debug.Log("STORY_GUIDANCE_CAPTURE_COMPLETE");EditorApplication.Exit(0);
        }
        IEnumerator Point(int id,string name,string shot)
        {
            BeginClip(name);rig.GoTo(id+1);var p=game.points[id];rig.demoAim=p.transform.position+Vector3.up;yield return Frames(.6f);
            rig.demoAim=p.transform.position;yield return Frames(1.5f);
            Check(guide.CardOpen&&guide.cardKind==(id==1?2:4),"Story card appears at first encounter: "+name);CheckCardFits();Shot(id==1?"05a_Submission_Card":"06a_Window_Card");
            yield return Click(guide.closeButton.transform.position);yield return Frames(.5f);
            rig.demoAim=p.transform.position+Vector3.up;yield return Frames(.7f);rig.demoAim=p.transform.position;yield return Frames(.8f);
            Check(!guide.CardOpen,"Instruction card does not repeat: "+name);
            rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(4);
            rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;yield return Frames(4);
            Check(game.complete[id]&&p.hoverIn>0&&p.hoverOut>0&&p.selects>0&&p.uses>0,"Native hover, exit, select, activation and state change at "+name);if(id==1)
            {
                Check(game.assignmentsSubmitted==1&&!game.SubmissionPending&&game.laptopText.text.Contains("North_Pier_Study.pdf")&&game.laptopText.text.Contains("Submission received")&&game.timerText.text.Contains("SUBMITTED")&&game.elapsedFocus==0,"Laptop submits the actual in-game file, shows a receipt, and has no focus timer.");
                rig.demoActivate=true;yield return Frames(.25f);rig.demoActivate=false;yield return Frames(.5f);
                Check(game.assignmentsSubmitted==1,"Repeated activation does not submit the file twice.");
            }
            Shot(shot);EndClip();
        }
        void CheckCardFits()
        {
            Canvas.ForceUpdateCanvases();
            Check(guide.cardBody.preferredHeight<=guide.cardBody.rectTransform.rect.height+1,"Story text fits the card: "+guide.cardTitle.text);
        }
        IEnumerator Click(Vector3 position)
        {
            rig.demoAim=position;yield return Frames(.7f);rig.demoSelect=true;yield return Frames(.25f);rig.demoSelect=false;yield return Frames(.5f);
        }
        void BeginClip(string name){folder=Root+"/Frames/"+name;Directory.CreateDirectory(folder);frame=0;samples.Clear();rig.demoSelect=false;rig.demoActivate=false;}
        void EndClip()
        {
            using(var w=new BinaryWriter(File.Create(folder+"/Audio.wav")))
            {int bytes=samples.Count*2,rate=AudioSettings.outputSampleRate;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+bytes);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)2);w.Write(rate);w.Write(rate*4);w.Write((short)4);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(bytes);foreach(float f in samples)w.Write((short)(Mathf.Clamp(f,-1,1)*32767));}folder=null;
        }
        void Shot(string name){File.Copy(folder+$"/frame_{frame-1:0000}.jpg",Shots+"/"+name+".jpg",true);}
        IEnumerator Frames(float seconds)
        {
            for(int i=0;i<Mathf.CeilToInt(seconds*Fps);i++)
            {
                yield return null;
                int n=AudioRenderer.GetSampleCountForCaptureFrame()*2;
                using(var buffer=new NativeArray<float>(n,Allocator.Temp)){AudioRenderer.Render(buffer);if(folder!=null)samples.AddRange(buffer.ToArray());}
                if(folder==null)continue;
                var cam=rig.playerCamera;var old=cam.targetTexture;var active=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();cam.targetTexture=old;RenderTexture.active=active;
                File.WriteAllBytes(folder+$"/frame_{frame++:0000}.jpg",image.EncodeToJPG(93));
            }
        }
        void Check(bool ok,string message){File.AppendAllText(Root+"/Checks.txt",(ok?"PASS ":"FAIL ")+message+"\n");if(!ok){Debug.LogError(message);EditorApplication.Exit(1);throw new Exception(message);}}
    }
}
#endif
