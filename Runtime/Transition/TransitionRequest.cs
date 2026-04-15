using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace Damdor.Finestrio
{
    public enum TransitionType
    {
        Add,
        Change,
        Back
    }

    public class TransitionRequest
    {
        public static TransitionRequest<TWindow> Of<TWindow>(TransitionType transitionType) where TWindow : Window
            => FinestrioInternalHelper.GetTransitionRequest<TWindow>(transitionType);
    }
    
    public class TransitionRequest<TWindow> : TransitionRequest where TWindow : Window
    {
        internal TransitionType TransitionType { get; private set; }
        internal object Create { get; private set; }
        internal object Destroy { get; private set; }
        internal TransitionRequestAnimationGetter<TWindow> GetAnimation { get; private set; }
        
        private TransitionType type;
        private readonly List<TransitionRequestSetup<TWindow>> setup = new();
        
        internal TransitionRequest() {}
        
        internal TransitionRequest<TWindow> Type(TransitionType type)
        {
            TransitionType = type;
            return this;
        }

        public TransitionRequest<TWindow> CreateParameter<TCreateParameter>(TCreateParameter parameter) where TCreateParameter : class
        {
            Create = parameter;
            return this;
        }
        
        public TransitionRequest<TWindow> DestroyParameter<TDestroyParameter>(TDestroyParameter parameter) where TDestroyParameter : class
        {
            Destroy = parameter;
            return this;
        }

        public TransitionRequest<TWindow> Setup<TModel>(
            TModel model,
            TransitionRequestModelSetup<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(model, setup);
            this.setup.Add(s);
            return this;
        }
        
        public TransitionRequest<TWindow> Setup<TModel>(
            TransitionRequestModelRetrieveAsync<TModel> retrieve,
            TransitionRequestModelSetup<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(retrieve, setup);
            this.setup.Add(s);
            return this;
        }
        
        public TransitionRequest<TWindow> Setup<TModel>(
            TModel model,
            TransitionRequestModelSetupAsync<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(model, setup);
            this.setup.Add(s);
            return this;
        }

        public TransitionRequest<TWindow> Setup<TModel>(
            TransitionRequestModelRetrieveAsync<TModel> retrieve,
            TransitionRequestModelSetupAsync<TWindow, TModel> setup)
        {
            var s = FinestrioInternalHelper.GetTransitionRequestSetup<TWindow, TModel>();
            s.Set(retrieve, setup);
            this.setup.Add(s);
            return this;
        }
        
        public TransitionRequest<TWindow> Animation(TransitionRequestAnimationGetter<TWindow> getAnimation)
        {
            GetAnimation = getAnimation;
            return this;
        }

        public void Reset()
        {
            foreach (var transitionRequestSetup in setup)
            {
                FinestrioInternalHelper.ReleaseTransitionRequestSetup(transitionRequestSetup);
            }
            setup.Clear();
            Create = null;
            Destroy = null;
            GetAnimation = null;
        }

        internal void RetrieveAllModels(CancellationToken cancellationToken)
        {
            foreach (var s in setup)
            {
                s.Retrieve(cancellationToken);
            }
        }
        
        internal UniTask SetupAllModels(TWindow window, CancellationToken cancellationToken)
        {
            if (ReadyToSyncSetup())
            {
                SetupAllModelsSync(window, cancellationToken);
                return UniTask.CompletedTask;
            }

            if (setup.Count == 1)
            {
                return setup[0].Setup(window, cancellationToken);
            }
            
            return SetupAllModelsAsync(window, cancellationToken);
        }

        private void SetupAllModelsSync(TWindow window, CancellationToken cancellationToken)
        {
            foreach (var s in setup)
            {
                s.Setup(window, cancellationToken).Forget();
            }
        }

        private async UniTask SetupAllModelsAsync(TWindow window, CancellationToken cancellationToken)
        {
            foreach (var s in setup)
            {
                await s.Setup(window, cancellationToken);
            }
        }

        private bool ReadyToSyncSetup()
        {
            foreach (var s in setup)
            {
                if (!s.IsReadyToSyncSetup) return false;
            }

            return true;
        }
        
    }
}