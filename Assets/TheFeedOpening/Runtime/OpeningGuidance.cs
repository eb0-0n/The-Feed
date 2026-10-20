using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages=UnityEngine.XR.CommonUsages;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TheFeed.Quiet;
namespace TheFeed.Opening
{
    [DefaultExecutionOrder(500)]
    public class OpeningGuidance : MonoBehaviour
    {
        public QuietMorning game;
        public const string EvaMessage=QuietMorning.MorningMessages;
        public Font storyFont;
        public Texture2D roomExample,phoneExample,deskExample,windowExample;
        public Button closeButton;
        public Text cardTitle,cardBody,cardControl,cardFooter,contextText,thought,lensText;
        public RawImage example;
        public GameObject card,smallPrompt;
        public CanvasGroup thoughtGroup,lensGroup;
        public int cardKind=-1,closedCards;
        public bool phoneHelpSeen,deskHelpSeen,windowHelpSeen;
        public bool CardOpen=>card&&card.activeSelf;
        bool wasStarted,wasFinished,primaryHeld;
        float feedUntil,hoverSince,thoughtUntil,finishAt;
        string lastThought="";
        bool endPresented;
        int lastHover=-1,lastUses=-1;
        Canvas sourceCanvas;
        Color cream=new Color(.90f,.88f,.79f);
        void Start()
        {
            Build();
            foreach(var p in game.points)p.enabled=true;
            game.title.SetActive(false);game.ending.SetActive(false);
            ShowCard(0);
        }
        void Build()
        {
            sourceCanvas=game.subtitle.GetComponentInParent<Canvas>();
            var canvas=new GameObject("The Feed — contextual guidance",typeof(RectTransform),typeof(Canvas),typeof(TrackedDeviceGraphicRaycaster));
            canvas.transform.SetParent(game.rig.playerCamera.transform,false);
            canvas.transform.localPosition=new Vector3(0,0,2.5f);canvas.transform.localScale=Vector3.one*.004f;
            var c=canvas.GetComponent<Canvas>();c.renderMode=RenderMode.WorldSpace;c.worldCamera=game.rig.playerCamera;c.sortingOrder=80;
            canvas.GetComponent<RectTransform>().sizeDelta=new Vector2(1200,675);
            var root=canvas.transform;
            smallPrompt=Panel(root,"Available action",new Vector2(0,-266),new Vector2(650,45),new Color(.024f,.03f,.025f,.86f)).gameObject;
            Panel(smallPrompt.transform,"Upper rule",new Vector2(0,22),new Vector2(650,1),new Color(.59f,.57f,.43f,.5f));
            contextText=Text(smallPrompt.transform,"Context action","",23,Vector2.zero,new Vector2(620,40),TextAnchor.MiddleCenter);
            thought=Text(root,"Maya — optional subtitle","",22,new Vector2(0,-235),new Vector2(820,72),TextAnchor.MiddleCenter);
            thoughtGroup=thought.gameObject.AddComponent<CanvasGroup>();
            var shadow=thought.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.9f);shadow.effectDistance=new Vector2(1,-1);
            lensText=Text(root,"Personal Feed status","",18,new Vector2(-325,248),new Vector2(450,95),TextAnchor.UpperLeft);
            lensText.color=new Color(.76f,.83f,.81f);lensGroup=lensText.gameObject.AddComponent<CanvasGroup>();
            card=Rect(root,"First-time instruction card",new Vector2(0,5),new Vector2(1030,470)).gameObject;
            Panel(card.transform,"Dimming",Vector2.zero,new Vector2(1450,850),new Color(.008f,.011f,.01f,.35f)).raycastTarget=true;
            Panel(card.transform,"Card body",Vector2.zero,new Vector2(1030,470),new Color(.035f,.036f,.028f,1f)).raycastTarget=true;
            Panel(card.transform,"Upper edge",new Vector2(0,235),new Vector2(1030,1),new Color(.53f,.51f,.37f,.7f));
            Panel(card.transform,"Header rule",new Vector2(0,171),new Vector2(1000,1),new Color(.48f,.47f,.35f,.53f));
            Panel(card.transform,"Footer rule",new Vector2(0,-171),new Vector2(1000,1),new Color(.48f,.47f,.35f,.53f));
            Panel(card.transform,"Lower edge",new Vector2(0,-235),new Vector2(1030,1),new Color(.53f,.51f,.37f,.7f));
            cardTitle=Text(card.transform,"Subject","",32,new Vector2(0,203),new Vector2(920,58),TextAnchor.MiddleCenter);
            cardTitle.color=new Color(.82f,.79f,.65f);
            cardBody=Text(card.transform,"Meaning and context","",22,new Vector2(-238,29),new Vector2(460,260),TextAnchor.UpperLeft);cardBody.lineSpacing=1.08f;
            cardControl=Text(card.transform,"Input instruction","",20,new Vector2(-238,-133),new Vector2(460,60),TextAnchor.MiddleLeft);
            cardControl.color=new Color(.78f,.77f,.65f);
            var picture=Rect(card.transform,"In-game example",new Vector2(252,12),new Vector2(425,239));
            example=picture.gameObject.AddComponent<RawImage>();example.raycastTarget=false;example.material=game.subtitle.material;
            cardFooter=Text(card.transform,"Context caption","",15,new Vector2(252,-119),new Vector2(425,30),TextAnchor.MiddleCenter);cardFooter.color=new Color(.56f,.58f,.5f);
            var closeImage=Panel(card.transform,"Close card",new Vector2(0,-203),new Vector2(390,48),new Color(.08f,.08f,.06f,.25f));closeImage.raycastTarget=true;
            closeButton=closeImage.gameObject.AddComponent<Button>();
            var colors=closeButton.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.5f,1.45f,1.2f);colors.pressedColor=new Color(.75f,.75f,.6f);closeButton.colors=colors;closeButton.navigation=new Navigation{mode=Navigation.Mode.None};
            Text(closeImage.transform,"Close label","GRIP  ·  Continue",23,Vector2.zero,new Vector2(380,45),TextAnchor.MiddleCenter);
            closeButton.onClick.AddListener(CloseCard);
            smallPrompt.SetActive(false);
        }
        RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        Image Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var i=Rect(parent,name,position,size).gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;i.material=game.subtitle.material;return i;
        }
        Text Text(Transform parent,string name,string value,int size,Vector2 position,Vector2 dimensions,TextAnchor alignment)
        {
            var t=Rect(parent,name,position,dimensions).gameObject.AddComponent<Text>();t.font=storyFont?storyFont:game.subtitle.font;t.material=game.subtitle.material;t.fontSize=size;t.text=value;t.color=cream;t.alignment=alignment;t.raycastTarget=false;t.supportRichText=true;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        void LateUpdate()
        {
            if(!card)return;
            game.title.SetActive(false);game.ending.SetActive(false);game.prompt.enabled=false;game.objective.enabled=false;game.subtitleGroup.alpha=0;
            foreach(var g in game.subtitleGroup.GetComponentsInChildren<Graphic>())g.enabled=false;
            if(game.Started&&!wasStarted)feedUntil=Time.time+7;
            wasStarted=game.Started;
            if(game.Finished&&!wasFinished)finishAt=Time.time+1.8f;
            if(game.Finished&&!endPresented&&Time.time>=finishAt){endPresented=true;ShowCard(3);}
            wasFinished=game.Finished;
            if(CardOpen)
            {
                smallPrompt.SetActive(false);thought.text="";thoughtGroup.alpha=0;lensGroup.alpha=0;
                bool primary=InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton,out bool p)&&p;
                if((primary&&!primaryHeld)||(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame))CloseCard();
                primaryHeld=primary;return;
            }
            primaryHeld=false;
            var hover=System.Array.Find(game.points,p=>p.hovered);
            int id=hover?hover.id:-1;
            if(id!=lastHover){hoverSince=Time.time;lastHover=id;lastUses=hover?hover.uses:-1;}
            if(Time.time-hoverSince>.55f)
            {
                if(id==0&&!phoneHelpSeen){phoneHelpSeen=true;ShowCard(1);return;}
                if(id==1&&game.complete[0]&&!deskHelpSeen){deskHelpSeen=true;ShowCard(2);return;}
                if(id==2&&game.complete[0]&&game.complete[1]&&!windowHelpSeen){windowHelpSeen=true;ShowCard(4);return;}
            }
            smallPrompt.SetActive(id>=0&&!game.Finished);
            bool vr=game.rig.GetComponent<QuietControllers>().ShowingControllers||game.rig.HardwareActive;
            string grip=vr?"GRIP":"LEFT CLICK",trigger=vr?"TRIGGER":"RIGHT CLICK";
            string action="";
            if(id>=0)
            {
                if(id==0)action=game.ReplyPending?"Sending your reply…":game.complete[0]?"Reply delivered":hover.selects==0?grip+"  ·  Read Eva's messages":trigger+"  ·  Ask Eva to get a table";
                if(id==1)action=game.SubmissionPending?"Submitting assignment…":game.complete[1]?"Assignment submitted":hover.selects==0?grip+"  ·  Review your assignment":trigger+"  ·  Submit assignment";
                if(id==2)action=hover.selects==0?grip+"  ·  Look out over the courtyard":trigger+"  ·  "+(game.WindowOpen?"Close window":"Open window");
                if(id==3)action=trigger+"  ·  "+(game.complete[0]&&game.complete[1]&&game.complete[2]?"Go downstairs to Eva":"Check before leaving");
            }
            if(id==1&&!game.complete[0])action="Reply to Eva before you sit down";
            if(id==2&&!game.complete[1])action="Send the assignment before taking a breather";
            contextText.text=action;
            if(game.subtitle.text!=lastThought){lastThought=game.subtitle.text;thoughtUntil=Time.time+6.5f;}
            thought.text=lastThought;
            thoughtGroup.alpha=Mathf.MoveTowards(thoughtGroup.alpha,string.IsNullOrEmpty(thought.text)?0:1,Time.deltaTime*4);
            if(Time.time>thoughtUntil)thoughtGroup.alpha=0;
            lensGroup.alpha=Mathf.MoveTowards(lensGroup.alpha,game.Started&&Time.time<feedUntil?1:0,Time.deltaTime*2);
            lensText.text="F E E D   /   MONDAY\n"+game.StoryTime+"     19°  ·  Clearing\n10:10  Studio     ·     Eva, 2 messages";
        }
        public void ShowCard(int kind)
        {
            cardKind=kind;card.SetActive(true);
            foreach(var p in game.points)p.GetComponent<XRSimpleInteractable>().enabled=false;
            var locomotion=game.rig.GetComponent<QuietLocomotion>();if(locomotion)locomotion.enabled=false;
            if(kind==0)
            {
                game.phoneText.text=EvaMessage;game.phoneText.fontSize=18;
                cardTitle.text="Monday, 8:42 a.m.";
                cardBody.text="It's Monday morning. Light slips through the curtains as your phone buzzes on the nightstand. Eva's downstairs for coffee, and your first class isn't until 10:10.\n\nThen you remember: last night's assignment is still on your laptop, waiting to be submitted.";
                cardControl.text="Left stick: move     Right stick: turn\nAim at an object to see its available action.";
                example.texture=roomExample;cardFooter.text="MAYA'S ROOM  /  MONDAY MORNING";
                closeButton.GetComponentInChildren<Text>().text="GRIP  ·  Begin morning";
            }
            else if(kind==1)
            {
                cardTitle.text="A message from Eva";
                cardBody.text="Eva's outside the coffee shop, wondering whether to go in without you. Her message is still open on your phone.\n\nLet her know you're awake and ask her to get a table. You'll be down as soon as you send off your assignment.";
                cardControl.text="GRIP  ·  Read Eva's messages\nTRIGGER  ·  Send your reply";
                example.texture=phoneExample;cardFooter.text="NIGHTSTAND  /  EVA, 2 MESSAGES";
                closeButton.GetComponentInChildren<Text>().text="GRIP  ·  Close";
            }
            else if(kind==2)
            {
                cardTitle.text="Last night's work";
                cardBody.text="The assignment is finished. You remember saving it before bed, but the submission page is still open on your laptop.\n\nThere's no time for another read-through. Check the file and send it before the 8:45 deadline.";
                cardControl.text="GRIP  ·  Review your assignment\nTRIGGER  ·  Submit the file";
                example.texture=deskExample;cardFooter.text="NORTH PIER STUDY  /  DUE 08:45";
                closeButton.GetComponentInChildren<Text>().text="GRIP  ·  Close";
            }
            else if(kind==4)
            {
                cardTitle.text="A little fresh air";
                cardBody.text="With the assignment finally sent, you notice how stuffy the room has become. The coffee on your desk has gone cold.\n\nOpen the window for a moment before you go. Eva can spare you another minute.";
                cardControl.text="GRIP  ·  Look outside\nTRIGGER  ·  Open the window";
                example.texture=windowExample;cardFooter.text="THE COURTYARD  /  08:46";
                closeButton.GetComponentInChildren<Text>().text="GRIP  ·  Close";
            }
            else
            {
                cardTitle.text="Coffee first";
                cardBody.text="Your reply is sent, the assignment is in, and a cool breeze is moving through the room. You give the place one last look and head for the door.\n\nDownstairs, Eva should have found a table by now. Coffee first. Then the rest of Monday.";
                cardControl.text="The opening continues beyond Maya's room.";
                example.texture=roomExample;cardFooter.text="A QUIET MORNING  /  END OF PROTOTYPE";
                closeButton.GetComponentInChildren<Text>().text="GRIP  ·  Return to the morning";
            }
        }
        public void CloseCard()
        {
            int kind=cardKind;closedCards++;card.SetActive(false);cardKind=-1;
            var locomotion=game.rig.GetComponent<QuietLocomotion>();if(locomotion)locomotion.enabled=true;
            if(kind==0)game.Begin();
            else if(kind==3){game.Restart();wasStarted=false;wasFinished=false;endPresented=false;phoneHelpSeen=deskHelpSeen=windowHelpSeen=false;lastThought="";thought.text="";ShowCard(0);return;}
            foreach(var p in game.points)p.GetComponent<XRSimpleInteractable>().enabled=game.Started&&!game.Finished;
        }
    }
}
