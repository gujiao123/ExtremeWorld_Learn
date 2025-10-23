using SkillBridge.Message;

namespace GameServer.Entities
{
    class Character : CharacterBase
    {

        public TCharacter Data;

        //这里只有Data 是数据库中拿出来的 其他都是根据Data初始化的Info内容用于网络传输
        public Character(CharacterType type, TCharacter cha) :
            base(new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ), new Core.Vector3Int(100, 0, 0))
        {
            //!!这里没有设置character.ID 而是info.ID
            this.Data = cha;
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Id = cha.ID;//这里ID是初始化成功了的啊
            this.Info.Name = cha.Name;
            this.Info.Level = 1;//cha.Level;
            this.Info.Tid = cha.TID;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Entity = this.EntityData;
            //this.Define = DataManager.Instance.Characters[this.Info.Tid];
        }
    }
}
