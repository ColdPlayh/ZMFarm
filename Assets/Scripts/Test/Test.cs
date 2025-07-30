using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        // 隐藏游戏窗口内的鼠标光标
        Cursor.visible = false;
        
        // 限制鼠标在游戏窗口内
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        // 如果按下了 Escape 键，则显示鼠标并解锁状态
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // 当游戏窗口失去焦点时（例如，用户切换到其他应用程序）
        if (!hasFocus)
        {
            // 显示系统鼠标光标
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 当游戏窗口重新获得焦点时，隐藏鼠标光标并将其锁定在窗口内
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void OnDisable()
    {
        // 恢复鼠标的默认行为（显示鼠标并解锁状态），当脚本禁用或游戏退出时
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}