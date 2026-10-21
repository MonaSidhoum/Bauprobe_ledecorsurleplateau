/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Meta.XR.Samples;
using Meta.XR.ImmersiveDebugger;
using TMPro;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;


[MetaCodeSample("MRUKSample-Editor")]

partial class BuildMRUKSamples
{
    private const string BUNDLE_PREFIX = "com.samples.";
    private const string APK_NAME = "MRUKSamples.apk";

    private static string[] _scenePaths
    {
        get
        {
            var paths = new string[EditorBuildSettings.scenes.Length];
            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = EditorBuildSettings.scenes[i].path;
            }

            return paths;
        }
    }



    [MenuItem("Meta/Samples/Build MRUK Samples")]
    private static void Build()
    {
        InitializeBuild("mruksample", "MRUKSample");


        var projectSettings = OVRProjectConfig.CachedProjectConfig;
        projectSettings.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Required;
        projectSettings.anchorSupport = OVRProjectConfig.AnchorSupport.Enabled;
        projectSettings.sceneSupport = OVRProjectConfig.FeatureSupport.Required;
        projectSettings.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersOnly;
        var buildPlayerOptions = new BuildPlayerOptions
        {
            target = BuildTarget.Android,
            locationPathName = APK_NAME,
            scenes = _scenePaths
        };
        var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (!Application.isBatchMode && buildReport.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.RevealInFinder(APK_NAME);
        }
    }

    private static void InitializeBuild(string identifierSuffix, string productName = null)
    {
        PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
        var graphicsApis = new GraphicsDeviceType[1];
        graphicsApis[0] = GraphicsDeviceType.Vulkan;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, graphicsApis);
        PlayerSettings.colorSpace = ColorSpace.Linear;
        //Set ARM64 Requirements
#if UNITY_6000_0_OR_NEWER
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), 1); // 0 - None, 1 - ARM64, 2 - Universal
#else
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 1); // 0 - None, 1 - ARM64, 2 - Universal
#endif
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        QualitySettings.antiAliasing = 4;
#if UNITY_6000_0_OR_NEWER
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), $"{BUNDLE_PREFIX}{identifierSuffix}");
#else
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, $"{BUNDLE_PREFIX}{identifierSuffix}");
#endif
        if (!string.IsNullOrEmpty(productName))
        {
            PlayerSettings.productName = productName;
        }
    }
}

