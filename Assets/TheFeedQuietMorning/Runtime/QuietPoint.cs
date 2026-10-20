using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace TheFeed.Quiet
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class QuietPoint : MonoBehaviour
    {
        public int id;
        public QuietMorning game;
        public Renderer[] surfaces;
        public int hoverIn,hoverOut,selects,uses;
        public bool hovered;
        XRSimpleInteractable xr;
        MaterialPropertyBlock block;
        void Awake(){xr=GetComponent<XRSimpleInteractable>();block=new MaterialPropertyBlock();}
        void OnEnable()
        {
            if(!xr)xr=GetComponent<XRSimpleInteractable>();
            xr.firstHoverEntered.AddListener(Enter); xr.lastHoverExited.AddListener(Exit);
            xr.selectEntered.AddListener(Select); xr.activated.AddListener(Use);
        }
        void OnDisable()
        {
            if(!xr)return;
            xr.firstHoverEntered.RemoveListener(Enter); xr.lastHoverExited.RemoveListener(Exit);
            xr.selectEntered.RemoveListener(Select); xr.activated.RemoveListener(Use); Clear();
        }
        void Enter(HoverEnterEventArgs e){hoverIn++;hovered=true;Tint(true);game.Hover(id,true);}
        void Exit(HoverExitEventArgs e){hoverOut++;hovered=false;Tint(false);game.Hover(id,false);}
        void Select(SelectEnterEventArgs e){selects++;game.Inspect(id);}
        void Use(ActivateEventArgs e){uses++;game.Use(id);}
        void Tint(bool active)
        {
            if(block==null)block=new MaterialPropertyBlock();
            foreach(var r in surfaces)
            {
                if(!r)continue;
                if(!active){r.SetPropertyBlock(null);continue;}
                block.Clear();
                Color c=r.sharedMaterial.HasProperty("_BaseColor")?r.sharedMaterial.GetColor("_BaseColor"):Color.white;
                block.SetColor("_BaseColor",Color.Lerp(c,new Color(.8f,.74f,.57f),.13f));
                r.SetPropertyBlock(block);
            }
        }
        public void Clear(){hovered=false;Tint(false);}
    }
}
