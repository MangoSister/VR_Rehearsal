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
        private Dictionary<int, HashSet<BaseNode<T>>> _openNodes;

        public BehaviorTree(BaseNode<T> root)
        {
            _root = root;
            _openNodes = new Dictionary<int, HashSet<BaseNode<T>>>();
        }

        public void NextTick(T target)
        {
            Tick<T> tick = new Tick<T>(this, target);

            _root.Execute(tick);

            HashSet<BaseNode<T>> lastOpenNodes;
            if (_openNodes.TryGetValue(target.agentId, out lastOpenNodes))
            {
                HashSet<BaseNode<T>> currOpenNodes = tick.openNodes;

                //close nodes that cannot perform self-close
                var closedNodes = Enumerable.Except(lastOpenNodes, currOpenNodes);
                foreach (BaseNode<T> node in closedNodes)
                    node.Close(tick);
                _openNodes[target.agentId] = currOpenNodes;
            }
            else _openNodes.Add(target.agentId, tick.openNodes);
        } 
    }
}

