using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MangoBehaviorTree
{
    public enum NodeStatus
    {
        SUCCESS, FAILURE, RUNNING, ERROR,
    }

    public abstract class BaseNode<T> where T : IAgent
    {
        //handle first enter event
        protected abstract void Enter(Tick<T> tick);

        protected abstract void Open(Tick<T> tick);

        protected abstract NodeStatus Tick(Tick<T> tick);

        protected abstract void Close(Tick<T> tick);

        protected abstract void Exit(Tick<T> tick);

        public NodeStatus Execute(Tick<T> tick)
        {
            if (!isOpenDict.ContainsKey(tick.target.agentId))
            {
                isOpenDict.Add(tick.target.agentId, true);
                Open(tick);
            }
            else if (!isOpenDict[tick.target.agentId])
            {
                isOpenDict[tick.target.agentId] = true;
                Open(tick); 
            }

            Enter(tick);
            
            NodeStatus status = Tick(tick);

            Exit(tick);

            if (status != NodeStatus.RUNNING)
            { 
                Close(tick);
                isOpenDict[tick.target.agentId] = false;
            }

            return status;
        }

        public Dictionary<int, bool> isOpenDict { get; private set; }

        public BaseNode()
        {
            isOpenDict = new Dictionary<int, bool>();
        }

        //may define custom node execution data (per agent) as
        //Dictionary<int, customStruct> customNodeInfo 
    }

    internal static class UniqueIdGenerator
    {
        private static int _count = 0;
        public static int Next
        {
            get
            {
                if (_count == int.MaxValue)
                    throw new Exception("cannot assign id anymore");
                return _count++;
            }
        }
    }
}

