using Common.Data;
using GameServer.Core;
using GameServer.Managers;
using SkillBridge.Message;

namespace GameServer.Entities
{
    class CharacterBase : Entity
    {
        /// <summary>
        /// 这里的ID完全是entityID 由内存创建 1+1+1的逐渐递增 控制所有实体的唯一标识
        /// </summary>
        public int Id
        {
            get
            {
                return this.entityId;
            }
        }
        public NCharacterInfo Info;
        /// <summary>
        /// 配置信息
        /// </summary>
        /// character专用 monster们没有
        public CharacterDefine Define;

        public CharacterBase(Vector3Int pos, Vector3Int dir) : base(pos, dir)
        {

        }

        public CharacterBase(CharacterType type, int tid, int level, Vector3Int pos, Vector3Int dir) :
           base(pos, dir)
        {
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.Tid = tid;//这里tid代表怪物的类型ID
            this.Info.Entity = this.EntityData;
            //怪物的配置数据 是在这里初始化的 与人物的地点不同
            this.Define = DataManager.Instance.Characters[this.Info.Tid];
            this.Info.Name = this.Define.Name;
        }
    }
}
