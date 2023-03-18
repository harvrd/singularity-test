using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sngty;

public class ExampleCommunicatorScript : MonoBehaviour
{
    public Debugger debugger;
    public SingularityManager mySingularityManager;
    public DeviceSignature myDevice;
    // Start is called before the first frame update
    void Start()
    {
        //debugger.Log("Started...");
        //debugger.Log("This is working.");
        List<DeviceSignature> pairedDevices = mySingularityManager.GetPairedDevices();
        //debugger.Log("This is working!!!!!!");
        //debugger.Log("pairedDevices");
        debugger.Log(pairedDevices);
        myDevice = new DeviceSignature();

        //If you are looking for a device with a specific name (in this case exampleDeviceName):
        for (int i = 0; i < pairedDevices.Count; i++)
        {
            debugger.Log(pairedDevices[i].name);
            if (pairedDevices[i].name == "HC-05")
            {
                myDevice = pairedDevices[i];
                break;
            }
        }

        if (!myDevice.Equals(default(DeviceSignature)))
        {
            //Do stuff to connect to the device here
            mySingularityManager.ConnectToDevice(myDevice);
            // mySingularityManager.sendMessage("Connected!", myDevice);
            // debugger.Log("After Send message");
        }
    }

    //KEEP IN MIND IF YOU ARE USING THIS WITH AN ANDROID DEVICE YOU WON'T BE ABLE TO READ LOGS FROM UNITY.
    //You'll have to read the logcat logs through USB debugging. Or just display your messages on a GUI instead of Debug.Log.
    public void onConnected()
    {
        Debug.Log("Connected to device!");
        debugger.Log("Connected to device!");
        debugger.Log("myDevice inside onConnected signature name is : ");
        debugger.Log(myDevice.name);
        // mySingularityManager.sendMessage("Connected inside onConnected!", myDevice);
        // debugger.Log("After Send message inside onConnected");
        

    }

    public void onMessageRecieved(string message)
    {
        Debug.Log("Message recieved from device: " + message);
        debugger.Log("Message recieved from device: " + message);
    }

    public void onError(string errorMessage)
    {
        Debug.LogError("Error with Singularity: " + errorMessage);
        debugger.Log("Error with Singularity: " + errorMessage);
    }

    // public void sendMessage(string message) 
    // {

    //     mySingularityManager.sendMessage(message, myDevice);
    //     debugger.Log("After Send message inside SendMessage!");

    // }

    public void printMessage(string message)
    {
        debugger.Log("Received: ");
        debugger.Log(message);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
