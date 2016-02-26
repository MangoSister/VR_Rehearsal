using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MangoBehaviorTree
{
    public class BehaviorTree<T> where T : IAgent
    {
        private BaseNode<T> _root;

        //per tree exec data
        private Dictionary<int, Dictionary<int, BaseNode<T>>> _openNodes;

        public BehaviorTree(BaseNode<T> root)
        {
            _root = root;
            _openNodes = new Dictionary<int, Dictionary<int, BaseNode<T>>>();
        }

        public void NextTick(T target)
        {
            Tick<T> tick = new Tick<T>(this, target);

            _root.Execute(tick);

            Dictionary<int, BaseNode<T>> lastOpenNodes;
            if (_openNodes.TryGetValue(target.agentId, out lastOpenNodes))
            {
                Dictionary<int, BaseNode<T>> currOpenNodes = tick.openNodes;
                /**/
                lastOpenNodes = currOpenNodes;
            }
            else _openNodes.Add(target.agentId, tick.openNodes);
        } 
    }
}

