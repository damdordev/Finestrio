using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Finestrio
{
    public abstract class MonoWindowAnimation : MonoBehaviour, IWindowAnimation
    {
        public abstract UniTask Prepare();
        public abstract UniTask Play();
        public abstract WindowOrderInAnimation Order { get; }
    }
}