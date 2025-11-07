using Common.Data;
using GameServer.Core;
using GameServer.Managers;
using SkillBridge.Message;

namespace GameServer.Entities
{
    class CharacterBase : Entity
    {
        /// <summary>
        /// 数据库的ID
        /// </summary>
        public int Id
        {
            get;
            set;
        }
        public NCharacterInfo Info;
        /// <summary>
        /// 配置信息
        /// </summary>
        /// character专用 monster们没有
        public CharacterDefine Define;
        public string Name
        {
            get { return this.Info.Name; }
        }

        public CharacterBase(Vector3Int pos, Vector3Int dir) : base(pos, dir)
        {

        }

        public CharacterBase(CharacterType type, int ConfigId, int level, Vector3Int pos, Vector3Int dir) :
           base(pos, dir)
        {
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.ConfigId = ConfigId;//这里tid代表怪物的类型ID
            this.Info.Entity = this.EntityData;
            this.Info.EntityId = this.entityId;
            //怪物的配置数据 是在这里初始化的 与人物的地点不同
            this.Define = DataManager.Instance.Characters[this.Info.ConfigId];
            this.Info.Name = this.Define.Name;
        }
    }
}
