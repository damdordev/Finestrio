using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Represents an animation to be played for a single window during a transition.
    /// </summary>
    public interface IWindowAnimation
    {
        /// <summary>
        /// Prepares the window animation before playing, such as resetting properties or capturing initial state.
        /// </summary>
        /// <returns>A UniTask representing the asynchronous preparation.</returns>
        UniTask Prepare();
        
        /// <summary>
        /// Plays the window animation.
        /// </summary>
        /// <returns>A UniTask representing the completion of the animation.</returns>
        UniTask Play();
        
        /// <summary>
        /// Gets the visual ordering of the window when played in a compound transition.
        /// </summary>
        WindowOrderInAnimation Order { get; }
    }
}
