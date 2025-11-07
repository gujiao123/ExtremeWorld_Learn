using Common;
using GameServer.Entities;
using System.Collections.Generic;

namespace GameServer.Managers
{
    class EntityManager : Singleton<EntityManager>
    {
        //递增唯一标识内存自带序号
        private int idx = 0;
        //一章地图
        public List<Entity> AllEntities = new List<Entity>();
        //所有地图对应实体
        public Dictionary<int, List<Entity>> mapEntities = new Dictionary<int, List<Entity>>();
        /// <summary>
        /// 我靠这里生成内存ID然后添加进实体管理器
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="entity"></param>
        /// 这里才是character.ID的生成点唯一标识我去
        public void AddEntity(int mapId, Entity entity)
        {
            //管理到实体对象中
            AllEntities.Add(entity);
            entity.EntityData.Id = ++this.idx;


            List<Entity> entities = null;

            if (!mapEntities.TryGetValue(mapId, out entities))
            {
                entities = new List<Entity>();
                mapEntities[mapId] = entities;
            }
            entities.Add(entity);
        }

        public void RemoveEntity(int mapId, Entity entity)
        {
            this.AllEntities.Remove(entity);
            this.mapEntities[mapId].Remove(entity);
        }
    }
}