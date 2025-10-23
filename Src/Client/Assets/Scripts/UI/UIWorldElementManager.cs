
using Entities;
using System.Collections.Generic;
using UnityEngine;



//这个就是管理对外开放的接口 ,内部细致管理还是UIWorldElement类来管理
public class UIWorldElementManager : MonoSingleton<UIWorldElementManager>
{
    public GameObject nameBarPrefab;
    /// <summary>
    /// 管理所有的UI元素
    /// 这个字典的key是物体的Transform 这样就可以知道这个UI元素属于哪个物体
    /// </summary>
    private Dictionary<Transform, GameObject> elements = new Dictionary<Transform, GameObject>();

    // Use this for initialization
    //!!!!!啊啊啊啊啊啊啊啊啊啊啊啊啊啊这个覆盖了原来的Start啊啊啊啊啊
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
    /// <summary>
    /// 添加角色名字条
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="character"></param>
    public void AddCharacterNameBar(Transform owner, Character character)
    {
        //把物体初始化到这个对象下面 方便一起管理 消除
        GameObject goNameBar = Instantiate(nameBarPrefab, this.transform);
        goNameBar.name = "NameBar" + character.entityId;
        goNameBar.GetComponent<UIWorldElement>().owner = owner;
        goNameBar.GetComponent<UINameBar>().character = character;
        goNameBar.SetActive(true);
        //这个元素属于哪一个逻辑上角色,但还是挂在UIWorldElementManager下面 方便切换场景时候销毁
        this.elements[owner] = goNameBar;
    }
    /// <summary>
    /// 移除角色名字条
    /// </summary>
    /// <param name="owner"></param>
    public void RemoveCharacterNameBar(Transform owner)
    {
        if (this.elements.ContainsKey(owner))
        {
            Destroy(this.elements[owner]);
            this.elements.Remove(owner);
        }
    }
}