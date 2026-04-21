using System;
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

    [Header("Inputs")]
    public KeyCode logHapticsButton;
    public InputActionReference triggerButton;


    private StreamWriter _writer;
    private float _sessionStart;
    private int _participantID = 0;
    private string _condition = "0";
    private int _triggerPressCount = 0;
    private int _toyHeldCount = 0;
    private int _playerScore = 0;
    private float _latestLHaptic;
    private float _latestRHaptic;
    private float _latestHandHaptic;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (doLogging)
            InitLogFile();
    }

    void InitLogFile()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.persistentDataPath, $"session_{timestamp}.txt");

        _writer = new StreamWriter(path, append: false);
        _sessionStart = Time.time;

        WriteLine("===== SESSION START =====");
        WriteLine($"Time:     {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLine("=========================");
    }

    void Update()
    {
        if (Input.GetKeyDown(logHapticsButton))
        {
            GetLatestHaptics();
            Log("Left Arm: " + _latestLHaptic + " | Right Arm: " + _latestRHaptic + " | Hand: " + _latestHandHaptic);
            Debug.Log("Recorded haptic numbers");

        }


        if (triggerButton.action.WasPerformedThisFrame()) 
        {
            _triggerPressCount++;
            //Log("Increased Trigger Count to " + _triggerPressCount);
        }
    }

    // --- Public API for other scripts ---

    
    public void Log(string message)
    {
        string entry = $"[{TimeStamp()}] {message}";
        WriteLine(entry);
    }

    public void SetParticipantID(int id) => _participantID = id;
    public void SetCondition(string conditionName) => _condition = conditionName;
    public void IncreaseToyHeldCount() => _toyHeldCount++;


    // --- Shutdown ---

    void OnApplicationQuit()
    {
        if (!doLogging)
            return;

        float duration = Time.time - _sessionStart;

        if (caughtGhostsCounter != null)
            _playerScore = caughtGhostsCounter.TotalCaught();

        WriteLine("");
        WriteLine("===== SESSION SUMMARY =====");
        WriteLine($"Participant:    {_participantID}");
        WriteLine($"Condition:      {_condition}");
        WriteLine($"Played time:    {TimeSpan.FromSeconds(duration):hh\\:mm\\:ss}");
        WriteLine($"Trigger presses:{_triggerPressCount}");
        WriteLine($"Toys held:      {_toyHeldCount}");
        WriteLine($"Score:          {_playerScore}");
        WriteLine("===========================");

        _writer.Flush();
        _writer.Close();
    }

    // --- Helpers ---
    private void GetLatestHaptics()
    {
        _latestHandHaptic = handHaptics.GetLatestValue();
        float[] armNumbers = armHaptics.getVibrationValues();
        _latestLHaptic = armNumbers[0];
        _latestRHaptic = armNumbers[1];
    }

    private void WriteLine(string line)
    {
        if (!doLogging)
            return;

        _writer.WriteLine(line);
    }
    private string TimeStamp() => (Time.time - _sessionStart).ToString("F2") + "s";
}