#if DAMDOR_FINESTRIO_SEQUENTIO

using Cysharp.Threading.Tasks;
using Damdor.Sequentio;
using UnityEngine;

namespace Damdor.Finestrio
{
    /// <summary>
    /// An <see cref="IWindowAnimation"/> implemented using the Damdor.Sequentio animation library.
    /// Can be attached as a MonoBehavior to a window for visual configuration in the Inspector.
    /// </summary>
    public class SequentioWindowAnimation : MonoWindowAnimation
    {
        /// <summary>
        /// Defines how this animation relates visually to another window during a compound transition.
        /// </summary>
        public override WindowOrderInAnimation Order => order;
        
        [SerializeField] private Sequence sequence;
        [SerializeField] private WindowOrderInAnimation order;
        
        /// <summary>
        /// Prepares the sequence by invoking <see cref="Sequence.Init"/> without playing it.
        /// </summary>
        /// <returns>A completed UniTask after initialization.</returns>
        public override UniTask Prepare()
        {
           if(sequence != null) sequence.Init();
           return UniTask.CompletedTask;
        }

        /// <summary>
        /// Plays the Sequentio sequence and asynchronously waits for it to complete.
        /// </summary>
        /// <returns>A UniTask representing the completion of the entire animation sequence.</returns>
        public override UniTask Play()
        {
            return sequence != null ? sequence.PlayAndWait() : UniTask.CompletedTask;
        }
    }
}

#endif
