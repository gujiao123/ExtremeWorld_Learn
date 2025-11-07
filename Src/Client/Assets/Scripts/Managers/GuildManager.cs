using Models;
using SkillBridge.Message;

public class GuildManager : Singleton<GuildManager>
{
    public NGuildInfo guildInfo;

    public NGuildMemberInfo myMemberInfo;

    public bool HasGuild
    {
        get { return (this.guildInfo != null); }
    }
    /// <summary>
    /// 每个人都初始化公会信息
    /// </summary>
    /// <param name="guild"></param>
    public void Init(NGuildInfo guild)
    {
        //初始化公会信息
        this.guildInfo = guild;

        if (guild == null)
        {
            myMemberInfo = null;
            return;
        }
        foreach (var mem in guild.Members)
        {
            //得到自己的成员信息
            if (mem.characterId == User.Instance.CurrentCharacterInfo.Id)
            {
                myMemberInfo = mem;
                break;
            }
        }
    }

    /// <summary>
    /// 一切的入口 显示公会界面
    /// </summary>
    /// 根据是否有工会显示不同的界面
    public void ShowGuild()
    {
        if (this.HasGuild)
            UIManager.Instance.Show<UIGuild>();
        else
        {

            var win = UIManager.Instance.Show<UIGuildPopNoGuild>();
            win.OnClose += PopNoGuild_OnClose;
        }
    }



    /// <summary>
    /// 处理没有公会时的弹出窗口选择
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="result"></param>
    private void PopNoGuild_OnClose(UIWindow sender, UIWindow.WindowResult result)
    {
        if (result == UIWindow.WindowResult.Yes)
            UIManager.Instance.Show<UIGuildPopCreate>();
        else if (result == UIWindow.WindowResult.No)
            UIManager.Instance.Show<UIGuildList>();
    }
}

