using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MangoBehaviorTree
{
    public class Tick<T> where T : IAgent
    {
        public BehaviorTree<T> tree;
        public T target;
        public Dictionary<int, BaseNode<T>> openNodes;

        public Tick(BehaviorTree<T> tree, T target)
        {
            this.tree = tree;
            this.target = target;
            openNodes = new Dictionary<int, BaseNode<T>>();
        }
    }
}

