using UnityEngine;
using System.Collections;

public class Robot : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed   = 3f;
    public float rotateSpeed = 360f;
    public float cellSize    = 1f;

    [Header("Состояние")]
    public bool isMoving   = false;
    public bool isRotating = false;

    private Renderer robotRenderer;
    private bool hasColorProp  = false;
    private Color originalColor = Color.white;

    private readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
    private int currentDirIndex = 0;

    void Awake()
    {
        SyncDirFromRotation();
    }

    void Start()
    {
        robotRenderer = GetComponentInChildren<Renderer>();
        if (robotRenderer == null)
            robotRenderer = GetComponent<Renderer>();

        if (robotRenderer != null && robotRenderer.material.HasProperty("_Color"))
        {
            hasColorProp  = true;
            originalColor = robotRenderer.material.color;
        }
    }

    public void ResetDirection(int dirIndex = 1)
    {
        StopAllCoroutines();
        isMoving   = false;
        isRotating = false;
        currentDirIndex       = dirIndex;
        transform.rotation    = Quaternion.LookRotation(directions[currentDirIndex]);
    }

    public void TryMoveForward()
    {
        if (!gameObject.activeInHierarchy) return;
        if (isMoving || isRotating) return;
        Vector3 target = transform.position + directions[currentDirIndex] * cellSize;
        if (CanMoveTo(target)) StartCoroutine(MoveTo(target));
        else                   StartCoroutine(BumpEffect());
    }

    public void RotateRight()
    {
        if (!gameObject.activeInHierarchy) return;
        if (isMoving || isRotating) return;
        currentDirIndex = (currentDirIndex + 1) % 4;
        StartCoroutine(RotateTo(Quaternion.LookRotation(directions[currentDirIndex])));
    }

    public void RotateLeft()
    {
        if (!gameObject.activeInHierarchy) return;
        if (isMoving || isRotating) return;
        currentDirIndex = (currentDirIndex + 3) % 4;
        StartCoroutine(RotateTo(Quaternion.LookRotation(directions[currentDirIndex])));
    }

    public bool IsWallAhead()
    {
        return !CanMoveTo(transform.position + directions[currentDirIndex] * cellSize);
    }

    bool CanMoveTo(Vector3 pos)
    {
        if (Mathf.Abs(pos.x) > 4.5f || Mathf.Abs(pos.z) > 4.5f) return false;

        int mask = LayerMask.GetMask("Default");
        foreach (var col in Physics.OverlapBox(pos, Vector3.one * 0.4f, Quaternion.identity, mask))
            if (col.CompareTag("Wall")) return false;

        return true;
    }

    void SyncDirFromRotation()
    {
        Vector3 fwd = transform.forward; fwd.y = 0; fwd.Normalize();
        float best = -1f;
        for (int i = 0; i < directions.Length; i++)
        {
            float d = Vector3.Dot(fwd, directions[i]);
            if (d > best) { best = d; currentDirIndex = i; }
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;
        Vector3 start = transform.position;
        for (float t = 0f; t < 1f; t += Time.deltaTime * moveSpeed)
        {
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
        for (float t = 0f; t < 1f; t += Time.deltaTime * (rotateSpeed / 90f))
        {
            transform.rotation = Quaternion.Slerp(start, targetRot, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        transform.rotation = targetRot;
        isRotating = false;
    }

    IEnumerator BumpEffect()
    {
        Vector3 orig = transform.position;
        Vector3 bump = directions[currentDirIndex] * 0.12f;

        transform.position = orig + bump;
        if (hasColorProp && robotRenderer != null)
            robotRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.12f);
        transform.position = orig;

        yield return new WaitForSeconds(0.12f);
        if (hasColorProp && robotRenderer != null)
            robotRenderer.material.color = originalColor;
    }

    void CheckGoal()
    {
        foreach (var col in Physics.OverlapSphere(transform.position, 0.35f))
            if (col.CompareTag("Goal")) { GameManager.Instance?.OnGoalReached(); return; }
    }
}