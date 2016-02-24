using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MangoBehaviorTree
{
    public abstract class RandomSelectorNode<T> : BaseNode<T> where T : IAgent
    {
        protected struct NodeInfo
        {
            public bool needSample;
            public int sampleCount;
            public int lastIdx;
            public float[] weights;
            public float[] cumulativeWeights
            {
                get
                {
                    float[] output = new float[weights.Length];
                    for (int i = 0; i < weights.Length; ++i)
                    {
                        output[i] = 0.0f;
                        for (int j = 0; j <= i; ++j)
                            output[i] += weights[j];
                    }
                    return output;
                }
            }
            public NodeInfo(int num)
            {
                needSample = true;
                lastIdx = 0;
                sampleCount = 0;

                weights = Enumerable.Repeat<float>(1f / (float)num, num).ToArray();
            }

            public void Init()
            {
                needSample = true;
                lastIdx = 0;
                sampleCount = 0;
            }
        }

        protected Dictionary<int, NodeInfo> _agentExecInfo;
        private int _sampleTimes;

        public List<BaseNode<T>> children { get; set; }

        public int childrenCount
        { get { return children.Count; } }

        public RandomSelectorNode(int sampleTimes, List<BaseNode<T>> children) : base()
        {
            _sampleTimes = sampleTimes;
            this.children = children;
            _agentExecInfo = new Dictionary<int, NodeInfo>();
           
        }

        public RandomSelectorNode(int sampleTime, params BaseNode<T>[] children) : base()
        {
            _sampleTimes = sampleTime;
            this.children = new List<BaseNode<T>>(children);
            _agentExecInfo = new Dictionary<int, NodeInfo>();
        }

        protected abstract void UpdateWeights(Tick<T> tick);

        protected override void Enter(Tick<T> tick)
        {
            UpdateWeights(tick);
        }

        protected override void Open(Tick<T> tick)
        {
            if (!_agentExecInfo.ContainsKey(tick.target.agentId))
            {
                _agentExecInfo.Add(tick.target.agentId, new NodeInfo(childrenCount));
            }
            else
                _agentExecInfo[tick.target.agentId].Init();
            return;
        }

        protected override NodeStatus Tick(Tick<T> tick)
        {
            NodeInfo info = _agentExecInfo[tick.target.agentId];
            while (info.sampleCount < _sampleTimes)
            {
                if (info.needSample)
                {
                    float sample = Random.value;
                    float[] cumulativeWeights = info.cumulativeWeights;
                    for (int j = 0; j < childrenCount; ++j)
                    {
                        if (cumulativeWeights[j] >= sample)
                        {
                            info.lastIdx = j;
                            info.sampleCount++;
                            break;
                        }
                    }
                }

                NodeStatus status = children[info.lastIdx].Execute(tick);
                if (status != NodeStatus.FAILURE)
                    return status;
                else info.needSample = true;
            }

            return NodeStatus.FAILURE;
        }

        public override void Close(Tick<T> tick)
        {
            return;
        }

        protected override void Exit(Tick<T> tick)
        {
            return;
        }
    }
}

