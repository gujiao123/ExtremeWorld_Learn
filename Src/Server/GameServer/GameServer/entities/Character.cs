using Common;
using GameServer.Managers;
using GameServer.Models;
using Network;
using SkillBridge.Message;

namespace GameServer.Entities
{
    class Character : CharacterBase, IPostResponser
    {

        public TCharacter Data;

        public ItemManager ItemManager;
        public StatusManager StatusManager;
        public QuestManager QuestManager;
        public FriendManager FriendManager;



        public Team Team;
        public double TeamUpdateTS;
        public Guild Guild;
        public double GuildUpdateTS;
        //!!多人数据 我们用时间戳
        //!!对于双人 一个状态量即可
        //这里只有Data 是数据库中拿出来的 其他都是根据Data初始化的Info内容用于网络传输
        public Character(CharacterType type, TCharacter cha) :
            base(new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ), new Core.Vector3Int(100, 0, 0))
        {
            //!!这里没有设置character.ID 而是info.ID
            this.Id = cha.ID;//数据库ID
            this.Data = cha;
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Id = cha.ID;//数据库ID 不是唯一标识
            this.Info.EntityId = this.entityId;//entityID才是唯一标识
            this.Info.ConfigId = cha.TID;//配置表ID

            this.Info.Name = cha.Name;
            this.Info.Level = 10;//cha.Level;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Entity = this.EntityData;
            this.Define = DataManager.Instance.Characters[this.Info.ConfigId];


            this.ItemManager = new ItemManager(this);
            this.ItemManager.GetItemInfos(this.Info.Items);
            this.Info.Bag = new NBagInfo();
            this.Info.Bag.Unlocked = this.Data.Bag.Unlocked;
            this.Info.Bag.Items = this.Data.Bag.Items;
            this.StatusManager = new StatusManager(this);


            this.Info.Equips = this.Data.Equips;

            this.QuestManager = new QuestManager(this);
            this.QuestManager.GetQuestInfos(this.Info.Quests);

            this.FriendManager = new FriendManager(this);
            this.FriendManager.GetFriendInfos(this.Info.Friends);


            this.Guild = GuildManager.Instance.GetGuild(this.Data.GuildId);
            if (this.Guild != null)
                this.Info.Guild = this.Guild.GuildInfo(this);

        }

        public long Gold
        {
            get { return this.Data.Gold; }
            set
            {
                if (this.Data.Gold == value)
                    return;
                this.StatusManager.AddGoldChange((int)(value - this.Data.Gold));
                this.Data.Gold = value;
            }
        }
        /// <summary>
        /// 作为中介者 处理角色相关的后续操作
        /// </summary>
        /// <param name="message"></param>
        public void PostProcess(NetMessageResponse message)
        {
            Log.InfoFormat("PostProcess > Character: characterID:{0}{1}", this.Id, this.Info.Name);

            this.FriendManager.PostProcess(message);

            if (this.Team != null)
            {
                Log.InfoFormat("PostProcess > Team: characterID:{0}:{1} {2}<{3}", this.Id, this.Info.Name, TeamUpdateTS, this.Team.timestamp);
                //me 就是保证队伍更新 所有人有通知 并且只通知一次
                //!!还不是很理解
                //就是角色自己的有关于队伍的信息要小于队伍的时间戳才更新
                if (this.TeamUpdateTS < this.Team.timestamp)
                {
                    TeamUpdateTS = Team.timestamp;
                    this.Team.PostProcess(message);
                }
            }

            if (this.Guild != null)
            {
                Log.InfoFormat("PostProcess > Guild: characterID:{0}:{1} {2}<{3}", this.Id, this.Info.Name, GuildUpdateTS, this.Guild.timestamp);
                if (this.Info.Guild != null)
                {
                    //更新上一次的公会信息时间戳
                    //Info.GUild 几乎每次都会更新
                    this.Info.Guild = this.Guild.GuildInfo(this);
                    //进入地图同步时间戳
                    if (message.mapCharacterEnter != null)
                        GuildUpdateTS = Guild.timestamp;
                }
                //??message.mapCharacterEnter == null什么意思
                //如果玩家在挂机，此时会长在公会里T了个人，this.Guild.timestamp 就会更新。
                if (GuildUpdateTS < this.Guild.timestamp && message.mapCharacterEnter == null)
                {
                    GuildUpdateTS = Guild.timestamp;
                    this.Guild.PostProcess(this, message);
                }
            }


            if (this.StatusManager.HasStatus)
                this.StatusManager.PostProcess(message);

        }

        /// <summary>
        /// 角色离开时调用
        /// </summary>
        public void Clear()
        {
            this.FriendManager.OfflineNotify();
        }

        public NCharacterInfo GetBasicInfo()
        {
            return new NCharacterInfo()
            {
                Id = this.Id,
                Name = this.Info.Name,
                Class = this.Info.Class,
                Level = this.Info.Level,
            };
        }
    }
}

