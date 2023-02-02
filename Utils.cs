namespace CocoaAni.Net.Downloader;

public class CalculateSpeedHandle
{
    public long LastSavedSize = 0;
    public DateTime LastCalcTime = DateTime.Now;
    public float CalcSecondInterval = 0;
    public long LastAppendSize = 0;
}

public class Utils
{
    public static long? CalcSpeedOfBytes(CalculateSpeedHandle handle, long savedSize)
    {
        if (savedSize == 0) return null;
        var now = DateTime.Now;
        var timeSpan = (now - handle.LastCalcTime).TotalSeconds;
        var appendSize = savedSize - handle.LastSavedSize;
        if (timeSpan < handle.CalcSecondInterval && appendSize < handle.LastAppendSize) return null;
        handle.LastCalcTime = now;
        handle.LastSavedSize = savedSize;
        handle.LastAppendSize = appendSize;
        return (long)(appendSize / timeSpan);
    }
}