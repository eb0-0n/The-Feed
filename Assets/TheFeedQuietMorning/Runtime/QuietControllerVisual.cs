using UnityEngine;
namespace TheFeed.Quiet
{
    public class QuietControllerVisual : MonoBehaviour
    {
        Transform[] nodes;
        void Awake(){nodes=GetComponentsInChildren<Transform>(true);}
        Transform Find(string n){foreach(var t in nodes)if(t.name==n)return t;return null;}
        void Pose(string key,float value)
        {
            var t=Find(key+"_value");var min=Find(key+"_min");var max=Find(key+"_max");
            if(!t||!min||!max)return;
            t.localPosition=Vector3.Lerp(min.localPosition,max.localPosition,value);
            t.localRotation=Quaternion.Slerp(min.localRotation,max.localRotation,value);
        }
        public void Animate(float grip,float trigger,Vector2 stick,bool primary,bool secondary,bool right)
        {
            if(nodes==null)nodes=GetComponentsInChildren<Transform>(true);
            Pose("xr_standard_squeeze_pressed",grip);Pose("xr_standard_trigger_pressed",trigger);
            Pose("xr_standard_thumbstick_xaxis_pressed",(stick.x+1)/2);
            Pose("xr_standard_thumbstick_yaxis_pressed",(stick.y+1)/2);
            Pose(right?"a_button_pressed":"x_button_pressed",primary?1:0);
            Pose(right?"b_button_pressed":"y_button_pressed",secondary?1:0);
        }
    }
}
