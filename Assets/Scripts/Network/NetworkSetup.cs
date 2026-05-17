using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkSetup : MonoBehaviour
{
    //Código do excelentissimo stor Diogo
    private bool isServer = false;

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for(int i = 0; i < args.Length; i++) 
        {
            if(args[i] == "--server")
            {
                //--server found, this should be a server application
                isServer = true;
            }

        }
        if (isServer)
        {
            StartCoroutine(StartAsServerCR());
        }
        else
        {
            StartCoroutine(StartAsClientCR());
        }
        
    }

    IEnumerator StartAsServerCR()
    {
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        var transport = GetComponent<UnityTransport>();
        transport.enabled = true;
        //wait a frame for setups to be done
        yield return null;
        if (networkManager.StartServer())
        {
            UnityEngine.Debug.Log($"Serving on port {transport.ConnectionData.Port}...");
        }
        else
        {
            UnityEngine.Debug.LogError($"Failed to serve on port {transport.ConnectionData.Port}...");
        }

    }


    IEnumerator StartAsClientCR()
    {
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        var transport = GetComponent<UnityTransport>();
        transport.enabled = true;
        //Wait a frame for setups to be done
        yield return null;
        if (networkManager.StartClient())
        {
            UnityEngine.Debug.Log($"Connecting on port {transport.ConnectionData.Port}...");
        }
        else
        {
            UnityEngine.Debug.LogError($"Failed to connect on port {transport.ConnectionData.Port}...");
        }
    }
}
