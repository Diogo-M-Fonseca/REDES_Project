using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;


#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
using System.Diagnostics;
#endif

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
        SetWindowTile("Starting as server...");
        //wait a frame for setups to be done
        yield return null;
        if (networkManager.StartServer())
        {
            SetWindowTile("Blackjack - Server");
            UnityEngine.Debug.Log($"Serving on port {transport.ConnectionData.Port}...");
        }
        else
        {
            SetWindowTile("Fail to start as server");
            UnityEngine.Debug.LogError($"Failed to serve on port {transport.ConnectionData.Port}...");
        }

    }
    IEnumerator StartAsClientCR()
    {
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        var transport = GetComponent<UnityTransport>();
        transport.enabled = true;
        SetWindowTile("Starting as client...");
        //Wait a frame for setups to be done
        yield return null;
        if (networkManager.StartClient())
        {
            SetWindowTile("Blackjack - client");
            UnityEngine.Debug.Log($"Connecting on port {transport.ConnectionData.Port}...");
        }
        else
        {
            SetWindowTile("Fail to start as client");
            UnityEngine.Debug.LogError($"Failed to connect on port {transport.ConnectionData.Port}...");
        }
    }

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll",  SetLastError = true)]

    static extern bool SetWindowText(IntPtr hWnd, string lpString);
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")]
    static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    //Delegate to filter widows
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private static IntPtr FindWindowByProcessId(uint processId)
    {
        IntPtr windowHandle = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            uint windowProcessId;
            GetWindowThreadProcessId(hWnd,  out windowProcessId);
            if (windowProcessId == processId)
            {
                windowHandle = hWnd;
                return false; //Found the window, stop enumerating
            }
            return true; // Continue enumerating
        }, IntPtr.Zero);
        return windowHandle;

    }




    static void SetWindowTile(string title)
    {
#if !UNITY_EDITOR
        uint processid = (uint)Process.GetCurrentProcess().id;
        IntPtr hWnd = FindWindowByProcessId(processId)
        if (hWnd != IntPtr.Zero)
        {
            SetWindowText(hWnd, title);
        }
#endif
    }
#else
    static void SetWindowTile(string tile)
    {

    }
#endif
}
