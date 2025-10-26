using Common.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    class NpcManager : Singleton<NpcManager>
    {

        //  NPC事件委托 让别人注册
        public delegate void NPCEventHandler(NpcDefine npc);

        Dictionary<NpcFunction, NPCEventHandler> EventMap = new Dictionary<NpcFunction, NPCEventHandler>();


        public void RegisterNpcEvent(NpcFunction function, NPCEventHandler handler)
        {
            if (EventMap.ContainsKey(function))
            {
                EventMap[function] += handler;
            }
            else
            {
                EventMap[function] = handler;
            }
        }
        /// <summary>
        /// 获取NPC定义
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns></returns>
        public NpcDefine GetNpcDefine(int npcID)
        {
            NpcDefine npcDefine;
            npcDefine = DataManager.Instance.NPCs.TryGetValue(npcID, out npcDefine) ? npcDefine : null;
            return npcDefine;
        }

        /// <summary>
        /// 一个检查方法
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        public bool Interactive(int npcId)
        {
            if (DataManager.Instance.NPCs.ContainsKey(npcId))
            {
                var npc = DataManager.Instance.NPCs[npcId];
                return Interactive(npc);
            }
            return false;
        }

        /// <summary>
        /// 根据类型分配npc交互
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public bool Interactive(NpcDefine npc)
        {
            if (npc.Type == NpcType.Task)
            {
                return DoTaskInteractive(npc);
            }
            else if (npc.Type == NpcType.Functional)
            {
                return DoFunctionInteractive(npc);
            }
            return false;

        }

        private bool DoFunctionInteractive(NpcDefine npc)
        {
            Debug.Log("DoFunctionInteractive" + npc.Name);
            return true;
        }


        private bool DoTaskInteractive(NpcDefine npc)
        {
            if (npc.Type != NpcType.Task)
            {
                return false;
            }


            if (!EventMap.ContainsKey(npc.Function))
            {
                return false;
            }
            EventMap[npc.Function]?.Invoke(npc);
            return true;

        }






    }
}