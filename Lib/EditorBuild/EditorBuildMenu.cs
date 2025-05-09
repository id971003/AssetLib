/*
maid 7rzr 2025-03-16
update 7rzr 2025-03-16

현직 개발하다보니 
Build 형태에 따라 ScriptingSymbol 관리도 해줘야하고
빌드과정자동화 까진 다니지만 정규화 정도는 해줘야할 거같아서 만듬
Editor 폴더에 넣으면 상단에 나오니 넣고쓰기
*/




using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;


namespace Kd
{
    public static class EditorBuildMenu
    {
        private const string BUILD_MENU_FORMAT = "Kd/Build/";
        private const string DEFINE_SEPARATOR = ";";

        private static ScriptingImplementation _scriptingImplementation = ScriptingImplementation.Mono2x;

        #region DefineSymbols
        private static readonly string[] sDefineSymbol_Dev = { "DEV" };
        private static readonly string[] sDefineSymbol_Live = { "Live" };
        #endregion

        #region Dev Build
        [MenuItem(BUILD_MENU_FORMAT + "Dev / 0.Apply")]
        public static void Apply_Develop()
        {
            ApplySettings(GetDefineSymbolsString(sDefineSymbol_Dev), "Dev");
        }

        [MenuItem(BUILD_MENU_FORMAT + "Dev / 1.Build")]
        public static void Build_Develop()
        {
            Apply_Develop();
            BuildReport report = BuildInstallFile(false, "Dev", BuildOptions.None);
        }
        #endregion

        #region Live Build
        [MenuItem(BUILD_MENU_FORMAT + "Live / 0.Apply")]
        public static void Apply_Live()
        {
            ApplySettings(GetDefineSymbolsString(sDefineSymbol_Live), "Live");
        }

        [MenuItem(BUILD_MENU_FORMAT + "Live / 1.Build")]
        public static void Build_Live()
        {
            Apply_Live();
            BuildReport report = BuildInstallFile(false, "Live", BuildOptions.None);
        }
        #endregion

        private static string GetDefineSymbolsString(params string[] symbols)
        {
            return string.Join(DEFINE_SEPARATOR, symbols);
        }

        /// <summary>
        /// Scripting Define Symbol 및 플랫폼별 설정 적용
        /// </summary>
        private static void ApplySettings(string defineSymbols, string addressableProfileName)
        {

            // Live 빌드일 경우 IL2CPP 강제 적용
            if (addressableProfileName == "Live")
            {
                _scriptingImplementation = ScriptingImplementation.IL2CPP;
            }
            else
            {
                _scriptingImplementation = ScriptingImplementation.Mono2x;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(), defineSymbols);
        }

        /// <summary>
        /// 빌드 실행
        /// </summary>
        public static BuildReport BuildInstallFile(bool appBundle, string saveFilePrefix, BuildOptions buildOptions)
        {
            BuildTarget buildTarget = GetBuildTarget();
            EditorUserBuildSettings.buildAppBundle = appBundle;
            EditorUserBuildSettings.development = false;

            string projectPath = Application.dataPath.Replace("/Assets", "");
            string buildVersion = Application.version;
            string buildFolderName = $"{saveFilePrefix}{buildVersion}"; // 예: "Live1.0"
            string savePath = Path.Combine(projectPath, "Builds", buildFolderName);

            // 새로운 빌드 폴더 생성
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string extension = GetBuildExtension(buildTarget, appBundle);
            string saveFileName = $"{saveFilePrefix}_v{Application.version}.{extension}";
            string buildPath = Path.Combine(savePath, saveFileName);

            if (buildTarget == BuildTarget.iOS)
            {
                buildPath = savePath;
            }

            string[] scenePaths = GetBuildScenes();
            if (scenePaths.Length == 0)
            {
                Debug.LogError("❌ 빌드할 씬이 없습니다!");
                return null;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = buildPath,
                target = buildTarget,
                options = buildOptions
            };

            ApplyPlatformSettings(buildTarget, appBundle);

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"✅ Build Completed: {report.summary.result} | Path: {buildPath}");

            // 빌드 완료 후 폴더 열기
            OpenFolder(savePath);

            return report;
        }

        /// <summary>
        /// 현재 Unity 플랫폼에 맞게 빌드할 플랫폼 반환
        /// </summary>
        private static BuildTarget GetBuildTarget()
        {
#if UNITY_STANDALONE_WIN
            return BuildTarget.StandaloneWindows64;
#elif UNITY_STANDALONE_LINUX
            return BuildTarget.StandaloneLinux64;
#elif UNITY_IOS
            return BuildTarget.iOS;
#elif UNITY_ANDROID
            return BuildTarget.Android;
#else
            Debug.LogError("❌ 지원하지 않는 플랫폼입니다!");
            return BuildTarget.NoTarget;
#endif
        }

        /// <summary>
        /// 현재 Unity 플랫폼에 맞게 빌드 그룹 반환
        /// </summary>
        private static BuildTargetGroup GetBuildTargetGroup()
        {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            return BuildTargetGroup.Standalone;
#elif UNITY_IOS
            return BuildTargetGroup.iOS;
#elif UNITY_ANDROID
            return BuildTargetGroup.Android;
#else
            Debug.LogError("❌ 지원하지 않는 플랫폼입니다!");
            return BuildTargetGroup.Unknown;
#endif
        }

        /// <summary>
        /// 플랫폼별 확장자 반환
        /// </summary>
        private static string GetBuildExtension(BuildTarget buildTarget, bool appBundle)
        {
            return buildTarget switch
            {
                BuildTarget.Android => appBundle ? "aab" : "apk",
                BuildTarget.StandaloneWindows => "exe",
                BuildTarget.StandaloneWindows64 => "exe",
                BuildTarget.StandaloneLinux64 => "x86_64",
                BuildTarget.iOS => "", // iOS는 폴더 형태로 저장됨
                _ => "build"
            };
        }

        /// <summary>
        /// 빌드할 씬 목록 가져오기
        /// </summary>
        private static string[] GetBuildScenes()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            string[] scenePaths = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                scenePaths[i] = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            }
            return scenePaths;
        }

        /// <summary>
        /// 플랫폼별 추가 설정 적용
        /// </summary>
        private static void ApplyPlatformSettings(BuildTarget buildTarget, bool appBundle)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, _scriptingImplementation);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                    break;

                case BuildTarget.iOS:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, _scriptingImplementation);
                    break;

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, _scriptingImplementation);
                    break;
            }
        }
        /// <summary>
        /// 빌드가 완료된 후 해당 폴더를 자동으로 엽니다.
        /// </summary>
        private static void OpenFolder(string path)
        {
            path = Path.GetFullPath(path); // 경로를 절대경로로 변환

#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
    System.Diagnostics.Process.Start("open", path);
#elif UNITY_EDITOR_LINUX
    System.Diagnostics.Process.Start("xdg-open", path);
#endif
        }
    }
}

