using Entities;
using Managers;
using Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏对象管理器
//me 角色管理器与地图有关 但是游戏对象管理器与什么都无关 负责实体的创建和销毁 完全与角色无关
public class GameObjectManager : MonoSingleton<GameObjectManager>
{


    //管理所有游戏对象

    /// <summary>
    /// entityID -> GameObject
    /// </summary>
    Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();

    // Use this for initialization

    //me 这里采用Onstart 是与MonoSingleton的设计有关
    //因为Start里面调用OnStart而已 没什么特别的
    protected override void OnStart()
    {

        //!!卧槽 这个注册了事件 所以就相当于 别人在外面调用了内部的函数了 ,但是这个跟出现两个有什么关系
        //@ 没有关系 出现两个单纯是因为双重加载场景 


        //me 这个函数是用于初始化已经存在的角色对象  很重要
        //第一次进入场景的时候调用
        StartCoroutine(InitGameObjects());
        //注册一个角色进入事件
        CharacterManager.Instance.OnCharacterEnter = OnCharacterEnter;
        CharacterManager.Instance.OnCharacterLeave = OnCharacterLeave;

    }

    private void OnDestroy()
    {
        // 只有当前实例是真正的单例实例时才清理事件
        // 避免重复实例销毁时错误地清空了真正单例注册的事件
        //me 在重复创建的GameObjectManager销毁下取消订阅的仍然是全局唯一的CharacterManager的事件  这是不对的
        if (Instance == this)
        {
            CharacterManager.Instance.OnCharacterEnter = null;
            CharacterManager.Instance.OnCharacterLeave = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCharacterEnter(Character cha)
    {
        CreateCharacterObject(cha);
    }
    /// <summary>
    /// 对于进入游戏的所有角色创建游戏对象  这个是用于其他NPC或者游戏玩家
    /// </summary>
    /// <returns></returns>
    /// !!实例化从这里开始
    IEnumerator InitGameObjects()
    {
        //调试：检查角色数量
        Debug.LogFormat("InitGameObjects: CharacterManager.characters.Count = {0}", CharacterManager.Instance.characters.Count);

        //这里是对于已经存在的角色进行创建游戏对象
        foreach (var cha in CharacterManager.Instance.characters.Values)
        {
            CreateCharacterObject(cha);
            yield return null;
        }
    }
    /// <summary>
    /// 这个是对于单个角色创建游戏对象
    /// </summary>
    /// <param name="character">服务器返回的角色列表信息</param>
    /// !! 这里创建游戏角色
    private void CreateCharacterObject(Character character)
    {
        //1.创建一个角色游戏对象 但没有初始化
        if (!gameObjects.ContainsKey(character.entityId) || gameObjects[character.entityId] == null)
        {
            //这个就是游戏资源下的预制体地址
            Object obj = Resloader.Load<Object>(character.Define.Resource);
            if (obj == null)
            {
                Debug.LogErrorFormat("Character[{0}] Resource[{1}] not existed.", character.Define.TID, character.Define.Resource);
                return;
            }
            //创建的角色挂载在GameObjectManager下面
            GameObject go = (GameObject)Instantiate(obj, this.transform);
            go.name = "Character_" + character.Info.Id + "_" + character.Info.Name;
            gameObjects[character.entityId] = go;
            //初始化游戏对象

            //角色创建同时创建名字栏
            UIWorldElementManager.Instance.AddCharacterNameBar(go.transform, character);

        }
        //2.初始化游戏对象
        this.InitGameObjects(gameObjects[character.entityId], character);
    }
    void InitGameObjects(GameObject go, Character character)
    {
        //设置位置和朝向
        go.transform.position = GameObjectTool.LogicToWorld(character.position);
        go.transform.forward = GameObjectTool.LogicToWorld(character.direction);
        go.SetActive(true);

        //??我创建的预制体都要带EntityController和PlayerInputController 卧槽
        //@对的通过IsPlayer来区分罢了
        //对于不是玩家的角色 只需要EntityController
        EntityController ec = go.GetComponent<EntityController>();
        if (ec != null)
        {
            ec.entity = character;
            ec.isPlayer = character.IsPlayer;
        }
        //!!根据NPC和Player身上脚本不同来进行区分
        //如果是玩家的话 还需要PlayerInputController 来处理玩家输入
        PlayerInputController pc = go.GetComponent<PlayerInputController>();
        if (pc != null)
        {

            if (character.Info.Id == Models.User.Instance.CurrentCharacterInfo.Id)
            {
                //设置当前角色对象
                User.Instance.CurrentCharacterObject = go;
                //把摄像机设置到玩家身上
                MainPlayerCamera.Instance.player = go;
                pc.enabled = true;
                pc.character = character;
                pc.entityController = ec;
            }
            else
            {
                pc.enabled = false;
            }
        }
    }

    /// <summary>
    /// 角色的离开与实体管理器有关
    /// </summary>
    /// <param name="charId"></param>
    void OnCharacterLeave(Character character)
    {
        //存在且不为空
        if (gameObjects.ContainsKey(character.entityId) && gameObjects[character.entityId] != null)
        {
            //这里清空GameObject 不应该有残留啊
            Destroy(gameObjects[character.entityId]);
            gameObjects.Remove(character.entityId);
        }
    }


}