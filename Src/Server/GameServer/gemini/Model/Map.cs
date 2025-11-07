using Common;
using Common.Data;
using GameServer.Entities;
using GameServer.Managers;
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
            public NetConnection<NetSession> connection;//其他人的session信息
            public Character character;//对应的角色

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }
        /// <summary>
        /// 地图配置表中的ID
        /// </summary>
        public int ID
        {
            get { return this.Define.ID; }
        }
        internal MapDefine Define;

        /// <summary>
        /// 这一个地图中的角色，以EntityId 为Key
        /// </summary>
        public Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();
        /// <summary>
        /// 刷怪管理器
        /// </summary>
        SpawnManager SpawnManager = new SpawnManager();

        public MonsterManager MonsterManager = new MonsterManager();

        internal Map(MapDefine define)
        {
            this.Define = define;
            //
            this.SpawnManager.Init(this);
            this.MonsterManager.Init(this);
        }

        internal void Update()
        {
            SpawnManager.Update();
        }


        /// <summary>
        /// 角色进入地图 同时告诉这个地图的所有人 有人进地图了
        /// </summary>
        /// <param name="character"></param>
        internal void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}characterName{2}", this.Define.ID, character.Id, character.Info.Name);

            character.Info.mapId = this.ID;

            conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();


            conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;
            // 1. 把进入者自己的信息添加到给他的响应中
            conn.Session.Response.mapCharacterEnter.Characters.Add(character.Info);

            // 2. 遍历地图上的其他玩家
            foreach (var kv in this.MapCharacters)
            {
                // 2a. 把其他玩家的信息告诉进入者
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);
                // 2b. 把进入者的信息告诉其他玩家
                this.AddCharacterEnterMap(kv.Value.connection, character.Info);
            }
            //这个没有尊重时间
            //// 3. ***** 这是新增的修复逻辑 *****
            //// 遍历地图上所有现存的怪物
            //foreach (var monster in this.MonsterManager.Monsters.Values)
            //{
            //    //每次进入地图 都把怪物信息发过去
            //    // 3a. 把现存怪物的信息告诉进入者
            //    conn.Session.Response.mapCharacterEnter.Characters.Add(monster.Info);
            //}
            //// ***** 修复逻辑结束 *****

            //先告诉别人 然后再把你加入进去 就是一个遍历问题
            //!! 可以避免有竞争
            //characterid重复了
            this.MapCharacters[character.Id] = new MapCharacter(conn, character);
            conn.SendResponse();

        }

        /// <summary>
        /// 发送角色进入地图消息给其他所有人 没所有人的内存中添加一份ID
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        /// 
        //me由于是先对新角色添加到角色管理器 所以info.ID也已经与entityID一致了
        void AddCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            //这里也把怪物信息发过去
            if (conn.Session.Response.mapCharacterEnter == null)
            {
                conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
                conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;
            }
            conn.Session.Response.mapCharacterEnter.Characters.Add(character);
            //发送一遍
            //这里的send可以去掉 反正信息都在response里了 晚点发也没有关系
            //主要还是因为如果进入5000人 对于一个人来说 还是一次发5000人的数据快一些
            conn.SendResponse();
        }
        /// <summary>
        /// 这里删除地图管理器中的角色 并且告诉其他人 有人离开了地图
        /// </summary>
        /// <param name="cha"></param>
        internal void CharacterLeave(Character cha)
        {
            Log.InfoFormat("CharacterLeave: Map:{0} characterId:{1}", this.Define.ID, cha.Id);


            foreach (var kv in this.MapCharacters)
            {
                this.SendCharacterLeaveMap(kv.Value.connection, cha);
            }
            this.MapCharacters.Remove(cha.Id);
        }



        /// <summary>
        /// 告诉每个session有一个cha离开了地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        void SendCharacterLeaveMap(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("SendCharacterLeaveMap To {0}:{1} : Map:{2} Character:{3}:{4}", conn.Session.Character.Id, conn.Session.Character.Info.Name, this.Define.ID, character.Id, character.Info.Name);
            conn.Session.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            //conn.Session.Response.mapCharacterLeave.entityId = character.Id;
            conn.Session.Response.mapCharacterLeave.entityId = character.entityId;
            conn.SendResponse();
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

        /// <summary>
        /// 怪物进入地图
        /// </summary>
        /// <param name="monster"></param>
        internal void MonsterEnter(Monster monster)
        {
            Log.InfoFormat("MonsterEnter: Map:{0} monsterId:{1}", this.Define.ID, monster.Id);
            //通知角色有怪物进来了
            //你生成怪的地方 要有人才通知 嗯正常
            //不对不对 这里怪物是人的逻辑 意味着 即便地图不一样 你也应该通知
            //目前是地图不一样你就通知不了
            foreach (var kv in this.MapCharacters)
            {
                this.AddCharacterEnterMap(kv.Value.connection, monster.Info);
            }
        }

    }
}
