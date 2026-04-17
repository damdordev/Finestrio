using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Represents the fundamental type of action a transition will perform.
    /// </summary>
    public enum TransitionType
    {
        /// <summary>
        /// Instantiates a new window and adds it to the top of the active stack, pausing the previous top window.
        /// </summary>
        Add,
        
        /// <summary>
        /// Swaps the current top window with a newly instantiated window, destroying the previous one.
        /// </summary>
        Change,
        
        /// <summary>
        /// Removes and destroys the current top window, resuming the window beneath it in the stack.
        /// </summary>
        Back
    }

    /// <summary>
    /// Base non-generic class providing factory methods for creating specific window transition requests.
    /// </summary>
    public class TransitionRequest
    {
        /// <summary>
        /// Begins constructing a new transition request for a specified window type.
        /// </summary>
        /// <typeparam name="TWindow">The type of <see cref="Window"/> this request targets.</typeparam>
        /// <param name="transitionType">The operation to perform (e.g., Add, Change, Back).</param>
        /// <returns>A typed <see cref="TransitionRequest{TWindow}"/> builder object.</returns>
        public static TransitionRequest<TWindow> Of<TWindow>(TransitionType transitionType) where TWindow : Window
            => FinestrioInternalHelper.GetTransitionRequest<TWindow>(transitionType);
    }
    
    /// <summary>
    /// A fluent builder class used to configure models, data, and animations for a specific window transition.
    /// Passed to <see cref="WindowManager.Transite{TWindow}"/> to execute the operation.
    /// </summary>
    /// <typeparam name="TWindow">The concrete type of the window being transitioned to.</typeparam>
    /// <example>
    /// <code>
    /// var request = TransitionRequest.Of&lt;MyWindow&gt;(TransitionType.Add)
    ///     .Setup(myModel, (window, model) => window.Initialize(model))
    ///     .Animation((source, target, type) => myCustomAnimation);
    /// </code>
    /// </example>
    public class TransitionRequest<TWindow> : TransitionRequest where TWindow : Window
    {
        internal TransitionType TransitionType { get; private set; }
        internal TransitionRequestAnimationGetter<TWindow> GetAnimation { get; private set; }
        
        private TransitionType type;
        private readonly List<TransitionRequestSetup<TWindow>> setup = new();
        
        internal TransitionRequest() {}
        
        internal TransitionRequest<TWindow> Type(TransitionType type)
        {
            TransitionType = type;
            return this;
        }

        /// <summary>
        /// Injects a pre-existing synchronous model into the window via a synchronous setup callback.
        /// </summary>
        /// <typeparam name="TModel">The type of the model to inject.</typeparam>
        /// <param name="model">The instance of the model.</param>
        /// <param name="setup">The callback to configure the window using the model.</param>
        /// <returns>The current request builder instance.</returns>
        public TransitionRequest<TWindow> Setup<TModel>(
            TModel model,
            TransitionRequestModelSetup<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(model, setup);
            this.setup.Add(s);
            return this;
        }
        
        /// <summary>
        /// Resolves a model asynchronously, then configures the window synchronously once the model is available.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being retrieved.</typeparam>
        /// <param name="retrieve">The asynchronous function fetching the model.</param>
        /// <param name="setup">The callback to configure the window once the model is retrieved.</param>
        /// <returns>The current request builder instance.</returns>
        public TransitionRequest<TWindow> Setup<TModel>(
            TransitionRequestModelRetrieveAsync<TModel> retrieve,
            TransitionRequestModelSetup<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(retrieve, setup);
            this.setup.Add(s);
            return this;
        }
        
        /// <summary>
        /// Injects a pre-existing synchronous model into the window, configuring the window via an asynchronous setup task.
        /// </summary>
        /// <typeparam name="TModel">The type of the injected model.</typeparam>
        /// <param name="model">The instance of the model.</param>
        /// <param name="setup">The asynchronous callback to configure the window.</param>
        /// <returns>The current request builder instance.</returns>
        public TransitionRequest<TWindow> Setup<TModel>(
            TModel model,
            TransitionRequestModelSetupAsync<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(model, setup);
            this.setup.Add(s);
            return this;
        }

        /// <summary>
        /// Resolves a model asynchronously and subsequently configures the window via an asynchronous setup task.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being retrieved.</typeparam>
        /// <param name="retrieve">The asynchronous function fetching the model.</param>
        /// <param name="setup">The asynchronous callback to configure the window.</param>
        /// <returns>The current request builder instance.</returns>
        public TransitionRequest<TWindow> Setup<TModel>(
            TransitionRequestModelRetrieveAsync<TModel> retrieve,
            TransitionRequestModelSetupAsync<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(retrieve, setup);
            this.setup.Add(s);
            return this;
        }
        
        /// <summary>
        /// Overrides the default window animations with a custom transition animation resolved via a callback.
        /// </summary>
        /// <param name="getAnimation">A function that returns a custom <see cref="ITransitionAnimation"/> given the source/target windows and transition type.</param>
        /// <returns>The current request builder instance.</returns>
        public TransitionRequest<TWindow> Animation(TransitionRequestAnimationGetter<TWindow> getAnimation)
        {
            GetAnimation = getAnimation;
            return this;
        }
        
        internal void Reset()
        {
            foreach (var transitionRequestSetup in setup)
            {
                FinestrioInternalHelper.ReleaseTransitionRequestSetup(transitionRequestSetup);
            }
            setup.Clear();
            GetAnimation = null;
        }

        internal UniTask RetrieveAllModels(CancellationToken cancellationToken) => setup.Count switch
        {
            0 => UniTask.CompletedTask,
            1 => setup[0].Retrieve(cancellationToken),
            2 => UniTask.WhenAll(
                setup[0].Retrieve(cancellationToken),
                setup[1].Retrieve(cancellationToken)
            ),
            3 => UniTask.WhenAll(
                setup[0].Retrieve(cancellationToken),
                setup[1].Retrieve(cancellationToken),
                setup[2].Retrieve(cancellationToken)
            ),
            _ => UniTask.WhenAll(setup.Select(s => s.Retrieve(cancellationToken)))
        };

        internal UniTask SetupAllModels(TWindow window, CancellationToken cancellationToken) => setup.Count switch
        {
            0 => UniTask.CompletedTask,
            1 => setup[0].Setup(window, cancellationToken),
            2 => UniTask.WhenAll(
                setup[0].Setup(window, cancellationToken),
                setup[1].Setup(window, cancellationToken)
            ),
            3 => UniTask.WhenAll(
                setup[0].Setup(window, cancellationToken),
                setup[1].Setup(window, cancellationToken),
                setup[2].Setup(window, cancellationToken)
            ),
            _ => UniTask.WhenAll(setup.Select(s => s.Setup(window, cancellationToken)))
        };
    }
}
