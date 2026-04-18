using Domain.AlbumAggregate.AlbumEntity;
using Domain.UserAggregate.UserEntity;
using Shouldly;

namespace Domain.Tests.AlbumEntity;

[TestClass]
public class CollaboratorsTests
{
    [TestMethod]
    public void Return_False_When_Too_Many_Elements()
    {
        var collaborators_more_than_MaxCount = Enumerable
            .Range(default, Collaborators.MaxCount + 1)
            .Select(index => new UserId(index))
            .ToArray();

        bool result = Collaborators.TryCreateNew(
            collaborators_more_than_MaxCount,
            out var collaborators
        );

        result.ShouldBeFalse();
    }

    [TestMethod]
    public void Return_True_When_Create_From_Valid()
    {
        const int collaborator_count = Collaborators.MaxCount - 1;

        var valid_Collaborators = Enumerable
            .Range(default, Collaborators.MaxCount - 1)
            .Select(index => new UserId(index))
            .ToArray();

        Collaborators.TryCreateNew(valid_Collaborators, out var collaborators);

        Assert.IsNotNull(collaborators);
        collaborators.Value.Length.ShouldBe(collaborator_count);
    }

    [TestMethod]
    public void Return_True_When_Count_Valid_After_Remove_Duplicate()
    {
        const int duplicate_id = 0;
        const int duplicate_count = 10;
        var collaborators_less_than_MaxCount = Enumerable
            .Range(default, Collaborators.MaxCount - 1)
            .Select(index => new UserId(index));
        var duplicate_collaborators = Enumerable.Repeat(new UserId(duplicate_id), duplicate_count);
        var total_collaborators = collaborators_less_than_MaxCount
            .Concat(duplicate_collaborators)
            .ToArray();

        bool result = Collaborators.TryCreateNew(total_collaborators, out var _);

        result.ShouldBeTrue();
    }

    [TestMethod]
    public void Should_Not_Contain_Duplicate_Elements()
    {
        const int duplicate_id = 0;
        const int duplicate_count = Collaborators.MaxCount / 2;
        var collaborators_with_duplicate_ones = Enumerable
            .Repeat(new UserId(duplicate_id), duplicate_count)
            .ToArray();

        _ = Collaborators.TryCreateNew(collaborators_with_duplicate_ones, out var collaborators);

        Assert.IsNotNull(collaborators);
        collaborators.Value.ShouldBeUnique();
    }

    [TestMethod]
    public void Should_Have_Same_Count_When_Create_From_No_Duplicate()
    {
        const int collaborator_count = Collaborators.MaxCount - 1;

        var valid_Collaborators = Enumerable
            .Range(default, Collaborators.MaxCount - 1)
            .Select(index => new UserId(index))
            .ToArray();

        Collaborators.TryCreateNew(valid_Collaborators, out var collaborators);

        Assert.IsNotNull(collaborators);
        collaborators.Value.Length.ShouldBe(collaborator_count);
    }

    [TestMethod]
    public void Should_Equal_When_Create_From_Same_Set_With_No_Order()
    {
        var collaborators = Enumerable
            .Range(default, Collaborators.MaxCount - 1)
            .Select(index => new UserId(index))
            .ToArray();

        Collaborators.TryCreateNew(collaborators, out var collaborators1);
        Random.Shared.Shuffle(collaborators);
        Collaborators.TryCreateNew(collaborators, out var collaborators2);
        Assert.IsNotNull(collaborators1);
        Assert.IsNotNull(collaborators2);

        collaborators1.Equals(collaborators2).ShouldBeTrue();
        (collaborators1 == collaborators2).ShouldBeTrue();
        EqualityComparer<Collaborators>
            .Default.Equals(collaborators1, collaborators2)
            .ShouldBeTrue();
    }

    [TestMethod]
    public void Return_True_When_Create_From_Empty()
    {
        var empty_collaborators = new Collaborators()
            .Value.Select(c => new UserId(c.Value))
            .ToArray();

        bool result = Collaborators.TryCreateNew(empty_collaborators, out var _);

        result.ShouldBeTrue();
    }

    [TestMethod]
    public void Return_True_When_Create_From_MaxCount()
    {
        var maxcount_collaborators = Enumerable
            .Range(default, Collaborators.MaxCount)
            .Select(index => new UserId(index))
            .ToArray();

        bool result = Collaborators.TryCreateNew(maxcount_collaborators, out var _);

        result.ShouldBeTrue();
    }
}
