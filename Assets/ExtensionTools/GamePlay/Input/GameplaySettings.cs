using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Gameplay
{
	[CreateAssetMenu(fileName = "GameplaySettings", menuName = "ExtensionTools/GameplaySettings", order = 1)]
	public class GameplaySettings : ScriptableObject
    {
		static GameplaySettings _instance;
		static GameplaySettings INSTANCE {
			get {
				if (_instance == null)
				{
					GameplaySettings settings = null;
					settings = Resources.Load<GameplaySettings>("ExtensionTools/GameplaySettings");

					if (settings == null)
						settings = ScriptableObject.CreateInstance<GameplaySettings>();

					_instance = settings;
				}

				return _instance;
			}
		}

		public static float GetHorizontalInput()
		{
			return GetAxisValue(INSTANCE.m_HorizontalAxes);
		}
		public static float GetVerticalInput()
		{
			return GetAxisValue(INSTANCE.m_VerticalAxes);
		}
		public static float GetLookXAxis()
		{
			return GetAxisValue(INSTANCE.m_XLookAxes);
		}
		public static float GetLookYAxis()
		{
			return GetAxisValue(INSTANCE.m_YLookAxes);
		}
		public static bool GetUp()
		{
			return GetKeyValue(INSTANCE.m_UpKeyCodes);
		}
		public static bool GetDown()
		{
			return GetKeyValue(INSTANCE.m_DownKeyCodes);
		}
		public static bool GetSprint()
		{
			return GetKeyValue(INSTANCE.m_SprintKeyCodes);
		}

		static float GetAxisValue(string[] axes)
		{
			float value = 0f;
			foreach (string axis in axes)
			{
				value += Input.GetAxis(axis);
			}

			return Mathf.Clamp(value, -1, 1f);
		}

		static bool GetKeyValue(KeyCode[] keyCodes)
		{
			foreach (KeyCode key in keyCodes)
			{
				if (Input.GetKey(key))
					return true;
			}

			return false;
		}


		[GroupItem("Input")] [SerializeField] string[] m_HorizontalAxes = new string[] { "Horizontal" };
		[GroupItem("Input")] [SerializeField] string[] m_VerticalAxes = new string[] { "Vertical" };
		[GroupItem("Input")] [SerializeField] string[] m_XLookAxes = new string[] { "Mouse X" };
		[GroupItem("Input")] [SerializeField] string[] m_YLookAxes = new string[] { "Mouse Y" };
		[GroupItem("Input")] [SerializeField] KeyCode[] m_UpKeyCodes = new KeyCode[] { KeyCode.Space, KeyCode.Joystick1Button0 };
		[GroupItem("Input")] [SerializeField] KeyCode[] m_DownKeyCodes = new KeyCode[] { KeyCode.LeftControl, KeyCode.Joystick1Button1 };
		[GroupItem("Input")] [SerializeField] KeyCode[] m_SprintKeyCodes = new KeyCode[] { KeyCode.LeftShift, KeyCode.Joystick1Button5 };
	}
}
