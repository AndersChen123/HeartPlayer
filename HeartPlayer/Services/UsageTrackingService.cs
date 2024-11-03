namespace HeartPlayer.Services;

public class UsageTrackingService
{
    private DateTime _sessionStart;
    private IPreferences _preferences;

    public UsageTrackingService()
    {
        _preferences = Preferences.Default;
        _sessionStart = DateTime.Now;
    }

    public void StartSession()
    {
        var lastUsedTime = _preferences.Get("last_used_time", DateTime.Now);
        var resetInterval = _preferences.Get("ResetInterval", 5.0);
        if (lastUsedTime.AddHours(resetInterval) < DateTime.Now)
        {
            _preferences.Set("total_usage_time", 0L);
        }
        _sessionStart = DateTime.Now;
    }

    public void EndSession()
    {
        var duration = DateTime.Now - _sessionStart;
        var totalTime = _preferences.Get("total_usage_time", 0L);
        _preferences.Set("total_usage_time", totalTime + (long)duration.TotalSeconds);
        _preferences.Set("last_used_time", DateTime.Now);
    }

    public TimeSpan GetTotalUsageTime()
    {
        var seconds = _preferences.Get("total_usage_time", 0L);
        return TimeSpan.FromSeconds(seconds);
    }
}