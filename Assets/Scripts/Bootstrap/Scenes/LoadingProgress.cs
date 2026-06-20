using System;
using Core.Scenes;

namespace Bootstrap.Scenes
{
    public sealed class LoadingProgress : ILoadingProgress
    {
        public event Action<float> PercentChanged;

        public void SetPercent(float percent) => PercentChanged?.Invoke(percent);
    }
}
