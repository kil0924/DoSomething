using System;
using Core.Singleton;
using UnityEngine;

namespace RPG
{
    public class RPGManager : Singleton<RPGManager>
    {
        public RPGStateManager stateManager;

        protected override void Awake()
        {
            base.Awake();
            stateManager = new RPGStateManager();
            stateManager.Init();
        }

        private void Start()
        {
        }

        private void Update()
        {
            stateManager.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            stateManager.OnFixedUpdate(Time.fixedDeltaTime);
        }
    }
}