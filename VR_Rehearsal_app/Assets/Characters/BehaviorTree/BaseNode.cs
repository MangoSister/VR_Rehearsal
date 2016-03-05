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

        public abstract void Close(Tick<T> tick);

        protected abstract void Exit(Tick<T> tick);

        public NodeStatus Execute(Tick<T> tick)
        {
            if (!tick.tree._openNodes[tick.target.agentId].Contains(this))
                Open(tick);

            tick.traverseNodes.Add(this);

            Enter(tick);
            
            NodeStatus status = Tick(tick);

            Exit(tick);

            if (status != NodeStatus.RUNNING)
            { 
                Close(tick);
                tick.traverseNodes.Remove(this);
            }


            return status;
        }

        public BaseNode() { }

        //may define custom node execution data (per agent) as
        //Dictionary<int, customStruct> customNodeInfo 
    }
}

