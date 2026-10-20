#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheFeed.Quiet.Editor
{
    [InitializeOnLoad]
    public static class InteractionSubmissionRecorder
    {
        const string Flag="TheFeed.DialogueSubmissionCapture";
        static InteractionSubmissionRecorder()
        {
            EditorApplication.playModeStateChanged+=state=>{
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Flag,false))
                {
                    SessionState.SetBool(Flag,false);
                    new GameObject("Interaction submission recording").AddComponent<InteractionSubmissionCapture>();
                }
            };
        }
        [MenuItem("The Feed/Submission/Record Five Dialogue Interactions")]
        public static void Record()
        {
            if(EditorApplication.isPlaying) return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            System.IO.Directory.CreateDirectory(QuietReview.Output);
            if(!System.IO.File.Exists(QuietBuilder.Scene)) QuietBuilder.Build();
            QuietQuestBuilder.Configure();
            EditorSceneManager.OpenScene("Assets/Scenes/TheFeed_QuietMorning.unity");
            SessionState.SetBool(Flag,true);
            EditorApplication.EnterPlaymode();
        }
    }
}
#endif
