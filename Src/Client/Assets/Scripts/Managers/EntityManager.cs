
using Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// 由EntityController实现该接口 用于接收实体的增删改查等事件通知
    /// </summary>
    interface IEntityNotify
    {
        void OnEntityRemoved();
        void OnEntityChanged(Entity entity);
        void OnEntityEvent(EntityEvent @event);
    }

    class EntityManager : Singleton<EntityManager>, IDisposable
    {
        /// <summary>
        /// entityID
        /// </summary>
        Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        Dictionary<int, IEntityNotify> notifies = new Dictionary<int, IEntityNotify>();

        public void RegisterEntityNotify(int entityId, IEntityNotify notify)
        {
            this.notifies[entityId] = notify;
        }
        public void AddEntity(Entity entity)
        {
            this.entities[entity.entityId] = entity;
        }

        public void RemoveEntity(int entityId)
        {
            if (this.entities.ContainsKey(entityId))
            {
                this.entities.Remove(entityId);
            }
            if (this.notifies.ContainsKey(entityId))
            {
                this.notifies[entityId].OnEntityRemoved();
                this.notifies.Remove(entityId);
            }
        }
        /// <summary>
        /// 实体同步处理
        /// </summary>
        /// <param name="data"></param>
        internal void OnEntitySync(NEntitySync data)
        {
            Entity entity = null;
            //G 同时检查存在和取值
            //得到管理器中的实体
            //根据服务器发过来的实体ID查找本地是否有
            this.entities.TryGetValue(data.Id, out entity);
            Debug.LogFormat("OnEntitySync: Id:{0} Entity:{1}", data.Id, data.Entity != null ? data.Entity.String() : "null");
            if (entity != null)
            {
                if (data.Entity != null)
                {
                    entity.EntityData = data.Entity;
                }
                if (notifies.ContainsKey(data.Id))
                {
                    //通知相关对象实体更新了
                    notifies[data.Id].OnEntityChanged(entity);
                    notifies[data.Id].OnEntityEvent(data.Event);

                }
            }
        }

        public void Dispose()
        {
        }
    }
}