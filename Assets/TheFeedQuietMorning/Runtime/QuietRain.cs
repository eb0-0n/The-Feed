using UnityEngine;
namespace TheFeed.Quiet
{
    public class QuietRain : MonoBehaviour
    {
        public float fallSpeed=1.6f;
        void Update()
        {
            transform.position+=new Vector3(-.12f,-fallSpeed,0)*Time.deltaTime;
            if(transform.position.y<-.5f)transform.position+=new Vector3(.48f,6.4f,0);
        }
    }
}
