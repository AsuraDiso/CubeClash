using System;
using Bootstrap.UI.Views;
using Core.Scenes;
using VContainer.Unity;

namespace Bootstrap.UI.Controllers
{
    public sealed class LoadingController : IStartable, IDisposable
    {
        private readonly IUiViewFactory _viewFactory;
        private readonly ILoadingProgress _loadingProgress;
        private LoadingView _view;

        public LoadingController(IUiViewFactory viewFactory, ILoadingProgress loadingProgress)
        {
            _viewFactory = viewFactory;
            _loadingProgress = loadingProgress;
        }

        public void Start()
        {
            _view = _viewFactory.CreateLoadingView();
            _loadingProgress.PercentChanged += OnPercentChanged;
        }

        public void Dispose()
        {
            _loadingProgress.PercentChanged -= OnPercentChanged;

            if (_view == null) return;
            _viewFactory.Destroy(_view);
            _view = null;
        }

        private void OnPercentChanged(float percent) => _view.SetPercent(percent);
    }
}
