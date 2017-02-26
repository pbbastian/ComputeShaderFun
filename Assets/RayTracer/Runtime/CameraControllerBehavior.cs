using UnityEngine;

namespace RayTracer.Runtime
{
    public class CameraControllerBehavior : MonoBehaviour
    {
        public float sensitivity = 2f;

        private Vector2 m_Rotation;
        private Quaternion m_OriginalRotation;

        void Start()
        {
            m_OriginalRotation = transform.localRotation;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                m_Rotation.x += Input.GetAxisRaw("Mouse X") * sensitivity;
                m_Rotation.y += Input.GetAxisRaw("Mouse Y") * sensitivity;
                transform.localRotation = m_OriginalRotation * Quaternion.AngleAxis(m_Rotation.x, Vector3.up) * Quaternion.AngleAxis(m_Rotation.y, Vector3.left);
            }

            if (Input.GetKey(KeyCode.L))
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
