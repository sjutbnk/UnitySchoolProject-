using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Ссылки")]
    public Robot robot;
    public Transform levelContainer;
    
    [Header("Настройки")]
    public int currentLevel = 1;
    public float cellSize = 1f;
    public int gridSize = 9;
    
    private List<GameObject> levelObjects = new List<GameObject>();
    private GameObject goalObject;
    private Keyboard keyboard;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (levelContainer == null)
        {
            levelContainer = new GameObject("LevelContainer").transform;
        }
        
        keyboard = Keyboard.current;
    }
    
    void Start()
    {
        LoadLevel(currentLevel);
    }
    
    void Update()
    {
        if (robot == null || keyboard == null) return;
        
        // Блокируем ввод пока робот двигается
        if (robot.isMoving || robot.isRotating) return;
        
        if (keyboard.wKey.wasPressedThisFrame) robot.TryMoveForward();
        if (keyboard.aKey.wasPressedThisFrame) robot.RotateLeft();
        if (keyboard.dKey.wasPressedThisFrame) robot.RotateRight();
        if (keyboard.rKey.wasPressedThisFrame) ResetLevel();
        if (keyboard.nKey.wasPressedThisFrame) NextLevel();
    }
    
    public void LoadLevel(int level)
    {
        currentLevel = level;
        ClearLevel();
        
        GenerateGrid();
        
        switch (level)
        {
            case 1:
                CreateLevel1();
                break;
            case 2:
                CreateLevel2();
                break;
            default:
                CreateRandomLevel();
                break;
        }
        
        ResetRobot();
    }
    
    void CreateLevel1()
    {
        // Простой уровень: робот -> цель
        CreateGoal(new Vector3(3, 0.1f, 0));
    }
    
    void CreateLevel2()
    {
        // Уровень со стеной
        CreateWall(new Vector3(1, 0.5f, 0));
        CreateWall(new Vector3(2, 0.5f, 0));
        CreateGoal(new Vector3(4, 0.1f, 0));
    }
    
    void CreateRandomLevel()
    {
        Vector3 goalPos = new Vector3(4, 0.1f, Random.Range(-2, 3));
        Vector3 robotStartPos = new Vector3(-3, 0.5f, 0);
        
        // Случайные стены
        int wallCount = Mathf.Min(3 + currentLevel, 8);
        int attempts = 0;
        int createdWalls = 0;
        
        while (createdWalls < wallCount && attempts < 50)
        {
            attempts++;
            Vector3 pos = new Vector3(
                Random.Range(-3, 4),
                0.5f,
                Random.Range(-3, 4)
            );
            
            // Не создавать стену на старте робота, цели или слишком близко к роботу
            if (Vector3.Distance(pos, robotStartPos) < 1.5f) continue;
            if (Vector3.Distance(new Vector3(pos.x, 0.1f, pos.z), goalPos) < 1f) continue;
            
            // Проверка что стена не пересекается с другими
            bool overlaps = false;
            foreach (GameObject obj in levelObjects)
            {
                if (obj != null && obj.name.StartsWith("Wall") && Vector3.Distance(obj.transform.position, pos) < 0.5f)
                {
                    overlaps = true;
                    break;
                }
            }
            
            if (!overlaps)
            {
                CreateWall(pos);
                createdWalls++;
            }
        }
        
        CreateGoal(goalPos);
    }
    
    void GenerateGrid()
    {
        int offset = gridSize / 2;
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 pos = new Vector3(x - offset, 0, z - offset);
                
                // Пол
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                floor.transform.position = pos;
                floor.transform.rotation = Quaternion.Euler(90, 0, 0);
                floor.transform.localScale = Vector3.one * cellSize * 0.95f;
                floor.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
                floor.transform.SetParent(levelContainer);
                floor.name = $"Floor_{x}_{z}";
                levelObjects.Add(floor);
                
                Destroy(floor.GetComponent<Collider>());
            }
        }
    }
    
    void CreateWall(Vector3 pos)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = pos;
        wall.transform.localScale = Vector3.one * cellSize;
        wall.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
        wall.tag = "Wall";
        wall.name = "Wall";
        wall.transform.SetParent(levelContainer);
        levelObjects.Add(wall);
    }
    
    void CreateGoal(Vector3 pos)
    {
        goalObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalObject.transform.position = pos;
        goalObject.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
        goalObject.GetComponent<Renderer>().material.color = Color.yellow;
        goalObject.tag = "Goal";
        goalObject.name = "Goal";
        goalObject.transform.SetParent(levelContainer);
        levelObjects.Add(goalObject);
        
        Collider col = goalObject.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
    
    void ResetRobot()
    {
        if (robot != null)
        {
            robot.transform.position = new Vector3(-3, 0.5f, 0);
            // Робот всегда смотрит направо (по оси X)
            robot.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }
    
    void ClearLevel()
    {
        foreach (GameObject obj in levelObjects)
        {
            if (obj != null) Destroy(obj);
        }
        levelObjects.Clear();
    }
    
    public void NextLevel()
    {
        LoadLevel(currentLevel + 1);
    }
    
    public void ResetLevel()
    {
        LoadLevel(currentLevel);
    }
    
    public void OnGoalReached()
    {
        StartCoroutine(GoalAnimation());
    }
    
    IEnumerator GoalAnimation()
    {
        if (goalObject != null)
        {
            Vector3 originalScale = goalObject.transform.localScale;
            
            for (int i = 0; i < 20; i++)
            {
                if (goalObject == null) yield break;
                
                float pulse = Mathf.Sin(i * 0.3f) * 0.3f + 1f;
                goalObject.transform.localScale = originalScale * pulse;
                yield return new WaitForSeconds(0.05f);
            }
            
            if (goalObject != null)
                goalObject.transform.localScale = originalScale;
        }
        
        yield return new WaitForSeconds(1f);
        NextLevel();
    }
}
