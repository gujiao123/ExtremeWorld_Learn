using SkillBridge.Message;



//这个定义 是人为的注意,不是通过程序生成的
//!!比如 我们角色要新增一个属性  
//!! 策划就在excel中填写额外的属性 ,用程序转化为json 然后我们程序员为了读取这个属性,就在角色类中新增这个属性
//!! 生成dll文件后 还要拷贝一份到客户端 卧槽

namespace Common.Data
{
    public class CharacterDefine
    {
        public int TID { get; set; }
        public string Name { get; set; }
        public CharacterClass Class { get; set; }
        public string Resource { get; set; }
        public string Description { get; set; }
        public float Height { get; set; }

        //基本属性
        public int Speed { get; set; }

        /// <summary>
        /// 生命
        /// </summary>
        public float MaxHP { get; set; }

        /// <summary>
        /// 法力
        /// </summary>
        public float MaxMP { get; set; }

        /// <summary>
        /// 力量成长
        /// </summary>
        public float GrowthSTR { get; set; }

        /// <summary>
        /// 智力成长
        /// </summary>
        public float GrowthINT { get; set; }

        /// <summary>
        /// 敏捷成长
        /// </summary>
        public float GrouthDEX { get; set; }

        /// <summary>
        /// 力量
        /// </summary>
        public float STR { get; set; }

        /// <summary>
        /// 智力
        /// </summary>
        public float INT { get; set; }

        /// <summary>
        /// 敏捷
        /// </summary>
        public float DEX { get; set; }

        /// <summary>
        /// 物理攻击
        /// </summary>
        public float AD { get; set; }

        /// <summary>
        /// 法术攻击
        /// </summary>
        public float AP { get; set; }

        /// <summary>
        /// 物理防御
        /// </summary>
        public float DEF { get; set; }

        /// <summary>
        /// 法术防御
        /// </summary>
        public float MDEF { get; set; }

        /// <summary>
        /// 攻击速度
        /// </summary>
        public float SPD { get; set; }

        /// <summary>
        /// 暴击概率
        /// </summary>
        public float CRI { get; set; }

        public string AI { get; set; }
    }
}

