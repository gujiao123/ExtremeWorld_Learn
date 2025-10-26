

//自己写 现在 记录一下

// 一个节点 == 资源+cache+ gameobject
//type + element 字典
//23 分钟

using System;
using System.Collections.Generic;
using UnityEngine;

//规定一些UI窗口的基本行为
public abstract class UIWindow : MonoBehaviour
{
    public enum WindowResult
    {
        None,
        Yes,
        No
    }
    public delegate void CloseHandler(UIWindow window, WindowResult result);
    public event CloseHandler OnClose;




    public void Close(WindowResult result = WindowResult.None)
    {
        UIManager.Instance.close(this.GetType());
        OnClose?.Invoke(this, result);
        //取消订阅
        this.OnClose = null;
    }
    public virtual void OnCloseClick()
    {
        this.Close();
    }
    public virtual void OnYesClick()
    {
        this.Close(WindowResult.Yes);
    }
    void OMouseDown()
    {
        Debug.LogFormat(this
        .name + " 被点击了");
    }



}