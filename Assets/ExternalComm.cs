using UnityEngine;
using System.IO.Ports;
using System.Net.Sockets;
using System;
using System.Text;

public class ExternalComm : MonoBehaviour {

    public static ExternalComm instance = null;

    //Variables for communicating through serial ports
    private SerialPort serialPort;
    private UdpClient UDPTX_resistance, UDPTX_weight;
    private bool initialContactWithArduino = true;     //establish a contact with the serial port as soon as it's ready
    private string IP = "224.0.0.200";
    public int PORT_resistance = 50000;
    private int PORT_weight = 51000;

    //Values for measuring + converting the FSR readings
    private int currentPressureValue = 0;
    private int fsrVoltage;
    private float fsrResistance, fsrConductance, fsrForce;
    private float fsrTrendSlope = 1.1328f;
    private float fsrTrendOffset = 89.35f;

    private string pressurePath;
    private string performancePath;
    private string scorePath;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        EstablishSerialPortConnection();
        EstablishUDPConnection();

        //GenerateFilePaths();

        gameObject.SetActive(false);
    }

    private void Update() {
        try {
            //We should not attempt to read data, if the port is not open.
            if (serialPort.IsOpen) {
                if (serialPort.BytesToRead > 1) {
                    //Read, process and save the data from the serial port buffer
                    try {
                        ReadBytes();
                        ConvertResistanceToConductance();

                        //OBS: This is the place where data is parsed to wherever it is needed.
                        
                    } catch (Exception e) {
                        Debug.LogWarning("Error when reading bytes or writing to document: " + e);
                    }

                    //Send data to Processing
                    try {
                        Byte[] sendBytes = Encoding.ASCII.GetBytes(currentPressureValue.ToString());
                        UDPTX_resistance.Send(sendBytes, sendBytes.Length);

                        sendBytes = Encoding.ASCII.GetBytes(string.Format("{0}", (int)fsrConductance));
                        UDPTX_weight.Send(sendBytes, sendBytes.Length);
                    } catch (Exception e) {
                        Debug.LogWarning("Connection failed: " + e);
                    }
                }
            }
        } catch (Exception e) {
            Debug.LogWarning("There's no port: " + e);
        }
    }

    private void ConvertResistanceToConductance() {
        //Calculating resistance
        fsrVoltage = NormalizeValue(currentPressureValue, 0, 255, 1, 5001);
        fsrResistance = 5001 - fsrVoltage;

        fsrResistance *= 10000;
        fsrResistance /= fsrVoltage;

        //Calculating conductance
        fsrConductance = (1000000 / fsrResistance);
    }

    int NormalizeValue(int value, int inMin, int inMax, int outMin, int outMax) {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    private void EstablishSerialPortConnection() {
        //Setting up the SerialPort to read values from the Arduino
        string[] ports = SerialPort.GetPortNames();

        //If there is anything attached to the SerialPort (e.g. Arduino) then establish connection to that port.
        if (ports.Length > 0) {
            serialPort = new SerialPort(ports[0], 9600);
            serialPort.Open();
            serialPort.ReadTimeout = 1;
        }
        else {
            Debug.LogWarning("NO PORT ATTACHED, PRESSURE DATA COLLECTION IGNORED!");
        }
    }

    private void EstablishUDPConnection() {
        //This is used for communication with Processing
        try {
            UDPTX_resistance = new UdpClient(PORT_resistance + 1);
            UDPTX_resistance.Connect(IP, PORT_resistance);
            UDPTX_weight = new UdpClient(PORT_weight + 1);
            UDPTX_weight.Connect(IP, PORT_weight);

        } catch (Exception e) {
            Debug.LogWarning("Can't access server: " + e);
        }
    }

    private void ReadBytes() {
        currentPressureValue = serialPort.ReadByte();
    }

    private void OnEnable() {
        try {
            serialPort.DiscardInBuffer();
        } catch (Exception e) {
            Debug.LogWarning("Could not discard buffer when ExternalComm.gameObject was enabled: " + e);
        }
    }

    private void OnApplicationQuit() {
        if (serialPort != null) {
            serialPort.Close();
        }
        UDPTX_resistance.Close();
        UDPTX_weight.Close();
    }
}
