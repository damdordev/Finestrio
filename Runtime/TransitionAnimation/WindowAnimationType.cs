namespace Damdor.Finestrio
{
    /// <summary>
    /// Specifies the type of animation to be played for a window during a transition.
    /// </summary>
    public enum WindowAnimationType
    {
        /// <summary>
        /// The animation played when a new window is added or instantiated.
        /// </summary>
        Add,
        
        /// <summary>
        /// The animation played when a window is permanently removed or destroyed.
        /// </summary>
        Remove,
        
        /// <summary>
        /// The animation played when a window remains alive but is obscured by a new window.
        /// </summary>
        Pause,
        
        /// <summary>
        /// The animation played when a window becomes active again after a covering window is removed.
        /// </summary>
        Resume
    }
}
