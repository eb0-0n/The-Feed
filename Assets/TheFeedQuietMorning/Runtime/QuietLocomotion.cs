using UnityEngine;
using UnityEngine.XR;
namespace TheFeed.Quiet
{
    // Separate from the original rig so the first scene keeps its controls.
    public class QuietLocomotion : MonoBehaviour
    {
        public FeedRig rig;
        public float speed=1.1f;
        bool turnHeld;
        bool wasTracked;
        public void ResetOrigin()
        {
            var spawn=rig.viewpoints[0];
            rig.transform.SetPositionAndRotation(new Vector3(spawn.position.x,0,spawn.position.z),Quaternion.Euler(0,spawn.eulerAngles.y,0));
            wasTracked=true;
        }
        void LateUpdate()
        {
            if(!rig)return;
            var controllers=rig.GetComponent<QuietControllers>();
            if(!(controllers?controllers.IsHardwareActive:rig.HardwareActive)){wasTracked=false;return;}
            if(!wasTracked)
            {
                ResetOrigin();
            }
            var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.primary2DAxis,out Vector2 move);
            right.TryGetFeatureValue(CommonUsages.primary2DAxis,out Vector2 turn);
            if(move.magnitude>.2f)
            {
                var head=rig.playerCamera.transform;var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;
                var direction=forward*move.y+Vector3.Cross(Vector3.up,forward)*move.x;
                float distance=speed*Time.deltaTime*Mathf.Min(1,move.magnitude);
                var lower=new Vector3(head.position.x,rig.transform.position.y+.3f,head.position.z);
                var upper=head.position-Vector3.up*.2f;
                if(!Physics.CapsuleCast(lower,upper,.2f,direction.normalized,out _,distance+.08f,~(1<<8),QueryTriggerInteraction.Ignore))
                    rig.transform.position+=direction.normalized*distance;
            }
            if(Mathf.Abs(turn.x)>.75f && !turnHeld)
            {rig.transform.RotateAround(rig.playerCamera.transform.position,Vector3.up,Mathf.Sign(turn.x)*30);turnHeld=true;}
            if(Mathf.Abs(turn.x)<.3f)turnHeld=false;
        }
    }
}
