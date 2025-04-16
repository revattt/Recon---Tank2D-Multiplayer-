using UnityEngine;
using TMPro;

public class SystemStatsDisplay : MonoBehaviour
{
    public TMP_Text statsText; // Assign TextMeshPro Text in Inspector

    void Update()
    {
        float cpuUsage = GetCPUUsage(); // Placeholder function (requires external library)
        float totalRam = SystemInfo.systemMemorySize; // Total RAM in MB
        float allocatedRam = System.GC.GetTotalMemory(false) / (1024 * 1024); // Unity-managed RAM in MB

        statsText.text = $"RAM: {allocatedRam:F0}MB / {totalRam}MB";
    }

    float GetCPUUsage()
    {
        // Unity does not provide CPU usage directly
        return 0; // You need an external library for actual CPU usage
    }
}
