using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Sngty
{
    public class SingularityManager : MonoBehaviour
    {
        public Debugger debugger;
        public UnityEvent onConnected;
        public UnityEvent<string> onMessageRecieved;
        public UnityEvent<string> onError;

        public AndroidJavaClass BluetoothManager;
        public AndroidJavaObject bluetoothManager;

        public List<AndroidJavaObject> connectedDevices;

        // Start is called before the first frame update
        void Start()
        {
            debugger.Log("Sanity Check");
            try{

                BluetoothManager = new AndroidJavaClass("com.harrysoft.androidbluetoothserial.BluetoothManager");
                // debugger.Log("Before Bluetooth manager");
                if (BluetoothManager == null) {
                    debugger.Log("BluetoothManager is null");
                }
                // debugger.Log(BluetoothManager);
                debugger.Log("After Bluetooth manager");
                bluetoothManager = BluetoothManager.CallStatic<AndroidJavaObject>("getInstance");
                // debugger.Log("bluetoothManager");
                debugger.Log(bluetoothManager);
                if (bluetoothManager == null) {
                    debugger.Log("bluetoothManager is null");
                }
                // debugger.Log("Startofdevices");
                connectedDevices = new List<AndroidJavaObject>();
                // debugger.Log("Endofdevices");
            }

            catch (Exception e)
            {
                debugger.Log(e);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ConnectToDevice(DeviceSignature sig)
        {
            AndroidJavaClass Schedulers = new AndroidJavaClass("io.reactivex.schedulers.Schedulers");
            AndroidJavaClass AndroidSchedulers = new AndroidJavaClass("io.reactivex.android.schedulers.AndroidSchedulers");
            bluetoothManager.Call<AndroidJavaObject>("openSerialDevice", sig.mac)
                            .Call<AndroidJavaObject>("subscribeOn",Schedulers.CallStatic<AndroidJavaObject>("io"))
                            .Call<AndroidJavaObject>("observeOn", AndroidSchedulers.CallStatic<AndroidJavaObject>("mainThread"))
                            .Call("subscribe", new RxSingleObserver(onError, onConnected, onMessageRecieved, connectedDevices));

        }

        public void sendMessagfe(string message, DeviceSignature sig)
        {
            for (int i = 0; i < connectedDevices.Count; i++)
            {
                if (connectedDevices[i].Get<string>("mac") == sig.mac)
                {
                    AndroidJavaObject connectedDevice = connectedDevices[i];
                    AndroidJavaObject deviceInterface = connectedDevice.Call<AndroidJavaObject>("toSimpleDeviceInterface");
                    deviceInterface.Call("sendMessage", message);
                    break;
                }
            }
        }

        public void DisconnectDevice(DeviceSignature sig)
        {
            bluetoothManager.Call("closeDevice", sig.mac);
            for (int i = 0; i < connectedDevices.Count; i++)
            {
                if (connectedDevices[i].Get<string>("mac") == sig.mac)
                {
                    connectedDevices.RemoveAt(i);
                    break;
                }
            }
        }

        public void DisconnectAll()
        {
            bluetoothManager.Call("close");
            connectedDevices.Clear();
        }

        public List<DeviceSignature> GetPairedDevices()
        {
            try
            {
                // debugger.Log("hello");


                // BluetoothManager = new AndroidJavaClass("com.harrysoft.androidbluetoothserial.BluetoothManager");


                // if (BluetoothManager == null) {
                //     debugger.Log("BluetoothManager is null");
                // }


                // bluetoothManager = BluetoothManager.CallStatic<AndroidJavaObject>("getInstance");

        
                // if (bluetoothManager == null) {
                //     debugger.Log("bluetoothManager is null");
                // }


                debugger.Log("got in");
                // debugger.Log(bluetoothManager);
                // debugger.Log(BluetoothManager);
                // debugger.Log("Please work");
                AndroidJavaObject pairedDevicesCollection = bluetoothManager.Call<AndroidJavaObject>("getPairedDevices");
                // debugger.Log("This is an error");
                // debugger.Log("got ina");
                AndroidJavaObject pairedDevicesIterator = pairedDevicesCollection.Call<AndroidJavaObject>("iterator");
                // debugger.Log("got inb");
                int size = pairedDevicesCollection.Call<int>("size");
//                debugger.Log("got inc");
                List<DeviceSignature> pairedDevices = new List<DeviceSignature>();
  //              debugger.Log("got ind");
                for (int i = 1; i <= size; i++)
                {
    //                debugger.Log("for loop");
                    AndroidJavaObject thisDevice = pairedDevicesIterator.Call<AndroidJavaObject>("next");
      //              debugger.Log("for loop1");
                    DeviceSignature thisSignature;
        //            debugger.Log("for loop2");
                    thisSignature.name = thisDevice.Call<string>("getName");
          //          debugger.Log("for loop3");
                    thisSignature.mac = thisDevice.Call<string>("getAddress");
            //        debugger.Log("for loop4");
                    pairedDevices.Add(thisSignature);
                }
              //  debugger.Log("got in2");
                return pairedDevices;
            }
            catch (Exception e)
            {
                //debugger.Log(e);
                List<DeviceSignature> pairedDevices = new List<DeviceSignature>();
                return pairedDevices;
            }
        }

        class RxSingleObserver : AndroidJavaProxy
        {
            private UnityEvent<string> onErrorEvent;
            private UnityEvent onConnectedEvent;
            private UnityEvent<string> onMessageRecievedEvent;
            private List<AndroidJavaObject> connectedDevices;
            public RxSingleObserver(UnityEvent<string> onErrorEvent, UnityEvent onConnectedEvent, UnityEvent<string> onMessageRecievedEvent, List<AndroidJavaObject> connectedDevices) : base("io.reactivex.SingleObserver")
            {
                this.onErrorEvent = onErrorEvent; 
                this.onConnectedEvent = onConnectedEvent;
                this.onMessageRecievedEvent = onMessageRecievedEvent;
                this.connectedDevices = connectedDevices;
            }

            void onError(AndroidJavaObject e) //e is type throwable in Java
            {
                Debug.LogWarning("Singularity BLUETOOTH ERROR");

                onErrorEvent.Invoke("Singularity: " + e.Call<string>("getMessage"));
            }

            void onSuccess(AndroidJavaObject connectedDevice) //connectedDevice is type BluetoothSerialDevice in Java
            {
                onConnectedEvent.Invoke();

                AndroidJavaObject deviceInterface = connectedDevice.Call<AndroidJavaObject>("toSimpleDeviceInterface");
                deviceInterface.Call("setMessageReceivedListener", new messageRecievedListener(onMessageRecievedEvent));
                connectedDevices.Add(connectedDevice);
            }

            void onSubscribe(AndroidJavaObject obj){}
        }

        class messageRecievedListener : AndroidJavaProxy
        {
            private UnityEvent<string> onMessageRecievedEvent;
            public messageRecievedListener(UnityEvent<string> onMessageRecievedEvent) : base("com.harrysoft.androidbluetoothserial.SimpleBluetoothDeviceInterface$OnMessageReceivedListener")
            {
                this.onMessageRecievedEvent = onMessageRecievedEvent;
            }

            void onMessageReceived(string message)
            {
                onMessageRecievedEvent.Invoke(message);
            }
        }

    }
    public struct DeviceSignature
    {
        public string name;
        public string mac;
    }
}