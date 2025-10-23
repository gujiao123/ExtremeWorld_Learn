using Common.Data;
using Managers;
using Models;
using Network;
using SkillBridge.Message;
using System;
using UnityEngine;
namespace Services
{
    class MapService : Singleton<MapService>, IDisposable
    {

        public int CurrentMapId { get; set; }


        public MapService()
        {

            MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Subscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);
            MessageDistributer.Instance.Subscribe<MapEntitySyncResponse>(this.OnMapEntitySync);
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Unsubscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);
            MessageDistributer.Instance.Unsubscribe<MapEntitySyncResponse>(this.OnMapEntitySync);
        }


        public void Init()
        {
        }


        /// <summary>
        /// 服务器返回的进入地图的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="respnce"></param>
        /// 把角色信息交给客户端的角色管理器
        /// 切换地图
        private void OnMapCharacterEnter(object sender, MapCharacterEnterResponse response)
        {

            //!! 因为现在场景里面就只有一个角色 所以这里直接取第一个
            Debug.LogFormat("OnCharacterEnter:{0}", response.mapId);//地图ID
            //!! 这里进入新地图
            //!!!!! 啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊我草泥马 这个就不应该这力调用 我还没有录入当前地图信息啊啊啊
            //me 这里调用会导致双重加载场景我日你妈
            //me 我们第一次进入 默认currentMapId是0 所以 不仅这里会加载 下面的循环里面也会加载 哭哭哭哭哭 
            //!! SceneManager.Instance.LoadScene(DataManager.Instance.Maps[response.mapId].Resource);

            Debug.LogFormat("OnMapCharacterEnter:Map:{0} Count:{1}", response.mapId, response.Characters.Count);
            //response里面是地图里面所有角色的列表 不止你一个还有其他人
            foreach (var cha in response.Characters)
            {

                //就是空的所有第一次进入游戏CurrentCharacter
                //要么退出进入游戏或者第一次进入游戏设置当前角色 ,
                //?cha.Id == User.Instance.CurrentCharacter.Id 这个逻辑是什么
                //@难道是别人进入时候进一步确认自己是自己吗
                if (User.Instance.CurrentCharacter == null || cha.Id == User.Instance.CurrentCharacter.Id)
                {
                    User.Instance.CurrentCharacter = cha;
                }
                //切换地图逻辑
                //当前地图 与服务器返回的地图ID不一致

                Debug.LogFormat("切换地图逻辑CurrentMapId:{0} ResponseMapId:{1}", CurrentMapId, response.mapId);
                //你再这里添加角色 但是地图还没有加载呢
                CharacterManager.Instance.AddCharacter(cha);


                // 把服务器返回的所有角色交给客户端的角色管理器
            }
            if (CurrentMapId != response.mapId)
            {

                this.EnterMap(response.mapId);

                this.CurrentMapId = response.mapId;

            }
        }



        private void EnterMap(int mapId)
        {
            //先检查一下本地有没有这个地图信息
            if (DataManager.Instance.Maps.ContainsKey(mapId))
            {
                MapDefine map = DataManager.Instance.Maps[mapId];
                //进入地图后把地图信息保存在User里面
                User.Instance.CurrentMapData = map;//保存当前地图信息
                SceneManager.Instance.LoadScene(map.Resource);
            }
            else
            {
                UnityEngine.Debug.Log("没有地图信息");
            }
        }
        /// <summary>
        /// 响应服务器返回的离开地图的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void OnMapCharacterLeave(object sender, MapCharacterLeaveResponse response)
        {

            //如果是当前角色离开地图 说明是切换地图了
            //清除当前角色的游戏对象
            Debug.LogFormat("OnMapCharacterLeave: CharacterId:{0}", response.characterId);
            Debug.LogFormat("CurrentCharacter Id:{0}", User.Instance.CurrentCharacter != null ? User.Instance.CurrentCharacter.Id.ToString() : "null");
            if (response.characterId == User.Instance.CurrentCharacter.Id)
            {
                //是自己的话就直接毁掉所有
                //!!注意这个不会删除GameObjectManager里面的角色游戏对象 啊啊啊
                CharacterManager.Instance.Clear();
            }
            else
            {
                //其他角色离开地图 直接从角色管理器删除
                CharacterManager.Instance.RemoveCharacter(response.characterId);
            }


        }

        /// <summary>
        /// 发送实体同步请求
        /// </summary>
        /// <param name="entityEvent"></param>
        /// <param name="entity"></param>
        public void SendMapEntitySync(EntityEvent entityEvent, NEntity entity)
        {
            MapEntitySyncRequest request = new MapEntitySyncRequest();
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.mapEntitySync = new MapEntitySyncRequest();
            message.Request.mapEntitySync.entitySync = new NEntitySync()
            {
                Id = entity.Id,
                Event = entityEvent,
                Entity = entity
            };
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 响应服务器返回的实体同步响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        /// me 目前来看这个是用于接受其他人移动的信息同步
        private void OnMapEntitySync(object sender, MapEntitySyncResponse response)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("OnMapEntitySync: Count{0}\n", response.entitySyncs.Count);
            sb.AppendLine();
            foreach (var sync in response.entitySyncs)
            {

                EntityManager.Instance.OnEntitySync(sync);
                sb.AppendFormat("  EntityId:{0} Event:{1}\n", sync.Id, sync.Event);
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }


    }
}

