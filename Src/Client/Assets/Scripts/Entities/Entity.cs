using SkillBridge.Message;
using UnityEngine;

namespace Entities
{
    public class Entity
    {
        /// <summary>
        /// 一个由内存生成的随机唯一ID 完全不一样
        /// 别管哪里生成的了服务器传过来的
        /// </summary>
        public int entityId;


        //一份放本地进行实时运算
        //me 这个就由玩家控制了
        //me 当网络传过来更改NEntity的set时候这个值就会更新 然后在playercontroller里面使用 
        public Vector3Int position;
        public Vector3Int direction;
        public int speed;

        //存放一份网络过来的数据
        //me 就是自己动了发送消息给服务器 服务器再广播给其他人 自己怎么动的
        private NEntity entityData;
        public NEntity EntityData
        {
            get
            {
                //每次获取网络数据前都更新一下数据
                //???不知道为什么要每次获取都更新
                //就是发送的时候要确保数据是最新的 要把自己的数据更新后发送到网络
                UpdateEntityData();
                return entityData;
            }
            set
            {
                entityData = value;
                this.SetEntityData(value);
            }
        }

        public Entity(NEntity entity)
        {
            this.entityId = entity.Id;
            this.entityData = entity;
            this.SetEntityData(entity);
        }
        /// <summary>
        /// 更新实体数据 和网络中的数据同步
        /// </summary>
        /// <param name="delta"></param>
        public virtual void OnUpdate(float delta)
        {
            if (this.speed != 0)
            {
                Vector3 dir = this.direction;
                this.position += Vector3Int.RoundToInt(dir * speed * delta / 100f);
            }
            entityData.Position.FromVector3Int(this.position);
            entityData.Direction.FromVector3Int(this.direction);
            entityData.Speed = this.speed;
        }

        public void SetEntityData(NEntity entity)
        {
            //me FromNVector3 这个函数就是我们写的common.dll里面的扩展函数
            this.position = this.position.FromNVector3(entity.Position);
            this.direction = this.direction.FromNVector3(entity.Direction);
            this.speed = entity.Speed;
        }

        public void UpdateEntityData()
        {
            entityData.Position.FromVector3Int(this.position);
            entityData.Direction.FromVector3Int(this.direction);
            entityData.Speed = this.speed;
        }


    }
}
