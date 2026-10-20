using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR;
using CommonUsages=UnityEngine.XR.CommonUsages;
namespace TheFeed.Quiet
{
    [DefaultExecutionOrder(-150)]
    public class QuietControllers : MonoBehaviour
    {
        public FeedRig rig;
        public InputActionAsset actions;
        public QuietControllerVisual leftModel,rightModel;
        public QuietControllerVisual quest2LeftModel,quest2RightModel;
        public bool previewQuest2;
        public Transform leftAim,rightAim;
        public bool previewInEditor=true;
        public bool ShowingControllers { get; private set; }
        public bool IsPreview { get; private set; }
        public bool IsHardwareActive { get; private set; }
        float yaw,pitch;
        void OnEnable()
        {
            if(!rig)return;
            // This scene-local driver owns the rays; the original scene's rig stays unchanged.
            if(rig)rig.enabled=false;
            if(actions)actions.Enable();
            foreach(var r in new[]{rig.leftRay,rig.rightRay})
            {
                r.selectInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                r.activateInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                r.uiPressInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                r.allowHoveredActivate=true;r.raycastTriggerInteraction=QueryTriggerInteraction.Collide;
            }
        }
        void Start(){rig.GoTo(0);}
        void OnDisable(){if(actions)actions.Disable();}
        InputAction A(bool right,string name)=>actions?actions.FindAction((right?"Right/":"Left/")+name):null;
        float Value(bool right,string name)=>A(right,name)?.ReadValue<float>()??0;
        Vector2 Stick(bool right)=>A(right,"Stick")?.ReadValue<Vector2>()??Vector2.zero;
        void Update()
        {
            var head=InputDevices.GetDeviceAtXRNode(XRNode.Head);
            IsHardwareActive=head.isValid&&!rig.demoMode;
            if(IsHardwareActive)
            {
                if(head.TryGetFeatureValue(CommonUsages.centerEyePosition,out Vector3 position))rig.playerCamera.transform.localPosition=position;
                if(head.TryGetFeatureValue(CommonUsages.centerEyeRotation,out Quaternion rotation))rig.playerCamera.transform.localRotation=rotation;
            }
            bool preview=false;
            #if UNITY_EDITOR
            preview=previewInEditor&&!IsHardwareActive;
            #endif
            IsPreview=preview;ShowingControllers=IsHardwareActive||preview;
            if(!ShowingControllers)
            {
                rig.leftRay.gameObject.SetActive(false);rig.rightRay.gameObject.SetActive(false);
                return;
            }
            rig.desktopRay.gameObject.SetActive(false);rig.reticle.gameObject.SetActive(false);
            rig.leftRay.gameObject.SetActive(true);rig.rightRay.gameObject.SetActive(true);
            bool quest2=preview?previewQuest2:(head.name+" "+SystemInfo.deviceModel).ToLowerInvariant().Contains("quest 2");
            quest2=quest2&&quest2LeftModel&&quest2RightModel;
            leftModel.gameObject.SetActive(!quest2);rightModel.gameObject.SetActive(!quest2);
            if(quest2LeftModel)quest2LeftModel.gameObject.SetActive(quest2);
            if(quest2RightModel)quest2RightModel.gameObject.SetActive(quest2);
            var shownLeft=quest2?quest2LeftModel:leftModel;var shownRight=quest2?quest2RightModel:rightModel;
            if(preview)
            {
                var cam=rig.playerCamera;var mouse=Mouse.current;var keyboard=Keyboard.current;
                if(!rig.demoMode&&keyboard!=null)DesktopMove(cam,mouse,keyboard);
                bool select=rig.demoMode?rig.demoSelect:(mouse!=null&&mouse.leftButton.isPressed)||(keyboard!=null&&keyboard.eKey.isPressed);
                bool activate=rig.demoMode?rig.demoActivate:(mouse!=null&&mouse.rightButton.isPressed)||(keyboard!=null&&keyboard.spaceKey.isPressed);
                var aim=rig.demoMode?rig.demoAim:cam.ScreenPointToRay(mouse==null?new Vector2(Screen.width/2f,Screen.height/2f):mouse.position.ReadValue()).GetPoint(4);
                rig.rightRay.transform.position=cam.transform.TransformPoint(new Vector3(.24f,-.21f,.43f));
                rig.leftRay.transform.position=cam.transform.TransformPoint(new Vector3(-.24f,-.23f,.45f));
                rig.rightRay.transform.rotation=Quaternion.LookRotation(aim-rig.rightRay.transform.position)*Quaternion.Euler(-18,0,0);
                rig.leftRay.transform.rotation=cam.transform.rotation*Quaternion.Euler(5,-12,-8);
                rightAim.SetPositionAndRotation(rig.rightRay.transform.position,Quaternion.LookRotation(aim-rig.rightRay.transform.position));
                leftAim.SetPositionAndRotation(rig.leftRay.transform.position,cam.transform.rotation*Quaternion.Euler(20,-18,0));
                Input(rig.rightRay,select,activate);Input(rig.leftRay,false,false);
                shownRight.Animate(select?1:0,activate?1:0,Vector2.zero,false,false,true);
                shownLeft.Animate(0,0,Vector2.zero,false,false,false);
            }
            else
            {
                Hardware(rig.leftRay,leftAim,shownLeft,false);Hardware(rig.rightRay,rightAim,shownRight,true);
            }
        }
        void Hardware(XRRayInteractor ray,Transform aim,QuietControllerVisual visual,bool right)
        {
            var device=InputDevices.GetDeviceAtXRNode(right?XRNode.RightHand:XRNode.LeftHand);
            bool tracked=device.isValid&&device.TryGetFeatureValue(CommonUsages.isTracked,out bool value)&&value;
            ray.gameObject.SetActive(tracked);
            if(!tracked){Input(ray,false,false);return;}
            if(device.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 position))ray.transform.localPosition=position;
            if(device.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion rotation))ray.transform.localRotation=rotation;
            var grip=Value(right,"Grip");var trigger=Value(right,"Trigger");
            Input(ray,grip>.6f,trigger>.6f);
            var pointer=A(right,"AimRotation");
            if(pointer!=null&&pointer.activeControl!=null)
            {
                var origin=ray.transform.parent;
                aim.SetPositionAndRotation(origin.TransformPoint(A(right,"AimPosition").ReadValue<Vector3>()),origin.rotation*pointer.ReadValue<Quaternion>());
            }
            else {aim.localPosition=Vector3.zero;aim.localRotation=Quaternion.Euler(25,0,0);}
            visual.Animate(grip,trigger,Stick(right),Value(right,"Primary")>.5f,Value(right,"Secondary")>.5f,right);
        }
        void DesktopMove(Camera cam,Mouse mouse,Keyboard keys)
        {
            int view=keys.digit1Key.wasPressedThisFrame?0:keys.digit2Key.wasPressedThisFrame?1:keys.digit3Key.wasPressedThisFrame?2:keys.digit4Key.wasPressedThisFrame?3:keys.digit5Key.wasPressedThisFrame?4:-1;
            if(view>=0)rig.GoTo(view);
            var forward=Vector3.ProjectOnPlane(cam.transform.forward,Vector3.up).normalized;
            var move=(forward*((keys.wKey.isPressed?1:0)-(keys.sKey.isPressed?1:0))+cam.transform.right*((keys.dKey.isPressed?1:0)-(keys.aKey.isPressed?1:0)))*1.1f*Time.deltaTime;
            if(move.sqrMagnitude>0&&!Physics.SphereCast(cam.transform.position,.2f,move.normalized,out _,move.magnitude+.08f,~(1<<8),QueryTriggerInteraction.Ignore))rig.transform.position+=move;
            if(mouse!=null&&mouse.middleButton.isPressed)
            {
                var e=cam.transform.eulerAngles;pitch=e.x>180?e.x-360:e.x;yaw=e.y;
                var delta=mouse.delta.ReadValue();cam.transform.rotation=Quaternion.Euler(Mathf.Clamp(pitch-delta.y*.1f,-75,75),yaw+delta.x*.1f,0);
            }
        }
        void Input(XRRayInteractor ray,bool select,bool activate)
        {
            FeedRig.Button(ray.selectInput,select);FeedRig.Button(ray.activateInput,activate);
            FeedRig.Button(ray.uiPressInput,select||activate);
        }
    }
}
