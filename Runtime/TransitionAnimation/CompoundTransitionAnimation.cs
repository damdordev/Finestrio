using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    public class CompoundTransitionAnimation : ITransitionAnimation
    {
        public WindowOrderInAnimation Order { get; private set; }

        private static readonly Stack<CompoundTransitionAnimation> pool = new();
        
        private IWindowAnimation sourceAnimation;
        private IWindowAnimation targetAnimation;
        private readonly List<UniTask> tasks = new();

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
        
        public UniTask Prepare()
        {
            tasks.Clear();
            if(sourceAnimation != null) tasks.Add(sourceAnimation.Prepare());
            if(targetAnimation != null) tasks.Add(targetAnimation.Prepare());

            return RunTaskList();
        }

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