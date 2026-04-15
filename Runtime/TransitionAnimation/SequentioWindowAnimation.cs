#if DAMDOR_FINESTRIO_SEQUENTIO

using Cysharp.Threading.Tasks;
using Damdor.Sequentio;
using UnityEngine;

namespace Damdor.Finestrio
{
    public class SequentioWindowAnimation : MonoWindowAnimation
    {
        public override WindowOrderInAnimation Order => order;
        
        [SerializeField] private Sequence sequence;
        [SerializeField] private WindowOrderInAnimation order;
        
        public override UniTask Prepare()
        {
           if(sequence != null) sequence.Init();
           return UniTask.CompletedTask;
        }

        public override UniTask Play()
        {
            return sequence != null ? sequence.PlayAndWait() : UniTask.CompletedTask;
        }
        
    }
}

#endif