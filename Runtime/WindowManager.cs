using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Central manager responsible for the lifecycle, state, and transition queueing of windows.
    /// Manages an internal stack of active windows and orchestrates transitions using an <see cref="IWindowSource"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// var windowSource = new AddressableWindowSource(canvasParent);
    /// var windowManager = new WindowManager(windowSource);
    /// 
    /// var request = TransitionRequest.Of&lt;MyWindow&gt;(TransitionType.Add)
    ///     .Setup(new MyModel(), (w, m) => w.Initialize(m));
    ///     
    /// await windowManager.Transite(request);
    /// </code>
    /// </example>
    public class WindowManager : ITransitionRequestQueueReceiver
    {
        /// <summary>
        /// Retrieves the currently active and topmost window in the stack.
        /// </summary>
        public Window TopWindow => windows.Count > 0 ? windows[^1] : null;
        
        private readonly List<Window> windows = new();
        
        private readonly IWindowSource windowSource;
        private readonly TransitionRequestQueue requestQueue;

        /// <summary>
        /// Instantiates a new WindowManager using the provided factory source for loading and destroying windows.
        /// </summary>
        /// <param name="windowSource">The <see cref="IWindowSource"/> implementation configured to locate your specific windows.</param>
        public WindowManager(IWindowSource windowSource)
        {
            requestQueue = new TransitionRequestQueue(this);
            this.windowSource = windowSource;
        }
        
        /// <summary>
        /// Enqueues a window transition request to be processed synchronously or asynchronously based on queue state.
        /// </summary>
        /// <typeparam name="TWindow">The class extending <see cref="Window"/> targeted by the transition.</typeparam>
        /// <param name="request">The prepared request containing models, setups, and transition type details.</param>
        /// <returns>A UniTask resolving to the initialized and visible window instance upon transition completion.</returns>
        public UniTask<TWindow> Transite<TWindow>(TransitionRequest<TWindow> request) where TWindow : Window 
            => requestQueue.Enqueue(request);

        /// <summary>
        /// Cancels all pending transition requests in the queue and releases associated resources.
        /// </summary>
        public void Release()
        {
            requestQueue.Release();
        }

        UniTask<TWindow> ITransitionRequestQueueReceiver.ProcessRequest<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken)
            => request.TransitionType switch
            {
                TransitionType.Add => ProcessAdd(request, cancellationToken),
                TransitionType.Change => ProcessChange(request, cancellationToken),
                TransitionType.Back => ProcessBack(request, cancellationToken),
                _ => throw new ArgumentOutOfRangeException($"Unknown transition type: {request.TransitionType}")
            };

        private async UniTask<TWindow> ProcessAdd<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            TWindow target = null;

            try
            {
                var source = windows.Count > 0 ? windows[^1] : null;
                target = await windowSource.Create<TWindow>();
                windows.Add(target);
                target.IndexOnStack = windows.Count;

                await SetupAndAnimate(request, source, target, cancellationToken);
                UpdateVisibilities();
            }
            catch (Exception)
            {
                if (target != null)
                {
                    windows.Remove(target);
                    windowSource.Destroy(target);
                }
                
                UpdateVisibilities();
                throw;
            }

            return target;
        }
        
        private async UniTask<TWindow> ProcessChange<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            var source = windows.Count > 0 ? windows[^1] : null;
            var target = await windowSource.Create<TWindow>();
            target.IndexOnStack = windows.Count;

            if (source != null) windows.Remove(source);
            windows.Add(target);
            
            await SetupAndAnimate(request, source, target, cancellationToken);
            if (source != null) windowSource.Destroy(source);
            UpdateVisibilities();
            
            return target;
        }
        
        private async UniTask<TWindow> ProcessBack<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            var source = windows.Count > 0 ? windows[^1] : null;
            var target = windows.Count > 1 ? (TWindow) windows[^2] : null;
            if (source != null) windows.Remove(source);
            
            await SetupAndAnimate(request, source, target, cancellationToken);
            
            if (source != null) windowSource.Destroy(source);
            UpdateVisibilities();
            
            return (TWindow) TopWindow;
        }
        
        private static async UniTask SetupAndAnimate<TWindow>(TransitionRequest<TWindow> request, Window source, TWindow target, CancellationToken cancellationToken)
            where TWindow : Window
        {
            source?.SetVisible(true);
            target?.SetVisible(true);
            var animation = GetTransitionAnimation(request, source, target, request.TransitionType);
            var shouldRevertWindowsForAnimation = animation != null && source != null && target != null &&
                                      ShouldRevertWindowsForAnimation(request.TransitionType, animation.Order);
           
            if(shouldRevertWindowsForAnimation) RevertWindowsForAnimations(source, target);
            if (animation != null) await animation.Prepare();
            await request.RetrieveAllModels(cancellationToken);
            await request.SetupAllModels(target, cancellationToken);
            if (animation != null)
            {
                await animation.Play();
            }
            if(shouldRevertWindowsForAnimation) RevertWindowsForAnimations(source, target);
        }

        private static ITransitionAnimation GetTransitionAnimation<TWindow>(
            TransitionRequest<TWindow> request,
            Window source,
            TWindow target,
            TransitionType transitionType) where TWindow : Window
            => request.GetAnimation != null
                ? request.GetAnimation(source, target, transitionType)
                : CompoundTransitionAnimation.Combine(source, target, transitionType);

        private static bool ShouldRevertWindowsForAnimation(TransitionType transitionType, WindowOrderInAnimation order)
            => transitionType switch
            {
                TransitionType.Add or TransitionType.Change => order == WindowOrderInAnimation.OldOnTop,
                TransitionType.Back => order == WindowOrderInAnimation.NewOnTop,
                _ => throw new ArgumentOutOfRangeException(nameof(transitionType), transitionType, null)
            };

        private static void RevertWindowsForAnimations(Window source, Window target)
        {
            var sourceIndex = source.IndexOnStack;
            var targetIndex = target.IndexOnStack;

            source.IndexOnStack = targetIndex;
            target.IndexOnStack = sourceIndex;
        }

        private void UpdateVisibilities()
        {
            var wasFullscreenWindow = false;
            for (var i = windows.Count - 1; i >= 0; i--)
            {
                windows[i].SetVisible(!wasFullscreenWindow);
                if (!windows[i].IsTransparent) wasFullscreenWindow = true;
            }
        }
    }
}
