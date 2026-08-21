using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TidiModularUISystem.Core
{
    public class UIComponentLifecycle
    {
        private readonly VisualElement _owner;

        public UIComponentLifecycle(VisualElement owner)
        {
            _owner = owner;
        }

        public void Show()
        {
            _owner.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            _owner.style.display = DisplayStyle.None;
        }
    }
}