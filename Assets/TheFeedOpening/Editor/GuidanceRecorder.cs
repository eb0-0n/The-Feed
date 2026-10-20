using UnityEngine;
using UnityEditor;
namespace TheFeed.Opening.Editor
{
    [InitializeOnLoad]
    public static class GuidanceRecorder
    {
        const string Flag="TheFeed.StoryGuidance.Record";
        static GuidanceRecorder(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Flag,false)){SessionState.SetBool(Flag,false);new GameObject("Story guidance verification").AddComponent<GuidanceCapture>();}};}
        public static void Record(){GuidanceBuilder.Build();SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}
    }
}
