using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TidiModularUISystem.Scripts.Core
{
    /// <summary>
    /// Routes UI controllers to the right views. Acts as the entry point for our modular layer ontop of UI Toolkit.
    /// </summary>
    public abstract class UIRouter : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        protected VisualElement Root;

        private void Awake()
        {
            RouteAwake();
        }

        private void Start()
        {
            RouteStart();
        }

        private void Update()
        {
            RouteUpdate();
        }

        private void FixedUpdate()
        {
            RouteFixedUpdate();
        }

        private void OnDestroy()
        {
            RouteDispose();
        }

        #region Virtual Logic Gates

        protected virtual void RouteAwake()
        {
            Root = document.rootVisualElement;
            CallInitOnScreens();
        }

        protected virtual void RouteStart()
        {
            CallStartScreen();
        }

        protected virtual void RouteUpdate()
        {
            CallTickScreen(Time.deltaTime);
        }

        protected virtual void RouteFixedUpdate()
        {
            CallFixedTickScreen(Time.fixedDeltaTime);
        }

        protected virtual void RouteDispose()
        {
            CallDisposeScreen();
        }
        #endregion

        #region Screen Lifecycle
        public abstract void CallInitOnScreens();
        public abstract void CallStartScreen();
        public abstract void CallTickScreen(float deltaTime);
        public abstract void CallFixedTickScreen(float fixedDeltaTime);
        public abstract void CallDisposeScreen();
        #endregion
    }
}