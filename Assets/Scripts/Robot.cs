using UnityEngine;
using System.Collections;

public class Robot : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 360f;
    public float cellSize = 1f;
    
    [Header("Состояние")]
    public bool isMoving = false;
    public bool isRotating = false;
    
    private Renderer robotRenderer;
    private Color originalColor;
    private Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
    private int currentDirIndex = 0;
    
    void Awake()
    {
        // Синхронизируем индекс направления с текущей rotation робота
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        // Находим ближайшее направление
        float maxDot = -1f;
        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(forward, directions[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                currentDirIndex = i;
            }
        }
    }
    
    void Start()
    {
        robotRenderer = GetComponent<Renderer>();
        if (robotRenderer != null)
            originalColor = robotRenderer.material.color;
    }
    
    public void TryMoveForward()
    {
        if (isMoving || isRotating) return;
        
        Vector3 targetPos = transform.position + directions[currentDirIndex] * cellSize;
        
        if (CanMoveTo(targetPos))
            StartCoroutine(MoveTo(targetPos));
        else
            StartCoroutine(CollisionEffect());
    }
    
    public void RotateRight()
    {
        if (isMoving || isRotating) return;
        
        currentDirIndex = (currentDirIndex + 1) % 4;
        StartCoroutine(RotateTo(Quaternion.LookRotation(directions[currentDirIndex])));
    }
    
    public void RotateLeft()
    {
        if (isMoving || isRotating) return;
        
        currentDirIndex = (currentDirIndex + 3) % 4; // +3 = -1 mod 4
        StartCoroutine(RotateTo(Quaternion.LookRotation(directions[currentDirIndex])));
    }
    
    bool CanMoveTo(Vector3 pos)
    {
        // Проверка границ сетки (-4 до 4)
        if (Mathf.Abs(pos.x) > 4f || Mathf.Abs(pos.z) > 4f)
            return false;
            
        // Исключаем слой робота из проверки
        int wallLayer = LayerMask.GetMask("Default");
        Collider[] colliders = Physics.OverlapBox(pos, Vector3.one * 0.4f, Quaternion.identity, wallLayer);
        foreach (Collider col in colliders)
            if (col.CompareTag("Wall"))
                return false;
                
        return true;
    }
    
    // Публичный метод для проверки стены впереди (для UI)
    public bool IsWallAhead()
    {
        Vector3 targetPos = transform.position + directions[currentDirIndex] * cellSize;
        return !CanMoveTo(targetPos);
    }
    
    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0f;
        
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        transform.position = target;
        isMoving = false;
        CheckGoal();
    }
    
    IEnumerator RotateTo(Quaternion targetRot)
    {
        isRotating = true;
        Quaternion start = transform.rotation;
        float t = 0f;
        
        while (t < 1f)
        {
            t += Time.deltaTime * (rotateSpeed / 90f);
            transform.rotation = Quaternion.Slerp(start, targetRot, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        transform.rotation = targetRot;
        isRotating = false;
    }
    
    IEnumerator CollisionEffect()
    {
        if (robotRenderer != null)
            robotRenderer.material.color = Color.red;
            
        Vector3 originalPos = transform.position;
        Vector3 bumpDir = -directions[currentDirIndex] * 0.1f;
        
        transform.position += bumpDir;
        yield return new WaitForSeconds(0.1f);
        transform.position = originalPos;
        
        if (robotRenderer != null)
            robotRenderer.material.color = originalColor;
    }
    
    void CheckGoal()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.3f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Goal"))
            {
                GameManager.Instance?.OnGoalReached();
                return;
            }
        }
    }
}
