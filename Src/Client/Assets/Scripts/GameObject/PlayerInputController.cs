using Entities;
using Services;
using SkillBridge.Message;
using UnityEngine;
//只用于玩家输入控制 其他人都用entitycontroller 啊啊
public class PlayerInputController : MonoBehaviour
{

    public Rigidbody rb;
    SkillBridge.Message.CharacterState state;

    public Character character;

    public float rotateSpeed = 2.0f;

    public float turnAngle = 10;

    public int speed;

    public EntityController entityController;


    // Use this for initialization
    void Start()
    {
        state = SkillBridge.Message.CharacterState.Idle;
        //这里完全没有用啊 因为character是外部赋值的
        if (this.character == null)
        {
            //这里的load有用吗 应该是没有用的
            DataManager.Instance.Load();
            NCharacterInfo cinfo = new NCharacterInfo();
            cinfo.Id = 1;
            cinfo.Name = "Test";
            cinfo.ConfigId = 1;//这个ID没有用 隔壁服务器没有用数据库Id的
            cinfo.Entity = new NEntity();
            cinfo.Entity.Position = new NVector3();
            cinfo.Entity.Direction = new NVector3();
            cinfo.Entity.Direction.X = 0;
            cinfo.Entity.Direction.Y = 100;
            cinfo.Entity.Direction.Z = 0;
            this.character = new Character(cinfo);

            if (entityController != null) entityController.entity = this.character;
        }

    }

    void FixedUpdate()
    {
        if (character == null)//----
            return;

        float v = Input.GetAxis("Vertical");
        if (v > 0.01)
        {
            //正的输入
            if (state != SkillBridge.Message.CharacterState.Move)
            {
                //改变状态
                state = SkillBridge.Message.CharacterState.Move;
                //设置速度
                this.character.MoveForward();
                //根据状态进行动画
                this.SendEntityEvent(EntityEvent.MoveFwd);
            }
            //根据方向和速度来移动
            this.rb.velocity = this.rb.velocity.y * Vector3.up + GameObjectTool.LogicToWorld(character.direction) * (this.character.speed + 9.81f) / 100f;
        }
        else if (v < -0.01)
        {
            //负的输入
            if (state != SkillBridge.Message.CharacterState.Move)
            {
                state = SkillBridge.Message.CharacterState.Move;
                this.character.MoveBack();
                this.SendEntityEvent(EntityEvent.MoveBack);
            }
            this.rb.velocity = this.rb.velocity.y * Vector3.up + GameObjectTool.LogicToWorld(character.direction) * (this.character.speed + 9.81f) / 100f;
        }
        else
        {
            if (state != SkillBridge.Message.CharacterState.Idle)
            {
                state = SkillBridge.Message.CharacterState.Idle;
                this.rb.velocity = Vector3.zero;
                this.character.Stop();
                this.SendEntityEvent(EntityEvent.Idle);
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            this.SendEntityEvent(EntityEvent.Jump);
        }

        float h = Input.GetAxis("Horizontal");
        //TODO 只能控制水平方向的摄像机移动 不好 以后改改
        if (h < -0.1 || h > 0.1)
        {
            this.transform.Rotate(0, h * rotateSpeed, 0);
            Vector3 dir = GameObjectTool.LogicToWorld(character.direction);
            Quaternion rot = new Quaternion();
            rot.SetFromToRotation(dir, this.transform.forward);

            if (rot.eulerAngles.y > this.turnAngle && rot.eulerAngles.y < (360 - this.turnAngle))
            {

                //这里是转向
                character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));
                rb.transform.forward = this.transform.forward;
                this.SendEntityEvent(EntityEvent.None);
            }

        }
        //Debug.LogFormat("velocity {0}", this.rb.velocity.magnitude);
    }
    Vector3 lastPos;
    float lastSync = 0;
    private void LateUpdate()
    {


        Vector3 offset = this.rb.transform.position - lastPos;
        this.speed = (int)(offset.magnitude * 100f / Time.deltaTime);
        //Debug.LogFormat("LateUpdate velocity {0} : {1}", this.rb.velocity.magnitude, this.speed);
        this.lastPos = this.rb.transform.position;

        Vector3Int goLogicPos = GameObjectTool.WorldToLogic(this.rb.transform.position);
        float logicOffset = (goLogicPos - this.character.position).magnitude;
        //逻辑坐标和现实坐标的差值
        if (logicOffset > 100)
        {
            this.character.SetPosition(GameObjectTool.WorldToLogic(this.rb.transform.position));
            this.SendEntityEvent(EntityEvent.None);
        }
        //把刚体位置赋值给transform 位置同步
        //????
        this.transform.position = this.rb.transform.position;

    }
    /// <summary>
    /// 发送实体事件
    /// </summary>
    /// <param name="entityEvent"></param>
    public void SendEntityEvent(EntityEvent entityEvent)
    {
        //1.通知实体控制器变更状态
        if (entityController != null)
            entityController.OnEntityEvent(entityEvent);

        //2.发送信息给服务器
        MapService.Instance.SendMapEntitySync(entityEvent, this.character.EntityData);
    }
}
