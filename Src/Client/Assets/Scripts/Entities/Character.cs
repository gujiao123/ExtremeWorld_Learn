using SkillBridge.Message;
using UnityEngine;

namespace Entities
{
    public class Character : Entity
    {
        public NCharacterInfo Info;
        public Common.Data.CharacterDefine Define;

        public int Id
        {
            get { return this.Info.Id; }
        }

        public string Name
        {
            get
            {
                if (this.Info.Type == CharacterType.Player)
                    return this.Info.Name;
                else
                    return this.Define.Name;
            }
        }
        //更新啦
        // public bool IsPlayer
        // {
        //     get
        //     {
        //         return this.Info.Type == CharacterType.Player;
        //     }
        // }
        public bool IsPlayer
        {
            //这个创建角色的ID 与当前User里面存储的当前使用角色ID对比
            get { return this.Info.Id == Models.User.Instance.CurrentCharacter.Id; }
        }


        public Character(NCharacterInfo info) : base(info.Entity)
        {
            this.Info = info;

            this.Define = DataManager.Instance.Characters[info.Tid];

        }
        /// <summary>
        /// 设置速度
        /// </summary>
        public void MoveForward()
        {
            // Debug.LogFormat("MoveForward");
            this.speed = this.Define.Speed;
        }

        public void MoveBack()
        {
            // Debug.LogFormat("MoveBack");
            this.speed = -this.Define.Speed;
        }

        public void Stop()
        {
            //  Debug.LogFormat("Stop");
            this.speed = 0;
        }
        public void SetDirection(Vector3Int direction)
        {
            //  Debug.LogFormat("SetDirection:{0}", direction);
            this.direction = direction;
        }

        public void SetPosition(Vector3Int position)
        {
            // Debug.LogFormat("SetPosition:{0}", position);
            this.position = position;
        }
    }
}
