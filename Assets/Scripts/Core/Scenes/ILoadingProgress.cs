using System;

namespace Core.Scenes
{
    public interface ILoadingProgress
    {
        event Action<float> PercentChanged;

        void SetPercent(float percent);
    }
}
