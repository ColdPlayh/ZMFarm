

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//为哪个Attribute绘制他的Property
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    //在下拉菜单选中的选项的序号
    private int sceneIndex = -1;
    //每一个GUIContent就是下拉菜单中的一个选项
    private GUIContent[] sceneNames;
    
    private readonly string[] scenePathSplit = {"/", ".unity"};
    //上一次选中的序号
    private int lastSelectedIndex = -1;

    // private int lastSelectedIndex = -1;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position">包含这个Property的高度宽度位置</param>
    /// <param name="property">我们标记的某个变量就是一个Property</param>
    /// <param name="label">此属性的标签 </param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //如果场景为0则返回
        if (EditorBuildSettings.scenes.Length == 0) return;

        //如果序号为-1代表没有初始化过 进行初始化
        if (sceneIndex == -1)
        {
            GetSceneNameArray(property);
        }
        lastSelectedIndex= sceneIndex;
        //显示下拉选项 并且将选择的选项的序号赋值给index
        sceneIndex= EditorGUI.Popup(position, label, sceneIndex, sceneNames);
        //如果index发生变化改变变量的值
        if (lastSelectedIndex != sceneIndex)
        {
            property.stringValue = sceneNames[sceneIndex].text;
        }

    }

    private void GetSceneNameArray(SerializedProperty property)
    {
        //获取所有的场景
        var scenes = EditorBuildSettings.scenes;
        //如果没有找到场景则自己创建一个选项提示没找到场景
        if (scenes.Length == 0)
        {
            sceneNames = new[] {new GUIContent("Not Found Scene")};
        }
        //初始化GuiContent
        sceneNames = new GUIContent[scenes.Length];
        

        for (int i = 0; i < sceneNames.Length; i++)
        {
            //获取每个场景的路径
            string path = scenes[i].path;
            //切割scenePathSplit中的字符 并且去除空格
            var splitPath = path.Split(scenePathSplit, StringSplitOptions.RemoveEmptyEntries);
            //储存场景名称的临时变量
            string sceneName = "";
            //如歌切割后的数组长度大0代表场景存在
            if (splitPath.Length > 0)
            {
                //数组的最后一个就是我们需要的场景名称
                sceneName = splitPath[^1];
            }
            else
            {
                //如果不存在则设置名称提示不存在的场景
                sceneName = "Deleted Scene";
            }
            //初始化GuiContent的名字
            sceneNames[i] = new GUIContent(sceneName);
        }

        //如果当前的变量已经填写了内容
        //property的类型需要我们自己去指定 
        if (!string.IsNullOrEmpty(property.stringValue))
        {
           
            //我们填写的场景名字是否存在
            bool nameFound = false;
            //遍历所有的场景
            for (int i = 0; i < sceneNames.Length; i ++)
            {
                //如果找到了我们写的场景
                if (sceneNames[i].text == property.stringValue)
                {
                    
                    //设置index 让下拉菜单默认选中这个选项
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }
            }

            //如果没找到 默认选择第一个
            if (!nameFound)
            {
                sceneIndex = 0;
            }
        }
        //如果没有填写任何内容则默认选择第一个
        else
        {
            sceneIndex = 0;
        }

        //把我们选择的变量传递回我们的变量
        property.stringValue = sceneNames[sceneIndex].text;
    }
}
#endif