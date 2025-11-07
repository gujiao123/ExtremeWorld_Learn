using Data;
using SkillBridge.Message;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildInfo : MonoBehaviour
{
    public Text guildName;
    public Text guildID;
    public Text leader;
    public Text notice;
    public Text memberNumber;
    private NGuildInfo info;//用来展示的数据
    //这就是数据绑定的核心思想：视图（UI）和数据（Model）被连接起来，当数据变化时，视图自动更新。
    public NGuildInfo Info
    {
        get { return this.info; }
        set { this.info = value; this.UpdateUI(); }//被设置就更新UI
    }
    void UpdateUI()
    {
        if (this.info == null)
        {
            this.guildName.text = "无";
            this.guildID.text = "ID: 0";
            this.leader.text = "会长: 无";
            this.notice.text = "公告: 无";
            this.memberNumber.text = string.Format("成员: 0/{0}", GameDefine.GuildMaxMemberCount);
        }
        else
        {
            this.guildName.text = this.Info.GuildName;
            this.guildID.text = "ID: " + this.Info.Id;
            this.leader.text = "会长: " + this.Info.leaderName;
            this.notice.text = this.Info.Notice;
            this.memberNumber.text = string.Format("成员: {0}/{1}", this.info.memberCount, GameDefine.GuildMaxMemberCount);
        }
    }
}

