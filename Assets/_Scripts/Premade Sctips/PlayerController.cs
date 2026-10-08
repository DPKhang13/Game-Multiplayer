
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private MyPlayerInput m_playerInput;

    [SerializeField]
    private AgentMover m_agentMover;

    [SerializeField]
    private InteractionDetector m_interactionDetector;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private AnimationEvents m_animationEvents;

    private bool m_isInteracting, m_isChopping;

    [SerializeField]
    private GameObject m_axeModel, m_pickAxeModel, m_woodModel, m_stoneModel;

    private ResourceSpawner m_resourceSpawner;

    private NetworkVariable<ulong> m_heldNetworkObjectId = new(ulong.MaxValue);
    private NetworkVariable<ObjectType> m_heldObjectType = new(ObjectType.None);

    private void Awake()
    {
        m_resourceSpawner = FindAnyObjectByType<ResourceSpawner>();
    }

    private void OnEnable()
    {
        m_playerInput.OnPickUpPressed += HandlePickUpPressed;
        m_playerInput.OnInteractPressed += HandleActionPressed;
    }

    private void OnDisable()
    {
        m_playerInput.OnPickUpPressed -= HandlePickUpPressed;
        m_playerInput.OnInteractPressed -= HandleActionPressed;
    }

    private void HandlePickUpPressed()
    {
        if (!IsOwner || !IsSpawned)
            return;

        if (m_isInteracting || m_isChopping)
            return;

        if (m_interactionDetector.ClosestInteractable == null)
            return;

        m_animator.SetBool("Interact", true);
        m_isInteracting = true;
    }

    private void HandleActionPressed()
    {
        if (!IsOwner || !IsSpawned)
            return;

        if (m_isChopping || m_isInteracting)
            return;

        if (m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            m_isChopping = true;
            m_animator.SetTrigger("Chop");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_interactionDetector.Initialize(IsOwner);
        m_heldObjectType.OnValueChanged += HandleHeldItemChanged;

        HandleItemOnJoin();

        if (IsOwner)
        {
            m_animationEvents.OnInteract += HandleInteractAction;
            m_animationEvents.OnAnimationDone += HandleAnimationDone;
            m_animationEvents.OnChop += HandleChopAction;
        }
    }

    private void HandleChopAction()
    {
        if (!IsOwner || !IsSpawned)
            return;

        if (m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if (m_interactionDetector.ClosestInteractable is ResourceNode)
            {
                RequestResourceNodeInteractionServerRpc(
                    m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId
                );
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestResourceNodeInteractionServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectId, out NetworkObject target))
        {
            return;
        }

        if (!target.TryGetComponent(out ResourceNode node))
            return;

        node.Harvest(m_heldObjectType.Value);
    }

    private void HandleItemOnJoin()
    {
        HandleHeldItemChanged(ObjectType.None, m_heldObjectType.Value);
    }

    private void HandleHeldItemChanged(ObjectType previousValue, ObjectType newValue)
    {
        m_axeModel.SetActive(newValue == ObjectType.Axe);
        m_pickAxeModel.SetActive(newValue == ObjectType.PickAxe);
        m_woodModel.SetActive(newValue == ObjectType.Wood);
        m_stoneModel.SetActive(newValue == ObjectType.Stone);
    }

    private void HandleAnimationDone()
    {
        m_isInteracting = false;
        m_isChopping = false;
    }

    // ==================================================
    // INTERACTION SYSTEM
    // ==================================================

    private void HandleInteractAction()
    {
        if (!IsOwner || !IsSpawned)
            return;

        if (m_interactionDetector.ClosestInteractable is PickableBase)
        {
            RequestPickUpServerRpc(
                m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId
            );
        }
        else if (m_interactionDetector.ClosestInteractable is ResourcePallet)
        {
            RequestGiveItemServerRpc(
                m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId
            );
        }
    }

    // ==================================================
    // RESOURCE PALLET
    // ==================================================

    [Rpc(SendTo.Server)]
    private void RequestGiveItemServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectId, out NetworkObject target))
        {
            return;
        }

        if (!target.TryGetComponent(out ResourcePallet resourcePallet))
            return;

        ObjectType heldType = m_heldObjectType.Value;

        // Chi cho phep giao Wood hoac Stone.
        if (heldType is not (ObjectType.Wood or ObjectType.Stone))
            return;

        if (resourcePallet.Interact(heldType))
        {
            m_heldObjectType.Value = ObjectType.None;
            m_heldNetworkObjectId.Value = ulong.MaxValue;
        }
    }

    // ==================================================
    // PICKUP SYSTEM
    // ==================================================

    [Rpc(SendTo.Server)]
    private void RequestPickUpServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectId, out NetworkObject target))
        {
            return;
        }

        if (!target.TryGetComponent(out PickableBase pickableItem))
        {
            return;
        }

        if (!pickableItem.CanBePickedUp)
        {
            return;
        }

        // Neu dang cam item, drop item cu truoc.
        if (m_heldObjectType.Value != ObjectType.None)
        {
            if (!DropCurrentItem())
                return;
        }

        if (pickableItem is PickableTool)
        {
            m_heldNetworkObjectId.Value = networkObjectId;
        }
        else
        {
            m_heldNetworkObjectId.Value = ulong.MaxValue;
        }

        m_heldObjectType.Value = pickableItem.ObjectType;
        pickableItem.PickUp();
    }

    // ==================================================
    // DROP SYSTEM - AXE / PICKAXE / WOOD / STONE
    // ==================================================

    private bool DropCurrentItem()
    {
        if (!IsServer)
            return false;

        ObjectType heldType = m_heldObjectType.Value;

        if (heldType == ObjectType.None)
        {
            m_heldNetworkObjectId.Value = ulong.MaxValue;
            return true;
        }

        // Drop Axe hoac Pickaxe.
        if (heldType is ObjectType.Axe or ObjectType.PickAxe)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                m_heldNetworkObjectId.Value, out NetworkObject target))
            {
                Debug.LogWarning("Held tool NetworkObject not found.");
                return false;
            }

            if (!target.TryGetComponent(out PickableTool pickableItem))
            {
                Debug.LogWarning("Held NetworkObject is not a PickableTool.");
                return false;
            }

            pickableItem.Drop(transform.position);
        }
        // Drop Wood hoac Stone.
        else if (heldType is ObjectType.Wood or ObjectType.Stone)
        {
            if (m_resourceSpawner == null)
            {
                m_resourceSpawner = FindAnyObjectByType<ResourceSpawner>();
            }

            if (m_resourceSpawner == null || !m_resourceSpawner.IsSpawned)
            {
                Debug.LogWarning(
                    "ResourceSpawner is missing or not spawned. Cannot drop resource."
                );
                return false;
            }

            m_resourceSpawner.SpawnResource(
                heldType,
                transform.position
            );
        }
        else
        {
            Debug.LogWarning($"Unsupported item type: {heldType}");
            return false;
        }

        // Reset item state.
        m_heldObjectType.Value = ObjectType.None;
        m_heldNetworkObjectId.Value = ulong.MaxValue;

        return true;
    }

    // ==================================================
    // DISCONNECT & NETWORK DESPAWN
    // ==================================================

    public override void OnNetworkPreDespawn()
    {
        // Server xu ly drop truoc khi Player despawn.
        // Khong drop khi toan bo NetworkManager dang shutdown.
        if (IsServer &&
            NetworkManager != null &&
            !NetworkManager.ShutdownInProgress)
        {
            DropCurrentItem();
        }

        base.OnNetworkPreDespawn();
    }

    public override void OnNetworkDespawn()
    {
        m_heldObjectType.OnValueChanged -= HandleHeldItemChanged;

        // Chi huy dang ky event, khong gui RPC.
        if (IsOwner)
        {
            m_animationEvents.OnInteract -= HandleInteractAction;
            m_animationEvents.OnAnimationDone -= HandleAnimationDone;
            m_animationEvents.OnChop -= HandleChopAction;
        }

        base.OnNetworkDespawn();
    }

    // ==================================================
    // PLAYER MOVEMENT
    // ==================================================

    private void Update()
    {
        if (!IsOwner || !IsSpawned)
            return;

        Vector2 movementInput = m_playerInput.MovementInput;

        if (m_isChopping || m_isInteracting)
        {
            movementInput = Vector2.zero;
        }

        m_agentMover.Move(movementInput);
    }
}
