using Entities;
using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    class CharacterManager : Singleton<CharacterManager>, IDisposable
    {
        //根据角色ID存储map里面的角色
        public Dictionary<int, Character> characters = new Dictionary<int, Character>();

        public UnityAction<Character> OnCharacterEnter;

        public UnityAction<Character> OnCharacterLeave;


        public CharacterManager()
        {

        }

        public void Dispose()
        {

        }

        public void Init()
        {

        }

        public void Clear()
        {
            //从Character获取信息后删除对应实例 ,而不是单单删除character 要删除gameobject
            int[] keys = this.characters.Keys.ToArray();
            foreach (var key in keys)
            {
                this.RemoveCharacter(key);
            }
            this.characters.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cha"></param>
        public void AddCharacter(SkillBridge.Message.NCharacterInfo cha)
        {


            Debug.LogFormat("AddCharacter:{0}:{1} Map:{2} Entity:{3}", cha.Id, cha.Name, cha.mapId, cha.Entity.String());
            Character character = new Character(cha);
            this.characters[cha.Id] = character;
            EntityManager.Instance.AddEntity(character);

            //哎呀这个事件虽然是在进入场景后触发,但是之前存起来的角色由专门的触发
            //me 这个是用于进入游戏后的其他角色 因为这个OnCharacterEnter还没有注册
            //me 第一次进入游戏不会触发这个事件  而是 StartCoroutine(InitGameObjects());
            if (this.OnCharacterEnter != null)
            {
                this.OnCharacterEnter(character);
            }
        }

        public void RemoveCharacter(int characterId)
        {
            Debug.LogFormat("RemoveCharacter:{0}", characterId);
            if (this.characters.ContainsKey(characterId))
            {
                EntityManager.Instance.RemoveEntity(this.characters[characterId].Info.Entity.Id);
                if (this.OnCharacterLeave != null)
                {
                    this.OnCharacterLeave(this.characters[characterId]);
                }
            }
            this.characters.Remove(characterId);
        }
    }
}