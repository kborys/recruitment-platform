using FluentAssertions;
using MassTransit.Testing;

public static class MassTransitFluentExtensions
{
    public static async Task ShouldHavePublished<T>(
        this ITestHarness harness,
        FilterDelegate<IPublishedMessage<T>> filter,
        string reason = "") where T : class
    {
        var any = await harness.Published.Any(filter);
        any.Should().BeTrue(reason ?? $"Expected message of type {typeof(T).Name} to be published matching the criteria.");
    }
}