using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExtensionTools.Gameplay
{
    [AddComponentMenu("ExtensionTools/Gameplay/CharacterMovement")]
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Walking and running")]
        [SerializeField] float m_WalkSpeed = 5f;
        [SerializeField] float m_RunSpeed = 10f;

        [Header("Jumping and falling")]
        [SerializeField] float m_JumpSpeed = 8.0f;
        [SerializeField] float m_GravityScale = 1.5f;
        [SerializeField] float m_AirControl = 1.5f;


        [Header("Camera Control")]
        [SerializeField] float m_LookSpeed = 360f;
        [SerializeField] float m_LookLimit = 90f;

        UnityEngine.Camera m_PlayerCamera;
        CharacterController m_CharacterController;

        Vector3 m_MoveDirection = Vector3.zero;
        float m_RotationX = 0;

        void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_PlayerCamera = GetComponentInChildren<UnityEngine.Camera>();
        }

        void LateUpdate()
        {

            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);


            bool isRunning = GameplaySettings.GetSprint();
            float curSpeedX = (isRunning ? m_RunSpeed : m_WalkSpeed) * GameplaySettings.GetVerticalInput();
            float curSpeedY = (isRunning ? m_RunSpeed : m_WalkSpeed) * GameplaySettings.GetHorizontalInput();

            bool Jump = GameplaySettings.GetUp();
            if (InputMode.currentInputmode != InputMode.Input.Game)
            {
                curSpeedX = 0;
                curSpeedY = 0;
                Jump = false;
            }

            Vector2 inputDirection = new Vector2(curSpeedX, curSpeedY);
            Vector3 TargetMoveDirection = ((forward * inputDirection.x) + (right * inputDirection.y));

            float movementDirectionY = m_MoveDirection.y;
            m_MoveDirection.y = 0f;

            if (!m_CharacterController.isGrounded)
                m_MoveDirection = Vector3.Lerp(m_MoveDirection, TargetMoveDirection, Time.deltaTime * m_AirControl);
            else
                m_MoveDirection = TargetMoveDirection;


            if (Jump && m_CharacterController.isGrounded)
            {
                m_MoveDirection.y = m_JumpSpeed;
            }
            else
            {
                m_MoveDirection.y = movementDirectionY;
            }


            if (!m_CharacterController.isGrounded)
            {
                m_MoveDirection.y += Physics.gravity.y * m_GravityScale * Time.deltaTime;
            }

            // Move the controller
            m_CharacterController.Move(m_MoveDirection * Time.deltaTime);

            if (InputMode.currentInputmode == InputMode.Input.Game)
            {
                m_RotationX += -GameplaySettings.GetLookYAxis() * m_LookSpeed * Time.deltaTime;
                m_RotationX = Mathf.Clamp(m_RotationX, -m_LookLimit, m_LookLimit);
                m_PlayerCamera.transform.localRotation = Quaternion.Euler(m_RotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, GameplaySettings.GetLookXAxis() * Time.deltaTime * m_LookSpeed, 0);
            }
        }
    }

}