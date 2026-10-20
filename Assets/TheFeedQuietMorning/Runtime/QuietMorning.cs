using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TheFeed.Quiet
{
    // Scene-local controller: the first assignment scene keeps its original scripts.
    public class QuietMorning : MonoBehaviour
    {
        public FeedRig rig;
        public bool storyMorning;
        public bool ReplyPending { get; private set; }
        public bool SubmissionPending { get; private set; }
        public int repliesSent, assignmentsSubmitted;
        public const string MorningMessages="MONDAY  08:42\nEVA\n\nCoffee before class?\nI'm downstairs.\n\nYou awake?";
        public string StoryTime => Finished?"08:47":complete[2]?"08:46":complete[1]?"08:44":complete[0]?"08:43":"08:42";
        public QuietPoint[] points;
        public GameObject title, ending;
        public Button beginButton, againButton;
        public Text prompt, subtitle, objective, phoneText, laptopText, timerText;
        public CanvasGroup subtitleGroup;
        public Transform door, sash;
        public Light deskLight, windowLight;
        public Renderer phoneDisplay, laptopDisplay;
        public Material phoneOn, phoneOff, laptopOn, laptopOff;
        public AudioSource ambience, effects;
        public AudioClip click, hush, latch, windowSound, chime, footstep;
        public bool Started { get; private set; }
        public bool Finished { get; private set; }
        public bool WindowOpen { get; private set; }
        public bool[] complete = new bool[3];
        public int starts, restarts;
        public float elapsedFocus;
        float captionUntil, doorT, sashT;
        Quaternion doorClosed;
        Vector3 sashClosed;
        int hovering = -1;
        Vector3 lastPosition;
        float walked;
        void Start()
        {
            doorClosed=door.localRotation; sashClosed=sash.localPosition;
            beginButton.onClick.AddListener(Begin);
            againButton.onClick.AddListener(Restart);
            ConfigureDialogue();
            ResetState();
            lastPosition=rig.playerCamera.transform.position;
            if(ambience) ambience.Play();
        }
        void Update()
        {
            var position=rig.playerCamera.transform.position;
            float distance=Vector3.Distance(position,lastPosition);lastPosition=position;
            if(Started&&!Finished&&!rig.demoMode&&distance<.25f)
            {
                walked+=distance;
                if(walked>.68f){Sound(footstep,.65f);walked=0;}
            }
            doorT=Mathf.MoveTowards(doorT,Finished?1:0,Time.deltaTime*.65f);
            door.localRotation=doorClosed*Quaternion.Euler(0,-100*Mathf.SmoothStep(0,1,doorT),0);
            sashT=Mathf.MoveTowards(sashT,WindowOpen?1:0,Time.deltaTime*.75f);
            sash.localPosition=sashClosed+Vector3.up*(.65f*Mathf.SmoothStep(0,1,sashT));
            if(ambience) ambience.volume=Mathf.Lerp(.14f,.3f,sashT);
            if(windowLight) windowLight.intensity=Mathf.Lerp(1.65f,2.1f,sashT);
            if(!storyMorning && complete[1] && Started && !Finished) elapsedFocus+=Time.deltaTime;
            float left=Mathf.Max(0,1500-elapsedFocus);
            if(timerText) timerText.text=storyMorning?(SubmissionPending?"UPLOADING…":complete[1]?"SUBMITTED  08:44":"DUE MONDAY  08:45"):(complete[1]?$"{(int)left/60:00}:{(int)left%60:00}":"");
            if(subtitleGroup) subtitleGroup.alpha=Mathf.MoveTowards(subtitleGroup.alpha,Time.time<captionUntil?1:0,Time.deltaTime*3);
            if(Finished && doorT>.8f) { ending.SetActive(true);subtitleGroup.alpha=0; }
        }
        public void Sound(AudioClip clip,float volume=.6f) { if(effects && clip)effects.PlayOneShot(clip,volume); }
        public void Begin()
        {
            starts++; Started=true; Finished=false; title.SetActive(false); ending.SetActive(false);
            SetPoints(true); Sound(click,.45f);
            Say(storyMorning?"Coffee sounds good. Let me tell her I’m up.":"Eva’s downstairs. I’ll check my phone, look over my notes, and open the window before I go.",5);
            Objective();
        }
        public void Restart()
        {
            restarts++; ResetState();
            var controllers=rig.GetComponent<QuietControllers>();
            if(controllers?controllers.IsHardwareActive:rig.HardwareActive)rig.GetComponent<QuietLocomotion>().ResetOrigin();else rig.GoTo(0);
            Sound(click,.35f);
        }
        void ResetState()
        {
            StopAllCoroutines(); Started=false; Finished=false; WindowOpen=false;
            complete=new bool[3]; elapsedFocus=0; doorT=sashT=0;walked=0;
            ReplyPending=SubmissionPending=false;repliesSent=assignmentsSubmitted=0;
            door.localRotation=doorClosed; sash.localPosition=sashClosed;
            phoneDisplay.sharedMaterial=phoneOn; laptopDisplay.sharedMaterial=laptopOff;
            phoneText.text="08:42\n\nEVA\nCoffee before class?\nI'm downstairs.";
            laptopText.text="MONDAY  /  10:10\n\nVirtual Worlds\nInteraction study\n\nThree ideas. One good morning.";
            if(storyMorning)
            {
                phoneText.text=MorningMessages;
                laptopText.text="STUDIO / MEMORY + PLACE\n\nNorth_Pier_Study.pdf\nReady to submit\n\nDue Monday, 08:45";
            }
            deskLight.enabled=false; title.SetActive(true); ending.SetActive(false);
            hovering=-1; prompt.text=""; objective.text=""; subtitle.text=""; captionUntil=0;
            if(subtitleGroup)subtitleGroup.alpha=0;
            foreach(var p in points)p.Clear(); SetPoints(false);
        }
        void SetPoints(bool active) { foreach(var p in points)p.GetComponent<XRSimpleInteractable>().enabled=active; }
        public void Hover(int id,bool active)
        {
            if(!Started || Finished)return;
            if(active) hovering=id; else if(hovering==id)hovering=-1;
            var hands=rig.GetComponent<QuietControllers>();
            bool vr=rig.HardwareActive||(hands&&hands.ShowingControllers);
            string read=vr?"GRIP":"LEFT CLICK", use=vr?"TRIGGER":"RIGHT CLICK";
            string[] descriptions={"Read message","Read notes","Look outside","Check before leaving"};
            string[] actions={complete[0]?"Phone silenced":"Silence phone",complete[1]?"Session started":"Start focus session",WindowOpen?"Close window":"Open window","Leave for class"};
            if(storyMorning)
            {
                descriptions=new[]{"Read Eva's messages","Review assignment","Look outside","Check before leaving"};
                actions=new[]{ReplyPending?"Sending reply":complete[0]?"Reply sent":"Reply to Eva",SubmissionPending?"Submitting":complete[1]?"Assignment submitted":"Submit assignment",WindowOpen?"Close window":"Open window","Meet Eva for coffee"};
            }
            prompt.text=hovering<0?"":read+"  "+descriptions[hovering]+"     ·     "+use+"  "+actions[hovering];
        }
        public void Inspect(int id)
        {
            if(!Started || Finished)return;
            Sound(click,.2f);
            if(id==3 && (!complete[0]||!complete[1]||!complete[2])) { Say(UnfinishedTask(),6.5f);return; }
            if(storyMorning)
            {
                string[] lines={complete[0]?"She knows I’m coming. Now, that assignment.":"She’s downstairs already. I’ll ask her to grab us a table.",complete[1]?"Good. It’s actually submitted this time.":"I finished this last night and never hit submit. Of course I didn’t.","It’s stuffy in here. A bit of fresh air before I go.","Right. I’ve kept her waiting long enough."};
                Say(lines[id],6);return;
            }
            Say(new[]{"Eva’s waiting downstairs. I’ll read her message, then put this thing on silent.","My notes are right here. I’ll start a focus session before I head out.","It’s finally stopped raining. I’ll open the window for a minute.","Phone’s quiet. Notes are ready. Time to meet Eva."}[id],5);
        }
        public void Use(int id)
        {
            if(!Started || Finished)return;
            if(storyMorning)
            {
                if(id==1&&!complete[0]){Say("I should let Eva know I’m coming first.",5);return;}
                if(id==2&&(!complete[0]||!complete[1])){Say(!complete[0]?"Let me text Eva back first.":"Let me send this assignment before I forget again.",5);return;}
                if(id<2){UseStoryObject(id);return;}
            }
            if(id==0)
            {
                complete[0]=true; phoneDisplay.sharedMaterial=phoneOff;
                phoneText.text="08:42\n\nDO NOT DISTURB\nUntil 10:00\n\nI'll see you downstairs.";
                Sound(hush,.45f); Say("There. No more notifications. Now, where did I leave my notes?",3);
            }
            else if(id==1)
            {
                complete[1]=true; laptopDisplay.sharedMaterial=laptopOn; deskLight.enabled=true;
                laptopText.text="FOCUS SESSION\n\nInteraction study\n\nKeep it simple.\nMake it mean something.";
                Sound(chime,.3f); Say("That’s the desk sorted. I could use some fresh air.",3);
            }
            else if(id==2)
            {
                WindowOpen=!WindowOpen; complete[2]=storyMorning?WindowOpen:true; Sound(windowSound,.7f);
                Say(storyMorning?(WindowOpen?"Oh, that’s better. I didn’t realise how warm it was in here.":"That’s enough for now."):(WindowOpen?"That’s better. I should head downstairs.":"I’ll keep the rain out."),4);
            }
            else
            {
                if(!complete[0]||!complete[1]||!complete[2]) { Say(UnfinishedTask(),6.5f); Objective(); return; }
                Finished=true; hovering=-1; prompt.text=""; objective.text=""; SetPoints(false);
                Sound(latch,.8f); Say(storyMorning?"Okay. Coffee. Finally.":"Coming, Eva.",3);
            }
            Objective();
            if(!Finished && hovering>=0) Hover(hovering,true);
        }
        void UseStoryObject(int id)
        {
            if(id==0)
            {
                if(ReplyPending||complete[0])return;
                ReplyPending=true;phoneText.text="MONDAY  08:43\nEVA\n\nCoffee before class?\nI'm downstairs.\n\nMAYA\nMorning! Grab us a table?\nJust submitting my assignment.\nBe down in five.\n\nSending…";
                Sound(click,.25f);StartCoroutine(SendReply());
            }
            else
            {
                if(SubmissionPending||complete[1])return;
                SubmissionPending=true;laptopDisplay.sharedMaterial=laptopOn;deskLight.enabled=true;
                laptopText.text="SUBMITTING ASSIGNMENT\n\nNorth_Pier_Study.pdf\n\nUploading your file…";
                Sound(click,.25f);StartCoroutine(SubmitAssignment());
            }
            if(hovering>=0)Hover(hovering,true);
        }
        IEnumerator SendReply()
        {
            yield return new WaitForSeconds(.65f);
            ReplyPending=false;complete[0]=true;repliesSent++;
            phoneDisplay.sharedMaterial=phoneOn;
            phoneText.text="MONDAY  08:43\nEVA\nCoffee before class?\nI'm downstairs.\n\nMAYA\nMorning! Grab us a table?\nJust submitting my assignment.\nBe down in five.\n\nDelivered  08:43";
            Sound(chime,.3f);Say("‘Be down in five.’ Better make that true.",5);
            Objective();if(hovering>=0)Hover(hovering,true);
        }
        IEnumerator SubmitAssignment()
        {
            yield return new WaitForSeconds(.9f);
            SubmissionPending=false;complete[1]=true;assignmentsSubmitted++;
            laptopText.text="ASSIGNMENT SUBMITTED\n\nNorth_Pier_Study.pdf\n\nSubmission received\nMonday, 08:44";
            Sound(chime,.4f);Say("There. Submitted. I would’ve been so annoyed with myself.",5);
            Objective();if(hovering>=0)Hover(hovering,true);
        }
        string UnfinishedTask()
        {
            if(storyMorning)
            {
                if(!complete[0])return "I should text Eva back. She’ll think I’ve gone back to sleep.";
                if(!complete[1])return "Wait—the assignment. I nearly left without submitting it.";
                return "One second. This room could do with some fresh air.";
            }
            if(!complete[0]) return "I still need to silence my phone before I leave.";
            if(!complete[1]) return "My notes are still on the desk. I’ll get those ready first.";
            return "One last thing—the window. This room needs some air.";
        }

        void ConfigureDialogue()
        {
            var box=subtitleGroup.GetComponent<RectTransform>();
            box.anchoredPosition=new Vector2(0,-215);
            box.sizeDelta=new Vector2(1050,166);
            var backing=box.Find("Caption backing");
            if(backing)
            {
                var rect=backing.GetComponent<RectTransform>();
                rect.anchoredPosition=Vector2.zero;
                rect.sizeDelta=new Vector2(1050,166);
                backing.GetComponent<Image>().color=new Color(.035f,.05f,.04f,.91f);
            }
            subtitle.fontSize=25;
            subtitle.alignment=TextAnchor.UpperLeft;
            subtitle.color=new Color(.94f,.93f,.88f);
            subtitle.lineSpacing=1.1f;
            subtitle.horizontalOverflow=HorizontalWrapMode.Wrap;
            subtitle.verticalOverflow=VerticalWrapMode.Overflow;
            subtitle.rectTransform.anchoredPosition=new Vector2(0,-1);
            subtitle.rectTransform.sizeDelta=new Vector2(950,78);

            var speaker=new GameObject("Dialogue speaker",typeof(RectTransform),typeof(Text));
            speaker.transform.SetParent(box,false);
            var name=speaker.GetComponent<Text>();
            name.font=subtitle.font;name.material=subtitle.material;
            name.text="MAYA";name.fontSize=18;name.raycastTarget=false;
            name.alignment=TextAnchor.MiddleLeft;
            name.color=new Color(.72f,.72f,.57f);
            name.rectTransform.anchoredPosition=new Vector2(0,54);
            name.rectTransform.sizeDelta=new Vector2(950,28);

            prompt.fontSize=14;
            prompt.alignment=TextAnchor.MiddleRight;
            prompt.color=new Color(.80f,.80f,.71f);
            prompt.rectTransform.anchoredPosition=new Vector2(0,-280);
            prompt.rectTransform.sizeDelta=new Vector2(950,30);
            prompt.transform.SetAsLastSibling();

            var accent=new GameObject("Dialogue accent",typeof(RectTransform),typeof(Image));
            accent.transform.SetParent(box,false);
            accent.GetComponent<Image>().color=new Color(.72f,.72f,.57f);
            accent.GetComponent<Image>().material=subtitle.material;
            accent.GetComponent<Image>().raycastTarget=false;
            var accentRect=accent.GetComponent<RectTransform>();
            accentRect.anchoredPosition=new Vector2(-492,54);
            accentRect.sizeDelta=new Vector2(3,20);
            var chapter=new GameObject("Dialogue chapter",typeof(RectTransform),typeof(Text));
            chapter.transform.SetParent(box,false);
            var chapterText=chapter.GetComponent<Text>();
            chapterText.font=subtitle.font;chapterText.material=subtitle.material;
            chapterText.text="A QUIET MORNING";chapterText.fontSize=11;
            chapterText.color=new Color(.59f,.60f,.54f);
            chapterText.alignment=TextAnchor.MiddleLeft;chapterText.raycastTarget=false;
            chapterText.rectTransform.anchoredPosition=new Vector2(-365,-65);
            chapterText.rectTransform.sizeDelta=new Vector2(220,25);

            var description=title.transform.Find("Description");
            if(description) description.GetComponent<Text>().text="Eva’s waiting downstairs. A few things to do before I go.";
        }

        void Objective()
        {
            if(Finished){objective.text="";return;}
            if(storyMorning){objective.text=!complete[0]?"REPLY TO EVA":!complete[1]?"SUBMIT THE ASSIGNMENT":!complete[2]?"LET SOME FRESH AIR IN":"MEET EVA FOR COFFEE";return;}
            objective.text=!complete[0]?"SILENCE YOUR PHONE":!complete[1]?"START A FOCUS SESSION":!complete[2]?"OPEN THE WINDOW":"MEET EVA DOWNSTAIRS";
        }
        public void Say(string line,float duration) { subtitle.text=line; captionUntil=Time.time+Mathf.Max(duration,6.5f); }
    }

}
