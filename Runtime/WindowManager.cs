using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Finestrio
{
    public class WindowCallbacks
    {
        /// <summary>
        /// Invoked when the top-most window in the stack changes. The callback receives the new top window, which can be null if the last window is removed.
        /// </summary>
        public event Action<Window> TopWindowChanged;

        /// <summary>
        /// Invoked when a new window is created and added to the stack.
        /// </summary>
        public event Action<Window> WindowCreated;

        /// <summary>
        /// Invoked when a window is destroyed and removed from the stack.
        /// </summary>
        public event Action<Window> BeforeWindowDestroyed;

        /// <summary>
        /// Invoked when a window is paused (e.g., a new window is pushed on top of it).
        /// </summary>
        public event Action<Window> WindowPaused;

        /// <summary>
        /// Invoked when a window is resumed (e.g., the window on top of it is removed).
        /// </summary>
        public event Action<Window> WindowResumed;

        internal void NotifyTopWindowChanged(Window window)
        {
            TopWindowChanged?.Invoke(window);
        }

        internal void NotifyWindowCreated(Window window)
        {
            WindowCreated?.Invoke(window);
        }

        internal void NotifyBeforeWindowDestroyed(Window window)
        {
            BeforeWindowDestroyed?.Invoke(window);
        }

        internal void NotifyOnWindowPaused(Window window)
        {
            WindowPaused?.Invoke(window);
        }

        internal void NotifyOnWindowResumed(Window window)
        {
            WindowResumed?.Invoke(window);
        }
    }

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
        
        /// <summary>
        /// An optional callback invoked when an exception occurs during window transitions.
        /// This allows consumers to implement custom error handling or logging logic for transition failures.
        /// Default behavior (reverting broken transition) will be performed after handler finishes
        /// </summary>
        public TransitionErrorHandler ErrorHandler { get; set; }

        /// <summary>
        /// Provides access to various lifecycle events of the windows.
        /// </summary>
        public WindowCallbacks Callbacks { get; } = new();

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
            var oldTop = TopWindow;

            try
            {
                var source = windows.Count > 0 ? windows[^1] : null;
                target = await windowSource.Create<TWindow>();
                windows.Add(target);
                target.IndexOnStack = windows.Count;
                Callbacks.NotifyWindowCreated(target);

                await SetupAndAnimate(request, source, target, cancellationToken);
                UpdateVisibilities();
                if(oldTop != null) Callbacks.NotifyOnWindowPaused(oldTop);
                Callbacks.NotifyTopWindowChanged(TopWindow);
            }
            catch (Exception e)
            {
                await HandleException(e, cancellationToken);
                if (target != null)
                {
                    Callbacks.NotifyBeforeWindowDestroyed(target);
                    windows.Remove(target);
                    windowSource.Destroy(target);
                }
                
                UpdateVisibilities();
                if(oldTop != null && oldTop != TopWindow) Callbacks.NotifyOnWindowResumed(oldTop);
                Callbacks.NotifyTopWindowChanged(TopWindow);
                throw;
            }

            return target;
        }
        
        private async UniTask<TWindow> ProcessChange<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            TWindow target = null;
            Window source = null;
            var oldTop = TopWindow;

            try
            {
                source = windows.Count > 0 ? windows[^1] : null;
                target = await windowSource.Create<TWindow>();
                target.IndexOnStack = windows.Count;
                Callbacks.NotifyWindowCreated(target);

                if (source != null) windows.Remove(source);
                windows.Add(target);

                await SetupAndAnimate(request, source, target, cancellationToken);
                if (source != null)
                {
                    Callbacks.NotifyBeforeWindowDestroyed(source);
                    windowSource.Destroy(source);
                }
                UpdateVisibilities();
                Callbacks.NotifyTopWindowChanged(TopWindow);
            }
            catch (Exception e)
            {
                await HandleException(e, cancellationToken);
                if (target != null)
                {
                    Callbacks.NotifyBeforeWindowDestroyed(target);
                    windows.Remove(target);
                    windowSource.Destroy(target);
                }

                if (source != null && !windows.Contains(source)) windows.Add(source);
                UpdateVisibilities();
                Callbacks.NotifyTopWindowChanged(TopWindow);

                throw;
            }

            return target;
        }
        
        private async UniTask<TWindow> ProcessBack<TWindow>(
            TransitionRequest<TWindow> request,
            CancellationToken cancellationToken) where TWindow : Window
        {
            Window source = null;
            var oldTop = TopWindow;

            try
            {
                source = windows.Count > 0 ? windows[^1] : null;
                var target = windows.Count > 1 ? (TWindow)windows[^2] : null;
                if (source != null) windows.Remove(source);

                await SetupAndAnimate(request, source, target, cancellationToken);

                if (source != null)
                {
                    Callbacks.NotifyBeforeWindowDestroyed(source);
                    windowSource.Destroy(source);
                }
                UpdateVisibilities();
                if(TopWindow != null) Callbacks.NotifyOnWindowResumed(TopWindow);
                Callbacks.NotifyTopWindowChanged(TopWindow);
            }
            catch (Exception e)
            {
                await HandleException(e, cancellationToken);
                if (source != null && !windows.Contains(source)) windows.Add(source);
                UpdateVisibilities();
                Callbacks.NotifyTopWindowChanged(TopWindow);
                
                throw;
            }

            return (TWindow) TopWindow;
        }

        private async UniTask HandleException(Exception e, CancellationToken cancellationToken)
        {
            if (ErrorHandler == null) return;

            try
            {
                await ErrorHandler(e, cancellationToken);
            }
            catch (Exception e2)
            {
                Debug.LogError($"[Finestrio] Error handler failed: {e2}");
                // ignored
            }
        }

        private static async UniTask SetupAndAnimate<TWindow>(TransitionRequest<TWindow> request, Window source, TWindow target, CancellationToken cancellationToken)
            where TWindow : Window
        {
            source?.SetVisible(true);
            target?.SetVisible(true);
            var animation = GetTransitionAnimation(request, source, target, request.TransitionType);
            var shouldRevertWindowsForAnimation = animation != null && source != null && target != null &&
                                      ShouldRevertWindowsForAnimation(request.TransitionType, animation.Order);

            try
            {
                if (shouldRevertWindowsForAnimation) RevertWindowsForAnimations(source, target);
                if (animation != null) await animation.Prepare();
                await request.RetrieveAllModels(cancellationToken);
                await request.SetupAllModels(target, cancellationToken);
                if (animation != null)
                {
                    await animation.Play();
                }
            }
            finally
            {
                if (shouldRevertWindowsForAnimation) RevertWindowsForAnimations(source, target);
            }
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