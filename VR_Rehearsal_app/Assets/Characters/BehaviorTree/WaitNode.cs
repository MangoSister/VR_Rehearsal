using UnityEngine;
using System;
using System.Collections;


namespace MangoBehaviorTree
{
    public sealed class WaitNode<T> : BaseNode<T> where T : IAgent
    {
        private float _timer;
        public float waitTime { get; set; }

        public WaitNode(float waitTime) : base()
        {
            this.waitTime = waitTime;
        }

        protected override void Enter(Tick<T> tick)
        {
            return;
        }

        protected override void Open(Tick<T> tick)
        {
            _timer = 0f;
        }

        protected override NodeStatus Tick(Tick<T> tick)
        {
            _timer += Time.deltaTime;
            if (_timer >= waitTime)
                return NodeStatus.SUCCESS;
            else return NodeStatus.RUNNING;
        }

        public override void Close(Tick<T> tick)
        {
            _timer = 0f;
        }

        protected override void Exit(Tick<T> tick)
        {
            return;
        }
    }
}

