using Services;
using SkillBridge.Message;
using System.Collections.Generic;
using UnityEngine;
//可以申请的工会列表
public class UIGuildList : UIWindow
{
    public GameObject itemPrefab;
    public ListView listMain;
    public Transform itemRoot;
    public UIGuildInfo uiInfo;
    public UIGuildItem selectedItem;

    void Start()
    {
        this.listMain.onItemSelected += this.OnGuildMemberSelected;
        this.uiInfo.Info = null;
        //根据向服务器请求公会列表 来自动刷新
        GuildService.Instance.OnGuildListResult += UpdateGuildList;
        //请求一次就是刷新
        GuildService.Instance.SendGuildListRequest();
    }

    private void OnDestroy()
    {
        GuildService.Instance.OnGuildListResult -= UpdateGuildList;
    }

    void UpdateGuildList(List<NGuildInfo> guilds)
    {
        ClearList();
        InitItems(guilds);
    }

    public void OnGuildMemberSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UIGuildItem;
        this.uiInfo.Info = this.selectedItem.Info;
    }

    /// <summary>
    /// 初始化所有公会列表
    /// </summary>
    /// <param name="guilds"></param>
    void InitItems(List<NGuildInfo> guilds)
    {
        foreach (var item in guilds)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIGuildItem ui = go.GetComponent<UIGuildItem>();
            ui.SetGuildItemInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    void ClearList()
    {
        this.listMain.RemoveAll();
    }
    /// <summary>
    /// 绑定在按钮上
    /// </summary>
    public void OnClickJoin()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择一个公会");
            return;
        }
        MessageBox.Show(string.Format("申请加入公会：[{0}]吗？", this.selectedItem.Info.GuildName), "申请加入公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendGuildJoinRequest(this.selectedItem.Info.Id);
        };
    }
}

