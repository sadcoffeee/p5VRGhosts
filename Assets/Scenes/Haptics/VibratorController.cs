using System.ComponentModel.Design.Serialization;
using System.IO.Ports;
using UnityEngine;

public class VibratorController : MonoBehaviour
{
    public string portName = "COM7";
    SerialPort arduinoPort;

    int vibrationVal = 55;

    float timerOn;
    float timerOff;
    bool on = false;
    bool increase = true;

    void Start()
    {
        arduinoPort = new SerialPort(portName, 115200);
        arduinoPort.ReadTimeout = 50;
        arduinoPort.Open();

        if (arduinoPort.IsOpen)
            Debug.Log("Serial port successfully opened");
        else
            Debug.LogError("Failed to open serial port");

        timerOn = 2.0f;
        timerOff = 0.5f;
    }

    void Update()
    {
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
        }

        //Debug.Log(vibrationVal);
        //Debug.Log(on);
    }

    void SendVibration(int intensity)
    {
        if (arduinoPort != null && arduinoPort.IsOpen)
        {
            Debug.Log("Sending signal");
            string message = intensity.ToString() + "\n";
            arduinoPort.WriteLine(message);
            arduinoPort.BaseStream.Flush();
        }
    }

    private void OnApplicationQuit()
    {
        if (arduinoPort != null && arduinoPort.IsOpen)
            arduinoPort.Close();
    }
}
