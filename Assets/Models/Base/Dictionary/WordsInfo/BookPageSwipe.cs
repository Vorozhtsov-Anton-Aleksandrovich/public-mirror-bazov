using UnityEngine;
using UnityEngine.XR.Hands;
using System.Linq;

/// <summary>
/// Вешается на Book_Opened.
/// Отслеживает свайп ладонью по странице и листает страницы.
/// </summary>
public class BookPageSwipe : MonoBehaviour
{
    [Header("Ссылки на руки")]
    public HandGrabController leftHand;
    public HandGrabController rightHand;

    [Header("Настройки свайпа")]
    [Tooltip("Минимальное расстояние свайпа (метры)")]
    public float swipeThreshold = 0.08f;

    [Tooltip("Максимальное время свайпа (секунды)")]
    public float swipeTimeLimit = 0.6f;

    [Tooltip("Радиус обнаружения ладони над страницей")]
    public float detectionRadius = 0.12f;

    [Tooltip("Кулдаун между перелистываниями (секунды)")]
    public float cooldown = 0.8f;

    private BookPageManager pageManager;
    private SwipeData leftSwipe = new SwipeData();
    private SwipeData rightSwipe = new SwipeData();
    private float lastSwipeTime = -999f;

    class SwipeData
    {
        public bool isTracking = false;
        public Vector3 startPosition;
        public float startTime;
    }

    void Awake()
    {
        pageManager = GetComponent<BookPageManager>();
    }

    void Update()
    {
        if (Time.time - lastSwipeTime < cooldown) return;
        ProcessHand(leftHand, leftSwipe);
        ProcessHand(rightHand, rightSwipe);
    }

    void ProcessHand(HandGrabController hand, SwipeData swipe)
    {
        if (hand == null) return;

        var skeletonDriver = hand.GetComponent<XRHandSkeletonDriver>();
        if (skeletonDriver == null) return;

        Transform palm = skeletonDriver.jointTransformReferences
            .FirstOrDefault(r => r.xrHandJointID == XRHandJointID.Palm).jointTransform;
        if (palm == null) return;

        Vector3 palmPos = palm.position;
        bool isOverPage = IsOverPage(palmPos);
        float elapsed = swipe.isTracking ? Time.time - swipe.startTime : 0f;

        if (!swipe.isTracking)
        {
            // Рука зашла в зону — начинаем запись
            if (isOverPage)
            {
                swipe.isTracking = true;
                swipe.startPosition = palmPos;
                swipe.startTime = Time.time;
            }
        }
        else
        {
            // Свайп завершён — рука вышла из зоны
            if (!isOverPage)
            {
                TryDetectSwipe(swipe, palmPos);
                swipe.isTracking = false;
            }
            // Таймаут — слишком долго держит руку
            else if (elapsed > swipeTimeLimit)
            {
                // Сбрасываем без срабатывания — это не свайп, а удержание
                swipe.isTracking = false;
                Debug.Log("[BookPageSwipe] Таймаут — не свайп, сброс");
            }
        }
    }

    bool IsOverPage(Vector3 palmPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(palmPos);
        bool withinDepth = Mathf.Abs(localPos.y) < detectionRadius;
        bool withinWidth = Mathf.Abs(localPos.x) < 0.25f;
        bool withinHeight = Mathf.Abs(localPos.z) < 0.15f;
        return withinDepth && withinWidth && withinHeight;
    }

    void TryDetectSwipe(SwipeData swipe, Vector3 endPosition)
    {
        Vector3 delta = transform.InverseTransformDirection(endPosition - swipe.startPosition);
        float horizontalDelta = delta.x;

        Debug.Log($"[BookPageSwipe] delta.x = {horizontalDelta:F3}");

        if (Mathf.Abs(horizontalDelta) < swipeThreshold)
        {
            Debug.Log("[BookPageSwipe] Слишком короткий свайп — игнор");
            return;
        }

        lastSwipeTime = Time.time;

        if (horizontalDelta > 0)
        {
            pageManager?.NextPage();
            Debug.Log("[BookPageSwipe] → Следующая страница");
        }
        else
        {
            pageManager?.PrevPage();
            Debug.Log("[BookPageSwipe] ← Предыдущая страница");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(0.5f, detectionRadius * 2, 0.3f));
    }
}
