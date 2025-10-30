

//自己写 现在 记录一下

// 一个节点 == 资源+cache+ gameobject
//type + element 字典
//23 分钟

using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    class UIElement
    {
        public string ResourcePath;//资源路径
        public GameObject instance;//实例化对象
        public bool cached;//是否缓存
    }

    Dictionary<Type, UIElement> UIelements = new Dictionary<Type, UIElement>();

    public UIManager()
    {
        //UIelements.Add(typeof(UITest), new UIElement() { ResourcePath = "UI/UITest", cached = true });
        //this.UIResources.Add(typeof(UITest), new UIElement() { Resources = "UI/UITest", Cache = true});
        this.UIelements.Add(typeof(UIBag), new UIElement() { ResourcePath = "UI/UIBag", cached = false });
        this.UIelements.Add(typeof(UIShop), new UIElement() { ResourcePath = "UI/UIShop", cached = false });
    }
    /// <summary>
    /// 显示UI 创建实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Show<T>()
    {
        Type type = typeof(T);
        if (UIelements.ContainsKey(type))
        {
            UIElement element = UIelements[type];
            if (element.instance == null)
            {
                // 预加载资源
                UnityEngine.Object prefab = Resources.Load(element.ResourcePath);
                //实例化
                GameObject go = GameObject.Instantiate(prefab) as GameObject;
                element.instance = go;
                Debug.LogFormat("创建UI实例: {0}", element.ResourcePath);
            }
            element.instance.SetActive(true);
            return element.instance.GetComponent<T>();
        }
        return default(T);
    }

    public void Close(Type type)
    {
        if (UIelements.ContainsKey(type))
        {
            UIElement element = UIelements[type];
            if (element.instance != null)
            {
                if (element.cached)
                {
                    element.instance.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(element.instance);
                    element.instance = null;
                }
            }
        }
    }


}