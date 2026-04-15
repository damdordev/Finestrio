using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Combines multiple window animations into a single overall transition.
    /// Used internally to execute the entry animation of the new window and exit animation of the old window concurrently.
    /// </summary>
    public class CompoundTransitionAnimation : ITransitionAnimation
    {
        /// <summary>
        /// Defines how the incoming and outgoing windows are visually layered.
        /// </summary>
        public WindowOrderInAnimation Order { get; private set; }

        private static readonly Stack<CompoundTransitionAnimation> pool = new();
        
        private IWindowAnimation sourceAnimation;
        private IWindowAnimation targetAnimation;
        private readonly List<UniTask> tasks = new();

        /// <summary>
        /// Combines default animations fetched from the source and target windows based on the provided transition type.
        /// </summary>
        /// <param name="source">The outgoing (old) window in the transition.</param>
        /// <param name="target">The incoming (new) window in the transition.</param>
        /// <param name="transitionType">The type of transition that dictates which default animations are chosen.</param>
        /// <param name="order">The required layer ordering during the transition, or Default to auto-calculate.</param>
        /// <returns>A pooled instance of <see cref="CompoundTransitionAnimation"/> prepared to animate.</returns>
        public static CompoundTransitionAnimation Combine(
            Window source, 
            Window target, 
            TransitionType transitionType,
            WindowOrderInAnimation order = WindowOrderInAnimation.Default)
        {
            return Combine(
                GetDefaultSourceAnimation(source, transitionType),
                GetDefaultTargetAnimation(target, transitionType),
                order
            );
        }
        
        /// <summary>
        /// Combines specified source and target animations into a single transition instance.
        /// </summary>
        /// <param name="sourceAnimation">The animation to apply to the outgoing window.</param>
        /// <param name="targetAnimation">The animation to apply to the incoming window.</param>
        /// <param name="order">The required layer ordering, or Default to auto-calculate based on the animations provided.</param>
        /// <returns>A pooled instance of <see cref="CompoundTransitionAnimation"/> prepared to animate.</returns>
        public static CompoundTransitionAnimation Combine(
            IWindowAnimation sourceAnimation,
            IWindowAnimation targetAnimation,
            WindowOrderInAnimation order = WindowOrderInAnimation.Default)
        {
            var anim = pool.Count > 0 ? pool.Pop() : new CompoundTransitionAnimation();
            anim.Setup(sourceAnimation, targetAnimation, order != WindowOrderInAnimation.Default ? order : CalculateOrder(sourceAnimation, targetAnimation));
            return anim;
        }

        private void Setup(IWindowAnimation sourceAnimation, IWindowAnimation targetAnimation, WindowOrderInAnimation order)
        {
            this.sourceAnimation = sourceAnimation;
            this.targetAnimation = targetAnimation;
            Order = order;
            tasks.Clear();
        }

        private void Clear()
        {
            sourceAnimation = null;
            targetAnimation = null;
            tasks.Clear();
            pool.Push(this);
        }
        
        /// <summary>
        /// Initializes all combined animations simultaneously by invoking their <see cref="IWindowAnimation.Prepare"/> methods.
        /// </summary>
        /// <returns>A UniTask representing completion of all preparation steps.</returns>
        public UniTask Prepare()
        {
            tasks.Clear();
            if(sourceAnimation != null) tasks.Add(sourceAnimation.Prepare());
            if(targetAnimation != null) tasks.Add(targetAnimation.Prepare());

            return RunTaskList();
        }

        /// <summary>
        /// Starts all combined animations simultaneously by invoking their <see cref="IWindowAnimation.Play"/> methods.
        /// Automatically returns to the pool upon completion.
        /// </summary>
        /// <returns>A UniTask representing the asynchronous completion of the entire combined transition.</returns>
        public UniTask Play()
        {
            tasks.Clear();
            
            if(sourceAnimation != null) tasks.Add(sourceAnimation.Play());
            if(targetAnimation != null) tasks.Add(targetAnimation.Play());

            return RunTaskList().ContinueWith(Clear);
        }
        
        private UniTask RunTaskList()
        {
            return tasks.Count switch
            {
                0 => UniTask.CompletedTask,
                1 => tasks[0],
                _ => UniTask.WhenAll(tasks[0], tasks[1])
            };
        }

        private static IWindowAnimation GetDefaultSourceAnimation(Window source, TransitionType transitionType)
        {
            if (source == null) return null;
            return transitionType switch
            {
                TransitionType.Add => source.GetDefaultWindowAnimation(WindowAnimationType.Pause),
                TransitionType.Change => source.GetDefaultWindowAnimation(WindowAnimationType.Remove),
                TransitionType.Back => source.GetDefaultWindowAnimation(WindowAnimationType.Remove),
                _ => throw new ArgumentOutOfRangeException(nameof(transitionType), transitionType, null)
            };
        }
        
        private static IWindowAnimation GetDefaultTargetAnimation(Window target, TransitionType transitionType)
        {
            if (target == null) return null;
            return transitionType switch
            {
                TransitionType.Add => target.GetDefaultWindowAnimation(WindowAnimationType.Add),
                TransitionType.Change => target.GetDefaultWindowAnimation(WindowAnimationType.Add),
                TransitionType.Back => target.GetDefaultWindowAnimation(WindowAnimationType.Resume),
                _ => throw new ArgumentOutOfRangeException(nameof(transitionType), transitionType, null)
            };
        }
        
        private static WindowOrderInAnimation CalculateOrder(IWindowAnimation EnterAnimation, IWindowAnimation ExitAnimation)
        {
            var enterOrder = EnterAnimation?.Order ?? WindowOrderInAnimation.Default;
            var exitOrder = ExitAnimation?.Order ?? WindowOrderInAnimation.Default;

            if(enterOrder == WindowOrderInAnimation.Default && exitOrder == WindowOrderInAnimation.Default)
                return WindowOrderInAnimation.NewOnTop;
            if(enterOrder == WindowOrderInAnimation.Default)
            {
                return exitOrder == WindowOrderInAnimation.OldOnTop ? WindowOrderInAnimation.OldOnTop : WindowOrderInAnimation.NewOnTop;
            }

            if(exitOrder == WindowOrderInAnimation.Default)
            {
                return enterOrder == WindowOrderInAnimation.OldOnTop ? WindowOrderInAnimation.OldOnTop : WindowOrderInAnimation.NewOnTop;
            }
            
            return exitOrder == WindowOrderInAnimation.OldOnTop ? WindowOrderInAnimation.OldOnTop : WindowOrderInAnimation.NewOnTop;
        }
    }
}
