// 推荐的 MonoSingleton 实现
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true;
    static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();
            }
            return instance;
        }
    }

    void Awake()
    {
        Debug.LogWarningFormat("{0} [{1}] Awake", typeof(T), this.GetInstanceID());

        if (global)
        {
            // 如果已经存在实例且不是当前对象，直接销毁
            if (instance != null && instance != this.gameObject.GetComponent<T>())
            {
                Debug.LogWarningFormat("销毁重复的 {0} 实例 [{1}]", typeof(T), this.GetInstanceID());
                Destroy(this.gameObject);
                return;
            }

            // 设置为持久化并保存实例引用
            DontDestroyOnLoad(this.gameObject);
            instance = this.gameObject.GetComponent<T>();
        }

        // 只有成功成为单例的实例才执行 OnStart
        this.OnStart();
    }

    protected virtual void OnStart()
    {
    }
}

// 推荐的 GameObjectManager 实现
public class GameObjectManager : MonoSingleton<GameObjectManager>
{
    Dictionary<int, GameObject> Characters = new Dictionary<int, GameObject>();
    private bool eventsRegistered = false; // 标记是否已注册事件

    protected override void OnStart()
    {
        StartCoroutine(InitGameObjects());

        // 确保只注册一次事件
        if (!eventsRegistered)
        {
            CharacterManager.Instance.OnCharacterEnter += OnCharacterEnter;
            CharacterManager.Instance.OnCharacterLeave += OnCharacterLeave;
            eventsRegistered = true;
            Debug.Log("GameObjectManager 事件注册成功");
        }
    }

    private void OnDestroy()
    {
        // 只有当前实例是真正的单例实例时才清理事件
        if (Instance == this && eventsRegistered)
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.OnCharacterEnter -= OnCharacterEnter;
                CharacterManager.Instance.OnCharacterLeave -= OnCharacterLeave;
                eventsRegistered = false;
                Debug.Log("GameObjectManager 事件清理成功");
            }
        }
    }

    // ... 其他方法保持不变
}