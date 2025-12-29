using TMPro;

using UnityEngine;

namespace FusionHelper
{
    public class VersionText : MonoBehaviour
    {
        public TMP_Text Text;

        private void Awake()
        {
            Text.text = $"v{Application.version}";
        }
    }
}