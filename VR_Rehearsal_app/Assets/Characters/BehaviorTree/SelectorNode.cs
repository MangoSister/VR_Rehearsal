using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MangoBehaviorTree
{
    public sealed class SelectorNode<T> : BaseNode<T> where T : IAgent
    {
        public List<BaseNode<T>> children { get; set; }

        public SelectorNode(List<BaseNode<T>> children) : base()
        { this.children = children; }

        public SelectorNode(params BaseNode<T>[] children) : base()
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
                if (status != NodeStatus.FAILURE)
                    return status;
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
