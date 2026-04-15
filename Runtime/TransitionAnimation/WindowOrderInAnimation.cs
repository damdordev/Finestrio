namespace Damdor.Finestrio
{
    /// <summary>
    /// Specifies the visual ordering of windows during a transition animation.
    /// </summary>
    public enum WindowOrderInAnimation
    {
        /// <summary>
        /// The default ordering behavior, usually determined by the transition type or underlying implementation.
        /// </summary>
        Default,
        
        /// <summary>
        /// The incoming (new) window is rendered on top of the outgoing (old) window.
        /// </summary>
        NewOnTop,
        
        /// <summary>
        /// The outgoing (old) window remains rendered on top of the incoming (new) window.
        /// </summary>
        OldOnTop
    }
}
