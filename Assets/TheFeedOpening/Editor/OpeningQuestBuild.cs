using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR.Features;
using UnityEditor.XR.OpenXR;
namespace TheFeed.Opening.Editor
{
    public static class OpeningQuestBuild
    {
        [MenuItem("The Feed/Story Guidance/Build Quest APK")]
        public static void Build()
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new InvalidOperationException("Android Build Support is not installed for Unity 6000.3.18f1. Add it in Unity Hub, with SDK/NDK tools and OpenJDK, then restart Unity.");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
                throw new InvalidOperationException("Select Android in Build Profiles, switch platform, wait for import, then run this build action again.");
            TheFeed.Quiet.Editor.QuietQuestProject.Configure();
            var issues=new List<OpenXRFeature.ValidationRule>();
            OpenXRProjectValidation.GetCurrentValidationIssues(issues,BuildTargetGroup.Android);
            if(issues.Any(i=>i.error))throw new InvalidOperationException("Resolve the Android OpenXR validation errors before building.");
            var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Builds/Quest"));Directory.CreateDirectory(folder);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{GuidanceBuilder.Scene},target=BuildTarget.Android,locationPathName=Path.Combine(folder,"TheFeed_StoryGuidance.apk"),options=BuildOptions.None});
            File.WriteAllText(Path.Combine(folder,"BuildReport.txt"),report.summary.result+"\n"+report.summary.totalErrors+" errors\n"+report.summary.totalSize+" bytes\nA successful build still needs installation and physical-headset testing.\n");
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Quest build failed. See the Unity Console and BuildReport.txt.");
        }
        public static void CompileCheck(){Debug.Log("STORY_GUIDANCE_EDITOR_COMPILED; Quest APK build remains blocked by missing Android module.");}
    }
}
