using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public class AutoSaveScenes : EditorWindow
{
    private static float saveInterval = 600f; // 默认的自动保存时间间隔，单位为秒
    private static bool isAutoSaveEnabled = false; // 自动保存开关
    private static string saveFolderPath = "Assets/AutoSaveScenes"; // 默认保存文件夹路径
    private static float nextSaveTime = 0f; // 下一次保存的时间戳
    private static string userIntervalInput = "600"; // 用户输入的保存间隔字符串
    private static string userSaveFolderInput = "Assets/AutoSaveScenes"; // 用户输入的保存文件夹路径
    private static int maxSaveFiles = 10; // 最大保存文件数量
    private static string saveFolderNameFormat = "AutoSave_{0}"; // 文件夹名称格式
    private static string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss"); // 当前保存文件的时间戳

    [MenuItem("My Tools/My Plugins/AutoSaveScenes")]
    public static void ShowWindow()
    {
        // 打开或创建 AutoSaveScenes 窗口
        GetWindow<AutoSaveScenes>("自动保存场景");
    }

    private void OnGUI()
    {
        GUILayout.Label("自动保存设置", EditorStyles.boldLabel);

        // 自动保存开关
        isAutoSaveEnabled = EditorGUILayout.Toggle("启用自动保存", isAutoSaveEnabled);

        // 用户设置保存间隔时间（秒）
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("自动保存间隔 (秒):");
        userIntervalInput = EditorGUILayout.TextField(userIntervalInput, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        // 用户设置保存文件夹路径
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("保存文件夹路径:");
        userSaveFolderInput = EditorGUILayout.TextField(userSaveFolderInput);
        if (GUILayout.Button("浏览..."))
        {
            // 打开文件夹选择对话框
            string selectedPath = EditorUtility.OpenFolderPanel("选择保存文件夹", userSaveFolderInput, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                userSaveFolderInput = selectedPath;
            }
        }
        GUILayout.EndHorizontal();

        // 用户设置最大保存文件数量
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("最大保存文件数量:");
        if (int.TryParse(EditorGUILayout.TextField(maxSaveFiles.ToString(), GUILayout.Width(50)), out int maxFiles))
        {
            maxSaveFiles = maxFiles;
        }
        GUILayout.EndHorizontal();

        // 应用设置按钮
        if (GUILayout.Button("应用设置") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
        {
            // 解析用户输入的保存间隔时间
            bool validInterval = float.TryParse(userIntervalInput, out float newInterval);
            if (validInterval)
            {
                saveInterval = newInterval;
                nextSaveTime = (float)EditorApplication.timeSinceStartup + saveInterval;
                Debug.Log($"自动保存间隔设置为 {saveInterval} 秒");
            }
            else
            {
                Debug.LogError("保存间隔无效。请输入有效的数字。");
            }

            // 验证并设置保存文件夹路径
            if (Directory.Exists(userSaveFolderInput))
            {
                saveFolderPath = userSaveFolderInput;
                Debug.Log($"保存文件夹路径设置为 {saveFolderPath}");
            }
            else
            {
                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                    Debug.Log($"已创建默认保存文件夹路径 {saveFolderPath}");
                }
                else
                {
                    Debug.LogError("文件夹路径无效。使用默认路径。");
                }
            }

            // 确保最大保存文件数量为正值
            maxSaveFiles = Mathf.Max(1, maxSaveFiles);
        }

        // 立即保存按钮
        if (GUILayout.Button("立即保存"))
        {
            SaveScenes();
        }
    }

    private void Update()
    {
        // 如果启用自动保存，并且当前时间超过了下一次保存时间，则触发自动保存
        if (isAutoSaveEnabled && EditorApplication.timeSinceStartup >= nextSaveTime)
        {
            SaveScenes();
        }
    }

    private void SaveScenes()
    {
        // 生成当前时间戳用于保存文件夹名称
        timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        // 创建新的保存文件夹路径
        string saveFolder = Path.Combine(saveFolderPath, string.Format(saveFolderNameFormat, timestamp));

        // 如果文件夹不存在，则创建它
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }

        int sceneCount = SceneManager.sceneCount;
        if (sceneCount == 0)
        {
            // 如果没有打开的场景，则发出警告
            Debug.LogWarning("没有打开的场景可供保存。");
            return;
        }

        bool allScenesSaved = true;

        // 遍历所有打开的场景
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                // 获取场景的原始路径
                string originalPath = scene.path;
                // 生成保存文件的名称
                string sceneName = Path.GetFileNameWithoutExtension(originalPath);
                string sceneFileName = $"{sceneName}_{timestamp}.unity";
                // 生成保存文件的完整路径
                string sceneFilePath = Path.Combine(saveFolder, sceneFileName);

                // 保存场景副本
                if (SaveSceneCopy(scene, sceneFilePath))
                {
                    Debug.Log($"场景 '{scene.name}' 已保存到 {sceneFilePath}");
                }
                else
                {
                    Debug.LogError($"保存场景 '{scene.name}' 到 {sceneFilePath} 失败！");
                    allScenesSaved = false;
                }
            }
        }

        // 如果有场景保存失败，则在10秒后重试保存
        if (!allScenesSaved)
        {
            Debug.LogWarning("有场景保存失败，将在10秒后重试保存...");
            EditorApplication.delayCall += () => SaveScenes();
        }

        // 管理保存的文件夹数量，删除旧文件夹以保持最大数量
        ManageSaveFolders();
        // 更新下一次保存的时间戳
        nextSaveTime = (float)EditorApplication.timeSinceStartup + saveInterval;
    }

    private bool SaveSceneCopy(Scene scene, string savePath)
    {
        // 临时打开场景副本
        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);

        // 将场景副本保存到目标路径
        bool saveSuccess = EditorSceneManager.SaveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1), savePath);

        // 关闭副本场景
        EditorSceneManager.CloseScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1), true);

        return saveSuccess;
    }

    private void ManageSaveFolders()
    {
        // 获取所有的保存文件夹
        string[] folders = Directory.GetDirectories(saveFolderPath, "AutoSave_*");
        if (folders.Length > maxSaveFiles)
        {
            // 按照文件夹创建时间排序
            System.Array.Sort(folders, (x, y) => Directory.GetCreationTime(x).CompareTo(Directory.GetCreationTime(y)));
            // 删除超过最大数量的旧文件夹
            for (int i = 0; i < folders.Length - maxSaveFiles; i++)
            {
                Directory.Delete(folders[i], true);
                Debug.Log($"删除旧的保存文件夹: {folders[i]}");
            }
        }
    }

    [InitializeOnLoadMethod]
    private static void StartAutoSave()
    {
        // 注册更新方法以在编辑器更新时触发自动保存
        EditorApplication.update += EditorUpdate;
    }

    private static void EditorUpdate()
    {
        // 如果 AutoSaveScenes 窗口打开，则触发更新方法
        if (EditorWindow.HasOpenInstances<AutoSaveScenes>())
        {
            AutoSaveScenes window = GetWindow<AutoSaveScenes>();
            window.Update();
        }
    }
}
