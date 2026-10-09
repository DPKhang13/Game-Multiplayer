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

    // Main Menu Elements
    private VisualElement m_mainMenuContainer;
    private Button m_hostButton;
    private Button m_joinMenuButton;
    private Button m_clientDisconnect;

    // Join Code Popup Elements
    private VisualElement m_joinCodeContainer;
    private TextField m_inputJoinCode;
    private Button m_clientButton;
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
        m_joinCodeContainer = root.Q<VisualElement>("JoinCodeContainer");

        // Query main buttons
        m_hostButton = root.Q<Button>("ButtonHost");
        m_joinMenuButton = root.Q<Button>("ButtonJoinMenu");
        m_clientDisconnect = root.Q<Button>("ButtonDisconnect");

        // Query join popup elements
        m_inputJoinCode = root.Q<TextField>("InputJoinCode");
        m_clientButton = root.Q<Button>("ButtonClient");
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

        if (m_clientButton != null)
            m_clientButton.clicked += () => OnStartClient?.Invoke();

        if (m_clientDisconnect != null)
            m_clientDisconnect.clicked += () => OnDiconnectClient?.Invoke();

        EnableButtons();
    }

    public void ShowJoinCodeMenu()
    {
        if (m_mainMenuContainer != null)
            m_mainMenuContainer.style.display = DisplayStyle.None;

        if (m_joinCodeContainer != null)
        {
            m_joinCodeContainer.style.display = DisplayStyle.Flex;
            m_inputJoinCode?.Focus();
        }

        SetStatusText(string.Empty);
    }

    public void HideJoinCodeMenu()
    {
        if (m_joinCodeContainer != null)
            m_joinCodeContainer.style.display = DisplayStyle.None;

        if (m_mainMenuContainer != null)
            m_mainMenuContainer.style.display = DisplayStyle.Flex;

        SetStatusText(string.Empty);
    }

    public void DisableButtons()
    {
        m_hostButton?.SetEnabled(false);
        m_joinMenuButton?.SetEnabled(false);
        m_clientButton?.SetEnabled(false);
        m_cancelJoinButton?.SetEnabled(false);
        m_inputJoinCode?.SetEnabled(false);
        m_clientDisconnect?.SetEnabled(true);

        // Sau khi đã bắt đầu kết nối, đóng popup nhập code và hiển thị lại main container
        if (m_joinCodeContainer != null && m_joinCodeContainer.style.display == DisplayStyle.Flex)
        {
            m_joinCodeContainer.style.display = DisplayStyle.None;
            if (m_mainMenuContainer != null)
                m_mainMenuContainer.style.display = DisplayStyle.Flex;
        }
    }

    public void EnableButtons()
    {
        if (m_mainMenuContainer != null)
            m_mainMenuContainer.style.display = DisplayStyle.Flex;

        if (m_joinCodeContainer != null)
            m_joinCodeContainer.style.display = DisplayStyle.None;

        m_hostButton?.SetEnabled(true);
        m_joinMenuButton?.SetEnabled(true);
        m_clientButton?.SetEnabled(true);
        m_cancelJoinButton?.SetEnabled(true);
        m_inputJoinCode?.SetEnabled(true);
        m_clientDisconnect?.SetEnabled(false);
    }

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
