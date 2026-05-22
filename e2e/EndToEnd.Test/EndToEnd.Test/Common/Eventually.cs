namespace EndToEnd.Test.Common;

public static class Eventually
{
    public static async Task<T> WaitFor<T>(
        Func<Task<T>> func,
        Predicate<T> predicate,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        timeout ??= TimeSpan.FromSeconds(20);
        interval ??= TimeSpan.FromMilliseconds(200);
        var deadLine = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadLine)
        {
            var result = await func();
            if (predicate(result))
            {
                return result;
            }
            
            await Task.Delay(interval.Value);
        }
        
        throw new TimeoutException();
    }
}