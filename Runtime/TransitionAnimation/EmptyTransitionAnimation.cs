using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Finestrio
{
    public class EmptyTransitionAnimation : ITransitionAnimation
    {
        private readonly float prepareTime;
        private readonly float playTime;

        public EmptyTransitionAnimation(float prepareTime, float playTime)
        {
            this.prepareTime = prepareTime;
            this.playTime = playTime;
        }

        public async UniTask Prepare()
        {
            Debug.Log("Animation prepare begin");
            if(prepareTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(prepareTime));
            Debug.Log("Animation prepare end");
        }

        public async UniTask Play()
        {
            Debug.Log("Animation play begin");
            if(playTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(playTime));
            Debug.Log("Animation play end");
        }
    }
}