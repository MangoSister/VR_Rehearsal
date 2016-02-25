using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MangoBehaviorTree
{
    public sealed class SequenceNode<T> : BaseNode<T> where T : IAgent
    {
        public List<BaseNode<T>> children { get; set; }

        public SequenceNode(List<BaseNode<T>> children) : base()
        { this.children = children; }

        public SequenceNode(params BaseNode<T>[] children) : base()
        { this.children = new List<BaseNode<T>>(children); }

        protected override void Enter(Tick<T> tick)
        {
            return;
        }

        protected override void Open(Tick<T> tick)
        {
            return;
        }

        protected override NodeStatus Tick(Tick<T> tick)
        {
            foreach (BaseNode<T> child in children)
            {
                NodeStatus status = child.Execute(tick);
                if (status != NodeStatus.SUCCESS)
                    return status;
            }
            return NodeStatus.SUCCESS;
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
