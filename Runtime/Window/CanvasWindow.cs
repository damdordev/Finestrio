using UnityEngine;

namespace Damdor.Finestrio
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasWindow : Window
    {
        /// <summary>
        /// Indicates whether windows positioned below this window on the stack should remain visible (not automatically paused or hidden).
        /// Configurable in the Inspector via the isTransparent property.
        /// </summary>
        public override bool IsTransparent => isTransparent;

        /// <summary>
        /// Gets or sets the visual depth ordering of this window on the Canvas.
        /// </summary>
        public override int IndexOnStack
        {
            get => GetCanvas().sortingOrder;
            set => GetCanvas().sortingOrder = value;
        }

        [SerializeField] private bool isTransparent;
        [SerializeField] private DefaultWindowAnimation defaultAnimations;

        private Canvas canvas;

        /// <summary>
        /// Retrieves the predefined Inspector-configured <see cref="IWindowAnimation"/> corresponding to the specified transition type.
        /// </summary>
        /// <param name="windowAnimationType">The state of the animation to retrieve (e.g., Add, Remove).</param>
        /// <returns>The assigned <see cref="IWindowAnimation"/> if present, otherwise null.</returns>
        public override IWindowAnimation GetDefaultWindowAnimation(WindowAnimationType windowAnimationType) 
            => defaultAnimations.GetDefaultAnimation(windowAnimationType);

        /// <summary>
        /// Toggles the active state of the window's GameObject in the Unity hierarchy.
        /// </summary>
        /// <param name="visible">True enables the window object; False disables it.</param>
        public override void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private Canvas GetCanvas()
        {
            if(canvas == null) canvas = GetComponent<Canvas>();
            return canvas;
        }
    }
}