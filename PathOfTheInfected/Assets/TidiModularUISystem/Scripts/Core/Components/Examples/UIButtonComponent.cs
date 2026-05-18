using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TidiModularUISystem.Core.Examples
{
    [UxmlElement]
    public partial class UIButtonComponent : Button, IUIComponent
    {

        private UIComponentLifecycle _lifecycle;
        public event Action OnButtonClicked;

        public UIButtonComponent()
        {
            _lifecycle  = new UIComponentLifecycle(this);
            Initialize();
        }
        ~UIButtonComponent()
        {
            Dispose();
        }

        public void Initialize()
        {
            AddToClassList("ui-button");
            text = "Button";
        }

        public void Bind()
        {
            clicked += OnClicked;
        }

        public void Unbind()
        {
            clicked -= OnClicked;
            _lifecycle = null;
        }

        public void Show()
        {
            _lifecycle.Show();
        }

        public void Hide()
        {
            _lifecycle.Hide();
        }

        public void Dispose()
        {
            Unbind();
            RemoveFromHierarchy();
        }

        protected virtual void OnClicked()
        {
            OnButtonClicked?.Invoke();
        }
    }
}