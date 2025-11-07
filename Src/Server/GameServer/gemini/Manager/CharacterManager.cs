
using Common;
using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Manager
{


    //只是一个地方 来存储所有角色
    class CharacterManager : Singleton<CharacterManager>
    {
        /// <summary>
        /// 数据库ID 到 角色实体 的映射
        /// 在线角色存储在这里
        /// </summary>
        public Dictionary<int, Character> characters = new Dictionary<int, Character>();
        public CharacterManager() { }
        public void Dispose() { }

        //四个基本功能初始化 清理 添加 删除

        public void Init() { }
        public void clear()
        {
            this.characters.Clear();
        }
        /// <summary>
        /// 不仅添加到角色管理器还添加到了实体管理器 还返回一个
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Character AddCharacter(TCharacter cha)
        {
            Character character = new Character(CharacterType.Player, cha);
            EntityManager.Instance.AddEntity(cha.MapID, character);
            //md下面才是对的 ,而且发给客户端的也是Info.ID也就同步为了entityID
            //character.Info.Id = character.Id;//Info.Id 原本是数据库的character.ID强行改为内存生成实体ID通信
            character.Info.EntityId = character.entityId;//改成entityID才是唯一标识
            this.characters[character.Id] = character;
            return character;
        }

        public void RemoveCharacter(int characterId)
        {
            var cha = this.characters[characterId];
            EntityManager.Instance.RemoveEntity(cha.Data.MapID, cha);
            this.characters.Remove(characterId);
        }/// <summary>
         /// 通过characterId获取角色
         /// </summary>
         /// <param name="characterId"></param>
         /// <returns></returns>
        public Character GetCharacter(int characterId)
        {
            Character character = null;
            this.characters.TryGetValue(characterId, out character);
            return character;
        }
    }
}

