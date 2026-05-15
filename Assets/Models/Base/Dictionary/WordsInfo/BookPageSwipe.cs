using UnityEngine;
using UnityEngine.XR.Hands;
using System.Linq;

/// <summary>
/// Вешается на Book_Opened.
/// Отслеживает свайп ладонью по странице и листает страницы.
/// Требует: BookPageManager на том же объекте.
/// </summary>
public class BookPageSwipe : MonoBehaviour
{
    [Header("Ссылки на руки")]
    public HandGrabController leftHand;
    public HandGrabController rightHand;

    [Header("Настройки свайпа")]
    [Tooltip("Минимальное расстояние свайпа для срабатывания (в метрах)")]
    public float swipeThreshold = 0.08f;

    [Tooltip("Максимальное время свайпа (секунды)")]
    public float swipeTimeLimit = 0.5f;

    [Tooltip("Радиус обнаружения ладони над страницей")]
    public float detectionRadius = 0.12f;

    [Tooltip("Кулдаун между перелистываниями (секунды)")]
    public float cooldown = 0.8f;

    private BookPageManager pageManager;

    // Данные свайпа для каждой руки
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

        // Получаем позицию ладони через XRHandSkeletonDriver
        var skeletonDriver = hand.GetComponent<XRHandSkeletonDriver>();
        if (skeletonDriver == null) return;

        Transform palm = skeletonDriver.jointTransformReferences
            .FirstOrDefault(r => r.xrHandJointID == XRHandJointID.Palm).jointTransform;

        if (palm == null) return;

        Vector3 palmPos = palm.position;

        // Проверяем — ладонь над страницей книги?
        bool isOverPage = IsOverPage(palmPos);

        if (isOverPage && !swipe.isTracking)
        {
            // Начинаем отслеживать свайп
            swipe.isTracking = true;
            swipe.startPosition = palmPos;
            swipe.startTime = Time.time;
        }
        else if (swipe.isTracking)
        {
            float elapsed = Time.time - swipe.startTime;

            if (!isOverPage || elapsed > swipeTimeLimit)
            {
                if (isOverPage)
                {
                    // Рука ушла — проверяем свайп
                    TryDetectSwipe(swipe, palmPos);
                }
                swipe.isTracking = false;
            }
        }
    }

    bool IsOverPage(Vector3 palmPos)
    {
        // Проверяем близость ладони к плоскости книги
        Vector3 localPos = transform.InverseTransformPoint(palmPos);

        // Книга лежит примерно в плоскости XZ локальных координат
        bool withinDepth = Mathf.Abs(localPos.y) < detectionRadius;
        bool withinWidth = Mathf.Abs(localPos.x) < 0.25f;
        bool withinHeight = Mathf.Abs(localPos.z) < 0.15f;

        return withinDepth && withinWidth && withinHeight;
    }

    void TryDetectSwipe(SwipeData swipe, Vector3 endPosition)
    {
        // Вектор свайпа в локальных координатах книги
        Vector3 delta = transform.InverseTransformDirection(endPosition - swipe.startPosition);
        float horizontalDelta = delta.x;

        if (Mathf.Abs(horizontalDelta) < swipeThreshold) return;

        lastSwipeTime = Time.time;

        if (horizontalDelta < 0)
        {
            // Свайп влево → следующая страница
            pageManager?.NextPage();
            Debug.Log("[BookPageSwipe] Свайп влево → следующая страница");
        }
        else
        {
            // Свайп вправо → предыдущая страница
            pageManager?.PrevPage();
            Debug.Log("[BookPageSwipe] Свайп вправо → предыдущая страница");
        }
    }

    // Визуализация зоны обнаружения в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(0.5f, detectionRadius * 2, 0.3f));
    }
}
