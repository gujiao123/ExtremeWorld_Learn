using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System;


//me 完全只是启动地图管理器 与客户端没有任何交互
namespace GameServer.Services
{
    class MapService : Singleton<MapService>
    {

        public MapService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapEntitySyncRequest>(this.OnMapEntitySync);
        }

        public void Init()
        {
            MapManager.Instance.Init();
        }


        /// <summary>
        /// 响应客户端的实体同步请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnMapEntitySync(NetConnection<NetSession> sender, MapEntitySyncRequest request)
        {
            Character cha = sender.Session.Character;
            Log.InfoFormat("OnMapEntitySync:characterID:{0}{1}Entity.Id{2}Evt{3}Entity{4}", cha.Id, cha.Info.Name, request.entitySync.Id, request.entitySync.Event, request.entitySync.Entity);
            MapManager.Instance[cha.Info.mapId].UpdateEntity(request.entitySync);
        }
        /// <summary>
        /// 发送实体更新到客户端
        /// 这个目前是通知其他人,有个人动了
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void SendEntityUpdate(NetConnection<NetSession> connection, NEntitySync entity)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.mapEntitySync = new MapEntitySyncResponse();
            //? 你协议还能写方法啊entitySyncs是一个列表
            //?为什么用列表明明是一个个发送的
            message.Response.mapEntitySync.entitySyncs.Add(entity);
            byte[] data = PackageHandler.PackMessage(message);
            connection.SendData(data, 0, data.Length);
        }
    }
}
