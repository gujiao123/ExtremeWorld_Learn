

//自己写 现在 记录一下

// 一个节点 == 资源+cache+ gameobject
//type + element 字典
//23 分钟

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//规定一些UI窗口的基本行为
public class UITest : UIWindow
{
    public Text testText;
    public void Start()
    {
        testText.text = "这是一个测试窗口";

    }
    public void Update()
    {

    }


}