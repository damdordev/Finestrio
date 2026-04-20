using UnityEngine;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Base class for all managed window views within the Finestrio library.
    /// Provides sorting and basic lifecycle/visibility capabilities, relying on Unity's <see cref="Canvas"/> for rendering.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyWindow : Window 
    /// {
    ///     public void Initialize(MyModel model) { ... }
    /// }
    /// </code>
    /// </example>
    public abstract class Window : MonoBehaviour
    {
        /// <summary>
        /// Indicates whether windows positioned below this window on the stack should remain visible (not automatically paused or hidden).
        /// Configurable in the Inspector via the isTransparent property.
        /// </summary>
        public abstract bool IsTransparent { get; }

        /// <summary>
        /// Gets or sets the visual depth ordering of this window on the Canvas.
        /// </summary>
        public abstract int IndexOnStack { get; set; }

        /// <summary>
        /// Retrieves the predefined Inspector-configured <see cref="IWindowAnimation"/> corresponding to the specified transition type.
        /// </summary>
        /// <param name="windowAnimationType">The state of the animation to retrieve (e.g., Add, Remove).</param>
        /// <returns>The assigned <see cref="IWindowAnimation"/> if present, otherwise null.</returns>
        public abstract IWindowAnimation GetDefaultWindowAnimation(WindowAnimationType windowAnimationType);

        /// <summary>
        /// Toggles the active state of the window's GameObject in the Unity hierarchy.
        /// </summary>
        /// <param name="visible">True enables the window object; False disables it.</param>
        public abstract void SetVisible(bool visible);
    }
}
