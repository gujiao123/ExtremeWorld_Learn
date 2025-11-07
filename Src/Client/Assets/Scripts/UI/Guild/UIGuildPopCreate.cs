using Services;
using UnityEngine.UI;


//创建工会
public class UIGuildPopCreate : UIWindow
{
    public InputField inputName;
    public InputField inputNotice;

    private void Start()
    {
        GuildService.Instance.OnGuildCreateResult += OnGuildCreated;
    }

    private void OnDestroy()
    {
        GuildService.Instance.OnGuildCreateResult = null;
    }
    /// <summary>
    /// 重写一下 不用关闭窗口 直接发协议
    /// </summary>
    public override void OnYesClick()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            MessageBox.Show("请输入公会名称", "错误", MessageBoxType.Error);
            return;
        }

        if (inputName.text.Length < 4 || inputName.text.Length > 10)
        {
            MessageBox.Show("公会名称长度必须在4-10个字符之间", "错误", MessageBoxType.Error);
            return;
        }

        if (string.IsNullOrEmpty(inputNotice.text))
        {
            MessageBox.Show("请输入公会宣言", "错误", MessageBoxType.Error);
            return;
        }

        if ((inputNotice.text.Length < 3 || inputNotice.text.Length > 50))
        {
            MessageBox.Show("公会宣言需为3-50个字符", "错误", MessageBoxType.Error);
        }

        GuildService.Instance.SendGuildCreate(inputName.text, inputNotice.text);
    }
    /// <summary>
    /// 等服务器返回创建结果 才关闭掉
    /// </summary>
    /// <param name="result"></param>
    void OnGuildCreated(bool result)
    {
        if (result)
            this.Close(WindowResult.Yes);
    }
}

