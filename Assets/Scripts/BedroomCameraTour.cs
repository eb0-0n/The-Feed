using UnityEngine;

public class BedroomCameraTour : MonoBehaviour
{
    public Transform[] waypoints;
    public float secondsPerSegment = 5f;
    public bool playOnStart = true;
    private float tourTime;
    private bool playing;

    private void Start()
    {
        playing = playOnStart;
        ApplyWaypoint(0);
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Alpha1)) { playing = false; ApplyWaypoint(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { playing = false; ApplyWaypoint(1); }
        if (Input.GetKeyDown(KeyCode.T)) { tourTime = 0f; playing = true; }
        if (Input.GetKeyDown(KeyCode.Space)) playing = !playing;
#endif

        if (!playing || waypoints == null || waypoints.Length < 2) return;
        tourTime += Time.deltaTime;
        float segmentLength = Mathf.Max(0.1f, secondsPerSegment);
        float total = segmentLength * (waypoints.Length - 1);
        float wrapped = Mathf.Repeat(tourTime, total);
        int segment = Mathf.Min(Mathf.FloorToInt(wrapped / segmentLength), waypoints.Length - 2);
        float t = Mathf.SmoothStep(0f, 1f, (wrapped - segment * segmentLength) / segmentLength);
        transform.position = Vector3.Lerp(waypoints[segment].position, waypoints[segment + 1].position, t);
        transform.rotation = Quaternion.Slerp(waypoints[segment].rotation, waypoints[segment + 1].rotation, t);
    }

    private void ApplyWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0) return;
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        transform.SetPositionAndRotation(waypoints[index].position, waypoints[index].rotation);
    }
}
