using System.ComponentModel.Design.Serialization;
using System.IO.Ports;
using UnityEngine;
using System;

public class VibratorController : MonoBehaviour
{

    public string portName = "COM7";
    SerialPort arduinoPort;
    float connectionCheckTimer;
    float connectionCheckInterval = 0.25f;
    [HideInInspector] public bool connectionEstablished = false;


    void Start()
    {

        connectionCheckTimer = connectionCheckInterval;

        arduinoPort = new SerialPort(portName, 115200);
        if (arduinoPort != null)
        {
            arduinoPort.ReadTimeout = 50;
            arduinoPort.Open();

            if (arduinoPort.IsOpen)
            {
                Debug.Log("Serial port successfully opened");
                connectionEstablished = true;
            } 
            else
                Debug.LogError("Failed to open serial port");
        }
    }

    void Update()
    {
        if (arduinoPort.IsOpen) CheckConnection();
    }

    // function for sending messages to the arduino
    public void SendArduinoSignal(string code, float intensity = -1)
    {
        /* Code tells the arduino what it should manipulate and intensity (0-165 Hz) is the value for the vibrators
         * Codes:
            PC      = procedure arm 
            PL      = play arm
            TN      = Timing LED on
            TF      = Timing LED off
         * Examples
            SendArduinoSignal("PL", 0);     sets play arm vibrators to 0
            SendArduinoSignal("PC", 165);   sets procedure arm to 165
            SendArduinoSignal("TF");        turns off timing LED
         * Error LED will blink rapidley for 1 sec if it recives a faulty message
        */

        if (arduinoPort != null && arduinoPort.IsOpen)
        {
            int intensitySignal = -1;
            
            if (intensity != -1)
            {
                //Nomalize value
                intensity = intensity / 165;
                //convert normalized value to Arduino output
                intensitySignal = Mathf.RoundToInt(intensity * 255);
            }

            //Send signal
            string message = (intensitySignal >= 0) ? code + intensitySignal.ToString() + "\n" : code + "\n";
            arduinoPort.WriteLine(message);
            arduinoPort.BaseStream.Flush();
        }
    }

    //function that checks the connection, error LED will blink if no connection.
    void CheckConnection()
    {
        connectionCheckTimer -= 1* Time.deltaTime;
        if (connectionCheckTimer <= 0)
        {
            string message = "connectionCheck" + "\n";
            //Debug.Log(message);
            arduinoPort.WriteLine(message);
            arduinoPort.BaseStream.Flush();

            connectionCheckTimer = connectionCheckInterval;
        }
    }

    private void OnApplicationQuit()
    {
        if (arduinoPort != null && arduinoPort.IsOpen)
            arduinoPort.Close();
    }
    private void OnDestroy()
    {
        if (arduinoPort != null && arduinoPort.IsOpen)
            arduinoPort.Close();
    }
}
