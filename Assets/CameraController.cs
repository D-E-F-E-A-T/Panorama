using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class CameraController : MonoBehaviour
{
	[SerializeField] [Range(1, 10)] private float _rotationSensitivityX = 5;
	[SerializeField] [Range(1, 10)] private float _rotationSensitivityY = 5;

	[SerializeField] [Range(-360, 0)] private float _minimumAngleX = -360;
	[SerializeField] [Range(0, 360)] private float _maximumAngleX = 360;

	[SerializeField] [Range(-360, 0)] private float _minimumAngleY = -360;
	[SerializeField] [Range(0, 360)] private float _maximumAngleY = 360;

	[SerializeField] [Range(1, 60)] private int _frameCounter = 30;

	private Camera _camera = null;
	private Quaternion _defaultCameraRotation;

	private List<float> _rotationListX = new List<float>();
	private List<float> _rotationListY = new List<float>();

	private float _rotationX = 0;
	private float _rotationY = 0;

	private float _rotationAverageX = 0;
	private float _rotationAverageY = 0;

	void Start()
	{
		_camera = Camera.main;
		_defaultCameraRotation = _camera.transform.localRotation;
	}

	void LateUpdate()
	{
		HandleCameraMovement();
	}

	private void HandleCameraMovement()
	{
		float threshold = 1f; // camera border threshold.

		if (Input.GetKey(KeyCode.Mouse0))
		{
			_rotationAverageX = 0f;
			_rotationAverageY = 0f;

			if (_rotationX <= _maximumAngleX && _rotationX >= _minimumAngleX)
			{
				_rotationX += Input.GetAxis("Mouse X") * _rotationSensitivityX;
			}
			else if (_rotationX > _maximumAngleX - threshold)
			{
				_rotationX = _maximumAngleX;
			}
			else if (_rotationX < _minimumAngleX + threshold)
			{
				_rotationX = _minimumAngleX;
			}

			if (_rotationY <= _maximumAngleY && _rotationY >= _minimumAngleY)
			{
				_rotationY += Input.GetAxis("Mouse Y") * _rotationSensitivityY;
			}
			else if (_rotationY > _maximumAngleY - threshold)
			{
				_rotationY = _maximumAngleY;
			}
			else if (_rotationY < _minimumAngleY + threshold)
			{
				_rotationY = _minimumAngleY;
			}

			_rotationListX.Add(_rotationX);
			_rotationListY.Add(_rotationY);

			if (_rotationListX.Count >= _frameCounter)
			{
				_rotationListX.RemoveAt(0);
			}

			if (_rotationListY.Count >= _frameCounter)
			{
				_rotationListY.RemoveAt(0);
			}

			foreach (float rotation in _rotationListX)
			{
				_rotationAverageX += rotation;
			}

			foreach (float rotation in _rotationListY)
			{
				_rotationAverageY += rotation;
			}

			_rotationAverageX /= _rotationListX.Count;
			_rotationAverageY /= _rotationListY.Count;

			_rotationAverageX = ClampAngle(_rotationAverageX, _minimumAngleX, _maximumAngleX);
			_rotationAverageY = ClampAngle(_rotationAverageY, _minimumAngleY, _maximumAngleY);

			Quaternion xQuaternion = Quaternion.AngleAxis(_rotationAverageX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis(_rotationAverageY, Vector3.left);

			_camera.transform.localRotation = _defaultCameraRotation * xQuaternion * yQuaternion;
		}

		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			ResetCameraMovement();
		}
	}

	private void ResetCameraMovement()
	{
		_rotationX = _rotationAverageX;
		_rotationY = _rotationAverageY;

		_rotationListX.Clear();
		_rotationListY.Clear();

		// Little hack. After resetting camera position and clearing X/Y rotation lists,
		// we're filling lists with zero values.
		for (int i = 0; i < _frameCounter; i++)
		{
			_rotationListX.Add(_rotationX);
			_rotationListY.Add(_rotationY);
		}
	}

	private float ClampAngle(float angle, float min, float max)
	{
		angle = angle % 360;

		if ((angle >= -360) && (angle <= 360))
		{
			if (angle < -360)
			{
				angle += 360;
			}

			if (angle > 360)
			{
				angle -= 360;
			}
		}

		return Mathf.Clamp(angle, min, max);
	}
}
