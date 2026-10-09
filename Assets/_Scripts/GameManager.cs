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

    private async void StartHost()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.DisableButtons();
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
                m_multiplayerUI.EnableButtons();
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
            m_multiplayerUI.DisableButtons();
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

            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.SetStatusText($"Đang tham gia phòng: {joinCode}");
            }

            Debug.Log($"[Relay] Đang kết nối tới phòng {joinCode}...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Relay] Lỗi kết nối Client: {ex.Message}");
            if (m_multiplayerUI != null)
            {
                m_multiplayerUI.EnableButtons();
                m_multiplayerUI.ShowJoinCodeMenu();
                m_multiplayerUI.SetStatusText($"Lỗi kết nối: {ex.Message}");
            }
        }
    }

    private void DisconnectClient()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.EnableButtons();
            m_multiplayerUI.SetStatusText("Đã ngắt kết nối.");
            m_multiplayerUI.SetJoinCodeText(string.Empty);
        }

        NetworkManager.Shutdown();
    }
}