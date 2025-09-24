using Common;
using Network;
using SkillBridge.Message;

namespace GameServer.Services
{
    internal class HelloWorldServer : Singleton<HelloWorldServer>
    {
        public void Init()
        {


        }
        public void Start()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FirstTestRequest>(this.OnFirstTestRequest);
        }
        public void Stop()
        {

        }
        void OnFirstTestRequest(NetConnection<NetSession> sender, FirstTestRequest request)
        {
            Log.InfoFormat("OnFirstTestRequest: {0}", request.Helloworld);
        }
    }
}
