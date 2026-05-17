using UnityEngine;
using UnityEngine.UIElements;

namespace TidiModularUISystem.Core
{
    public abstract class UIComponent : VisualElement, IUIComponent
    {
        private bool _initialized;

        public UIComponent()
        {
            Initialize();
        }

        #region Interface methods
        public virtual void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            OnInitialize();
        }

        public virtual void Bind()
        {
            OnBind();
        }

        public virtual void Unbind()
        {
            OnUnbind();
        }

        public virtual void Show()
        {
            style.display = DisplayStyle.Flex;
            OnShow();
        }

        public virtual void Hide()
        {
            style.display = DisplayStyle.None;
            OnHide();
        }

        public virtual void Dispose()
        {
            Unbind();
            OnDispose();
        }
        #endregion

        #region Protected Virtual and Abstract methods
        protected virtual void OnInitialize() { }

        protected virtual void OnBind() { }

        protected virtual void OnUnbind() { }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }

        protected virtual void OnDispose() { }
        #endregion
    }
}