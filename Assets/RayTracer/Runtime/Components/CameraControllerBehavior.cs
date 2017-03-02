using UnityEngine;

namespace RayTracer.Runtime
{
    public class CameraControllerBehavior : MonoBehaviour
    {
        public float lookSpeed = 2f;
        public float moveSpeed = 1f;

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
                m_Rotation.x += Input.GetAxisRaw("Mouse X") * lookSpeed;
                m_Rotation.y += Input.GetAxisRaw("Mouse Y") * lookSpeed;
                transform.localRotation = m_OriginalRotation * Quaternion.AngleAxis(m_Rotation.x, Vector3.up) * Quaternion.AngleAxis(m_Rotation.y, Vector3.left);
            }

            transform.position += transform.forward * moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * moveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.L))
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

            if (Input.GetKeyDown(KeyCode.E))
            {
                GetComponent<BasicRayTracerImageEffect>().enabled = false;
                GetComponent<BvhRayTracerImageEffect>().enabled = false;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                GetComponent<BasicRayTracerImageEffect>().enabled = true;
                GetComponent<BvhRayTracerImageEffect>().enabled = false;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                GetComponent<BasicRayTracerImageEffect>().enabled = false;
                GetComponent<BvhRayTracerImageEffect>().enabled = true;
            }
        }
    }
}
