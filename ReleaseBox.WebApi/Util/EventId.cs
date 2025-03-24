namespace ReleaseBox.Util;

public static class EventId
{
    public static AsyncLocal<Microsoft.Extensions.Logging.EventId> CurrentEventId = new ();    
}