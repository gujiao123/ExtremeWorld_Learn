using Entities;
using Models;
using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏对象管理器
//me 角色管理器与地图有关 但是游戏对象管理器与什么都无关 负责实体的创建和销毁 完全与角色无关
public class testManager : MonoSingleton<testManager>
{
    // 添加静态构造函数来追踪类型加载
    static testManager()
    {
        Debug.LogError("=== testManager 静态构造函数被调用 ===");
        Debug.LogError("静态构造调用栈: " + System.Environment.StackTrace);
    }

    // 构造函数追踪
    public testManager()
    {
        Debug.LogError("=== testManager 构造函数被调用 ===");
        Debug.LogError("构造函数调用栈: " + System.Environment.StackTrace);
    }

    //!! 我就不信了 这个脚本会出现两个实例化对象


}