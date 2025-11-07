using UnityEngine;

class InputBox
{
    static Object cacheObject = null;
    /// <summary>
    /// 显示输入框 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="btnOK"></param>
    /// <param name="btnCancel"></param>
    /// <param name="emptyTips"></param>
    /// <returns></returns>
    public static UIInputBox Show(string message, string title = "", string btnOK = "", string btnCancel = "", string emptyTips = "")
    {
        if (cacheObject == null)
            cacheObject = Resloader.Load<Object>("UI/UIInputBox");

        GameObject go = (GameObject)GameObject.Instantiate(cacheObject);
        UIInputBox inputBox = go.GetComponent<UIInputBox>();
        inputBox.Init(title, message, btnOK, btnCancel, emptyTips);
        return inputBox;
    }
}
