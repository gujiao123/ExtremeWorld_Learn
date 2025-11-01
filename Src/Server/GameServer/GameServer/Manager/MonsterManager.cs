using GameServer.Entities;
using GameServer.Models;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Managers
{
    class MonsterManager
    {
        private Map Map;//哪个地图的怪物管理器

        //管理一个地图的所有怪物
        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();

        public void Init(Map map)
        {
            this.Map = map;
        }
        /// <summary>
        /// 生成怪物
        /// </summary>
        /// <param name="spwnMonID">生成点ID</param>
        /// <param name="spawnLevel">对应关卡</param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        internal Monster Create(int spwnMonID, int spawnLevel, NVector3 position, NVector3 direction)
        {
            Monster monster = new Monster(spwnMonID, spawnLevel, position, direction);
            EntityManager.Instance.AddEntity(this.Map.ID, monster);
            monster.Info.Id = monster.entityId;
            //monster.Info.EntityId = monster.entityId;
            monster.Info.mapId = this.Map.ID;
            Monsters[monster.Id] = monster;

            this.Map.MonsterEnter(monster);
            return monster;
        }
    }
}
