using Common;
using Common.Data;
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
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapTeleportRequest>(this.OnMapTeleport);

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
        /// <summary>
        /// 客户端传入的是 自己离开的传送点ID  
        //  服务器检验 进来和出去的传送点是否合法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnMapTeleport(NetConnection<NetSession> sender, MapTeleportRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnMapTeleport:characterID:{0}{1}to Map:{2}", character.Id, character.Info.Name, message.teleporterId);
            //检查传送点是否合法
            //从哪个点进来 使用否合法
            if (!DataManager.Instance.Teleporters.ContainsKey(message.teleporterId))
            {
                Log.WarningFormat("OnMapTeleport:characterID:{0}{1} to invalid Teleporters:{2}", character.Id, character.Info.Name, message.teleporterId);
                return;
            }
            TeleporterDefine teleporterDefine = DataManager.Instance.Teleporters[message.teleporterId];
            //检查传送点的目标是否合法 
            if (teleporterDefine.LinkTo == 0 || !DataManager.Instance.Teleporters.ContainsKey(teleporterDefine.LinkTo))
            {
                //提醒一下 这个传送点没有链接目标 这是正常的 出生点就没有链接目标 不应该返回
                Log.WarningFormat("OnMapTeleport:characterID:{0}{1} to invalid LinkTo Map:{2}", character.Id, character.Info.Name, teleporterDefine.LinkTo);
            }
            //对对对对 linkto不存在会报错
            TeleporterDefine targetTeleporter = DataManager.Instance.Teleporters[teleporterDefine.LinkTo];

            //这里才拿到最后地图ID
            MapManager.Instance[teleporterDefine.MapID].CharacterLeave(character);
            character.Position = targetTeleporter.Position;
            character.Direction = targetTeleporter.Direction;
            MapManager.Instance[targetTeleporter.MapID].CharacterEnter(sender, character);

        }
    }
}
