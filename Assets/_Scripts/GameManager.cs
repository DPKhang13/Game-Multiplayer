using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private MultiplayerUI m_multiplayerUI;

    [SerializeField]
    private int m_maxPlayers = 4;

    private async void Start()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDiconnectClient += DisconnectClient;
            m_multiplayerUI.SetStatusText("Đang khởi tạo Unity Services...");
        }

        // Đăng ký sự kiện ngắt kết nối và kết nối thành công từ Netcode
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetStatusText("Đã sẵn sàng kết nối Relay.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Lỗi khởi tạo Unity Services: {ex.Message}");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetStatusText("Lỗi khởi tạo Unity Services!");
            }
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        base.OnDestroy();
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Khi bản thân kết nối thành công (áp dụng cho Client hoặc Host)
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Relay] Bản thân (ClientId: {clientId}) đã kết nối thành công!");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetInGameState();
                if (!NetworkManager.Singleton.IsServer)
                {
                    m_multiplayerUI.SetStatusText("Đã kết nối vào phòng!");
                }
            }
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        // Khi chính client này bị ngắt kết nối (hoặc Host tắt server)
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[Relay] Bị ngắt kết nối khỏi máy chủ.");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetOfflineState();
                m_multiplayerUI.SetStatusText("Host đã đóng phòng hoặc bị ngắt kết nối.");
                m_multiplayerUI.SetJoinCodeText(string.Empty);
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        else
        {
            // Host nhận được thông báo client khác rời phòng
            Debug.Log($"[Relay] Player {clientId} đã rời phòng.");
        }
    }

    private async void StartHost()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.SetConnectingState();
            m_multiplayerUI.SetStatusText("Đang khởi tạo Host Relay...");
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

            NetworkManager.StartHost();

            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetInGameState();
                m_multiplayerUI.SetJoinCodeText(joinCode);
                m_multiplayerUI.SetStatusText($"Host đang chạy. Join Code: {joinCode}");
            }

            Debug.Log($"[Relay] Khởi tạo Host thành công! Join Code: {joinCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Lỗi khi tạo Host: {ex.Message}");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetOfflineState();
                m_multiplayerUI.SetStatusText($"Lỗi tạo Host: {ex.Message}");
            }
        }
    }

    private async void StartClient()
    {
        string joinCode = m_multiplayerUI != null ? m_multiplayerUI.JoinCode : string.Empty;

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            Debug.LogWarning("[Relay] Chưa nhập Join Code!");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.ShowJoinCodeMenu();
                m_multiplayerUI.SetStatusText("Vui lòng nhập Join Code trước khi kết nối!");
            }
            return;
        }

        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.SetConnectingState();
            m_multiplayerUI.SetStatusText($"Đang kết nối tới phòng {joinCode}...");
        }

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            NetworkManager.StartClient();
            Debug.Log($"[Relay] Đang bắt tay với phòng {joinCode}...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Lỗi kết nối Client: {ex.Message}");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.ShowJoinCodeMenu();
                m_multiplayerUI.SetStatusText($"Lỗi kết nối: {ex.Message}");
            }
        }
    }

    private void DisconnectClient()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.SetOfflineState();
            m_multiplayerUI.SetStatusText("Đã ngắt kết nối.");
            m_multiplayerUI.SetJoinCodeText(string.Empty);
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}