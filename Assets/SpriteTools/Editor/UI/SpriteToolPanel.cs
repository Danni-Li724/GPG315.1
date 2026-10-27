namespace SpriteTools
{
    /// <summary>
    /// the base every tab inherits from, it just gives each tab a Draw and a way to hear when the list changes
    /// the SpriteToolsWindow keeps one of each tab and only calls Draw on the one that's open
    /// </summary>
    public abstract class SpriteToolPanel
    {
        protected readonly SpriteToolsWindow Window;

        protected SpriteToolPanel(SpriteToolsWindow window)
        {
            Window = window;
        }

        public abstract void Draw();

        public virtual void OnEntriesChanged()
        {
        }

        public virtual void OnDisable()
        {
        }
    }
}
