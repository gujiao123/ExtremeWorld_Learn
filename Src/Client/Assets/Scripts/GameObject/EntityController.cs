using Entities;
using Services;
using SkillBridge.Message;
using UnityEngine;



//每个物体都有

public class EntityController : MonoBehaviour, IEntityNotify
{

    public Animator anim;
    public Rigidbody rb;
    private AnimatorStateInfo currentBaseState;

    public Entity entity;

    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 direction;
    Quaternion rotation;

    public UnityEngine.Vector3 lastPosition;
    Quaternion lastRotation;

    public float speed;
    public float animSpeed = 1.5f;
    public float jumpPower = 3.0f;

    public bool isPlayer = false;

    // Use this for initialization
    void Start()
    {
        if (entity != null)
        {
            this.UpdateTransform();
        }
        if (!this.isPlayer)
        {
            rb.useGravity = false;
        }
    }

    /// <summary>
    /// 将服务器的坐标转换到Unity世界坐标
    /// 也就是根据服务器来移动
    /// </summary>
    void UpdateTransform()
    {
        this.position = GameObjectTool.LogicToWorld(entity.position);
        this.direction = GameObjectTool.LogicToWorld(entity.direction);

        this.rb.MovePosition(this.position);
        this.transform.forward = this.direction;
        this.lastPosition = this.position;
        this.lastRotation = this.rotation;
    }

    void OnDestroy()
    {
        if (entity != null)
            Debug.LogFormat("{0} OnDestroy :ID:{1} POS:{2} DIR:{3} SPD:{4} ", this.name, entity.entityId, entity.position, entity.direction, entity.speed);

        //!! 统一都在这里销毁 所有角色或者UI
        if (UIWorldElementManager.Instance != null)
        {
            UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.entity == null)
            return;
        //是玩家可以动
        this.entity.OnUpdate(Time.fixedDeltaTime);
        //不是玩家 只能根据服务器数据来移动
        if (!this.isPlayer)
        {
            this.UpdateTransform();
        }
    }
    /// <summary>
    /// 根据状态播放动画
    /// </summary>
    /// <param name="entityEvent"></param>
    public void OnEntityEvent(EntityEvent entityEvent)
    {
        switch (entityEvent)
        {
            case EntityEvent.Idle:
                anim.SetBool("Move", false);
                anim.SetTrigger("Idle");
                break;
            case EntityEvent.MoveFwd:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.MoveBack:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.Jump:
                anim.SetTrigger("Jump");
                break;
        }
    }

    public void OnEntityRemoved()
    {
        throw new System.NotImplementedException();
    }

    public void OnEntityChanged(Entity entity)
    {
        //目前先简单打印变化
        Debug.LogFormat("Entity Changed : ID:{0} POS:{1} DIR:{2} SPD:{3} ", entity.entityId, entity.position, entity.direction, entity.speed);
    }

}