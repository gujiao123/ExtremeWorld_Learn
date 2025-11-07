using UnityEngine;

class MessageBox
{
    static Object cacheObject = null;
    /// <summary>
    /// 显示消息对话框
    /// </summary>
    /// <param name="message">内容一般为errormessage</param>
    /// <param name="title">标题</param>
    /// <param name="type"></param>
    /// <param name="btnOK"></param>
    /// <param name="btnCancel"></param>
    /// <returns></returns>
    public static UIMessageBox Show(string message, string title = "", MessageBoxType type = MessageBoxType.Information, string btnOK = "", string btnCancel = "")
    {
        if (cacheObject == null)
        {
            // 对messagebox进行缓存
            cacheObject = Resloader.Load<Object>("UI/UIMessageBox");
        }
        // 对messagebox进行实例化
        GameObject go = (GameObject)GameObject.Instantiate(cacheObject);
        UIMessageBox msgbox = go.GetComponent<UIMessageBox>();
        msgbox.Init(title, message, type, btnOK, btnCancel);
        return msgbox;
    }
}

public enum MessageBoxType
{
    /// <summary>
    /// Information Dialog with OK button
    /// </summary>
    /// 信息提示
    Information = 1,

    /// <summary>
    /// Confirm Dialog whit OK and Cancel buttons
    /// </summary>
    /// 确认提示
    Confirm = 2,

    /// <summary>
    /// Error Dialog with OK buttons
    /// </summary>
    /// 错误提示
    Error = 3
}