using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TidiModularUISystem.Core.Examples
{
    [UxmlElement]
    public partial class UIButtonComponent : UIComponent
    {

        private Button _button;

        public Action OnPressed;


        protected override void OnInitialize()
        {
            _button = this.Q<Button>();
            if (_button == null)
            {
                Debug.LogError("[UIButtonComponent] Button element not found in UXML.");
            }
            Debug.Log("[UIButtonComponent] OnInitialize");
        }

        protected override void OnBind()
        {
            _button.clicked += HandleClicked;
            Debug.Log("[UIButtonComponent] OnBind - handler attached");
        }

        protected override void OnUnbind()
        {

            _button.clicked -= HandleClicked;
            Debug.Log("[UIButtonComponent] OnUnbind - handler removed");
            base.OnUnbind();
        }

        private void HandleClicked()
        {
            Debug.Log("[UIButtonComponent] Clicked");
            try
            {
                OnPressed?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        // Helper for automated tests - triggers the same internal action as a user click
        public void SimulateClick()
        {
            // Invoke the same flow as the real click handler
            HandleClicked();
        }

        protected override void OnDispose()
        {
            Debug.Log("[UIButtonComponent] OnDispose");
            base.OnDispose();
        }
    }
}