using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Finestrio
{
    internal abstract class TransitionRequestSetup<TWindow> where TWindow : Window
    {
        public abstract UniTask Retrieve(CancellationToken cancelToken);
        public abstract UniTask Setup(TWindow window, CancellationToken cancelToken);
        public abstract void Reset();
    }

    internal class TransitionRequestSetup<TWindow, TModel> : TransitionRequestSetup<TWindow> where TWindow : Window
    {
        private TModel model;
        private TransitionRequestModelRetrieveAsync<TModel> retrieveAsync;
        private TransitionRequestModelSetup<TWindow, TModel> setup;
        private TransitionRequestModelSetupAsync<TWindow, TModel> setupAsync;

        public override UniTask Retrieve(CancellationToken cancelToken)
        {
            if (retrieveAsync == null)
            {
                return UniTask.CompletedTask;
            }

            return retrieveAsync(cancelToken).ContinueWith(result => { model = result; });
        }

        public override UniTask Setup(TWindow window, CancellationToken cancelToken)
        {
            if (setup != null)
            {
                setup(window, model);
                return UniTask.CompletedTask;
            }

            if (setupAsync != null)
            {
                return setupAsync(window, model, cancelToken);
            }

            return UniTask.CompletedTask;
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
        }

    }

}