using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MangoBehaviorTree
{
    public class Tick<T> where T : IAgent
    {
        public BehaviorTree<T> tree;
        public T target;
        public HashSet<BaseNode<T>> traverseNodes;
       
        public Tick(BehaviorTree<T> tree, T target)
        {
            this.tree = tree;
            this.target = target;
            traverseNodes = new HashSet<BaseNode<T>>();
            
        }
    }
}

