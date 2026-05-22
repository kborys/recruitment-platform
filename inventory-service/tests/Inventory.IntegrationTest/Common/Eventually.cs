namespace Inventory.IntegrationTest.Common;

public static class Eventually
{
    public static async Task<T> WaitFor<T>(
        Func<Task<T>> func,
        Predicate<T> predicate,
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        string? description = null)
    {
        timeout ??= TimeSpan.FromSeconds(20);
        interval ??= TimeSpan.FromMilliseconds(200);
        var deadLine = DateTime.UtcNow + timeout;
        var iterations = 0;
        T? lastResult = default;

        while (DateTime.UtcNow < deadLine)
        {
            var result = await func();
            lastResult = result;
            iterations++;

            if (predicate(result))
            {
                return result;
            }

            await Task.Delay(interval.Value);
        }

        var message = $"Timeout waiting for condition{(description != null ? $": {description}" : "")}. " +
                      $"Iterations: {iterations}, Last result: {lastResult}, Timeout: {timeout.Value.TotalSeconds}s";
        throw new TimeoutException(message);
    }
}