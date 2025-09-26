using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//!!目前这个处于没有用的状态 因为服务器那边删除了firstRequest 这个消息的服务
public class Login : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Network.NetClient.Instance.Init("127.0.0.1", 8000);
        Network.NetClient.Instance.Connect();


        SkillBridge.Message.NetMessage msg = new SkillBridge.Message.NetMessage();
        msg.Request = new SkillBridge.Message.NetMessageRequest();


        msg.Request.firstRequest = new SkillBridge.Message.FirstTestRequest();
        msg.Request.firstRequest.Helloworld = "Hello World";

        Network.NetClient.Instance.SendMessage(msg);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
