using UnityEngine;
using System;
using System.Collections.Generic;


namespace MangoBehaviorTree
{
    public sealed class WaitNode<T> : BaseNode<T> where T : IAgent
    {
        private struct NodeInfo
        {
            public float timer;
            public NodeInfo(float time) { timer = time; }
        }

        private Dictionary<int, NodeInfo> _agentExecInfo;
        private bool _initSucc;
        public float waitTime { get; set; }

        public WaitNode(float waitTime, bool initSucc = false) : base()
        {
            this.waitTime = waitTime;
            _initSucc = initSucc;
            _agentExecInfo = new Dictionary<int, NodeInfo>();
        }

        protected override void Enter(Tick<T> tick)
        {
            return;
        }

        protected override void Open(Tick<T> tick)
        {
            //_timer = Time.time;
            //if (_initSucc)
            //    _timer -= waitTime;

            if (!_agentExecInfo.ContainsKey(tick.target.agentId))
            {
                NodeInfo info = new NodeInfo(_initSucc ? Time.time - waitTime : Time.time);
                _agentExecInfo.Add(tick.target.agentId, info);

            }
            else
            {
                NodeInfo info = _agentExecInfo[tick.target.agentId];
                info.timer= Time.time;
                if (_initSucc)
                    info.timer -= waitTime;
                _agentExecInfo[tick.target.agentId] = info;
            }
            return;
        }

        protected override NodeStatus Tick(Tick<T> tick)
        {
            if (Time.time - _agentExecInfo[tick.target.agentId].timer >= waitTime)
                return NodeStatus.SUCCESS;
            else return NodeStatus.RUNNING;
        }

        public override void Close(Tick<T> tick)
        {
            NodeInfo info = _agentExecInfo[tick.target.agentId];
            info.timer = 0f;
            _agentExecInfo[tick.target.agentId] = info;
        }

        protected override void Exit(Tick<T> tick)
        {
            return;
        }
    }
}

