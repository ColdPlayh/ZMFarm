using UnityEditor; // 引入 Unity 编辑器 API
using UnityEditor.Compilation; // 引入编译管道 API
using UnityEngine; // 引入 Unity 引擎 API
using UnityEditor.SceneManagement;
using UnityEngine.UI; // 引入场景管理 API

public class AutoSaveProject : EditorWindow
{
    // 自动保存的时间间隔（秒）
    private static float saveInterval = 600f;
    // 下一次保存的时间戳
    private static float nextSaveTime = 0f;
    // 用户输入的保存间隔字符串
    private static string userIntervalInput = "300";
    // 是否启用自动保存的标志
    private static bool isAutoSaveEnabled = true;
    // 上一次保存的时间戳
    private static float lastSaveTime = 0f;

    // 保存失败的次数
    private static int saveFailCount = 0;
    // 最大保存失败次数
    private static int maxSaveFailCount = 3;
    // 用户输入的最大保存失败次数字符串
    private static string userMaxSaveFailCountInput = "3";
    // 保存失败后的重试间隔（秒）
    private static float retryInterval = 10f;
    // 用户输入的重试间隔字符串
    private static string userRetryIntervalInput = "10";
    // 是否启用保存失败重试
    private static bool isIntervalSaveEnable = false;
    private static bool isRetryEnabled = false;

    // 是否启用运行前保存
    private static bool isPlaySaveEnabled = true;
    // 是否启用编译完成后保存
    private static bool isCompileSaveEnabled = true;
    
    
    // 在 Unity 菜单中创建一个窗口选项
    [MenuItem("My Tools/My Plugins/AutoSaveProject")]
    public static void ShowWindow()
    {
        // 显示窗口并设置标题
        GetWindow<AutoSaveProject>("自动保存项目");
    }

    // 窗口中的 GUI 元素
    private void OnGUI()
    {
        // 设置标题
        GUILayout.Label("自动保存设置", EditorStyles.boldLabel);

        // 自动保存间隔输入框
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("自动保存间隔 (秒):");
        // 用户输入保存间隔
        userIntervalInput = EditorGUILayout.TextField(userIntervalInput, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        // 重试间隔输入框
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("重试间隔 (秒):");
        // 用户输入重试间隔
        userRetryIntervalInput = EditorGUILayout.TextField(userRetryIntervalInput, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        // 最大保存失败次数输入框
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("最大保存失败次数:");
        // 用户输入最大保存失败次数
        userMaxSaveFailCountInput = EditorGUILayout.TextField(userMaxSaveFailCountInput, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        // 启用/禁用自动保存
        isAutoSaveEnabled = EditorGUILayout.Toggle("启用自动保存", isAutoSaveEnabled);

        // 启用/禁用间隔保存
        isIntervalSaveEnable = EditorGUILayout.Toggle("启用间隔保存", isIntervalSaveEnable&& isAutoSaveEnabled);
        // 启用/禁用失败重试
        isRetryEnabled = EditorGUILayout.Toggle("启用失败重试", isRetryEnabled && isAutoSaveEnabled);

        // 启用/禁用运行前保存
        isPlaySaveEnabled = EditorGUILayout.Toggle("运行前保存", isPlaySaveEnabled && isAutoSaveEnabled);

        // 启用/禁用编译完成后保存
        isCompileSaveEnabled = EditorGUILayout.Toggle("编译完成后保存", isCompileSaveEnabled && isAutoSaveEnabled);

        // 当用户按下回车键或点击按钮时更新保存间隔和重试间隔
        if (GUILayout.Button("设置间隔") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
        {
            // 尝试解析用户输入的保存间隔
            bool validInterval = float.TryParse(userIntervalInput, out float newInterval);
            // 尝试解析用户输入的重试间隔
            bool validRetryInterval = float.TryParse(userRetryIntervalInput, out float newRetryInterval);
            // 尝试解析用户输入的最大保存失败次数
            bool validMaxSaveFailCount = int.TryParse(userMaxSaveFailCountInput, out int newMaxSaveFailCount);

            // 如果输入有效，则更新设置
            if (validInterval && validRetryInterval && validMaxSaveFailCount)
            {
                saveInterval = newInterval;
                retryInterval = newRetryInterval;
                maxSaveFailCount = newMaxSaveFailCount;
                nextSaveTime = (float)EditorApplication.timeSinceStartup + saveInterval;
                Debug.Log($"自动保存间隔已设置为 {saveInterval} 秒, 重试间隔已设置为 {retryInterval} 秒, 最大保存失败次数已设置为 {maxSaveFailCount}");
            }

            
        }

        // 显示下一次保存时间
        if (isAutoSaveEnabled)
        {
            // 计算距离下一次保存的时间
            float timeToNextSave = nextSaveTime - (float)EditorApplication.timeSinceStartup;
            // 显示剩余时间
            EditorGUILayout.LabelField("距离下一次保存还有:", $"{timeToNextSave:F2} 秒");
        }
      
        
    }

    // 每帧调用，用于检查是否需要保存
    private void Update()
    {
        // 如果启用了自动保存且当前不在播放模式，并且当前时间超过了下一次保存时间
        if (isAutoSaveEnabled &&isIntervalSaveEnable
            && !EditorApplication.isPlaying && EditorApplication.timeSinceStartup >= nextSaveTime)
        {
            // 保存项目
            SaveProject();
        }
    }

    // 保存当前项目
    /*private void SaveProject()
    {
        // 保存所有打开的场景
        bool saveSuccessful = EditorSceneManager.SaveOpenScenes();
        // 保存项目资源
        AssetDatabase.SaveAssets();

        if (saveSuccessful)
        {
            // 保存成功，重置保存失败计数器，更新下一次保存时间
            Debug.Log("项目已自动保存。");
            saveFailCount = 0;
            lastSaveTime = (float)EditorApplication.timeSinceStartup;
            nextSaveTime = lastSaveTime + saveInterval;
        }
        else
        {
            // 保存失败，增加保存失败计数器
            saveFailCount++;
            Debug.LogError($"自动保存项目失败！失败次数：{saveFailCount}");

            if (saveFailCount >= maxSaveFailCount)
            {
                // 超过最大保存失败次数，重置计数器，更新下一次保存时间
                Debug.LogError("连续三次自动保存失败，请检查项目设置和权限。");
                saveFailCount = 0;
                nextSaveTime = (float)EditorApplication.timeSinceStartup + saveInterval;
            }
            else if (isRetryEnabled)
            {
                // 启用重试，更新下一次保存时间为重试间隔后
                nextSaveTime = (float)EditorApplication.timeSinceStartup + retryInterval;
            }
        }
    }*/

    // 启动编辑器时注册 Update 回调
    [InitializeOnLoadMethod]
    private static void StartAutoSave()
    {
        // 注册编辑器更新回调
        EditorApplication.update += EditorUpdate;
        // 注册播放模式状态变化回调
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        // 注册编译完成回调
        CompilationPipeline.compilationFinished += OnCompilationFinished;
    }

    // 编辑器的更新回调
    private static void EditorUpdate()
    {
        // 如果窗口实例已打开
        if (EditorWindow.HasOpenInstances<AutoSaveProject>())
        {
            // 获取窗口实例并调用 Update 方法
            AutoSaveProject window = GetWindow<AutoSaveProject>();
            window.Update();
        }
    }

    // 运行模式改变时的回调
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 如果启用运行前保存且当前状态为即将退出编辑模式
        if (isAutoSaveEnabled && isPlaySaveEnabled && state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("进入播放模式前保存场景。");
            // 保存项目
            SaveProjectNoReTry("进入播放模式保存项目失败");
        }
    }

    // 编译完成后的回调
    private static void OnCompilationFinished(object obj)
    {
        if (isAutoSaveEnabled && isCompileSaveEnabled)
        {
            Debug.Log("脚本重新编译完成，保存项目。");
            // 保存项目
            SaveProjectNoReTry("编译完成保存项目失败");
        }
    }

    // 保存当前项目（静态方法）
    private static void SaveProject()
    {
        // 保存所有打开的场景
        bool saveSuccessful = EditorSceneManager.SaveOpenScenes();
        // 保存项目资源
        AssetDatabase.SaveAssets();

        if (saveSuccessful)
        {
            // 保存成功，重置保存失败计数器，更新下一次保存时间
            Debug.Log("项目已自动保存。");
            saveFailCount = 0;
            lastSaveTime = (float)EditorApplication.timeSinceStartup;
            nextSaveTime = lastSaveTime + saveInterval;
        }
        else
        {
            // 保存失败，增加保存失败计数器
            saveFailCount++;
            Debug.LogError($"自动保存项目失败！失败次数：{saveFailCount}");

            if (saveFailCount >= maxSaveFailCount)
            {
                // 超过最大保存失败次数，重置计数器，更新下一次保存时间
                Debug.LogError("连续三次自动保存失败，请检查当前项目");
                saveFailCount = 0;
                nextSaveTime = (float)EditorApplication.timeSinceStartup + saveInterval;
            }
            else if (isRetryEnabled)
            {
                // 启用重试，更新下一次保存时间为重试间隔后
                nextSaveTime = (float)EditorApplication.timeSinceStartup + retryInterval;
            }
        }
    }
    private static void SaveProjectNoReTry(string message)
    {
        // 保存所有打开的场景
        bool saveSuccessful = EditorSceneManager.SaveOpenScenes();
        // 保存项目资源
        AssetDatabase.SaveAssets();

        if (saveSuccessful)
        {
            // 保存成功，重置保存失败计数器，更新下一次保存时间
           
            saveFailCount = 0;
            lastSaveTime = (float)EditorApplication.timeSinceStartup;
            nextSaveTime = lastSaveTime + saveInterval;
        }
        else
        {
            Debug.LogError(message);
            saveFailCount = 0;
            nextSaveTime = (float)EditorApplication.timeSinceStartup + saveInterval;
        }
    }
}
