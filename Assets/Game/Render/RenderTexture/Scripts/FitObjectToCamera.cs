using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FitObjectToCamera : MonoBehaviour
{
	private const float _paddingPlane = 1f;
	private const float _paddingSphere = 0.8f;

	[SerializeField] private GameObject _target;
	[SerializeField] private FitMethod _fitMethod = FitMethod.Plane;
	[SerializeField] private float _paddingFactor = 0f;
	[SerializeField, SingleLayer] private int _layer;

	private Camera _cam;
	public Camera Cam => _cam ? _cam : _cam = GetComponent<Camera>();

	private readonly Vector3[] _cornersBuffer = new Vector3[8];

	private void Awake() => FitObject();

	[Button]
	public void FitObject()
	{
		if (_target == null) return;

		switch (_fitMethod)
		{
			case FitMethod.Plane:
				FitObjectPlane();
				break;
			case FitMethod.Sphere:
				FitObjectSphere();
				break;
		}
	}

	/// <summary>
	/// Cadrage exact via la projection plane (Tangente + Frustum depth).
	/// </summary>
	public void FitObjectPlane()
	{
		Renderer[] renderers = _target.GetComponentsInChildren<Renderer>();
		if (renderers.Length == 0) return;

		Bounds bounds = CumulateBounds(renderers);

		FillBoundsCorners(bounds);

		float minX = float.MaxValue, maxX = float.MinValue;
		float minY = float.MaxValue, maxY = float.MinValue;
		float minZ = float.MaxValue, maxZ = float.MinValue;

		for (int i = 0; i < 8; i++)
		{
			Vector3 localPoint = Cam.transform.InverseTransformPoint(_cornersBuffer[i]);
			if (localPoint.x < minX) minX = localPoint.x;
			if (localPoint.x > maxX) maxX = localPoint.x;
			if (localPoint.y < minY) minY = localPoint.y;
			if (localPoint.y > maxY) maxY = localPoint.y;
			if (localPoint.z < minZ) minZ = localPoint.z;
			if (localPoint.z > maxZ) maxZ = localPoint.z;
		}

		float halfWidth = (maxX - minX) * 0.5f;
		float halfHeight = (maxY - minY) * 0.5f;
		float halfDepth = (maxZ - minZ) * 0.5f;

		float vFovRad = Cam.fieldOfView * Mathf.Deg2Rad;
		float hFovRad = 2f * Mathf.Atan(Mathf.Tan(vFovRad * 0.5f) * Cam.aspect);

		float distanceV = halfHeight / Mathf.Tan(vFovRad * 0.5f);
		float distanceH = halfWidth / Mathf.Tan(hFovRad * 0.5f);

		float requiredDistance = (Mathf.Max(distanceV, distanceH) + halfDepth) * (_paddingPlane + _paddingFactor);

		ApplyPosition(bounds, requiredDistance);
	}

	/// <summary>
	/// Cadrage rapide via la sphère englobante (Sinus).
	/// </summary>
	public void FitObjectSphere()
	{
		Renderer[] renderers = _target.GetComponentsInChildren<Renderer>();
		if (renderers.Length == 0) return;

		Bounds bounds = CumulateBounds(renderers);

		float radius = bounds.extents.magnitude;
		float cameraAspect = Cam.aspect;
		float vFovRad = Cam.fieldOfView * Mathf.Deg2Rad;
		float minFov = Mathf.Min(vFovRad, 2f * Mathf.Atan(Mathf.Tan(vFovRad * 0.5f) * cameraAspect));

		float requiredDistance = (radius / Mathf.Sin(minFov * 0.5f)) * (_paddingSphere + _paddingFactor);

		ApplyPosition(bounds, requiredDistance);
	}

	private Bounds CumulateBounds(Renderer[] renderers)
	{
		Bounds bounds = renderers[0].bounds;
		for (int i = 1; i < renderers.Length; i++)
		{
			bounds.Encapsulate(renderers[i].bounds);
		}
		return bounds;
	}

	private void FillBoundsCorners(Bounds b)
	{
		_cornersBuffer[0] = new Vector3(b.min.x, b.min.y, b.min.z);
		_cornersBuffer[1] = new Vector3(b.min.x, b.min.y, b.max.z);
		_cornersBuffer[2] = new Vector3(b.min.x, b.max.y, b.min.z);
		_cornersBuffer[3] = new Vector3(b.min.x, b.max.y, b.max.z);
		_cornersBuffer[4] = new Vector3(b.max.x, b.min.y, b.min.z);
		_cornersBuffer[5] = new Vector3(b.max.x, b.min.y, b.max.z);
		_cornersBuffer[6] = new Vector3(b.max.x, b.max.y, b.min.z);
		_cornersBuffer[7] = new Vector3(b.max.x, b.max.y, b.max.z);
	}

	private void ApplyPosition(Bounds bounds, float distance)
	{
		Vector3 desiredCenter = Cam.transform.position + (Cam.transform.forward * distance);
		Vector3 pivotToCenterOffset = bounds.center - _target.transform.position;

		_target.transform.position = desiredCenter - pivotToCenterOffset;
	}

	private void OnValidate()
	{
		FitObject();
	}

	private enum FitMethod
	{
		Plane,
		Sphere
	}
}