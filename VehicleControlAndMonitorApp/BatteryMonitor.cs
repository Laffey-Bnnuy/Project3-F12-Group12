using EVSystem.Communication;
using EVSystem.Interfaces;

public class BatteryMonitor : IBattery
{
    private readonly J1939Adapter _j1939Adapter;

    public float BatteryLevel { get; private set; }
    public float Temperature { get; private set; }
    public float RemainingDriveTime { get; private set; }
    public float RemainingKm { get; private set; }
    public string BatteryMode { get; private set; }

    private readonly string _dataFilePath;
    private string[] _dataLines;
    private int _currentIndex = 0;

    public BatteryMonitor(J1939Adapter adapter, string dataFile)
    {
        _j1939Adapter = adapter;
        _dataFilePath = dataFile;
        _dataLines = File.ReadAllLines(_dataFilePath);
    }

    public void LoadNextData()
    {
        if (_dataLines.Length == 0) return;

        string line = _dataLines[_currentIndex];
        _currentIndex = (_currentIndex + 1) % _dataLines.Length;

        var parts = line.Split(',');

        foreach (var part in parts)
        {
            var kv = part.Split('=');

            if (kv[0] == "BatteryLevel")
                UpdateBatteryLevel(float.Parse(kv[1]));

            if (kv[0] == "Temperature")
                Temperature = float.Parse(kv[1]);

            if (kv[0] == "Mode")
                SetBatteryMode(kv[1]);
        }

        UpdateDriveEstimates();
    }


    public void SetBatteryMode(string mode)
    {
        BatteryMode = mode;
        UpdateDriveEstimates();
    }

    public void UpdateBatteryLevel(float newLevel)
    {
        BatteryLevel = newLevel;
        UpdateDriveEstimates();
    }

    private void UpdateDriveEstimates()
    {
        float baseKm = 400f;
        float baseTime = 8f; // in hours

        float modeMultiplier = BatteryMode switch
        {
            "Eco" => 1.2f,
            "Normal" => 1.0f,
            "Sport" => 0.8f,
            _ => 1.0f
        };

        RemainingKm = baseKm * (BatteryLevel / 100f) * modeMultiplier;
        RemainingDriveTime = baseTime * (BatteryLevel / 100f) * modeMultiplier;
    }

    public string GetStatus()
    {
        return $"Battery: {BatteryLevel}%, Mode: {BatteryMode}, Temp: {Temperature}°C, " +
               $"Range: {RemainingKm:F1} km, Drive Time: {RemainingDriveTime:F1} hr";
    }
}
