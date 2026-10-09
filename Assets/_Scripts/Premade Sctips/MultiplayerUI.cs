using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Helper script that exposes UI Document button clicks as C# events that can be listened to 
/// from the GameManager script to trigger the start host / client and disconnect actions.
/// </summary>
public class MultiplayerUI : MonoBehaviour
{
    [SerializeField]
    private UIDocument m_uiDocument;

    // Containers
    private VisualElement m_mainMenuContainer;
    private VisualElement m_inGameContainer;
    private VisualElement m_joinCodeContainer;

    // Main Menu Elements
    private Button m_hostButton;
    private Button m_joinMenuButton;

    // In-Game Elements
    private Button m_disconnectButton;

    // Join Code Popup Elements
    private TextField m_inputJoinCode;
    private Button m_connectButton;
    private Button m_cancelJoinButton;

    // Status Labels
    private Label m_labelJoinCode;
    private Label m_labelStatus;

    public event Action OnStartHost, OnStartClient, OnDiconnectClient;

    public string JoinCode => m_inputJoinCode != null ? m_inputJoinCode.value?.Trim() : string.Empty;

    private void Awake()
    {
        VisualElement root = m_uiDocument.rootVisualElement;

        // Query containers
        m_mainMenuContainer = root.Q<VisualElement>("MainMenuContainer");
        m_inGameContainer = root.Q<VisualElement>("InGameContainer");
        m_joinCodeContainer = root.Q<VisualElement>("JoinCodeContainer");

        // Query main buttons
        m_hostButton = root.Q<Button>("ButtonHost");
        m_joinMenuButton = root.Q<Button>("ButtonJoinMenu");

        // Query in-game buttons
        m_disconnectButton = root.Q<Button>("ButtonDisconnect");

        // Query join popup elements
        m_inputJoinCode = root.Q<TextField>("InputJoinCode");
        m_connectButton = root.Q<Button>("ButtonClient");
        m_cancelJoinButton = root.Q<Button>("ButtonCancelJoin");

        // Query info labels
        m_labelJoinCode = root.Q<Label>("LabelJoinCode");
        m_labelStatus = root.Q<Label>("LabelStatus");
    }

    private void Start()
    {
        if (m_hostButton != null)
            m_hostButton.clicked += () => OnStartHost?.Invoke();

        if (m_joinMenuButton != null)
            m_joinMenuButton.clicked += ShowJoinCodeMenu;

        if (m_cancelJoinButton != null)
            m_cancelJoinButton.clicked += HideJoinCodeMenu;

        if (m_connectButton != null)
            m_connectButton.clicked += () => OnStartClient?.Invoke();

        if (m_disconnectButton != null)
            m_disconnectButton.clicked += () => OnDiconnectClient?.Invoke();

        SetOfflineState();
    }

    /// <summary>
    /// Hiển thị bảng nhập Join Code
    /// </summary>
    public void ShowJoinCodeMenu()
    {
        if (m_mainMenuContainer != null)
            m_mainMenuContainer.style.display = DisplayStyle.None;

        if (m_inGameContainer != null)
            m_inGameContainer.style.display = DisplayStyle.None;

        if (m_joinCodeContainer != null)
        {
            m_joinCodeContainer.style.display = DisplayStyle.Flex;
            m_inputJoinCode?.SetEnabled(true);
            m_connectButton?.SetEnabled(true);
            m_cancelJoinButton?.SetEnabled(true);
            m_inputJoinCode?.Focus();
        }

        SetStatusText(string.Empty);
    }

    /// <summary>
    /// Đóng bảng nhập code và quay lại menu chính
    /// </summary>
    public void HideJoinCodeMenu()
    {
        SetOfflineState();
        SetStatusText(string.Empty);
    }

    /// <summary>
    /// Trạng thái đang kết nối (tạm thời khoá các nút để tránh bấm lặp lại)
    /// </summary>
    public void SetConnectingState()
    {
        m_hostButton?.SetEnabled(false);
        m_joinMenuButton?.SetEnabled(false);
        m_connectButton?.SetEnabled(false);
        m_cancelJoinButton?.SetEnabled(false);
        m_inputJoinCode?.SetEnabled(false);
    }

    /// <summary>
    /// Trạng thái trong game (cho cả Host lẫn Client): Ẩn các nút tạo phòng, chỉ hiện nút Disconnect
    /// </summary>
    public void SetInGameState()
    {
        if (m_mainMenuContainer != null)
            m_mainMenuContainer.style.display = DisplayStyle.None;

        if (m_joinCodeContainer != null)
            m_joinCodeContainer.style.display = DisplayStyle.None;

        if (m_inGameContainer != null)
        {
            m_inGameContainer.style.display = DisplayStyle.Flex;
            m_disconnectButton?.SetEnabled(true);
        }
    }

    /// <summary>
    /// Trạng thái Offline / Sau khi Disconnect / Bị văng khỏi server
    /// </summary>
    public void SetOfflineState()
    {
        if (m_inGameContainer != null)
            m_inGameContainer.style.display = DisplayStyle.None;

        if (m_joinCodeContainer != null)
            m_joinCodeContainer.style.display = DisplayStyle.None;

        if (m_mainMenuContainer != null)
        {
            m_mainMenuContainer.style.display = DisplayStyle.Flex;
            m_hostButton?.SetEnabled(true);
            m_joinMenuButton?.SetEnabled(true);
        }

        m_disconnectButton?.SetEnabled(false);
        m_inputJoinCode?.SetEnabled(true);
        m_connectButton?.SetEnabled(true);
        m_cancelJoinButton?.SetEnabled(true);
    }

    // Các hàm tương thích cũ
    public void EnableButtons() => SetOfflineState();
    public void DisableButtons() => SetConnectingState();

    public void SetJoinCodeText(string code)
    {
        if (m_labelJoinCode != null)
        {
            m_labelJoinCode.text = string.IsNullOrEmpty(code) ? string.Empty : $"Join Code: {code}";
        }
    }

    public void SetStatusText(string status)
    {
        if (m_labelStatus != null)
        {
            m_labelStatus.text = status;
        }
    }
}
