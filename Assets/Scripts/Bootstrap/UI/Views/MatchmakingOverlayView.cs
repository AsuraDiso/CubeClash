using TMPro;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public sealed class MatchmakingOverlayView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _statusText;

        public void SetVisible(bool isVisible) => _root.SetActive(isVisible);

        public void SetStatusText(string message) => _statusText.text = message;
    }
}
