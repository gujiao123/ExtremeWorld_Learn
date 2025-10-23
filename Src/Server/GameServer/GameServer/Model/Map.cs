using Common;
using Common.Data;
using GameServer.Entities;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Models
{
    //一个地图 
    class Map
    {
        internal class MapCharacter
        {
            public NetConnection<NetSession> connection;
            public Character character;

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }

        public int ID
        {
            get { return this.Define.ID; }
        }
        internal MapDefine Define;

        /// <summary>
        /// 地图中的角色，以CharacterID为Key
        /// </summary>
        public Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();


        internal Map(MapDefine define)
        {
            this.Define = define;


        }

        internal void Update()
        {


        }


        /// <summary>
        /// 角色进入地图 同时告诉其他地图所有人 有人进地图了
        /// </summary>
        /// <param name="character"></param>
        internal void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}characterName{2}", this.Define.ID, character.Id, character.Info.Name);

            character.Info.mapId = this.ID;

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();


            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character.Info);
            //me 把你进入的信息发给其他角色
            //对地图中所有人发一份
            foreach (var kv in this.MapCharacters)
            {
                message.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);
                this.SendCharacterEnterMap(kv.Value.connection, character.Info);
            }
            //先告诉别人 然后再把你加入进去 就是一个遍历问题
            //!! 可以避免有竞争
            //characterid重复了
            this.MapCharacters[character.Id] = new MapCharacter(conn, character);
            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);

        }

        /// <summary>
        /// 发送角色进入地图消息给其他所有人 没所有人的内存中添加一份ID
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        /// 
        //me由于是先对新角色添加到角色管理器 所以info.ID也已经与entityID一致了
        void SendCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character);

            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }
        /// <summary>
        /// 这里删除地图管理器中的角色 并且告诉其他人 有人离开了地图
        /// </summary>
        /// <param name="cha"></param>
        internal void CharacterLeave(Character cha)
        {
            Log.InfoFormat("CharacterLeave: Map:{0} characterId:{1}", this.Define.ID, cha.Id);


            //先把自己移除 然后再告诉别人
            foreach (var kv in this.MapCharacters)
            {
                //??这样你不就通知不了自己了吗,只通知其他人吗
                this.SendCharacterLeaveMap(kv.Value.connection, cha);
            }
            //所以最后才删除自己
            this.MapCharacters.Remove(cha.Id);
        }



        /// <summary>
        /// 告诉每个session有一个cha离开了地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        void SendCharacterLeaveMap(NetConnection<NetSession> conn, Character character)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            message.Response.mapCharacterLeave.characterId = character.Id;
            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }
        /// <summary>
        /// 实现了实体位置的更新和广播
        /// </summary>
        /// <param name="entity"></param>
        internal void UpdateEntity(NEntitySync entity)
        {
            foreach (var kv in this.MapCharacters)
            {
                if (kv.Value.character.entityId == entity.Id)
                {
                    //自己就更新自己的位置
                    kv.Value.character.Position = entity.Entity.Position;
                    kv.Value.character.Direction = entity.Entity.Direction;
                    kv.Value.character.Speed = entity.Entity.Speed;

                }
                else
                {
                    //把自己移动消息广播给别人
                    MapService.Instance.SendEntityUpdate(kv.Value.connection, entity);
                }
            }
        }




    }
}
