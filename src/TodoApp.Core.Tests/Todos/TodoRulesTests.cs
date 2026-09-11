using System.Collections.Immutable;
using FluentAssertions;
using TodoApp.Core.Results;
using TodoApp.Core.Todos;

namespace TodoApp.Core.Tests.Todos;

public sealed class TodoRulesTests
{
    [Fact]
    public void NormalizeTrimsOptionalValuesAndDeduplicatesTagsCaseInsensitively()
    {
        var result = TodoRules.Normalize(new TodoDraft(
            "  title  ",
            "  description  ",
            priority: TodoPriority.High,
            tags: [" Work ", "work", "", " Home "]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new TodoDraft(
            "title",
            "description",
            priority: TodoPriority.High,
            tags: ["Work", "Home"]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeRejectsMissingTitle(string title)
    {
        var result = TodoRules.Normalize(new TodoDraft(title));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TodoErrorCode.Validation);
    }

    [Fact]
    public void NormalizeRejectsTitleOver200Characters()
    {
        var result = TodoRules.Normalize(new TodoDraft(new string('x', 201)));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TodoErrorCode.Validation);
    }

    [Fact]
    public void NormalizeRejectsDescriptionAndTagLimits()
    {
        TodoRules.Normalize(new TodoDraft("title", new string('x', 2_001))).IsSuccess.Should().BeFalse();
        TodoRules.Normalize(new TodoDraft("title", tags: [new string('x', 41)])).IsSuccess.Should().BeFalse();
        TodoRules.Normalize(new TodoDraft("title", tags: Enumerable.Range(1, 11).Select(value => $"tag{value}"))).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void NextTimestampIsMonotonicAndUsesUtc()
    {
        var previous = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        TodoRules.NextTimestamp(previous.AddTicks(-1), previous).Should().Be(previous.AddTicks(1));
        TodoRules.NextTimestamp(previous.AddSeconds(1), previous).Should().Be(previous.AddSeconds(1));
    }
}
