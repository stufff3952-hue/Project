using UnityEngine;

public class ScreenTeleport : MonoBehaviour
{
    [Header("ОТНОСИТЕЛЬНЫЕ ГРАНИЦЫ")]
    public float leftBoundary = -5f;
    public float rightBoundary = 5f;
    public float teleportOffset = 0.2f;

    [Header("ВИЗУАЛИЗАЦИЯ")]
    public bool showBoundaries = true;

    [Header("АВТОСЛЕЖЕНИЕ ЗА КАМЕРОЙ")]
    public bool followCamera = true;
    public float updateFrequency = 0.1f;
    private Camera mainCamera;
    private float actualLeftBoundary;
    private float actualRightBoundary;
    private float nextUpdateTime;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera не найдена!");
            return;
        }
        UpdateBoundaries();
    }
    void Update()
    {
        if (mainCamera == null) return;
        if (followCamera && Time.time > nextUpdateTime)
        {
            UpdateBoundaries();
            nextUpdateTime = Time.time + updateFrequency;
        }
        CheckForTeleport();
    }
    void UpdateBoundaries()
    {
        if (!followCamera || mainCamera == null) return;
        float cameraX = mainCamera.transform.position.x;
        actualLeftBoundary = cameraX + leftBoundary;
        actualRightBoundary = cameraX + rightBoundary;
    }

    void CheckForTeleport()
    {
        Vector3 playerPos = transform.position;
        float currentLeft = followCamera ? actualLeftBoundary : leftBoundary;
        float currentRight = followCamera ? actualRightBoundary : rightBoundary;
        if (playerPos.x > currentRight)
        {
            TeleportToLeft(currentLeft, currentRight);
        }
        else if (playerPos.x < currentLeft)
        {
            TeleportToRight(currentLeft, currentRight);
        }
    }

    void TeleportToLeft(float currentLeft, float currentRight)
    {
        Vector3 newPos = transform.position;

        if (followCamera)
        {
            newPos.x = currentLeft + teleportOffset;
        }
        else
        {
            float screenWidth = currentRight - currentLeft;
            newPos.x = currentLeft + teleportOffset;
        }

        transform.position = newPos;
        Debug.Log($"Телепортация → налево: X={newPos.x:F2}");
    }

    void TeleportToRight(float currentLeft, float currentRight)
    {
        Vector3 newPos = transform.position;

        if (followCamera)
        {
            newPos.x = currentRight - teleportOffset;
        }
        else
        {
            newPos.x = currentRight - teleportOffset;
        }

        transform.position = newPos;
        Debug.Log($"Телепортация направо: X={newPos.x:F2}");
    }
    [ContextMenu("Автонастройка под камеру")]
    void AutoSetupFromCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            if (mainCamera.orthographic)
            {
                float screenHeight = 2f * mainCamera.orthographicSize;
                float screenWidth = screenHeight * mainCamera.aspect;
                leftBoundary = -screenWidth / 2f;
                rightBoundary = screenWidth / 2f;

                Debug.Log($"Границы установлены: {leftBoundary} до {rightBoundary}");
                Debug.Log($"Камера в X={mainCamera.transform.position.x}");
            }
        }
    }

    [ContextMenu("Переключить режим следования")]
    void ToggleFollowMode()
    {
        followCamera = !followCamera;
        Debug.Log($"Режим следования за камерой: {(followCamera ? "ВКЛ" : "ВЫКЛ")}");
        UpdateBoundaries();
    }
    void OnDrawGizmos()
    {
        if (!showBoundaries) return;

        Gizmos.color = Color.yellow;

        if (Application.isPlaying && mainCamera != null && followCamera)
        {
            float cameraTop = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, 0)).y;
            float cameraBottom = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)).y;
            Gizmos.DrawLine(
                new Vector3(actualLeftBoundary, cameraBottom - 5, 0),
                new Vector3(actualLeftBoundary, cameraTop + 5, 0)
            );
            Gizmos.DrawLine(
                new Vector3(actualRightBoundary, cameraBottom - 5, 0),
                new Vector3(actualRightBoundary, cameraTop + 5, 0)
            );
            DrawArrow(actualLeftBoundary, (cameraTop + cameraBottom) / 2, Vector2.right);
            DrawArrow(actualRightBoundary, (cameraTop + cameraBottom) / 2, Vector2.left);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.Label(new Vector3(actualLeftBoundary, cameraTop + 1, 0),
                $"Левая ({actualLeftBoundary:F1})");
            UnityEditor.Handles.Label(new Vector3(actualRightBoundary, cameraTop + 1, 0),
                $"Правая ({actualRightBoundary:F1})");
#endif
        }
        else
        {
            Gizmos.DrawLine(new Vector3(leftBoundary, -100, 0), new Vector3(leftBoundary, 100, 0));
            Gizmos.DrawLine(new Vector3(rightBoundary, -100, 0), new Vector3(rightBoundary, 100, 0));
        }
    }
    void DrawArrow(float x, float y, Vector2 direction)
    {
        Vector3 pos = new Vector3(x, y, 0);
        float arrowSize = 0.5f;

        Gizmos.DrawRay(pos, direction * arrowSize);
        Gizmos.DrawRay(pos + (Vector3)(direction * arrowSize),
                      Quaternion.Euler(0, 0, 45) * -direction * arrowSize * 0.5f);
        Gizmos.DrawRay(pos + (Vector3)(direction * arrowSize),
                      Quaternion.Euler(0, 0, -45) * -direction * arrowSize * 0.5f);
    }
}
