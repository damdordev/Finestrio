using System.Collections.Generic;

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
        internal ITransitionAnimation TransitionAnimation { get; private set; }
        
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
        
        public TransitionRequest<TWindow> Animation(ITransitionAnimation animation)
        {
            TransitionAnimation = animation;
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
            TransitionAnimation = null;
        }
        
    }
}