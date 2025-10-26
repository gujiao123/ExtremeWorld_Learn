using SkillBridge.Message;



//这个定义 是人为的注意,不是通过程序生成的
//!!比如 我们角色要新增一个属性  
//!! 策划就在excel中填写额外的属性 ,用程序转化为json 然后我们程序员为了读取这个属性,就在角色类中新增这个属性
//!! 生成dll文件后 还要拷贝一份到客户端 卧槽
namespace Common.Data
{
    public enum NpcType
    {
        None = 0,
        Functional,
        Task
    }
    public enum NpcFunction
    {
        None,
        InvokeShop,
        InvokeInsrance
    }
    public class NpcDefine
    {
        //与Data配置表一样
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public NpcType Type { get; set; }
        public NpcFunction Function { get; set; }
        public int Param { get; set; }
        public NVector3 Position { get; set; }


    }
}
