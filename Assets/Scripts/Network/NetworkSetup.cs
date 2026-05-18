#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using System.Linq;
#endif


using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using Debug = UnityEngine.Debug;






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
        uint processId = (uint)Process.GetCurrentProcess().Id;
        IntPtr hWnd = FindWindowByProcessId(processId);
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

#if UNITY_EDITOR
    [MenuItem("Tools/Build Windows (x64)", priority = 0)]

    public static bool BuildGame()
    {
        //Specify build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        buildPlayerOptions.locationPathName = Path.Combine("Build", "Blackjack.exe");
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.None;
        //Perform the build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Output the result of the build
        Debug.Log($"Build ended with status: {report.summary.result}");
        //Additional log on the build, looking at report.summary
        return report.summary.result == BuildResult.Succeeded;
    }
#endif

#if UNITY_EDITOR

    private static void Run(string path, string args)
    {
        //Start new process
        Process process = new Process();
        //configure the process using the StartInfo properties
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = args;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal; // choose the window style: Hidden, Minimized, Maximized, Normal
        process.StartInfo.RedirectStandardOutput = false; // Set to true to redirect the output (so you can read it in unity)
        process.StartInfo.UseShellExecute = true; //Set to false if you want to redirect output
        //Run to process
        process.Start();
    }

    [MenuItem("Tools/Build and Launch (Server)", priority = 10)]

    public static void BuildAndLaunch1()
    {
        CloseAll();
        if (BuildGame())
        {
            LaunchServer();
        }
    }
    [MenuItem("Tools/Build and Launch (Client)", priority = 15)]

    public static void BuildAndLaunchClient()
    {
        CloseAll();
        if (BuildGame())
        {
            LaunchClient();
        }
    }
    [MenuItem("Tools/Build and Launch (Server + Client)", priority = 20)]

    public static void BuildAndLaunchServerAndClient()
    {
        CloseAll();
        if (BuildGame())
        {
            LaunchClientAndServer();
        }
    }
    [MenuItem("Tools/Launch (Server) _F11", priority = 30)]

    public static void LaunchServer()
    {
        Run("Build\\Blackjack.exe", "--server");
    }
    [MenuItem("Tools/Launch (Server + Client)", priority = 40)]

    public static void LaunchClientAndServer()
    {
        LaunchServer();
        LaunchClient();
    }
    [MenuItem("Tools/Launch (Client)", priority = 45)]

    public static void LaunchClient()
    {
        Run("Build\\Blackjack.exe", "");
    }
    [MenuItem("Tools/Close All", priority = 100)]

    public static void CloseAll()
    {
        //Get all processes with the specified name
        Process[] processes = Process.GetProcessesByName("Blackjack");
        foreach (var  process in processes)
        {
            try
            {
                //close the process
                process.Kill();
                //Wait for the process to exit
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                // This could occur if the process has already exited or you dont have permission to kill it
                Debug.LogWarning($"Error trying to kill process {process.ProcessName}: {ex.Message}");
            }
        }
    }
#endif
}
