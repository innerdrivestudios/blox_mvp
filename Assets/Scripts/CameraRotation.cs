using UnityEngine;

/**
 * Orbits the Camera slightly around the playing field.
 */
public class CameraRotation : MonoBehaviour
{
    [SerializeField]    
    private Vector2 xyRotationRange;
    
    [Range(0.01f, 100)] [SerializeField] 
    private float easeSpeed;

    private Camera mainCamera;

	private void Awake()
	{
        mainCamera = Camera.main;
	}

	// Update is called once per frame
	void Update()
    {
        Vector3 eulerAngles = transform.eulerAngles;
        Vector3 viewportSpaceMouse = mainCamera.ScreenToViewportPoint(Input.mousePosition);

        eulerAngles.x = xyRotationRange.x * (viewportSpaceMouse.y - 0.5f);
        eulerAngles.y = xyRotationRange.y * (viewportSpaceMouse.x - 0.5f);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(eulerAngles), easeSpeed * Time.deltaTime);
    }
}
