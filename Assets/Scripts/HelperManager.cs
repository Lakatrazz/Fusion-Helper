using FusionHelper.Network;
using FusionHelper.Steamworks;

using UnityEngine;

using TMPro;

namespace FusionHelper
{
    public class HelperManager : MonoBehaviour
    {
        public static HelperManager Instance { get; private set; } = null;

        public TMP_Text firewallNote;

        public TMP_Text udpSocketStatus;

        public TMP_Text questStatus;

        private string _firewallNoteText = "Waiting for socket...";
        public string FirewallNoteText
        {
            get
            {
                return _firewallNoteText;
            }
            set
            {
                _firewallNoteText = value;

                StatusDirty = true;
            }
        }

        private string _udpSocketText = "UDP Socket Not Connected";
        public string UDPSocketText
        {
            get
            {
                return _udpSocketText;
            }
            set
            {
                _udpSocketText = value;

                StatusDirty = true;
            }
        }

        private Color _udpSocketColor = Color.red;
        public Color UDPSocketColor
        {
            get
            {
                return _udpSocketColor;
            }
            set
            {
                _udpSocketColor = value;
            }
        }

        private string _questStatusText = "Quest Not Connected";
        public string QuestStatusText
        {
            get
            {
                return _questStatusText;
            }
            set
            {
                _questStatusText = value;

                StatusDirty = true;
            }
        }

        private Color _questStatusColor = Color.red;
        public Color QuestStatusColor
        {
            get
            {
                return _questStatusColor;
            }
            set
            {
                _questStatusColor = value;
            }
        }


        private bool StatusDirty { get; set; } = false;

        private bool _initializedNetworkHandler = false;

        private void Awake()
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(800, 800, FullScreenMode.Windowed);

            Instance = this;

            bool running = SteamHandler.CheckSteamRunning();

            if (!running)
            {
                _initializedNetworkHandler = false;
                return;
            }

            NetworkHandler.Init();

            _initializedNetworkHandler = true;
        }

        private void Update()
        {
            if (!_initializedNetworkHandler)
            {
                return;
            }

            NetworkHandler.PollEvents();
            SteamHandler.Tick();
        }

        private void LateUpdate()
        {
            if (!StatusDirty)
            {
                return;
            }

            firewallNote.text = FirewallNoteText;

            udpSocketStatus.text = UDPSocketText;
            udpSocketStatus.color = UDPSocketColor;

            questStatus.text = QuestStatusText;
            questStatus.color = QuestStatusColor;

            StatusDirty = false;
        }
    }
}
