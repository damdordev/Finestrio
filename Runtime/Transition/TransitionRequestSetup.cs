using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    internal abstract class TransitionRequestSetup<TWindow> where TWindow : Window
    {
        public abstract bool IsReadyToSyncSetup { get; }
        public abstract void Retrieve(CancellationToken cancellationToken);
        public abstract UniTask Setup(TWindow window, CancellationToken cancellationToken);
        public abstract void Reset();
    }

    internal class TransitionRequestSetup<TWindow, TModel> : TransitionRequestSetup<TWindow> where TWindow : Window
    {
        public override bool IsReadyToSyncSetup => IsRetrieved && setup != null;
        private bool IsRetrieved => retrieveAsync == null || modelRetrieved;
        
        private TModel model;
        private bool modelRetrieved;
        private TransitionRequestModelRetrieveAsync<TModel> retrieveAsync;
        private TransitionRequestModelSetup<TWindow, TModel> setup;
        private TransitionRequestModelSetupAsync<TWindow, TModel> setupAsync;

        public override void Retrieve(CancellationToken cancellationToken)
        {
            if (IsRetrieved) return;
            RetrieveAsync(cancellationToken).Forget();
        }
        
        public override UniTask Setup(TWindow window, CancellationToken cancellationToken)
        {
            if (IsReadyToSyncSetup)
            {
                setup(window, model);
                return UniTask.CompletedTask;
            }

            if (IsRetrieved)
            {
                setupAsync(window, model, cancellationToken);
            }
            
            return UniTask.WaitUntil(() => IsRetrieved, cancellationToken: cancellationToken)
                .ContinueWith(() =>
                {
                    if(setupAsync != null) return setupAsync(window, model, cancellationToken);
                    setup(window, model);
                    return UniTask.CompletedTask;
                });
        }

        public void Set(TModel model, TransitionRequestModelSetup<TWindow, TModel> setup)
        {
            this.model = model;
            this.setup = setup;
        }

        public void Set(TransitionRequestModelRetrieveAsync<TModel> retrieve, TransitionRequestModelSetup<TWindow, TModel> setup)
        {
            retrieveAsync = retrieve;
            this.setup = setup;
        }

        public void Set(TModel model, TransitionRequestModelSetupAsync<TWindow, TModel> setup)
        {
            this.model = model;
            setupAsync = setup;
        }

        public void Set(TransitionRequestModelRetrieveAsync<TModel> retrieve, TransitionRequestModelSetupAsync<TWindow, TModel> setup)
        {
            retrieveAsync = retrieve;
            setupAsync = setup;
        }

        public override void Reset()
        {
            model = default;
            retrieveAsync = null;
            setup = null;
            setupAsync = null;
            modelRetrieved = false;
        }
        
        private async UniTask RetrieveAsync(CancellationToken cancelToken)
        {
            model = await retrieveAsync(cancelToken);
            modelRetrieved = true;
        }

    }

}