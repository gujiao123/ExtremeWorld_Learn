using Services;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    public InputField username;
    public InputField password;
    public Button buttonLogin;
    public Button buttonRegister;

    // Start is called before the first frame update
    void Start()
    {
        UserService.Instance.OnLogin += OnLogin;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnClickLogin()
    {
        if (string.IsNullOrEmpty(this.username.text))
        {
            MessageBox.Show("请输入用户名");
            return;
        }
        if (string.IsNullOrEmpty(this.password.text))
        {
            MessageBox.Show("请输入密码");
            return;
        }
        //UI层直接访问UIService层
        UserService.Instance.SendLogin(this.username.text, this.password.text);
    }
    /// <summary>
    /// 登录结果回调
    /// </summary>
    /// <param name="result"></param>
    /// <param name="message"></param>
    /// 登录结果回调
    /// !!这里就是订阅服务层 来实现UI层的更新
    /// !!服务层不能直接调用UI层的函数 ,所以UI层订阅服务层的事件
    public void OnLogin(Result result, string message)
    {
        if (result == Result.Success)
        {
            //进入下一个场景
            UnityEngine.SceneManagement.SceneManager.LoadScene("SelectCharacter");
        }
    }
}
