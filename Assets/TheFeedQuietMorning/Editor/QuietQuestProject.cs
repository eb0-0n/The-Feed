using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;

namespace TheFeed.Quiet.Editor
{
    public static class QuietQuestProject
    {
        [MenuItem("The Feed/Quiet Morning/Configure Quest Android")]
        public static void Configure()
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new InvalidOperationException("Install Android Build Support for Unity 6000.3.18f1, then restart this Editor.");
            const BuildTargetGroup group=BuildTargetGroup.Android;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,out XRGeneralSettingsPerBuildTarget settings);
            if(!settings)
            {
                var found=AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
                if(found.Length>0)settings=AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(AssetDatabase.GUIDToAssetPath(found[0]));
                else
                {
                    if(!AssetDatabase.IsValidFolder("Assets/XR"))AssetDatabase.CreateFolder("Assets","XR");
                    settings=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(settings,"Assets/XR/Quiet Quest XR Management.asset");
                }
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,settings,true);
            }
            if(!settings.HasSettingsForBuildTarget(group))settings.CreateDefaultSettingsForBuildTarget(group);
            if(!settings.HasManagerSettingsForBuildTarget(group))settings.CreateDefaultManagerSettingsForBuildTarget(group);
            var general=settings.SettingsForBuildTarget(group);
            if(!XRPackageMetadataStore.AssignLoader(general.Manager,typeof(OpenXRLoader).FullName,group))
                throw new InvalidOperationException("Could not assign the Android OpenXR loader.");
            general.InitManagerOnStart=true;
            FeatureHelpers.RefreshFeatures(group);
            var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if(!xr)throw new InvalidOperationException("Android OpenXR settings are unavailable.");
            Enable<OculusTouchControllerProfile>(xr);
            Enable<MetaQuestTouchPlusControllerProfile>(xr);
            var quest=Enable<MetaQuestFeature>(xr);
            var serialized=new SerializedObject(quest);
            var devices=serialized.FindProperty("targetDevices");
            for(int i=0;i<devices.arraySize;i++)
            {
                var device=devices.GetArrayElementAtIndex(i);
                var name=device.FindPropertyRelative("manifestName").stringValue;
                device.FindPropertyRelative("enabled").boolValue=name=="quest2"||name=="eureka"||name=="quest3s";
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            xr.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan});
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.thefeed.quietmorning");
            EditorUtility.SetDirty(settings);EditorUtility.SetDirty(general);EditorUtility.SetDirty(general.Manager);EditorUtility.SetDirty(xr);EditorUtility.SetDirty(quest);
            AssetDatabase.SaveAssets();
            Validate();Debug.Log("QUEST_ANDROID_CONFIGURED");
        }
        static T Enable<T>(OpenXRSettings xr) where T:OpenXRFeature
        {
            var feature=xr.GetFeature<T>();
            if(!feature)throw new InvalidOperationException("Missing OpenXR feature: "+typeof(T).Name);
            feature.enabled=true;EditorUtility.SetDirty(feature);return feature;
        }
        [MenuItem("The Feed/Quiet Morning/Validate Quest Android")]
        public static void Validate()
        {
            var issues=new List<OpenXRFeature.ValidationRule>();
            OpenXRProjectValidation.GetCurrentValidationIssues(issues,BuildTargetGroup.Android);
            Directory.CreateDirectory(QuietReview.Output);
            File.WriteAllText(QuietReview.Output+"/QuestValidation.txt","Quest 2 / Quest 3 configuration\n"+string.Join("\n",issues.Select(i=>(i.error?"ERROR: ":"RECOMMENDATION: ")+i.message))+"\nPhysical headset testing is still required.\n");
            foreach(var issue in issues)Debug.LogWarning((issue.error?"QUEST_ERROR ":"QUEST_RECOMMENDATION ")+issue.message);
        }
        [MenuItem("The Feed/Quiet Morning/Build Quest APK")]
        public static void Build()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
                Debug.Log("Quest Android target selected. After importing, run Build Quest APK again.");return;
            }
            Configure();
            var issues=new List<OpenXRFeature.ValidationRule>();OpenXRProjectValidation.GetCurrentValidationIssues(issues,BuildTargetGroup.Android);
            if(issues.Any(i=>i.error))throw new InvalidOperationException("Resolve the errors listed in QuestValidation.txt before building.");
            EditorUserBuildSettings.buildAppBundle=false;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{QuietBuilder.Scene},target=BuildTarget.Android,locationPathName=QuietReview.Output+"/TheFeed_Quest.apk",options=BuildOptions.None});
            File.WriteAllText(QuietReview.Output+"/QuestBuild.txt",report.summary.result+"\n"+report.summary.totalSize+" bytes\n"+report.summary.totalErrors+" errors\nBuilt in Unity; not yet tested on a physical headset.\n");
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Quest APK build failed.");
            Debug.Log("QUEST_APK_BUILD_SUCCEEDED");
        }
    }
}
