using UnityEngine;

/// <summary>
/// THis script makes the UI Document showing player tag / name always face the camera.
/// </summary>
public class BillboardUI : MonoBehaviour
{
    private Transform m_cameraTransform;

    private void Awake()
    {
        if (Camera.main != null)
        {
            m_cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (m_cameraTransform == null)
        {
            if (Camera.main != null)
            {
                m_cameraTransform = Camera.main.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 direction = transform.position - m_cameraTransform.position;
        direction.y = 0; // Keep the billboard upright
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
