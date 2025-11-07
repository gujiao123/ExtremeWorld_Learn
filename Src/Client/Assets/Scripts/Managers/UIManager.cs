

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
        public string Resources;//资源路径
        public GameObject instance;//实例化对象
        public bool Cache;//是否缓存
    }

    Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>();

    public UIManager()
    {
        //UIelements.Add(typeof(UITest), new UIElement() { ResourcePath = "UI/UITest", cached = true });
        //this.UIResources.Add(typeof(UITest), new UIElement() { Resources = "UI/UITest", Cache = true});
        this.UIResources.Add(typeof(UIBag), new UIElement() { Resources = "UI/UIBag", Cache = false });
        this.UIResources.Add(typeof(UIShop), new UIElement() { Resources = "UI/UIShop", Cache = false });
        this.UIResources.Add(typeof(UICharEquip), new UIElement() { Resources = "UI/UICharEquip", Cache = false });
        this.UIResources.Add(typeof(UIQuestSystem), new UIElement() { Resources = "UI/UIQuestSystem", Cache = false });
        this.UIResources.Add(typeof(UIQuestDialog), new UIElement() { Resources = "UI/UIQuestDialog", Cache = false });
        this.UIResources.Add(typeof(UIFriends), new UIElement() { Resources = "UI/UIFriends", Cache = false });
        this.UIResources.Add(typeof(UIGuild), new UIElement() { Resources = "UI/Guild/UIGuild", Cache = false });
        this.UIResources.Add(typeof(UIGuildList), new UIElement() { Resources = "UI/Guild/UIGuildList", Cache = false });
        this.UIResources.Add(typeof(UIGuildPopNoGuild), new UIElement() { Resources = "UI/Guild/UIGuildPopNoGuild", Cache = false });
        this.UIResources.Add(typeof(UIGuildPopCreate), new UIElement() { Resources = "UI/Guild/UIGuildPopCreate", Cache = false });
        this.UIResources.Add(typeof(UIGuildApplyList), new UIElement() { Resources = "UI/Guild/UIGuildApplyList", Cache = false });



    }
    /// <summary>
    /// 显示UI 创建实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Show<T>()
    {
        Type type = typeof(T);
        if (UIResources.ContainsKey(type))
        {
            UIElement element = UIResources[type];
            if (element.instance == null)
            {
                // 预加载资源
                UnityEngine.Object prefab = Resources.Load(element.Resources);
                //实例化
                GameObject go = GameObject.Instantiate(prefab) as GameObject;
                element.instance = go;
                Debug.LogFormat("创建UI实例: {0}", element.Resources);
            }
            element.instance.SetActive(true);
            return element.instance.GetComponent<T>();
        }
        return default(T);
    }

    public void Close(Type type)
    {
        if (UIResources.ContainsKey(type))
        {
            UIElement element = UIResources[type];
            if (element.instance != null)
            {
                if (element.Cache)
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