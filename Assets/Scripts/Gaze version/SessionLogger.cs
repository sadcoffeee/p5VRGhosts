using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionLogger : MonoBehaviour
{
    public static SessionLogger Instance { get; private set; }

    public bool doLogging;

    [Header("References")]
    public FlashlightController armHaptics;
    public HapticController handHaptics;
    public GhostCapsuleManager caughtGhostsCounter;
    public Transform PlayerHead;

    [Header("Inputs")]
    public KeyCode logPainButton;
    public InputActionReference triggerButton;

    [Header("File Writing Settings")]
    [SerializeField] float writeInterval = 1.0f;
    [SerializeField] int maxLinesPerWrite = 1000;


    private StreamWriter _writer;
    private float _sessionStart;
    //Helper metrics
    bool logging_started = false;           //TODO: Update on game start
    Vector3 look_direction = Vector3.zero;  //TODO: Fetch
    Vector3 ghost_direction = Vector3.zero; //TODO: Fect

    //Metrics
    float seconds_since_start = 0f;         
    private int participant_id = 0;
    private string condition = "0";
    private float l_vibration = 0f;         
    private float r_vibration = 0f;         
    private float hand_vibration = 0f;      
    private int num_pain = 0;
    private int num_current_ghosts = 0;
    private int num_total_ghosts = 0;       
    private int num_ghosts_captured = 0;
    private int num_current_haunted_toys = 0;
    private int num_total_toys_held = 0; 
    private int num_trigger_presses = 0;
    private float current_ghost_spawn_delay = 0f;
    private float absolute_look_ghost_direction_difference = 0f; //TODO: Fetch in update

    //Lines
    Queue<string> linesToWrite = new Queue<string>();


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (doLogging)
            InitLogFile();

        //TODO: Start logging coroutine
        StartCoroutine(WriteQueueCoroutine());
    }

    void InitLogFile()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.persistentDataPath, $"session_{timestamp}.csv");

        _writer = new StreamWriter(path, append: false);
        _sessionStart = Time.time;

        WriteLine("seconds_since_start;" +
            "participant_id;" +
            "condition;" +
            "l_vibration;" +
            "r_vibration;" +
            "hand_vibration;" +
            "num_pain;" +
            "num_current_ghosts;" +
            "num_total_ghosts;" +
            "num_ghosts_captured;" +
            "num_current_haunted_toys;" +
            "num_total_toys_held;" +
            "num_trigger_presses;" +
            "current_ghost_spawn_delay;" +
            "absolute_look_ghost_direction_difference");
    }

    void LateUpdate()
    {
        //Update Metrics
        GetLatestMetrics();

        if (Input.GetKeyDown(logPainButton)) num_pain++; //num_pain


        if (triggerButton.action.WasPerformedThisFrame()) 
        {
            num_trigger_presses++;
        }

        logging_started = GazeGameManager.Instance.IsGameStarted();

        //Queue Metric logging
        if (logging_started)
        {
            QueueLine();
        }
    }

    // --- Public API for other scripts ---

    
    public void Log(string message)
    {
        string entry = $"[{TimeStamp()}] {message}";
        WriteLine(entry);
    }

    public void SetParticipantID(int id) => participant_id = id;
    public void SetCondition(string conditionName) => condition = conditionName;
    public void IncreaseToyHeldCount() => num_total_toys_held++;


    // --- Shutdown ---

    void OnApplicationQuit()
    {
        if (!doLogging)
            return;

        //Stop coroutine
        StopCoroutine(WriteQueueCoroutine());
        //Write all lines in the queue to file
        for (int i = 0; i < linesToWrite.Count; i++)
        {
            WriteLineFromQueue();
        }

        _writer.Flush();
        _writer.Close();
    }

    // --- Helpers ---
    private void GetLatestMetrics()
    {
        //seconds_since_start
        if (logging_started) seconds_since_start += Time.deltaTime; 

        //Vibrations
        hand_vibration = handHaptics.GetLatestValue();
        float[] armNumbers = armHaptics.getVibrationValues();
        l_vibration = armNumbers[0];
        r_vibration = armNumbers[1];

        //Other metrics
        num_current_ghosts = GazeGameManager.Instance.GetCurrentNumOfGhosts();
        num_total_ghosts = GazeGameManager.Instance.GetTotalGhostsSpawned();
        num_ghosts_captured = caughtGhostsCounter.TotalCaught();
        num_current_haunted_toys = GazeGameManager.Instance.GetCurrentNumOfHauntedToys();
        current_ghost_spawn_delay = GazeGameManager.Instance.GetSpawnDelay();
        GetGhostLookDiference();
    }

    private void GetGhostLookDiference()
    {
        look_direction = PlayerHead.forward;

        GhostBehavior ghost = armHaptics.GetCurrentGhost();
        if (ghost != null) 
        {
            ghost_direction = (ghost.transform.position - PlayerHead.position).normalized;

            absolute_look_ghost_direction_difference = MathF.Abs(Vector3.Distance(look_direction, ghost_direction));
        }
        else
        {
            absolute_look_ghost_direction_difference = 0;
        }
    }

    private void WriteLine(string line)
    {
        if (!doLogging)
            return;

        _writer.WriteLine(line);
    }
    private string TimeStamp() => (Time.time - _sessionStart).ToString("F2") + "s";

    private void QueueLine()
    {
        linesToWrite.Enqueue($"{seconds_since_start};" +
            $"{participant_id};" +
            $"{condition};" +
            $"{l_vibration};" +
            $"{r_vibration};" +
            $"{hand_vibration};" +
            $"{num_pain};" +
            $"{num_current_ghosts};" +
            $"{num_total_ghosts};" +
            $"{num_ghosts_captured};" +
            $"{num_current_haunted_toys};" +
            $"{num_total_toys_held};" +
            $"{num_trigger_presses};" +
            $"{current_ghost_spawn_delay};" +
            $"{absolute_look_ghost_direction_difference}");
    }

    private void WriteLineFromQueue()
    {
        if (doLogging && linesToWrite.Count > 0)
        {
            WriteLine(linesToWrite.Dequeue());
        }
    }

    //Coroutine
    IEnumerator WriteQueueCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(writeInterval);
            for (int i = 0; i < MathF.Min(linesToWrite.Count, maxLinesPerWrite); i++)
            {
                WriteLineFromQueue();
            }
        }
    }
}