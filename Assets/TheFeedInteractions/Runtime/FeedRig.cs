using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace TheFeed
{
    [DefaultExecutionOrder(-200)]
    public class FeedRig : MonoBehaviour
    {
        public Camera playerCamera;
        public XRRayInteractor desktopRay, leftRay, rightRay;
        public Transform reticle;
        public Transform[] viewpoints;
        public bool demoMode;
        public Vector3 demoAim;
        public bool demoSelect, demoActivate;
        public bool HardwareActive { get; private set; }
        float pitch, yaw;
        void Start()
        {
            Configure(desktopRay); Configure(leftRay); Configure(rightRay);
            GoTo(0);
        }
        void Configure(XRRayInteractor r)
        {
            r.raycastTriggerInteraction=QueryTriggerInteraction.Collide;
            r.selectInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
            r.activateInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
            r.uiPressInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
            r.allowHoveredActivate=true;
        }
        public static void Button(XRInputButtonReader reader,bool held)
        {
            if(held!=reader.manualPerformed)
            {
                if(held) reader.manualFramePerformed=Time.frameCount;
                else reader.manualFrameCompleted=Time.frameCount;
            }
            reader.manualPerformed=held; reader.manualValue=held?1:0;
        }
        void Inputs(XRRayInteractor r,bool select,bool activate)
        {
            Button(r.selectInput,select); Button(r.uiPressInput,select); Button(r.activateInput,activate);
        }
        void Update()
        {
            HardwareActive=InputDevices.GetDeviceAtXRNode(XRNode.Head).isValid && !demoMode;
            desktopRay.gameObject.SetActive(!HardwareActive);
            leftRay.gameObject.SetActive(HardwareActive); rightRay.gameObject.SetActive(HardwareActive);
            if(HardwareActive)
            {
                var head=InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if(head.TryGetFeatureValue(CommonUsages.centerEyePosition,out Vector3 pos)) playerCamera.transform.localPosition=pos;
                if(head.TryGetFeatureValue(CommonUsages.centerEyeRotation,out Quaternion rot)) playerCamera.transform.localRotation=rot;
                Controller(leftRay,XRNode.LeftHand); Controller(rightRay,XRNode.RightHand);
                reticle.gameObject.SetActive(false); return;
            }
            var mouse=Mouse.current; var keyboard=Keyboard.current;
            if(!demoMode && keyboard!=null)
            {
                if(keyboard.digit1Key.wasPressedThisFrame) GoTo(0);
                if(keyboard.digit2Key.wasPressedThisFrame) GoTo(1);
                if(keyboard.digit3Key.wasPressedThisFrame) GoTo(2);
                if(keyboard.digit4Key.wasPressedThisFrame) GoTo(3);
                if(keyboard.digit5Key.wasPressedThisFrame) GoTo(4);
                float x=(keyboard.dKey.isPressed?1:0)-(keyboard.aKey.isPressed?1:0);
                float z=(keyboard.wKey.isPressed?1:0)-(keyboard.sKey.isPressed?1:0);
                Vector3 forward=playerCamera.transform.forward; forward.y=0; forward.Normalize();
                Vector3 move=(forward*z+playerCamera.transform.right*x)*1.8f*Time.deltaTime;
                if(move.sqrMagnitude>0 && !Physics.SphereCast(playerCamera.transform.position,.2f,move.normalized,out _,move.magnitude+.1f,~(1<<8),QueryTriggerInteraction.Ignore))
                    transform.position+=move;
                if(mouse!=null && mouse.middleButton.isPressed)
                {
                    Vector2 delta=mouse.delta.ReadValue(); yaw+=delta.x*.1f; pitch=Mathf.Clamp(pitch-delta.y*.1f,-75,75);
                    playerCamera.transform.rotation=Quaternion.Euler(pitch,yaw,0);
                }
            }
            Ray ray=demoMode ? new Ray(playerCamera.transform.position,(demoAim-playerCamera.transform.position).normalized) :
                playerCamera.ScreenPointToRay(mouse==null?new Vector2(Screen.width/2f,Screen.height/2f):mouse.position.ReadValue());
            desktopRay.transform.SetPositionAndRotation(ray.origin,Quaternion.LookRotation(ray.direction));
            bool select=demoMode?demoSelect:(mouse!=null && mouse.leftButton.isPressed)||(keyboard!=null && keyboard.eKey.isPressed);
            bool activate=demoMode?demoActivate:(mouse!=null && mouse.rightButton.isPressed)||(keyboard!=null && keyboard.spaceKey.isPressed);
            Inputs(desktopRay,select,activate);
            if(Physics.Raycast(ray,out RaycastHit hit,12,1<<8,QueryTriggerInteraction.Collide))
            {
                reticle.gameObject.SetActive(true); reticle.position=hit.point-ray.direction*.02f;
                reticle.localScale=Vector3.one*Mathf.Max(.012f,hit.distance*.006f);
            }
            else reticle.gameObject.SetActive(false);
        }
        void Controller(XRRayInteractor r,XRNode node)
        {
            var device=InputDevices.GetDeviceAtXRNode(node);
            if(device.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p)) r.transform.localPosition=p;
            if(device.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q)) r.transform.localRotation=q;
            device.TryGetFeatureValue(CommonUsages.gripButton,out bool grip);
            device.TryGetFeatureValue(CommonUsages.triggerButton,out bool trigger);
            Inputs(r,grip,trigger);
        }
        public void GoTo(int index)
        {
            if(viewpoints==null || viewpoints.Length==0) return;
            var point=viewpoints[Mathf.Clamp(index,0,viewpoints.Length-1)];
            transform.position=Vector3.zero;
            playerCamera.transform.SetPositionAndRotation(point.position,point.rotation);
            yaw=point.eulerAngles.y; pitch=point.eulerAngles.x;
            if(pitch>180)pitch-=360;
        }
    }
}
