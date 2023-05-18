namespace SolarTracker.Services;



public interface IClock
{
    DateTime Now { get; }
    TimeOnly TimeNow { get; }
    DateOnly DateNow { get; }


}

public class Clock : IClock
{
    public DateTime Now => DateTime.Now;
    public TimeOnly TimeNow => TimeOnly.FromDateTime(Now);
    public DateOnly DateNow => DateOnly.FromDateTime(Now);

}
