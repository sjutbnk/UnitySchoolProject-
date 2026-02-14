using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class RobotUI : MonoBehaviour
{
    public Robot robot;
    public GameManager gameManager;
    public int maxCommands  = 30;
    public int maxIterations = 200;
    public bool debugMode = true;

    private VisualElement root;
    private VisualElement programList; 
    private Label statusLbl;
    private Button runBtn;
    private Button stopBtn;

    private List<Block> blocks    = new List<Block>();
    private bool        running   = false;
    private bool        cancelled = false;

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out Color c);
        return c;
    }
    static readonly Color BG0    = Hex("#0D1117");
    static readonly Color BG1    = Hex("#161B22");
    static readonly Color BG2    = Hex("#1C2128");
    static readonly Color BORDER = Hex("#30363D");
    static readonly Color TXT    = Hex("#E6EDF3");
    static readonly Color TXT2   = Hex("#7D8590");
    static readonly Color BLUE   = Hex("#1F6FEB");
    static readonly Color BLUE2  = Hex("#388BFD");
    static readonly Color ORANGE = Hex("#D4671A");
    static readonly Color ORANGE2= Hex("#F09A4B");
    static readonly Color GREEN  = Hex("#1A7F37");
    static readonly Color GREEN2 = Hex("#3FB950");
    static readonly Color PURPLE = Hex("#6E40C9");
    static readonly Color PURPLE2= Hex("#A371F7");
    static readonly Color TEAL   = Hex("#1B7C83");
    static readonly Color TEAL2  = Hex("#39C5CF");
    static readonly Color RED    = Hex("#B62324");
    static readonly Color RED2   = Hex("#F85149");
    static readonly Color GREY   = Hex("#3D444D");
    static readonly Color GREY2  = Hex("#6E7681");
    static readonly Color AMBER  = Hex("#9A6700");
    static readonly Color AMBER2 = Hex("#D29922");

    void Start()
    {
        if (debugMode) Debug.Log("[RobotUI] === START ===");
        
        FindDeps();
        var doc = GetComponent<UIDocument>();
        doc.panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
        root = doc.rootVisualElement;

        Screen.orientation = ScreenOrientation.Portrait;
        BuildUI();
        
        if (debugMode) Debug.Log("[RobotUI] UI Built successfully");
    }

    void Update()
    {
        if (!RobotReady()) FindDeps();
    }

    void FindDeps()
    {
        if (debugMode) Debug.Log("[RobotUI] FindDeps called");
        
        if (robot == null || !robot.gameObject.activeInHierarchy)
        {
            var all = FindObjectsOfType<Robot>(true);
            foreach (var r in all) { robot = r; break; }
            if (debugMode && robot != null) Debug.Log($"[RobotUI] Robot found: {robot.name}");
        }
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (debugMode && gameManager != null) Debug.Log($"[RobotUI] GameManager found: {gameManager.name}");
    }

    bool RobotReady() => robot != null && robot.gameObject.activeInHierarchy;

    void BuildUI()
    {
        var shell = El();
        shell.style.position = Position.Absolute;
        shell.style.bottom = 0; shell.style.left = 0; shell.style.right = 0;
        shell.style.height = new Length(60, LengthUnit.Percent);
        shell.style.backgroundColor = BG0;
        shell.style.borderTopWidth = 1;
        shell.style.borderTopColor = BORDER;
        shell.style.flexDirection = FlexDirection.Column;

        float safeB = Screen.height - Screen.safeArea.yMax;
        shell.style.paddingBottom = Mathf.Max(0, safeB);
        root.Add(shell);

        shell.Add(BuildStatusBar());

        var main = El();
        main.style.flexDirection = FlexDirection.Row;
        main.style.flexGrow      = 1;
        shell.Add(main);

        main.Add(BuildPalette());
        main.Add(BuildProgram());

        shell.Add(BuildControlBar());
    }

    VisualElement BuildStatusBar()
    {
        var bar = El(FlexDirection.Row);
        bar.style.backgroundColor = BG1;
        bar.style.paddingLeft     = 16;
        bar.style.paddingRight    = 16;
        bar.style.paddingTop      = 10;
        bar.style.paddingBottom   = 10;
        bar.style.alignItems      = Align.Center;
        bar.style.borderBottomWidth = 1;
        bar.style.borderBottomColor = BORDER;

        var lvl = L("● 1.", 12, TXT2, FontStyle.Bold);
        lvl.style.letterSpacing = 1;

        statusLbl = L("Добавь команды и нажми СТАРТ", 12, TXT2);
        statusLbl.style.flexGrow        = 1;
        statusLbl.style.unityTextAlign  = TextAnchor.MiddleCenter;
        statusLbl.style.marginLeft      = 8;
        statusLbl.style.marginRight     = 8;

        bar.Add(lvl);
        bar.Add(statusLbl);
        return bar;
    }

    VisualElement BuildPalette()
    {
        var panel = El();
        panel.style.width           = new Length(40, LengthUnit.Percent);
        panel.style.backgroundColor = BG1;
        panel.style.borderRightWidth = 1;
        panel.style.borderRightColor = BORDER;
        panel.style.flexDirection   = FlexDirection.Column;

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow    = 1;
        scroll.style.paddingLeft  = 10;
        scroll.style.paddingRight = 10;
        scroll.style.paddingTop   = 12;
        scroll.style.paddingBottom = 12;
        panel.Add(scroll);

        var c = scroll.contentContainer;

        c.Add(SectionHdr("ДВИЖЕНИЕ"));
        c.Add(PBtn("▲  ВПЕРЁД",    BLUE,   BLUE2,   () => Add(BlockType.MoveForward)));
        c.Add(PBtn("◄  НАЛЕВО",    BLUE,   BLUE2,   () => Add(BlockType.RotateLeft)));
        c.Add(PBtn("►  НАПРАВО",   BLUE,   BLUE2,   () => Add(BlockType.RotateRight)));

        c.Add(Gap(8)); c.Add(SectionHdr("ПОВТОРИТЬ"));
        c.Add(PBtn("↺  3 РАЗА",   ORANGE, ORANGE2, () => AddLoop(3)));
        c.Add(PBtn("↺  5 РАЗА",   ORANGE, ORANGE2, () => AddLoop(5)));
        c.Add(PBtn("↺  7 РАЗ",    ORANGE, ORANGE2, () => AddLoop(7)));

        // c.Add(Gap(8)); c.Add(SectionHdr("БЕСКОНЕЧНО"));
        c.Add(PBtn("∞  ВСЕГДА",   PURPLE, PURPLE2, AddInfinite));
        c.Add(PBtn("└  КОНЕЦ",    GREY,   GREY2,   AddEnd));

        c.Add(Gap(8)); c.Add(SectionHdr("ПОКА ВЕРНО"));
        c.Add(PBtn("▣  СТЕНА",    GREEN,  GREEN2,  () => AddWhile(WhileCondition.WallAhead)));
        c.Add(PBtn("□  НЕТ СТЕНЫ",TEAL,   TEAL2,   () => AddWhile(WhileCondition.NoWallAhead)));

        return panel;
    }

    VisualElement BuildProgram()
    {
        var panel = El();
        panel.style.flexGrow      = 1;
        panel.style.flexDirection = FlexDirection.Column;
        panel.style.paddingLeft   = 10;
        panel.style.paddingRight  = 10;
        panel.style.paddingTop    = 12;
        panel.style.paddingBottom = 4;

        var hdr = El(FlexDirection.Row);
        hdr.style.alignItems    = Align.Center;
        hdr.style.marginBottom  = 8;
        var title = L("ПРОГРАММА   ", 13, TXT2, FontStyle.Bold);
        title.style.letterSpacing = 2;
        hdr.Add(title);

        var iconBtns = El(FlexDirection.Row);
        
        runBtn = IconBtn("▶", GREEN, GREEN2, OnRunClick);
        runBtn.style.marginRight = 8;
        stopBtn = IconBtn("■", RED, RED2, OnStopClick);
        stopBtn.style.marginRight = 8;
        var resetIcon = IconBtn("↺", GREY, GREY2, OnResetClick);
        
        stopBtn.SetEnabled(false);
        stopBtn.style.opacity = 0.4f;
        
        iconBtns.Add(runBtn);
        iconBtns.Add(stopBtn);
        iconBtns.Add(resetIcon);
        hdr.Add(iconBtns);
        
        panel.Add(hdr);

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1;
        scroll.style.backgroundColor = BG2;
        scroll.style.borderTopLeftRadius = 20;
        scroll.style.borderTopRightRadius = 20;
        scroll.style.borderBottomLeftRadius = 20;
        scroll.style.borderBottomRightRadius = 20;
        scroll.style.borderLeftWidth = 1; scroll.style.borderLeftColor   = BORDER;
        scroll.style.borderRightWidth = 1; scroll.style.borderRightColor  = BORDER;
        scroll.style.borderTopWidth = 1; scroll.style.borderTopColor    = BORDER;
        scroll.style.borderBottomWidth = 1; scroll.style.borderBottomColor = BORDER;
        scroll.style.paddingTop = 8;
        scroll.style.paddingBottom = 8;
        panel.Add(scroll);

        programList = scroll.contentContainer;
        RefreshProgram();
        return panel;
    }

    VisualElement BuildControlBar()
    {
        var bar = El(FlexDirection.Row);
        bar.style.backgroundColor = BG1;
        bar.style.borderTopWidth  = 1;
        bar.style.borderTopColor  = BORDER;
        bar.style.paddingLeft     = 10;
        bar.style.paddingRight    = 10;
        bar.style.paddingTop      = 6;
        bar.style.paddingBottom   = 6;
        
        var hint = L("← добавь блоки", 11, TXT2);
        bar.Add(hint);
        
        return bar;
    }

    Button IconBtn(string icon, Color bg, Color bgHov, System.Action action)
    {
        var btn = new Button(action);
        btn.text = icon;
        btn.style.width = 80;
        btn.style.height = 80;
        btn.style.fontSize = 42;
        btn.style.unityFontStyleAndWeight = FontStyle.Bold;
        btn.style.color = TXT;
        btn.style.backgroundColor = bg;
        btn.style.borderTopLeftRadius = 12;
        btn.style.borderTopRightRadius = 12;
        btn.style.borderBottomLeftRadius = 12;
        btn.style.borderBottomRightRadius = 12;
        btn.style.borderBottomWidth = 3;
        btn.style.borderBottomColor = MulColor(bg, 0.6f);

        btn.RegisterCallback<PointerDownEvent>(_ =>
        {
            btn.style.backgroundColor = bgHov;
            btn.style.opacity = 0.8f;
        });
        btn.RegisterCallback<PointerUpEvent>(_ =>
        {
            btn.style.backgroundColor = bg;
            btn.style.opacity = 1f;
        });
        btn.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            btn.style.backgroundColor = bg;
            btn.style.opacity = 1f;
        });
        return btn;
    }

    void Add(BlockType t)
    {
        if (debugMode) Debug.Log($"[RobotUI] Add block: {t}, running: {running}, count: {blocks.Count}");
        
        if (running || blocks.Count >= maxCommands) return;
        blocks.Add(new Block { type = t });
        RefreshProgram();
        if (debugMode) Debug.Log($"[RobotUI] Block added, total: {blocks.Count}");
    }

    void AddLoop(int n)
    {
        if (debugMode) Debug.Log($"[RobotUI] AddLoop: {n}");
        
        if (running || blocks.Count >= maxCommands) return;
        blocks.Add(new Block { type = BlockType.LoopStart, loopCount = n });
        RefreshProgram();
    }

    void AddInfinite()
    {
        if (debugMode) Debug.Log("[RobotUI] AddInfinite");
        
        if (running || blocks.Count >= maxCommands) return;
        blocks.Add(new Block { type = BlockType.InfiniteLoopStart });
        RefreshProgram();
    }

    void AddWhile(WhileCondition cond)
    {
        if (debugMode) Debug.Log($"[RobotUI] AddWhile: {cond}");
        
        if (running || blocks.Count >= maxCommands) return;
        blocks.Add(new Block { type = BlockType.WhileStart, whileCond = cond });
        RefreshProgram();
    }

    void AddEnd()
    {
        if (debugMode) Debug.Log("[RobotUI] AddEnd");
        
        if (running) return;
        int open = 0;
        foreach (var b in blocks)
        {
            if (b.IsLoopOpen()) open++;
            else if (b.type == BlockType.LoopEnd) open--;
        }
        if (open <= 0) { Status("Нет открытого цикла!", RED2); return; }
        blocks.Add(new Block { type = BlockType.LoopEnd });
        RefreshProgram();
    }

    void DeleteBlock(int idx)
    {
        if (debugMode) Debug.Log($"[RobotUI] DeleteBlock: {idx}");
        
        if (running || idx < 0 || idx >= blocks.Count) return;
        var b = blocks[idx];

        if (b.IsLoopOpen())
        {
            int nest = 1;
            for (int i = idx + 1; i < blocks.Count; i++)
            {
                if (blocks[i].IsLoopOpen()) nest++;
                else if (blocks[i].type == BlockType.LoopEnd)
                { nest--; if (nest == 0) { blocks.RemoveAt(i); break; } }
            }
        }
        else if (b.type == BlockType.LoopEnd)
        {
            int nest = 1;
            for (int i = idx - 1; i >= 0; i--)
            {
                if (blocks[i].type == BlockType.LoopEnd) nest++;
                else if (blocks[i].IsLoopOpen())
                { nest--; if (nest == 0) { blocks.RemoveAt(i); idx--; break; } }
            }
        }
        blocks.RemoveAt(idx);
        RefreshProgram();
    }

    void RefreshProgram()
    {
        programList.Clear();

        if (blocks.Count == 0)
        {
            var hint = L("Нажми блок слева →\nчтобы добавить", 13, TXT2);
            hint.style.unityTextAlign = TextAnchor.MiddleCenter;
            hint.style.marginTop = 24;
            hint.style.whiteSpace = WhiteSpace.Normal;
            programList.Add(hint);
            return;
        }

        int nest = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            var b = blocks[i];
            if (b.type == BlockType.LoopEnd && nest > 0) nest--;

            programList.Add(ProgramRow(i, b, nest));

            if (b.IsLoopOpen()) nest++;
        }
    }

    VisualElement ProgramRow(int idx, Block b, int nest)
    {
        (string icon, string label, Color accent) = Meta(b);

        var row = El(FlexDirection.Row);
        row.style.alignItems       = Align.Center;
        row.style.paddingTop       = 10;
        row.style.paddingBottom    = 10;
        row.style.paddingLeft      = 10 + nest * 14;
        row.style.paddingRight     = 6;
        row.style.marginBottom     = 4;
        row.style.borderLeftWidth  = 4;
        row.style.borderLeftColor  = accent;
        row.style.backgroundColor  = idx % 2 == 0 ? BG2 : BG1;
        row.style.borderTopLeftRadius     = 14;
        row.style.borderTopRightRadius    = 14;
        row.style.borderBottomLeftRadius  = 14;
        row.style.borderBottomRightRadius = 14;
        row.name = $"row_{idx}";

        var ic = L(icon, 15, accent);
        ic.style.width = 24;
        ic.style.unityTextAlign = TextAnchor.MiddleCenter;
        row.Add(ic);

        var nm = L(label, 13, TXT, FontStyle.Bold);
        nm.style.flexGrow    = 1;
        nm.style.marginLeft  = 4;
        nm.style.whiteSpace  = WhiteSpace.Normal;
        row.Add(nm);

        var del = new Button(() => DeleteBlock(idx));
        del.text = "×";
        del.style.width  = 32; del.style.height = 32;
        del.style.fontSize = 32; del.style.color = TXT2;
        del.style.backgroundColor  = BG0;
        del.style.borderLeftWidth  = 0; del.style.borderRightWidth  = 0;
        del.style.borderTopWidth   = 0; del.style.borderBottomWidth = 0;
        del.style.borderTopLeftRadius     = 12;
        del.style.borderTopRightRadius    = 12;
        del.style.borderBottomLeftRadius  = 12;
        del.style.borderBottomRightRadius = 12;
        del.RegisterCallback<PointerEnterEvent>(_ => { del.style.color = RED2; del.style.backgroundColor = Hex("#21262D"); });
        del.RegisterCallback<PointerLeaveEvent>(_ => { del.style.color = TXT2;  del.style.backgroundColor = BG0; });
        row.Add(del);

        return row;
    }

    (string, string, Color) Meta(Block b)
    {
        switch (b.type)
        {
            case BlockType.MoveForward:       return ("▲", "ВПЕРЁД",                     BLUE2);
            case BlockType.RotateLeft:        return ("◄", "НАЛЕВО",                      BLUE2);
            case BlockType.RotateRight:       return ("►", "НАПРАВО",                     BLUE2);
            case BlockType.LoopStart:         return ("↺", $"ПОВТОРИТЬ {b.loopCount} РАЗ",ORANGE2);
            case BlockType.InfiniteLoopStart: return ("∞", "ВСЕГДА",                      PURPLE2);
            case BlockType.WhileStart:        return ("⟳", b.whileCond == WhileCondition.WallAhead ? "ПОКА СТЕНА" : "ПОКА НЕТ СТЕНЫ", GREEN2);
            case BlockType.LoopEnd:           return ("└", "КОНЕЦ",                       GREY2);
            default:                          return ("?", "???",                          TXT2);
        }
    }

    void HighlightRow(int idx)
    {
        for (int i = 0; i < programList.childCount; i++)
        {
            var row = programList[i];
            if (i >= blocks.Count) break;
            row.style.backgroundColor = i == idx
                ? Hex("#0F2D20")
                : (i % 2 == 0 ? BG2 : BG1);
            row.style.borderLeftColor = i == idx
                ? GREEN2
                : Meta(blocks[i]).Item3;
        }
    }

    void OnRunClick()
    {
        if (debugMode) Debug.Log($"[RobotUI] OnRunClick - running: {running}, blocks: {blocks.Count}");
        
        FindDeps();
        if (!RobotReady())
        {
            if (robot != null) robot.gameObject.SetActive(true);
            if (!RobotReady()) { Status("Робот не найден!", RED2); Debug.LogError("[RobotUI] Robot not found!"); return; }
        }
        if (running || blocks.Count == 0) { if (debugMode) Debug.LogWarning("[RobotUI] Cannot run - already running or empty"); return; }

        int open = 0;
        foreach (var b in blocks)
        {
            if (b.IsLoopOpen()) open++;
            else if (b.type == BlockType.LoopEnd) open--;
        }
        if (open != 0) { Status("Незакрытый цикл!", RED2); Debug.LogError("[RobotUI] Unclosed loop!"); return; }

        running   = true;
        cancelled = false;

        runBtn.SetEnabled(false);  runBtn.style.opacity  = 0.4f;
        stopBtn.SetEnabled(true); stopBtn.style.opacity = 1.0f;
        Status("Выполняется…", GREEN2);
        
        if (debugMode) Debug.Log("[RobotUI] Starting program execution");

        StartCoroutine(RunAll());
    }

    void OnStopClick()
    {
        if (debugMode) Debug.Log("[RobotUI] OnStopClick");
        
        if (!running) return;
        cancelled = true;
    }

    void OnClearClick()
    {
        if (debugMode) Debug.Log("[RobotUI] OnClearClick");
        
        if (running) return;
        blocks.Clear();
        RefreshProgram();
        Status("Готов к работе", TXT2);
    }

    void OnResetClick()
    {
        if (debugMode) Debug.Log("[RobotUI] OnResetClick");
        
        if (running) return;
        blocks.Clear();
        RefreshProgram();
        gameManager?.ResetLevel();
        robot = FindObjectOfType<Robot>();
        Status("Уровень сброшен", TXT2);
    }

    void FinishRun(string msg, Color col)
    {
        running = false;
        HighlightRow(-1);
        runBtn.SetEnabled(true);  runBtn.style.opacity  = 1.0f;
        stopBtn.SetEnabled(false); stopBtn.style.opacity = 0.4f;
        Status(msg, col);
    }

    void Status(string msg, Color col)
    {
        if (statusLbl == null) return;
        statusLbl.text        = msg;
        statusLbl.style.color = col;
    }

    int  execIter;   // счётчик итераций

    IEnumerator RunAll()
    {
        execIter = 0;
        yield return StartCoroutine(ExecRange(0, blocks.Count, 1));
        if (cancelled) FinishRun("Остановлено", AMBER2);
        else           FinishRun($"Готово  ({execIter} шагов)", GREEN2);
    }

    IEnumerator ExecRange(int from, int to, int times)
    {
        for (int rep = 0; (times == -1 || rep < times) && !cancelled; rep++)
        {
            for (int i = from; i < to && !cancelled; i++)
            {
                execIter++;
                if (execIter > maxIterations)
                {
                    Status("Лимит итераций!", RED2);
                    cancelled = true;
                    yield break;
                }

                HighlightRow(i);
                var b = blocks[i];

                if (b.IsLoopOpen())
                {
                    int end = FindEnd(i);
                    if (end > i)
                    {
                        int n = b.type == BlockType.InfiniteLoopStart ? -1 : b.loopCount;
                        if (b.type == BlockType.WhileStart)
                            yield return StartCoroutine(ExecWhile(i + 1, end, b.whileCond));
                        else
                            yield return StartCoroutine(ExecRange(i + 1, end, n));
                        i = end;
                    }
                }
                else if (b.type != BlockType.LoopEnd)
                {
                    yield return StartCoroutine(ExecOne(b));
                }

                yield return null;
            }
        }
    }

    IEnumerator ExecWhile(int from, int to, WhileCondition cond)
    {
        int guard = 0;
        while (!cancelled && Eval(cond))
        {
            if (++guard > maxIterations) { Status("Лимит ПОКА!", RED2); cancelled = true; yield break; }
            yield return StartCoroutine(ExecRange(from, to, 1));
        }
    }

    bool Eval(WhileCondition c)
    {
        if (!RobotReady()) return false;
        return c == WhileCondition.WallAhead ? robot.IsWallAhead() : !robot.IsWallAhead();
    }

    IEnumerator ExecOne(Block b)
    {
        if (debugMode) Debug.Log($"[RobotUI] ExecOne: {b.type}");
        
        if (!RobotReady())
        {
            FindDeps();
            if (robot != null && !robot.gameObject.activeInHierarchy)
                robot.gameObject.SetActive(true);
            yield return null;
        }
        if (cancelled || !RobotReady()) yield break;

        float timeout = 0f;
        while (RobotReady() && (robot.isMoving || robot.isRotating) && timeout < 5f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }
        if (cancelled || !RobotReady()) yield break;

        switch (b.type)
        {
            case BlockType.MoveForward:  
                if (debugMode) Debug.Log("[RobotUI] Executing: MoveForward");
                robot.TryMoveForward(); break;
            case BlockType.RotateLeft:   
                if (debugMode) Debug.Log("[RobotUI] Executing: RotateLeft");
                robot.RotateLeft();     break;
            case BlockType.RotateRight:  
                if (debugMode) Debug.Log("[RobotUI] Executing: RotateRight");
                robot.RotateRight();    break;
        }

        yield return null;

        timeout = 0f;
        while (RobotReady() && (robot.isMoving || robot.isRotating) && timeout < 5f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.08f);
    }

    int FindEnd(int start)
    {
        int n = 1;
        for (int i = start + 1; i < blocks.Count; i++)
        {
            if (blocks[i].IsLoopOpen()) n++;
            else if (blocks[i].type == BlockType.LoopEnd) { n--; if (n == 0) return i; }
        }
        return start;
    }

    static VisualElement El(FlexDirection dir = FlexDirection.Column)
    {
        var e = new VisualElement();
        e.style.flexDirection = dir;
        return e;
    }

    static Label L(string text, int size, Color col, FontStyle fs = FontStyle.Normal)
    {
        var l = new Label(text);
        l.style.fontSize = size * 3.6f; 
        l.style.color    = col;
        l.style.unityFontStyleAndWeight = fs;
        return l;
    }

    static VisualElement Gap(float h)
    {
        var e = new VisualElement();
        e.style.height = h;
        return e;
    }

    static Label SectionHdr(string txt)
    {
        var l = L(txt, 10, TXT2, FontStyle.Bold);
        l.style.letterSpacing = 2;
        l.style.marginBottom  = 6;
        return l;
    }

    Button PBtn(string label, Color bg, Color bgHov, System.Action action)
    {
        var btn = new Button(action);
        btn.text = label;   
        btn.style.height     = 86;
        btn.style.marginBottom = 6;
        btn.style.fontSize   =42;
        btn.style.unityFontStyleAndWeight = FontStyle.Bold;
        btn.style.color                   = TXT;
        btn.style.backgroundColor         = bg;
        btn.style.unityTextAlign          = TextAnchor.MiddleLeft;
        btn.style.paddingLeft             = 12;
        btn.style.borderLeftWidth  = 0; btn.style.borderRightWidth  = 0;
        btn.style.borderTopWidth   = 0;
        btn.style.borderBottomWidth = 4;
        btn.style.borderBottomColor = MulColor(bg, 0.55f);
        btn.style.borderTopLeftRadius     = 20;
        btn.style.borderTopRightRadius    = 20;
        btn.style.borderBottomLeftRadius  = 20;
        btn.style.borderBottomRightRadius = 20;

        btn.RegisterCallback<PointerDownEvent>(_ =>
        {
            btn.style.backgroundColor   = bgHov;
            btn.style.translate         = new Translate(0, 2);
            btn.style.borderBottomWidth = 1;
        });
        btn.RegisterCallback<PointerUpEvent>(_ =>
        {
            btn.style.backgroundColor   = bg;
            btn.style.translate         = new Translate(0, 0);
            btn.style.borderBottomWidth = 4;
        });
        btn.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            btn.style.backgroundColor   = bg;
            btn.style.translate         = new Translate(0, 0);
            btn.style.borderBottomWidth = 4;
        });
        return btn;
    }

    Button CBtn(string label, Color bg, Color bgHov, System.Action action)
    {
        var btn = new Button(action);
        btn.text = label;
        btn.style.flexGrow   = 1;
        btn.style.height     = 64;
        btn.style.marginLeft = 4; btn.style.marginRight = 4;
        btn.style.fontSize   = 28;
        btn.style.unityFontStyleAndWeight = FontStyle.Bold;
        btn.style.color           = TXT;
        btn.style.backgroundColor = bg;
        btn.style.borderLeftWidth  = 0; btn.style.borderRightWidth  = 0;
        btn.style.borderTopWidth   = 0;
        btn.style.borderBottomWidth = 3;
        btn.style.borderBottomColor = MulColor(bg, 0.55f);
        btn.style.borderTopLeftRadius     = 8;
        btn.style.borderTopRightRadius    = 8;
        btn.style.borderBottomLeftRadius  = 8;
        btn.style.borderBottomRightRadius = 8;

        btn.RegisterCallback<PointerDownEvent>(_ =>
        {
            btn.style.backgroundColor   = bgHov;
            btn.style.translate         = new Translate(0, 2);
            btn.style.borderBottomWidth = 1;
        });
        btn.RegisterCallback<PointerUpEvent>(_ =>
        {
            btn.style.backgroundColor   = bg;
            btn.style.translate         = new Translate(0, 0);
            btn.style.borderBottomWidth = 3;
        });
        btn.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            btn.style.backgroundColor   = bg;
            btn.style.translate         = new Translate(0, 0);
            btn.style.borderBottomWidth = 3;
        });
        return btn;
    }

    static Color MulColor(Color c, float f) => new Color(c.r * f, c.g * f, c.b * f, 1f);
}

public class Block
{
    public BlockType      type;
    public int            loopCount = 0;
    public WhileCondition whileCond = WhileCondition.None;

    public bool IsLoopOpen() =>
        type == BlockType.LoopStart ||
        type == BlockType.InfiniteLoopStart ||
        type == BlockType.WhileStart;
}

public enum BlockType
{
    MoveForward, RotateLeft, RotateRight,
    LoopStart, InfiniteLoopStart, WhileStart, LoopEnd
}

public enum WhileCondition { None, WallAhead, NoWallAhead }

