using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Shouldly;
using TravelerCompanion.Models;

namespace TravelerCompanion.Tests;

public class BaseJobStateTests
{
    class TestJobState : BaseJobState
    {
        public int TestProperty { get; set; }
        
        public override string JobKey => $"{nameof(TestJobState)}:{TestProperty}".ToLowerInvariant();
    }
    
    [Fact]
    public void BaseJobState_WhenSameReference_ShouldBeEqual()
    {
        var jobState = new TestJobState { TestProperty = 1 };
        
        AssertShouldBeEqual(jobState, jobState);
    }

    [Fact]
    public void BaseJobState_WhenOneIsNull_ShouldNotBeEqual()
    {
        var jobState1 = new TestJobState { TestProperty = 1 };
        TestJobState? jobState2 = null;
        
        AssertShouldNotBeEqual(jobState1, jobState2);
    }
    
    [Fact]
    public void BaseJobState_WhenJobKeysAreSame_ShouldBeEqual()
    {
        var jobState1 = new TestJobState { TestProperty = 1 };
        var jobState2 = new TestJobState { TestProperty = 1 };

        AssertShouldBeEqual(jobState1, jobState2);
    }
    
    [Fact]
    public void BaseJobState_WhenJobKeysAreNotSame_ShouldNotBeEqual()
    {
        var jobState1 = new TestJobState { TestProperty = 1 };
        var jobState2 = new TestJobState { TestProperty = 2 };

        AssertShouldNotBeEqual(jobState1, jobState2);
    }
    
    [Fact]
    public void String_WhenSameAsJobKey_ShouldBeEqual()
    {
        var jobState = new TestJobState { TestProperty = 1 };
        var jobKey = "testjobstate:1";

        AssertShouldBeEqual(jobState, jobKey);
    }

    [Fact]
    public void BaseJobState_WhenUsedInIEnumerable_ExceptShouldWork()
    {
        var collection1 = new List<TestJobState>
        {
            new() { TestProperty = 1 },
            new() { TestProperty = 2 },
            new() { TestProperty = 3 }
        };
        
        var collection2 = new List<TestJobState>
        {
            new() { TestProperty = 2 },
            new() { TestProperty = 3 },
            new() { TestProperty = 4 },
            new() { TestProperty = 5 }
        };
        
        var except1 = collection1.Except(collection2).ToList();
        var except2 = collection2.Except(collection1).ToList();
        
        except1.Count.ShouldBe(1);
        except1.ShouldContain(new TestJobState{ TestProperty = 1 });
        except2.Count.ShouldBe(2);
        except2.ShouldContain(new TestJobState{ TestProperty = 4 });
    }
    
    private void AssertShouldBeEqual(BaseJobState x, BaseJobState y)
    {
        x.Equals(y).ShouldBeTrue();
        y.Equals(x).ShouldBeTrue();
        (x == y).ShouldBeTrue();
        (x != y).ShouldBeFalse();
        (y == x).ShouldBeTrue();
        (y != x).ShouldBeFalse();
    }
    private void AssertShouldBeEqual(BaseJobState x, string y)
    {
        x.Equals(y).ShouldBeTrue();
        (x == y).ShouldBeTrue();
        (x != y).ShouldBeFalse();
        (y == x).ShouldBeTrue();
        (y != x).ShouldBeFalse();
    }
    private void AssertShouldNotBeEqual(BaseJobState? x, BaseJobState? y)
    {
        (x?.Equals(y) ?? false).ShouldBeFalse();
        (y?.Equals(x) ?? false).ShouldBeFalse();
        (x == y).ShouldBeFalse();
        (x != y).ShouldBeTrue();
        (y == x).ShouldBeFalse();
        (y != x).ShouldBeTrue();
    }
    private void AssertShouldNotBeEqual(BaseJobState? x, string? y)
    {
        (x?.Equals(y) ?? false).ShouldBeFalse();
        (x == y).ShouldBeFalse();
        (x != y).ShouldBeTrue();
        (y == x).ShouldBeFalse();
        (y != x).ShouldBeTrue();
    }
}