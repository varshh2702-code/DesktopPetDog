using DesktopPetDog;
using System;
using System.Linq;
using Xunit;

namespace DesktopPetDog.Tests;

public class CoreTests
{
    [Theory]
    [InlineData("clean")][InlineData("dad")][InlineData("dog")][InlineData("school")]
    [InlineData("positive")][InlineData("motivation")][InlineData("compliment")]
    public void EveryCollectionHasAtLeast100UniqueItems(string category)
    {
        var items=MessageLibrary.ByCategory(category);
        Assert.True(items.Count>=100); Assert.Equal(items.Count,items.Distinct().Count());
    }
    [Fact]
    public void ShuffleBagAvoidsImmediateRepeats()
    {
        var store=new SettingsStore();store.Current.SaveLocally=false;var bag=new ShuffleBag(store);
        var values=Enumerable.Range(0,40).Select(i=>$"Item {i}").ToArray();var category="test-"+Guid.NewGuid();var drawn=Enumerable.Range(0,20).Select(_=>bag.Next(category,values)).ToArray();
        Assert.NotEqual(drawn[0],drawn[1]);
    }
    [Fact] public void DefaultsArePrivacyFriendly(){var p=new PetSettings();Assert.False(p.SoundEnabled);Assert.Equal("Manual",p.MoodFrequency);Assert.True(p.SaveLocally);}
}
