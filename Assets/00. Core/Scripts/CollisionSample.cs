using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Sample
{
    public class CollisionManager
    {
        public static CollisionManager Instance { get; private set; }
        public CollisionManager()
        {
            Instance = this;
        }
        
        public Queue<CollisionEvent> eventQueue = new Queue<CollisionEvent>();
        public void Update()
        {
            var e = eventQueue.Dequeue();
            e.DoSomething();
        }
    }
    public class CollisionEvent
    {
        public GameObject A;
        public GameObject B;
        public void DoSomething()
        {
            Debug.Log($"Collision : {A.name} {B.name}");   
        }
    }   
    public class CollisionSample : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var e = new CollisionEvent()
            {
                A = this.gameObject,
                B = other.gameObject
            };
            CollisionManager.Instance.eventQueue.Enqueue(e);
        }
    }
    
}
