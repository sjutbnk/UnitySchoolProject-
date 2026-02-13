using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;

[RequireComponent(typeof(UIDocument))]
public class RobotUI : MonoBehaviour
{
    [Header("Ссылки")]
    public Robot robot;
    public GameManager gameManager;
    
    [Header("Настройки")]
    public int maxCommands = 25;
    public int maxIterations = 100;
    
    private UIDocument uiDocument;
    private VisualElement root;
    private List<Block> blocks = new List<Block>();
    private bool isRunning = false;
    private int currentBlockIndex = 0;
    private int totalIterations = 0;
    
    // UI элементы
    private Label statusLabel;
    private ScrollView programScrollView;
    private VisualElement commandList;
    private VisualElement safeArea;
    
    // Цвета минималистичного стиля
    private readonly Color COLOR_BG = new Color(0.12f, 0.12f, 0.14f, 1f);
    private readonly Color COLOR_PANEL = new Color(0.18f, 0.18f, 0.20f, 1f);
    private readonly Color COLOR_BLOCK = new Color(0.25f, 0.25f, 0.28f, 1f);
    private readonly Color COLOR_BLOCK_HOVER = new Color(0.30f, 0.30f, 0.33f, 1f);
    private readonly Color COLOR_TEXT = new Color(0.90f, 0.90f, 0.92f, 1f);
    private readonly Color COLOR_TEXT_SECONDARY = new Color(0.60f, 0.60f, 0.62f, 1f);
    private readonly Color COLOR_ACCENT = new Color(0.35f, 0.40f, 0.48f, 1f);
    private readonly Color COLOR_SUCCESS = new Color(0.25f, 0.45f, 0.35f, 1f);
    private readonly Color COLOR_WARNING = new Color(0.55f, 0.45f, 0.25f, 1f);
    private readonly Color COLOR_ERROR = new Color(0.48f, 0.25f, 0.25f, 1f);
    
    void Start()
    {
        UpdateRobotReference();
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        BuildUI();
        Screen.orientation = ScreenOrientation.Portrait;
        
        Debug.Log("[RobotUI] UI инициализирован");
    }
    
    void Update()
    {
        if (robot == null || !robot.gameObject.activeInHierarchy)
        {
            UpdateRobotReference();
        }
    }
    
    void UpdateRobotReference()
    {
        var foundRobot = FindObjectOfType<Robot>();
        if (foundRobot != null && foundRobot.gameObject.activeInHierarchy)
        {
            robot = foundRobot;
            Debug.Log($"[RobotUI] Робот найден: {robot.name}");
        }
    }
    
    void BuildUI()
    {
        uiDocument.panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
        
        // UI занимает нижнюю часть (55% для большего интерфейса)
        var uiContainer = new VisualElement();
        uiContainer.style.position = Position.Absolute;
        uiContainer.style.bottom = 0;
        uiContainer.style.left = 0;
        uiContainer.style.right = 0;
        uiContainer.style.height = new Length(55, LengthUnit.Percent);
        uiContainer.style.flexDirection = FlexDirection.Row;
        uiContainer.style.backgroundColor = COLOR_BG;
        
        safeArea = new VisualElement();
        safeArea.style.flexGrow = 1;
        safeArea.style.flexDirection = FlexDirection.Row;
        safeArea.style.marginLeft = Screen.safeArea.x;
        safeArea.style.marginRight = Screen.width - Screen.safeArea.x - Screen.safeArea.width;
        safeArea.style.marginBottom = Screen.height - Screen.safeArea.y - Screen.safeArea.height;
        uiContainer.Add(safeArea);
        
        root.Add(uiContainer);
        
        // ЛЕВАЯ ПАНЕЛЬ - Блоки (45% - шире)
        var blocksPanel = new VisualElement();
        blocksPanel.style.width = new Length(45, LengthUnit.Percent);
        blocksPanel.style.flexDirection = FlexDirection.Column;
        blocksPanel.style.paddingLeft = 12;
        blocksPanel.style.paddingRight = 12;
        blocksPanel.style.paddingTop = 12;
        blocksPanel.style.paddingBottom = 12;
        blocksPanel.style.backgroundColor = COLOR_PANEL;
        blocksPanel.style.borderRightWidth = 1;
        blocksPanel.style.borderRightColor = new Color(0.08f, 0.08f, 0.10f, 1f);
        safeArea.Add(blocksPanel);
        
        // Заголовок
        var blocksTitle = new Label("БЛОКИ");
        blocksTitle.style.fontSize = 22;
        blocksTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        blocksTitle.style.color = COLOR_TEXT;
        blocksTitle.style.marginBottom = 12;
        blocksTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
        blocksTitle.style.letterSpacing = 2;
        blocksPanel.Add(blocksTitle);
        
        // Базовые команды
        var basicRow = new VisualElement();
        basicRow.style.flexDirection = FlexDirection.Row;
        basicRow.style.justifyContent = Justify.SpaceBetween;
        basicRow.style.marginBottom = 8;
        blocksPanel.Add(basicRow);
        
        basicRow.Add(CreateBlockButton("ВПЕРЕД", () => AddBlock(BlockType.MoveForward)));
        basicRow.Add(CreateBlockButton("НАЛЕВО", () => AddBlock(BlockType.RotateLeft)));
        basicRow.Add(CreateBlockButton("НАПРАВО", () => AddBlock(BlockType.RotateRight)));
        
        // Разделитель
        var divider1 = new VisualElement();
        divider1.style.height = 1;
        divider1.style.backgroundColor = new Color(0.3f, 0.3f, 0.32f, 1f);
        divider1.style.marginTop = 12;
        divider1.style.marginBottom = 12;
        blocksPanel.Add(divider1);
        
        // Циклы
        var loopsLabel = new Label("ЦИКЛЫ");
        loopsLabel.style.fontSize = 16;
        loopsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        loopsLabel.style.color = COLOR_TEXT_SECONDARY;
        loopsLabel.style.marginBottom = 10;
        loopsLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        loopsLabel.style.letterSpacing = 1;
        blocksPanel.Add(loopsLabel);
        
        // Счетные циклы
        var countRow1 = new VisualElement();
        countRow1.style.flexDirection = FlexDirection.Row;
        countRow1.style.justifyContent = Justify.SpaceBetween;
        countRow1.style.marginBottom = 8;
        blocksPanel.Add(countRow1);
        
        countRow1.Add(CreateBlockButton("2 РАЗА", () => AddLoopStartBlock(2)));
        countRow1.Add(CreateBlockButton("3 РАЗА", () => AddLoopStartBlock(3)));
        countRow1.Add(CreateBlockButton("5 РАЗ", () => AddLoopStartBlock(5)));
        
        // Циклы ПОКА
        var whileLabel = new Label("ПОКА");
        whileLabel.style.fontSize = 14;
        whileLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        whileLabel.style.color = COLOR_TEXT_SECONDARY;
        whileLabel.style.marginTop = 8;
        whileLabel.style.marginBottom = 8;
        whileLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        blocksPanel.Add(whileLabel);
        
        var whileRow = new VisualElement();
        whileRow.style.flexDirection = FlexDirection.Row;
        whileRow.style.justifyContent = Justify.SpaceBetween;
        whileRow.style.marginBottom = 8;
        blocksPanel.Add(whileRow);
        
        whileRow.Add(CreateBlockButton("СТЕНА", () => AddWhileBlock(WhileCondition.WallAhead)));
        whileRow.Add(CreateBlockButton("НЕТ СТЕНЫ", () => AddWhileBlock(WhileCondition.NoWallAhead)));
        
        // Всегда и конец
        var endRow = new VisualElement();
        endRow.style.flexDirection = FlexDirection.Row;
        endRow.style.justifyContent = Justify.SpaceBetween;
        endRow.style.marginBottom = 8;
        blocksPanel.Add(endRow);
        
        endRow.Add(CreateBlockButton("ВСЕГДА", () => AddInfiniteLoopStartBlock()));
        endRow.Add(CreateBlockButton("КОНЕЦ", AddLoopEndBlock));
        
        blocksPanel.Add(new VisualElement { style = { flexGrow = 1 } });
        
        // Разделитель
        var divider2 = new VisualElement();
        divider2.style.height = 1;
        divider2.style.backgroundColor = new Color(0.3f, 0.3f, 0.32f, 1f);
        divider2.style.marginBottom = 12;
        blocksPanel.Add(divider2);
        
        // Кнопки управления
        var controlsRow = new VisualElement();
        controlsRow.style.flexDirection = FlexDirection.Row;
        controlsRow.style.justifyContent = Justify.SpaceBetween;
        blocksPanel.Add(controlsRow);
        
        controlsRow.Add(CreateControlButton("ЗАПУСК", COLOR_SUCCESS, RunProgram));
        controlsRow.Add(CreateControlButton("ОЧИСТИТЬ", COLOR_WARNING, ClearProgram));
        controlsRow.Add(CreateControlButton("СБРОС", COLOR_ERROR, ResetLevel));
        
        // ПРАВАЯ ПАНЕЛЬ - Программа (55%)
        var programPanel = new VisualElement();
        programPanel.style.width = new Length(55, LengthUnit.Percent);
        programPanel.style.flexDirection = FlexDirection.Column;
        programPanel.style.paddingLeft = 12;
        programPanel.style.paddingRight = 12;
        programPanel.style.paddingTop = 12;
        programPanel.style.paddingBottom = 12;
        safeArea.Add(programPanel);
        
        var programTitle = new Label("ПРОГРАММА");
        programTitle.style.fontSize = 20;
        programTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        programTitle.style.color = COLOR_TEXT;
        programTitle.style.marginBottom = 8;
        programTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
        programTitle.style.letterSpacing = 2;
        programPanel.Add(programTitle);
        
        statusLabel = new Label("0 блоков");
        statusLabel.style.fontSize = 14;
        statusLabel.style.color = COLOR_TEXT_SECONDARY;
        statusLabel.style.marginBottom = 8;
        statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        programPanel.Add(statusLabel);
        
        programScrollView = new ScrollView();
        programScrollView.style.flexGrow = 1;
        programScrollView.style.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 1f);
        programScrollView.style.borderLeftWidth = 1;
        programScrollView.style.borderRightWidth = 1;
        programScrollView.style.borderTopWidth = 1;
        programScrollView.style.borderBottomWidth = 1;
        programScrollView.style.borderLeftColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        programScrollView.style.borderRightColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        programScrollView.style.borderTopColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        programScrollView.style.borderBottomColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        programPanel.Add(programScrollView);
        
        commandList = programScrollView.contentContainer;
    }
    
    Button CreateBlockButton(string label, System.Action onClick)
    {
        var button = new Button(onClick);
        button.text = label;
        button.style.width = 95;
        button.style.height = 55;
        button.style.fontSize = 13;
        button.style.unityFontStyleAndWeight = FontStyle.Bold;
        button.style.color = COLOR_TEXT;
        button.style.backgroundColor = COLOR_BLOCK;
        button.style.borderLeftWidth = 0;
        button.style.borderRightWidth = 0;
        button.style.borderTopWidth = 0;
        button.style.borderBottomWidth = 0;
        button.style.borderTopLeftRadius = 4;
        button.style.borderTopRightRadius = 4;
        button.style.borderBottomLeftRadius = 4;
        button.style.borderBottomRightRadius = 4;
        button.style.marginBottom = 4;
        button.style.letterSpacing = 0.5f;
        
        // Эффект параллелепипеда через тень
        button.style.shadowOffset = new Vector2(0, 2);
        button.style.shadowColor = new Color(0.05f, 0.05f, 0.07f, 0.5f);
        button.style.shadowBlurRadius = 0;
        button.style.shadowSpread = 0;
        
        button.RegisterCallback<PointerDownEvent>(evt => 
        {
            button.style.backgroundColor = COLOR_BLOCK_HOVER;
            button.style.shadowOffset = new Vector2(0, 1);
            button.style.translate = new Translate(0, 1);
        });
        
        button.RegisterCallback<PointerUpEvent>(evt => 
        {
            button.style.backgroundColor = COLOR_BLOCK;
            button.style.shadowOffset = new Vector2(0, 2);
            button.style.translate = new Translate(0, 0);
        });
        
        button.RegisterCallback<PointerLeaveEvent>(evt => 
        {
            button.style.backgroundColor = COLOR_BLOCK;
            button.style.shadowOffset = new Vector2(0, 2);
            button.style.translate = new Translate(0, 0);
        });
        
        return button;
    }
    
    Button CreateControlButton(string label, Color color, System.Action onClick)
    {
        var button = new Button(onClick);
        button.text = label;
        button.style.width = 90;
        button.style.height = 48;
        button.style.fontSize = 12;
        button.style.unityFontStyleAndWeight = FontStyle.Bold;
        button.style.color = COLOR_TEXT;
        button.style.backgroundColor = color;
        button.style.borderLeftWidth = 0;
        button.style.borderRightWidth = 0;
        button.style.borderTopWidth = 0;
        button.style.borderBottomWidth = 0;
        button.style.borderTopLeftRadius = 4;
        button.style.borderTopRightRadius = 4;
        button.style.borderBottomLeftRadius = 4;
        button.style.borderBottomRightRadius = 4;
        button.style.letterSpacing = 1;
        
        button.style.shadowOffset = new Vector2(0, 2);
        button.style.shadowColor = new Color(0.05f, 0.05f, 0.07f, 0.5f);
        
        button.RegisterCallback<PointerDownEvent>(evt => 
        {
            button.style.backgroundColor = color * 1.1f;
            button.style.shadowOffset = new Vector2(0, 1);
            button.style.translate = new Translate(0, 1);
        });
        
        button.RegisterCallback<PointerUpEvent>(evt => 
        {
            button.style.backgroundColor = color;
            button.style.shadowOffset = new Vector2(0, 2);
            button.style.translate = new Translate(0, 0);
        });
        
        return button;
    }
    
    void AddBlock(BlockType type)
    {
        if (isRunning) return;
        if (blocks.Count >= maxCommands)
        {
            UpdateStatus("Максимум блоков!", COLOR_ERROR);
            return;
        }
        
        blocks.Add(new Block { type = type });
        UpdateProgramDisplay();
        UpdateStatus($"{blocks.Count} блоков", COLOR_TEXT);
    }
    
    void AddLoopStartBlock(int count)
    {
        if (isRunning) return;
        if (blocks.Count >= maxCommands)
        {
            UpdateStatus("Максимум блоков!", COLOR_ERROR);
            return;
        }
        
        blocks.Add(new Block { type = BlockType.LoopStart, loopCount = count });
        UpdateProgramDisplay();
        UpdateStatus($"{blocks.Count} блоков", COLOR_TEXT);
    }
    
    void AddInfiniteLoopStartBlock()
    {
        if (isRunning) return;
        if (blocks.Count >= maxCommands)
        {
            UpdateStatus("Максимум блоков!", COLOR_ERROR);
            return;
        }
        
        blocks.Add(new Block { type = BlockType.InfiniteLoopStart });
        UpdateProgramDisplay();
        UpdateStatus($"{blocks.Count} блоков", COLOR_TEXT);
    }
    
    void AddWhileBlock(WhileCondition condition)
    {
        if (isRunning) return;
        if (blocks.Count >= maxCommands)
        {
            UpdateStatus("Максимум блоков!", COLOR_ERROR);
            return;
        }
        
        blocks.Add(new Block { type = BlockType.WhileStart, whileCondition = condition });
        UpdateProgramDisplay();
        UpdateStatus($"{blocks.Count} блоков", COLOR_TEXT);
        Debug.Log($"[RobotUI] Добавлен цикл 'пока' с условием: {condition}");
    }
    
    void AddLoopEndBlock()
    {
        if (isRunning) return;
        if (blocks.Count >= maxCommands)
        {
            UpdateStatus("Максимум блоков!", COLOR_ERROR);
            return;
        }
        
        int openLoops = 0;
        foreach (var block in blocks)
        {
            if (block.type == BlockType.LoopStart || block.type == BlockType.InfiniteLoopStart || block.type == BlockType.WhileStart)
                openLoops++;
            else if (block.type == BlockType.LoopEnd)
                openLoops--;
        }
        
        if (openLoops <= 0)
        {
            UpdateStatus("Нет открытого цикла!", COLOR_ERROR);
            return;
        }
        
        blocks.Add(new Block { type = BlockType.LoopEnd });
        UpdateProgramDisplay();
        UpdateStatus($"{blocks.Count} блоков", COLOR_TEXT);
    }
    
    void UpdateProgramDisplay()
    {
        commandList.Clear();
        int nestingLevel = 0;
        
        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var item = CreateBlockVisual(i, block, nestingLevel);
            commandList.Add(item);
            
            if (block.type == BlockType.LoopStart || block.type == BlockType.InfiniteLoopStart || block.type == BlockType.WhileStart)
                nestingLevel++;
            else if (block.type == BlockType.LoopEnd)
                nestingLevel--;
        }
        
        programScrollView.scrollOffset = new Vector2(0, float.MaxValue);
    }
    
    VisualElement CreateBlockVisual(int index, Block block, int nestingLevel)
    {
        var item = new VisualElement();
        item.style.flexDirection = FlexDirection.Row;
        item.style.alignItems = Align.Center;
        item.style.paddingLeft = 10 + (nestingLevel * 20);
        item.style.paddingRight = 10;
        item.style.paddingTop = 10;
        item.style.paddingBottom = 10;
        item.style.marginBottom = 2;
        item.style.borderLeftWidth = 2;
        
        // Цвета по типу блока
        if (block.type == BlockType.LoopStart)
        {
            item.style.backgroundColor = new Color(0.28f, 0.25f, 0.20f, 1f);
            item.style.borderLeftColor = new Color(0.50f, 0.45f, 0.35f, 1f);
        }
        else if (block.type == BlockType.InfiniteLoopStart)
        {
            item.style.backgroundColor = new Color(0.30f, 0.25f, 0.30f, 1f);
            item.style.borderLeftColor = new Color(0.55f, 0.45f, 0.55f, 1f);
        }
        else if (block.type == BlockType.WhileStart)
        {
            item.style.backgroundColor = new Color(0.22f, 0.28f, 0.24f, 1f);
            item.style.borderLeftColor = new Color(0.40f, 0.50f, 0.42f, 1f);
        }
        else if (block.type == BlockType.LoopEnd)
        {
            item.style.backgroundColor = new Color(0.20f, 0.20f, 0.22f, 1f);
            item.style.borderLeftColor = new Color(0.40f, 0.40f, 0.42f, 1f);
        }
        else
        {
            item.style.backgroundColor = index % 2 == 0 ? new Color(0.15f, 0.15f, 0.17f, 1f) : new Color(0.18f, 0.18f, 0.20f, 1f);
            item.style.borderLeftColor = new Color(0.35f, 0.35f, 0.38f, 1f);
        }
        
        var numberLabel = new Label($"{index + 1}.");
        numberLabel.style.width = 30;
        numberLabel.style.color = COLOR_TEXT_SECONDARY;
        numberLabel.style.fontSize = 13;
        item.Add(numberLabel);
        
        var commandLabel = new Label(GetBlockName(block));
        commandLabel.style.flexGrow = 1;
        commandLabel.style.color = COLOR_TEXT;
        commandLabel.style.fontSize = 15;
        commandLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        item.Add(commandLabel);
        
        var deleteBtn = new Button(() => RemoveBlock(index));
        deleteBtn.text = "×";
        deleteBtn.style.width = 32;
        deleteBtn.style.height = 32;
        deleteBtn.style.backgroundColor = new Color(0.35f, 0.25f, 0.25f, 1f);
        deleteBtn.style.color = COLOR_TEXT;
        deleteBtn.style.fontSize = 18;
        deleteBtn.style.borderLeftWidth = 0;
        deleteBtn.style.borderRightWidth = 0;
        deleteBtn.style.borderTopWidth = 0;
        deleteBtn.style.borderBottomWidth = 0;
        deleteBtn.style.borderTopLeftRadius = 3;
        deleteBtn.style.borderTopRightRadius = 3;
        deleteBtn.style.borderBottomLeftRadius = 3;
        deleteBtn.style.borderBottomRightRadius = 3;
        item.Add(deleteBtn);
        
        return item;
    }
    
    void RemoveBlock(int index)
    {
        if (isRunning || index < 0 || index >= blocks.Count) return;
        
        var block = blocks[index];
        
        if (block.type == BlockType.LoopStart || block.type == BlockType.InfiniteLoopStart || block.type == BlockType.WhileStart)
        {
            int nesting = 1;
            for (int i = index + 1; i < blocks.Count; i++)
            {
                if (blocks[i].type == BlockType.LoopStart || blocks[i].type == BlockType.InfiniteLoopStart || blocks[i].type == BlockType.WhileStart)
                    nesting++;
                else if (blocks[i].type == BlockType.LoopEnd)
                {
                    nesting--;
                    if (nesting == 0)
                    {
                        blocks.RemoveAt(i);
                        break;
                    }
                }
            }
            blocks.RemoveAt(index);
        }
        else if (block.type == BlockType.LoopEnd)
        {
            int nesting = 1;
            for (int i = index - 1; i >= 0; i--)
            {
                if (blocks[i].type == BlockType.LoopEnd)
                    nesting++;
                else if (blocks[i].type == BlockType.LoopStart || blocks[i].type == BlockType.InfiniteLoopStart || blocks[i].type == BlockType.WhileStart)
                {
                    nesting--;
                    if (nesting == 0)
                    {
                        blocks.RemoveAt(i);
                        index--;
                        break;
                    }
                }
            }
            blocks.RemoveAt(index);
        }
        else
        {
            blocks.RemoveAt(index);
        }
        
        UpdateProgramDisplay();
        UpdateStatus($"{blocks.Count} блоков", COLOR_TEXT);
    }
    
    string GetBlockName(Block block)
    {
        switch (block.type)
        {
            case BlockType.MoveForward: return "ВПЕРЕД";
            case BlockType.RotateLeft: return "НАЛЕВО";
            case BlockType.RotateRight: return "НАПРАВО";
            case BlockType.LoopStart: return $"ПОВТОРИТЬ {block.loopCount}";
            case BlockType.InfiniteLoopStart: return "ВСЕГДА";
            case BlockType.WhileStart: return $"ПОКА {GetWhileConditionName(block.whileCondition)}";
            case BlockType.LoopEnd: return "КОНЕЦ";
            default: return "???";
        }
    }
    
    string GetWhileConditionName(WhileCondition condition)
    {
        switch (condition)
        {
            case WhileCondition.WallAhead: return "СТЕНА";
            case WhileCondition.NoWallAhead: return "НЕТ СТЕНЫ";
            default: return "???";
        }
    }
    
    async void RunProgram()
    {
        UpdateRobotReference();
        
        if (robot == null || !robot.gameObject.activeInHierarchy)
        {
            UpdateStatus("Робот не найден!", COLOR_ERROR);
            return;
        }
        
        if (isRunning || blocks.Count == 0) return;
        
        int openLoops = 0;
        foreach (var block in blocks)
        {
            if (block.type == BlockType.LoopStart || block.type == BlockType.InfiniteLoopStart || block.type == BlockType.WhileStart)
                openLoops++;
            else if (block.type == BlockType.LoopEnd)
                openLoops--;
        }
        
        if (openLoops != 0)
        {
            UpdateStatus($"Ошибка: незакрытые циклы!", COLOR_ERROR);
            return;
        }
        
        isRunning = true;
        currentBlockIndex = 0;
        totalIterations = 0;
        UpdateStatus("ВЫПОЛНЕНИЕ...", COLOR_ACCENT);
        
        try
        {
            await ExecuteBlocks(0, blocks.Count, 1);
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Ошибка: {e.Message}", COLOR_ERROR);
        }
        
        isRunning = false;
        HighlightBlock(-1);
        UpdateStatus($"ГОТОВО ({totalIterations} шагов)", COLOR_SUCCESS);
    }
    
    async Task ExecuteBlocks(int startIndex, int endIndex, int repeatCount)
    {
        for (int repeat = 0; repeat < repeatCount || repeatCount == -1; repeat++)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                totalIterations++;
                if (totalIterations > maxIterations)
                {
                    UpdateStatus($"Остановка: лимит итераций!", COLOR_ERROR);
                    return;
                }
                
                if (robot == null || !robot.gameObject.activeInHierarchy)
                {
                    UpdateRobotReference();
                    if (robot == null) return;
                }
                
                currentBlockIndex = i;
                HighlightBlock(i);
                
                var block = blocks[i];
                
                if (block.type == BlockType.LoopStart || block.type == BlockType.InfiniteLoopStart)
                {
                    int loopIterations = block.type == BlockType.InfiniteLoopStart ? -1 : block.loopCount;
                    int loopEnd = FindLoopEnd(i);
                    if (loopEnd > i)
                    {
                        await ExecuteBlocks(i + 1, loopEnd, loopIterations);
                        i = loopEnd;
                    }
                }
                else if (block.type == BlockType.WhileStart)
                {
                    int loopEnd = FindLoopEnd(i);
                    if (loopEnd > i)
                    {
                        await ExecuteWhileLoop(i + 1, loopEnd, block.whileCondition);
                        i = loopEnd;
                    }
                }
                else if (block.type == BlockType.LoopEnd)
                {
                    // Пропускаем
                }
                else
                {
                    await ExecuteSingleBlock(block);
                }
                
                await Task.Delay(50);
            }
        }
    }
    
    async Task ExecuteWhileLoop(int startIndex, int endIndex, WhileCondition condition)
    {
        int iterations = 0;
        while (CheckCondition(condition))
        {
            iterations++;
            if (iterations > maxIterations)
            {
                UpdateStatus($"Остановка: лимит 'пока'!", COLOR_ERROR);
                return;
            }
            
            await ExecuteBlocks(startIndex, endIndex, 1);
            
            if (!CheckCondition(condition))
                break;
        }
    }
    
    bool CheckCondition(WhileCondition condition)
    {
        if (robot == null) return false;
        
        switch (condition)
        {
            case WhileCondition.WallAhead:
                return robot.IsWallAhead();
            case WhileCondition.NoWallAhead:
                return !robot.IsWallAhead();
            default:
                return false;
        }
    }
    
    int FindLoopEnd(int startIndex)
    {
        int nesting = 1;
        for (int i = startIndex + 1; i < blocks.Count; i++)
        {
            if (blocks[i].type == BlockType.LoopStart || blocks[i].type == BlockType.InfiniteLoopStart || blocks[i].type == BlockType.WhileStart)
                nesting++;
            else if (blocks[i].type == BlockType.LoopEnd)
            {
                nesting--;
                if (nesting == 0)
                    return i;
            }
        }
        return startIndex;
    }
    
    async Task ExecuteSingleBlock(Block block)
    {
        int waitCount = 0;
        while (robot.isMoving || robot.isRotating)
        {
            await Task.Delay(50);
            waitCount++;
            if (waitCount > 100) break;
        }
        
        if (!robot.gameObject.activeInHierarchy) return;
        
        switch (block.type)
        {
            case BlockType.MoveForward:
                robot.TryMoveForward();
                break;
            case BlockType.RotateLeft:
                robot.RotateLeft();
                break;
            case BlockType.RotateRight:
                robot.RotateRight();
                break;
        }
        
        await Task.Delay(500);
    }
    
    void HighlightBlock(int index)
    {
        for (int i = 0; i < commandList.childCount && i < blocks.Count; i++)
        {
            var item = commandList[i];
            if (i == index)
            {
                item.style.backgroundColor = new Color(0.25f, 0.40f, 0.30f, 1f);
                item.style.borderLeftColor = new Color(0.45f, 0.70f, 0.50f, 1f);
            }
            else
            {
                var block = blocks[i];
                if (block.type == BlockType.LoopStart)
                {
                    item.style.backgroundColor = new Color(0.28f, 0.25f, 0.20f, 1f);
                    item.style.borderLeftColor = new Color(0.50f, 0.45f, 0.35f, 1f);
                }
                else if (block.type == BlockType.InfiniteLoopStart)
                {
                    item.style.backgroundColor = new Color(0.30f, 0.25f, 0.30f, 1f);
                    item.style.borderLeftColor = new Color(0.55f, 0.45f, 0.55f, 1f);
                }
                else if (block.type == BlockType.WhileStart)
                {
                    item.style.backgroundColor = new Color(0.22f, 0.28f, 0.24f, 1f);
                    item.style.borderLeftColor = new Color(0.40f, 0.50f, 0.42f, 1f);
                }
                else if (block.type == BlockType.LoopEnd)
                {
                    item.style.backgroundColor = new Color(0.20f, 0.20f, 0.22f, 1f);
                    item.style.borderLeftColor = new Color(0.40f, 0.40f, 0.42f, 1f);
                }
                else
                {
                    item.style.backgroundColor = i % 2 == 0 ? new Color(0.15f, 0.15f, 0.17f, 1f) : new Color(0.18f, 0.18f, 0.20f, 1f);
                    item.style.borderLeftColor = new Color(0.35f, 0.35f, 0.38f, 1f);
                }
            }
        }
    }
    
    void ClearProgram()
    {
        if (isRunning) return;
        blocks.Clear();
        UpdateProgramDisplay();
        UpdateStatus("0 блоков", COLOR_TEXT);
    }
    
    void ResetLevel()
    {
        if (isRunning) return;
        blocks.Clear();
        UpdateProgramDisplay();
        gameManager?.ResetLevel();
        UpdateRobotReference();
        UpdateStatus("0 блоков", COLOR_TEXT);
    }
    
    void UpdateStatus(string message, Color color)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
            statusLabel.style.color = color;
        }
    }
}

public class Block
{
    public BlockType type;
    public int loopCount = 0;
    public WhileCondition whileCondition = WhileCondition.None;
}

public enum BlockType
{
    MoveForward,
    RotateLeft,
    RotateRight,
    LoopStart,
    InfiniteLoopStart,
    WhileStart,
    LoopEnd
}

public enum WhileCondition
{
    None,
    WallAhead,
    NoWallAhead
}
