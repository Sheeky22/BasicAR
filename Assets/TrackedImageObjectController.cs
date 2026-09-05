using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImageObjectController : MonoBehaviour
{
    private ARTrackedImage arTrackedImage;
    private GameObject childVisual;
    private Renderer childRenderer;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.4f;
    private bool isDraggingObject = false;

    [Header("Double Tap Settings")]
    private float lastClickTime = 0f;
    [SerializeField] private float doubleClickDelay = 0.3f; // Time window to register a double-tap

    void Awake()
    {
        arTrackedImage = GetComponent<ARTrackedImage>();
        childVisual = transform.GetChild(0).gameObject;
        childRenderer = childVisual.GetComponent<Renderer>();
    }

    void Update()
    {
        // 1. Maintain visibility based on tracking lock
        bool isTracking = arTrackedImage.trackingState == TrackingState.Tracking;
        childVisual.SetActive(isTracking);

        if (!isTracking)
        {
            isDraggingObject = false;
            return;
        }

        // 2. Process input based on platform runtime environment
        if (Input.touchCount > 0)
        {
            HandleMobileTouch();
        }
        else
        {
            HandleEditorMouse();
        }
    }

    // --- MOBILE SCREEN GESTURES ---
    private void HandleMobileTouch()
    {
        Touch touch = Input.GetTouch(0);
        Vector3 screenPos = touch.position;

        if (touch.phase == TouchPhase.Began)
        {
            // Check if the initial touch point hits our sphere mesh bounds
            if (IsTouchingSphere(screenPos))
            {
                isDraggingObject = true;

                // Handle double tap detection window
                float timeSinceLastTap = Time.time - lastClickTime;
                if (timeSinceLastTap <= doubleClickDelay)
                {
                    TriggerRandomColor();
                }
                lastClickTime = Time.time;
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDraggingObject)
        {
            // Calculate horizontal drag vector across screen matrix
            float rotateDegree = touch.deltaPosition.x * rotationSpeed;
            childVisual.transform.Rotate(Vector3.up, -rotateDegree, Space.World);
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isDraggingObject = false;
        }
    }

    // --- EDITOR MOUSE SIMULATION ---
    private void HandleEditorMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchingSphere(mousePos))
            {
                isDraggingObject = true;

                // Handle double click detection window
                float timeSinceLastClick = Time.time - lastClickTime;
                if (timeSinceLastClick <= doubleClickDelay)
                {
                    TriggerRandomColor();
                }
                lastClickTime = Time.time;
            }
        }
        
        // If holding down left-click and moving mouse while dragging asset
        if (Input.GetMouseButton(0) && isDraggingObject)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * 15f;
            childVisual.transform.Rotate(Vector3.up, -mouseX, Space.World);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingObject = false;
        }
    }

    // --- UTILITY METHODS ---
    private bool IsTouchingSphere(Vector3 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit;

        // Returns true only if the raycast intersects our sphere's collider
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == childVisual.transform)
            {
                return true;
            }
        }
        return false;
    }

    private void TriggerRandomColor()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        childRenderer.material.color = randomColor;
    }
}