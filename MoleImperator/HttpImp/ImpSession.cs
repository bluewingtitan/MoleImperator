namespace MoleImperator.HttpImp;

// stateful representation of a MI session
public sealed class ImpSession
{
    private const int SessionSeconds = 60 * 60 * 2;
    // invalidate sessions 5 minutes before actual invalidation.
    private const int SecondBuffer = 60 * 5;

    private object _token;
    private readonly DateTime _endTime;
    public bool IsElapsed => DateTime.Now < _endTime;
    public bool IsValid => _token != null && !IsElapsed; 

    public ImpSession()
    {
        _endTime = DateTime.Now.AddSeconds(SessionSeconds - SecondBuffer);
    }

}