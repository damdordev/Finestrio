using System;
using Cysharp.Threading.Tasks;
using Damdor.Foundation;

namespace Damdor.Finestrio
{
    public enum WindowRequestType
    {
        Add,
        Change,
        Back
    }
    
    public class WindowRequest<TWindow> where TWindow : IWindow
    {
        public WindowRequestType Type { get; }
        
        public WindowRequest(WindowRequestType type)
        {
            Type = type;
        }

        public void Setup<TModel>(TModel model, Action<TWindow, TModel> setup)
        {
            
        }
        
        public void Setup<TModel>(Func<UniTask<TModel>> retrieveModel, Action<TWindow, TModel> setup)
        {
        }
        
        public void Setup<TModelInput, TModel>(TModelInput modelParameter, Func<TModelInput, UniTask<TModel>> retrieveModel, Action<TWindow, TModel> setup)
        {
        }

        public void Setup<TModel>(TModel model, Func<TWindow, TModel, UniTask> setup)
        {
            
        }

        public void Setup<TModel>(UniTask<TModel> retrieveModel, Func<TWindow, TModel, UniTask> setup)
        {
        }
        
        public void Setup<TModelInput, TModel>(TModelInput modelParameter, Func<TModelInput, UniTask<TModel>> retrieveModel, Func<TWindow, TModel, UniTask> setup)
        {
        }

    }
}