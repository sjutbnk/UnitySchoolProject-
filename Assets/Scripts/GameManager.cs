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

    public int LevelNumber => currentLevel;

    private List<GameObject> levelObjects = new List<GameObject>();
    private GameObject goalObject;
    private Keyboard keyboard;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (levelContainer == null)
            levelContainer = new GameObject("LevelContainer").transform;

        keyboard = Keyboard.current;
    }

    void Start()
    {
        LoadLevel(currentLevel);
    }

    void Update()
    {
        if (robot == null || keyboard == null) return;
        if (robot.isMoving || robot.isRotating) return;

        if (keyboard.wKey.wasPressedThisFrame) robot.TryMoveForward();
        if (keyboard.aKey.wasPressedThisFrame) robot.RotateLeft();
        if (keyboard.dKey.wasPressedThisFrame) robot.RotateRight();
        if (keyboard.rKey.wasPressedThisFrame) ResetLevel();
        if (keyboard.nKey.wasPressedThisFrame) NextLevel();
    }

    void ClearLevel()
    {
        foreach (var obj in levelObjects)
            if (obj != null) Destroy(obj);
        levelObjects.Clear();
        goalObject = null;
    }

    public void LoadLevel(int level)
    {
        currentLevel = level;
        ClearLevel();
        GenerateGrid();

        switch (level)
        {
            case 1: CreateLevel1(); break;
            case 2: CreateLevel2(); break;
            case 3: CreateLevel3(); break;
            case 4: CreateLevel4(); break;
            case 5: CreateLevel5(); break;
            case 6: CreateLevel6(); break;
            default: CreateLevel1(); break;
        }

        PlaceRobot();
    }

    void CreateLevel1()
    {
        CreateGoal(new Vector3(3, 0.1f, 0));
    }

    void CreateLevel2()
    {
        CreateWall(new Vector3(1, 0.5f, 0));
        CreateWall(new Vector3(2, 0.5f, 0));
        CreateGoal(new Vector3(4, 0.1f, 0));
    }

    void CreateLevel3()
    {
        CreateWall(new Vector3(1, 0.5f, 0));
        CreateWall(new Vector3(1, 0.5f, 1));
        CreateWall(new Vector3(-1, 0.5f, -1));
        CreateGoal(new Vector3(3, 0.1f, 0));
    }

    void CreateLevel4()
    {
        CreateWall(new Vector3(0, 0.5f, 0));
        CreateWall(new Vector3(0, 0.5f, 1));
        CreateWall(new Vector3(0, 0.5f, 2));
        CreateWall(new Vector3(1, 0.5f, -1));
        CreateWall(new Vector3(2, 0.5f, -1));
        CreateWall(new Vector3(-2, 0.5f, 1));
        CreateGoal(new Vector3(4, 0.1f, -2));
    }

    void CreateLevel5()
    {
        CreateWall(new Vector3(1, 0.5f, 0));
        CreateWall(new Vector3(1, 0.5f, 2));
        CreateWall(new Vector3(2, 0.5f, 1));
        CreateWall(new Vector3(-1, 0.5f, -1));
        CreateWall(new Vector3(-1, 0.5f, 0));
        CreateWall(new Vector3(-2, 0.5f, 2));
        CreateWall(new Vector3(3, 0.5f, -2));
        CreateGoal(new Vector3(-4, 0.1f, 2));
    }

    void CreateLevel6()
    {
        CreateWall(new Vector3(0, 0.5f, 0));
        CreateWall(new Vector3(1, 0.5f, 1));
        CreateWall(new Vector3(1, 0.5f, -1));
        CreateWall(new Vector3(-1, 0.5f, 1));
        CreateWall(new Vector3(-1, 0.5f, -1));
        CreateWall(new Vector3(2, 0.5f, 0));
        CreateWall(new Vector3(-2, 0.5f, 0));
        CreateWall(new Vector3(3, 0.5f, 2));
        CreateWall(new Vector3(3, 0.5f, -2));
        CreateGoal(new Vector3(4, 0.1f, 0));
    }

    void GenerateGrid()
    {
        int offset = gridSize / 2;
        for (int x = 0; x < gridSize; x++)
        for (int z = 0; z < gridSize; z++)
        {
            var pos  = new Vector3(x - offset, 0, z - offset);
            var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tile.transform.position   = pos;
            tile.transform.rotation   = Quaternion.Euler(90, 0, 0);
            tile.transform.localScale = Vector3.one * cellSize * 0.95f;
            tile.GetComponent<Renderer>().material.color =
                (x + z) % 2 == 0
                    ? new Color(0.18f, 0.18f, 0.20f)
                    : new Color(0.22f, 0.22f, 0.24f);
            tile.transform.SetParent(levelContainer);
            tile.name = "Floor";
            Destroy(tile.GetComponent<Collider>());
            levelObjects.Add(tile);
        }
    }

    void CreateWall(Vector3 pos)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position   = pos;
        wall.transform.localScale = Vector3.one * cellSize;
        wall.GetComponent<Renderer>().material.color = new Color(0.35f, 0.35f, 0.40f);
        wall.tag  = "Wall";
        wall.name = "Wall";
        wall.transform.SetParent(levelContainer);
        levelObjects.Add(wall);
    }

    void CreateGoal(Vector3 pos)
    {
        goalObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalObject.transform.position   = pos;
        goalObject.transform.localScale = new Vector3(0.7f, 0.04f, 0.7f);
        goalObject.GetComponent<Renderer>().material.color = new Color(1f, 0.85f, 0.1f);
        goalObject.tag  = "Goal";
        goalObject.name = "Goal";
        goalObject.transform.SetParent(levelContainer);
        var col = goalObject.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        levelObjects.Add(goalObject);
    }

    void PlaceRobot()
    {
        if (robot == null) return;
        robot.transform.position = new Vector3(-3, 0.5f, 0);
        robot.ResetDirection(1);
    }

    public void ResetLevel() => LoadLevel(currentLevel);
    public void NextLevel()  => LoadLevel(currentLevel + 1);

    public void OnGoalReached() => StartCoroutine(GoalAnimation());

    IEnumerator GoalAnimation()
    {
        if (goalObject != null)
        {
            var orig = goalObject.transform.localScale;
            for (int i = 0; i < 20; i++)
            {
                if (goalObject == null) yield break;
                goalObject.transform.localScale = orig * (Mathf.Sin(i * 0.3f) * 0.35f + 1f);
                yield return new WaitForSeconds(0.05f);
            }
            if (goalObject != null) goalObject.transform.localScale = orig;
        }
        yield return new WaitForSeconds(0.8f);
        NextLevel();
    }
}
