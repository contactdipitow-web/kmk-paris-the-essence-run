#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KMK.EssenceRun.Editor
{
    [InitializeOnLoad]
    public static class KmkProjectSetup
    {
        private const string SceneFolder = "Assets/KMK/Scenes";
        private const string MainScenePath = SceneFolder + "/KMKMain.unity";
        private const string SessionKey = "KMK_ESSENCE_RUN_PROJECT_PREPARED";

        static KmkProjectSetup()
        {
            EditorApplication.delayCall += PrepareOncePerSession;
        }

        private static void PrepareOncePerSession()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            PrepareProject(false);
        }

        [MenuItem("KMK Paris/Prepare Unity Project", priority = 1)]
        public static void PrepareFromMenu()
        {
            PrepareProject(true);
        }

        [MenuItem("KMK Paris/Open Main Scene", priority = 2)]
        public static void OpenMainScene()
        {
            EnsureSceneExists(false);
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        [MenuItem("KMK Paris/Build iOS Xcode Project", priority = 20)]
        public static void BuildIos()
        {
            PrepareProject(false);
            EditorSceneManager.SaveOpenScenes();

            string outputPath = Path.GetFullPath("Builds/iOS");
            Directory.CreateDirectory(outputPath);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("KMK Paris iOS Xcode project created: " + outputPath);
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError("KMK Paris iOS build failed. Open Console for details.");
            }
        }

        [MenuItem("KMK Paris/Build macOS Development Preview", priority = 21)]
        public static void BuildMacPreview()
        {
            PrepareProject(false);
            EditorSceneManager.SaveOpenScenes();

            string outputDirectory = Path.GetFullPath("Builds/macOS");
            Directory.CreateDirectory(outputDirectory);
            string appPath = Path.Combine(outputDirectory, "KMK Essence Run.app");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = appPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(appPath);
            }
        }

        private static void PrepareProject(bool revealScene)
        {
            ConfigurePlayerSettings();
            EnsureSceneExists(revealScene);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (revealScene)
            {
                Debug.Log("KMK Paris — The Essence Run is prepared. Press Play to run the vertical slice.");
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "KMK PARIS";
            PlayerSettings.productName = KmkConstants.ProductName;
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = false;

            NamedBuildTarget ios = NamedBuildTarget.iOS;
            PlayerSettings.SetApplicationIdentifier(ios, KmkConstants.BundleIdentifier);
            PlayerSettings.SetScriptingBackend(ios, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.requiresPersistentWiFi = false;
            PlayerSettings.iOS.requiresFullScreen = true;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.statusBarHidden = true;

            NamedBuildTarget android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, KmkConstants.BundleIdentifier);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        }

        private static void EnsureSceneExists(bool openAfterCreation)
        {
            Directory.CreateDirectory(SceneFolder);
            bool created = false;

            if (!File.Exists(MainScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, MainScenePath);
                created = true;

                if (!openAfterCreation)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            if (openAfterCreation && (!created || SceneManager.GetActiveScene().path != MainScenePath))
            {
                EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            }
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };
        }
    }
}
#endif
