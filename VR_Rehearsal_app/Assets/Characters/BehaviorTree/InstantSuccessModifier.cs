using UnityEngine;
using System.Collections;

namespace MangoBehaviorTree
{
    public class InstantSuccessModifier<T> : BaseNode<T> where T : IAgent
    {
        private BaseNode<T> _victim;

        public InstantSuccessModifier(BaseNode<T> victim) : base()
        {
            _victim = victim;
        }

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
            NodeStatus status = _victim.Execute(tick);
            if (status != NodeStatus.SUCCESS)
                return NodeStatus.FAILURE;
            else return NodeStatus.SUCCESS;
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