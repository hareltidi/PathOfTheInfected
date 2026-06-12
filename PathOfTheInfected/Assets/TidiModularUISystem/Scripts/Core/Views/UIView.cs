using UnityEngine.UIElements;

namespace TidiModularUISystem.Scripts.Core
{
    /// <summary>
    /// The class with a direct reference to our UI. Exposes public methods and fields that the controller will call that will make the UI do things
    /// </summary>
    [UxmlElement]
    public partial class UIView : VisualElement, IUIScreenable
    {

        public virtual void Initialize()
        {
        }

        public void Show()
        {
            style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
    }
}