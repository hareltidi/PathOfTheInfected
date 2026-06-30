using UnityEngine.UIElements;

namespace TidiModularUISystem.Scripts.Core
{
    /// <summary>
    /// Receives messages that are relevant to the ui view it's attached to and tells the UI what to do.
    /// </summary>
    public class UIScreen<TView> where TView : IUIScreenable
    {
        protected TView AttachedElement;
        public virtual void ScreenInit(TView view)
        {
            AttachedElement = view;
            AttachedElement.Initialize();
        }

        public virtual void ScreenStart()
        {
        }

        public virtual void ScreenTick(float deltaTime)
        {

        }

        public virtual void ScreenFixedTick(float fixedDeltaTime)
        {

        }

        public virtual void ScreenDispose()
        {
            
        }

    }
}