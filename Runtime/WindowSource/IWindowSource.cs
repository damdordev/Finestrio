using System;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Represents a factory responsible for instantiating and destroying windows.
    /// Provides an abstraction layer so that WindowManager does not need to know where window assets are stored.
    /// </summary>
    /// <example>
    /// <code>
    /// IWindowSource source = new ResourcesWindowSource(parentTransform);
    /// var windowManager = new WindowManager(source);
    /// </code>
    /// </example>
    public interface IWindowSource
    {
        /// <summary>
        /// Checks if this source can create windows of the specified type.
        /// </summary>
        /// <param name="type">The Type of the window subclass to check.</param>
        /// <returns>True if the source is capable of instantiating the type, otherwise false.</returns>
        bool Support(Type type);
        
        /// <summary>
        /// Asynchronously instantiates a window of the given type.
        /// </summary>
        /// <typeparam name="TWindow">The specific window class deriving from <see cref="IFinestrioWindow"/>.</typeparam>
        /// <returns>A UniTask resolving to the initialized window instance.</returns>
        UniTask<TWindow> Create<TWindow>() where TWindow : IFinestrioWindow;
        
        /// <summary>
        /// Cleans up and destroys the specified window instance.
        /// </summary>
        /// <param name="finestrioWindow">The window to be destroyed.</param>
        void Destroy(IFinestrioWindow finestrioWindow);
    }
}
