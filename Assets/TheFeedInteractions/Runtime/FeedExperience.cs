using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TheFeed
{
    public class FeedExperience : MonoBehaviour
    {
        public Camera playerCamera;
        public FeedRig rig;
        public FeedInteractable[] points;
        public GameObject startPanel, endPanel, informationPanel;
        public Text heading, body, status, progress, ending;
        public Button startButton, restartButton;
        public Renderer windowView, phoneScreen, laptopScreen;
        public Light windowLight, focusLight;
        public Transform doorPivot;
        public Material[] windowMaterials;
        public Material phoneQuietMaterial, laptopFocusMaterial;
        public bool Started { get; private set; }
        public bool Finished { get; private set; }
        public int Mood { get; private set; }
        public bool[] completed = new bool[3];
        public int startClicks, restartClicks;
        Material initialPhone, initialLaptop;
        Quaternion closedDoor;
        float doorAmount;

        void Start()
        {
            initialPhone=phoneScreen.sharedMaterial; initialLaptop=laptopScreen.sharedMaterial;
            closedDoor=doorPivot.localRotation;
            startButton.onClick.AddListener(Begin);
            restartButton.onClick.AddListener(Restart);
            RestartState();
        }
        void Update()
        {
            doorAmount=Mathf.MoveTowards(doorAmount, Finished ? 1 : 0, Time.deltaTime * 1.2f);
            doorPivot.localRotation=closedDoor*Quaternion.Euler(0, -100*doorAmount,0);
        }
        void OnDestroy()
        {
            if(startButton) startButton.onClick.RemoveListener(Begin);
            if(restartButton) restartButton.onClick.RemoveListener(Restart);
        }
        public void Begin()
        {
            startClicks++; Started=true; Finished=false;
            startPanel.SetActive(false); endPanel.SetActive(false);
            SetPoints(true);
            heading.text="YOUR MORNING, YOUR CHOICE";
            body.text="Silence the phone. Start a focus session. Choose your window view. Then leave for class.";
            informationPanel.SetActive(true);
            status.text="Aim at a labeled object to begin.";
            UpdateProgress();
        }
        public void Restart() { restartClicks++; RestartState(); rig.GoTo(0); }
        void RestartState()
        {
            Started=false; Finished=false; Mood=0;
            completed=new bool[3]; doorAmount=0;
            doorPivot.localRotation=closedDoor;
            phoneScreen.sharedMaterial=initialPhone;
            laptopScreen.sharedMaterial=initialLaptop;
            windowView.sharedMaterial=windowMaterials[0];
            if(windowLight) { windowLight.color=new Color(.62f,.82f,1); windowLight.intensity=5.2f; }
            if(focusLight) focusLight.enabled=false;
            foreach(var p in points) p.ResetFeedback();
            SetPoints(false);
            startPanel.SetActive(true); endPanel.SetActive(false); informationPanel.SetActive(false);
            status.text="Mouse: point/click | WASD: move | Hold middle mouse: look | 1-5: viewpoints";
            UpdateProgress();
        }
        void SetPoints(bool enabled)
        {
            foreach(var p in points) p.GetComponent<XRSimpleInteractable>().enabled=enabled;
        }
        public void Feedback(FeedInteractable.Point p, string state, string hint)
        {
            if(!Started || Finished) return;
            status.text=p.ToString().ToUpper()+" - "+state+"\n"+hint;
        }
        public void Inspect(FeedInteractable.Point p)
        {
            if(!Started || Finished) return;
            informationPanel.SetActive(true);
            switch(p)
            {
                case FeedInteractable.Point.Phone:
                    heading.text="2 UNREAD MESSAGES";
                    body.text="Group chat: 'Coffee before class?'\nThe Feed: 'You have 12 new posts.'\n\nActivate the phone to turn on Do Not Disturb."; break;
                case FeedInteractable.Point.Laptop:
                    heading.text="TODAY AT 10:10";
                    body.text="Virtual Worlds: interaction study\nRead your notes before class.\n\nActivate the laptop to start a 25-minute focus session."; break;
                case FeedInteractable.Point.Window:
                    heading.text="A DIFFERENT PERSPECTIVE";
                    body.text="Choose the view that helps you focus.\n\nActivate to cycle Morning / Sunset / Night. The window material and room lighting change together."; break;
                case FeedInteractable.Point.Door:
                    heading.text="READY TO STEP AWAY?";
                    body.text="Complete your three morning choices, then activate the door to leave the feed behind."; break;
            }
            Feedback(p,"SELECTED","Right-click / trigger to activate.");
        }
        public void Activate(FeedInteractable.Point p)
        {
            if(!Started || Finished) return;
            informationPanel.SetActive(true);
            switch(p)
            {
                case FeedInteractable.Point.Phone:
                    completed[0]=true; phoneScreen.sharedMaterial=phoneQuietMaterial;
                    heading.text="DO NOT DISTURB";
                    body.text="Notifications paused.\nYour friends can wait a few minutes.\n\nOne less distraction."; break;
                case FeedInteractable.Point.Laptop:
                    completed[1]=true; laptopScreen.sharedMaterial=laptopFocusMaterial;
                    if(focusLight) focusLight.enabled=true;
                    heading.text="FOCUS SESSION STARTED";
                    body.text="25:00\nVirtual Worlds - review your notes\n\nYour desk light is on. Make this time yours."; break;
                case FeedInteractable.Point.Window:
                    completed[2]=true; Mood=(Mood+1)%windowMaterials.Length;
                    windowView.sharedMaterial=windowMaterials[Mood];
                    if(windowLight) { windowLight.color=new[]{new Color(.62f,.82f,1),new Color(1,.48f,.22f),new Color(.28f,.36f,1)}[Mood]; windowLight.intensity=new[]{5.2f,4.2f,2.0f}[Mood]; }
                    heading.text=new[]{"MORNING VIEW","SUNSET VIEW","NIGHT VIEW"}[Mood];
                    body.text="A little change of atmosphere.\n\nActivate again to change the view."; break;
                case FeedInteractable.Point.Door:
                    if(!completed[0]||!completed[1]||!completed[2])
                    {
                        heading.text="A MOMENT BEFORE YOU GO";
                        body.text="Finish the phone, laptop and window choices first.";
                        Feedback(p,"LOCKED","Complete all three morning choices."); return;
                    }
                    Finished=true; SetPoints(false); informationPanel.SetActive(false);
                    endPanel.SetActive(true);
                    ending.text="Notifications paused.\nFocus session started.\nYour view, your choice.\n\nThe feed can wait. Class starts at 10:10.";
                    status.text="MORNING COMPLETE"; break;
            }
            UpdateProgress();
            if(!Finished) Feedback(p,"ACTIVATED","Choice saved. Move to the next object.");
        }
        void UpdateProgress()
        {
            int n=0; foreach(bool done in completed) if(done)n++;
            progress.text="THE FEED     8:42 AM                         "+n+" / 3 MORNING CHOICES";
        }
    }
}
