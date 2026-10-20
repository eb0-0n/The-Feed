using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TheFeed
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class FeedInteractable : MonoBehaviour
    {
        public enum Point { Phone, Laptop, Window, Door }
        public Point point;
        public FeedExperience experience;
        public Renderer[] highlights;
        public Transform label;
        public int hoverEnters, hoverExits, selections, activations;
        public bool IsHovered { get; private set; }
        XRSimpleInteractable interactable;
        MaterialPropertyBlock block;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            block = new MaterialPropertyBlock();
        }
        void OnEnable()
        {
            if (!interactable) interactable = GetComponent<XRSimpleInteractable>();
            interactable.firstHoverEntered.AddListener(HoverIn);
            interactable.lastHoverExited.AddListener(HoverOut);
            interactable.selectEntered.AddListener(Selected);
            interactable.activated.AddListener(Activated);
        }
        void OnDisable()
        {
            if (!interactable) return;
            interactable.firstHoverEntered.RemoveListener(HoverIn);
            interactable.lastHoverExited.RemoveListener(HoverOut);
            interactable.selectEntered.RemoveListener(Selected);
            interactable.activated.RemoveListener(Activated);
            Highlight(false);
        }
        void LateUpdate()
        {
            if (label && experience && experience.playerCamera)
                label.rotation = Quaternion.LookRotation(label.position - experience.playerCamera.transform.position);
        }
        void HoverIn(HoverEnterEventArgs args)
        {
            hoverEnters++; IsHovered = true; Highlight(true);
            if (experience) experience.Feedback(point, "HOVER", "Click / grip to inspect. Right-click / trigger to activate.");
        }
        void HoverOut(HoverExitEventArgs args)
        {
            hoverExits++; IsHovered = false; Highlight(false);
            if (experience) experience.Feedback(point, "HOVER EXIT", "Highlight restored. Aim at another object.");
        }
        void Selected(SelectEnterEventArgs args)
        {
            selections++;
            if (experience) experience.Inspect(point);
        }
        void Activated(ActivateEventArgs args)
        {
            activations++;
            if (experience) experience.Activate(point);
        }
        void Highlight(bool on)
        {
            if (block == null) block = new MaterialPropertyBlock();
            foreach (var r in highlights)
            {
                if (!r) continue;
                if (!on) { r.SetPropertyBlock(null); continue; }
                r.GetPropertyBlock(block);
                block.SetColor("_BaseColor", new Color(.22f,.92f,.83f));
                block.SetColor("_EmissionColor", new Color(.04f,.4f,.3f));
                r.SetPropertyBlock(block);
            }
        }
        public void ResetFeedback() { IsHovered=false; Highlight(false); }
    }
}
