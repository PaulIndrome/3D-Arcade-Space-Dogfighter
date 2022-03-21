using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExtensionTools.Gameplay
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	[AddComponentMenu("ExtensionTools/Gameplay/FlyingCameraMovement")]



	public class FlyingCameraMovement : MonoBehaviour
    {
		[SerializeField]
		float m_CameraAcceleration = 40;
		[SerializeField]
		float m_SprintBoost = 6;

		[Header("Camera Control")]
		[SerializeField] float m_LookSpeed = 360f;
		[SerializeField] float m_LookLimit = 90f;

		float m_Damping = 5;
		Vector3 m_Velocity;
		bool m_IsDebugCamera = false;
		float m_RotationX = 0;
		float m_RotationY = 0;
		public bool DebugCamera { get => m_IsDebugCamera; set { m_IsDebugCamera = value; } }

		void LateUpdate()
		{
			if (InputMode.currentInputmode==InputMode.Input.Game || (InputMode.currentInputmode==InputMode.Input.Debug && m_IsDebugCamera))
				UpdateInput();

			m_Velocity = Vector3.Lerp(m_Velocity, Vector3.zero, m_Damping * Time.deltaTime); //We smoothly break the camera
			transform.position += m_Velocity * Time.deltaTime;
		}

		void UpdateInput()
		{
			// Position
			m_Velocity += GetAccelerationVector() * Time.deltaTime;

			// Rotation
			//Vector2 mouseDelta = m_Sensitivity * new Vector2(GameplaySettings.GetLookXAxis(), -GameplaySettings.GetLookYAxis())*Time.deltaTime;

			//Quaternion CurrentRotation = transform.rotation;
			//Quaternion HorizontalRotation = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
			//Quaternion VerticalRotation = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);

			//transform.rotation = HorizontalRotation * CurrentRotation * VerticalRotation;


			m_RotationX += -GameplaySettings.GetLookYAxis() * m_LookSpeed * Time.deltaTime;
			m_RotationX = Mathf.Clamp(m_RotationX, -m_LookLimit, m_LookLimit);
			m_RotationY += GameplaySettings.GetLookXAxis() * Time.deltaTime * m_LookSpeed;

			transform.rotation = Quaternion.Euler(m_RotationX, m_RotationY, 0);
		}

		Vector3 GetAccelerationVector()
		{
			Vector3 moveInput = Vector3.zero;

			moveInput += Vector3.forward * GameplaySettings.GetVerticalInput();
			moveInput += Vector3.right * GameplaySettings.GetHorizontalInput();

			if (GameplaySettings.GetUp())
				moveInput += Vector3.up;
			if(GameplaySettings.GetDown())
				moveInput += Vector3.down;



			Vector3 direction = transform.TransformVector(moveInput.normalized);

			Vector3 Acceleration = direction * m_CameraAcceleration;

			if (GameplaySettings.GetSprint())
				return Acceleration* m_SprintBoost;

			return Acceleration;
		}

	}
}
