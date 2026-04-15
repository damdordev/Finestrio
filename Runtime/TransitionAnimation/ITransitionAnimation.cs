using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Represents an overall transition animation between two windows (e.g., crossfade, slide).
    /// </summary>
    public interface ITransitionAnimation
    {
        /// <summary>
        /// Prepares the transition animation before playing, such as initializing values or capturing states.
        /// </summary>
        /// <returns>A UniTask representing the asynchronous preparation.</returns>
        UniTask Prepare();
        
        /// <summary>
        /// Plays the transition animation.
        /// </summary>
        /// <returns>A UniTask representing the completion of the animation.</returns>
        UniTask Play();
        
        /// <summary>
        /// Gets the visual ordering of the windows during this transition animation.
        /// </summary>
        WindowOrderInAnimation Order { get; }
    }
}
