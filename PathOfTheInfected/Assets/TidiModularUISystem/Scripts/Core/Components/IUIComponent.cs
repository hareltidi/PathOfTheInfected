namespace TidiModularUISystem.Core
{
    public interface IUIComponent
    {
        void Initialize();   // build internal structure
        void Bind();         // connect to data/messages
        void Unbind();       // disconnect safely

        void Show();         // becomes active/visible
        void Hide();         // becomes inactive/hidden

        void Dispose();      // full cleanup
    }
}