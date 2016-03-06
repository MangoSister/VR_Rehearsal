using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace MangoBehaviorTree
{
    public class BehaviorTree<T> where T : IAgent
    {
        private BaseNode<T> _root;

        //per tree exec data
        public Dictionary<int, HashSet<BaseNode<T>>> _openNodes;

        public BehaviorTree(BaseNode<T> root)
        {
            _root = root;
            _openNodes = new Dictionary<int, HashSet<BaseNode<T>>>();
        }

        public void NextTick(T target)
        {
            Tick<T> tick = new Tick<T>(this, target);
            if (!_openNodes.ContainsKey(target.agentId))
                _openNodes.Add(target.agentId, new HashSet<BaseNode<T>>());

            _root.Execute(tick);

            HashSet<BaseNode<T>> lastOpenNodes = _openNodes[target.agentId];
            HashSet<BaseNode<T>> currOpenNodes = tick.traverseNodes;

            //close nodes that cannot perform self-close
            var closedNodes = Enumerable.Except(lastOpenNodes, currOpenNodes);
            foreach (BaseNode<T> node in closedNodes)
                node.Close(tick);
            _openNodes[target.agentId] = currOpenNodes;

        }
    }
}

