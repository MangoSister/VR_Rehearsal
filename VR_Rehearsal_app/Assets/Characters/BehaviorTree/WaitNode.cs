﻿using UnityEngine;
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
        public float waitTime { get; set; }

        public WaitNode(float waitTime) : base()
        {
            this.waitTime = waitTime;
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
                NodeInfo info = new NodeInfo(Time.time - (tick.target as Audience).simInternalOffset);
                _agentExecInfo.Add(tick.target.agentId, info);

            }
            else
            {
                NodeInfo info = _agentExecInfo[tick.target.agentId];
                info.timer= Time.time;
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

