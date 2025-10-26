using Common.Data;
using Entities;
using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{

    //测试 npcmanager与其他系统的交互
    class TestManager : Singleton<TestManager>
    {

        public void Init()
        {
            NpcManager.Instance.RegisterNpcEvent(NpcFunction.InvokeShop, DoTaskInteractive);


            NpcManager.Instance.RegisterNpcEvent(NpcFunction.InvokeInsrance, OnNpcInvokeInsrance);
        }

        public void DoTaskInteractive(NpcDefine npc)
        {
            Debug.Log("TestManager Test");
        }

        public void OnNpcInvokeInsrance(NpcDefine npc)
        {
            Debug.Log("TestManager OnNpcInvokeInsrance");
        }





    }
}