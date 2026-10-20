using UnityEngine;
namespace TheFeed.Quiet
{
    public class QuietMotion : MonoBehaviour
    {
        public float amount=.45f,speed=.5f;
        Quaternion initial;
        void Start(){initial=transform.localRotation;}
        void Update(){transform.localRotation=initial*Quaternion.Euler(0,0,Mathf.Sin(Time.time*speed+transform.position.x)*amount);}
    }
}
