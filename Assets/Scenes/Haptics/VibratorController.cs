using Mono.Cecil.Cil;
using System.ComponentModel.Design.Serialization;
using System.IO.Ports;
using UnityEngine;

public class VibratorController : MonoBehaviour
{
    public string portName = "COM7";
    SerialPort arduinoPort;
    float connectionCheckTimer;
    float connectionCheckInterval = 0.5f;
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

        timerOn = 3f;
        timerOff = 3f;
    }

    void Update()
    {
        if (arduinoPort.IsOpen) CheckConnection();

        /* Just some test stuff
        if (on)
        {
            timerOff = 0.5f;

            if (increase)
            {
                if (vibrationVal <= 255) vibrationVal += 50;
                if (vibrationVal > 255) vibrationVal = 55;
                increase = false;
            }

            SendVibration(vibrationVal);

            timerOn -= 1*Time.deltaTime;
            if (timerOn <= 0) on = false;
        }
        else if (!on)
        {
            timerOn = 2.0f;
            increase = true;

            SendVibration(0);

            timerOff -= 1 * Time.deltaTime;
            if (timerOff <= 0) on = true;
        }*/

        //Debug.Log(vibrationVal);
        //Debug.Log(on);
    }

    // function for sending messages to the arduino
    public void SendArduinoSignal(string code, int intensity = -1)
    {
        /* Code tells the arduino what it should manipulate and intensity (0-255) is the value for the vibrators
         * Codes:
            PC      = procedure arm 
            PL      = play arm
            TN      = Timing LED on
            TF      = Timing LED off
         * Examples
            SendArduinoSignal("PL", 0);     sets play arm vibrators to 0
            SendArduinoSignal("PC", 200);   sets procedure arm to 200
            SendArduinoSignal("TF");        turns off timing LED
         * Error LED will blink rapidley for 1 sec if it recives a faulty message
        */

        if (arduinoPort != null && arduinoPort.IsOpen)
        {
            //Debug.Log("Sending signal");
            string message = (intensity >= 0) ? code + intensity.ToString() + "\n" : code + "\n";
            //Debug.Log(message);
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
}
