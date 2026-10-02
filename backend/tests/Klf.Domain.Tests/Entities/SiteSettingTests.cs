using Klf.Domain.Common;
using Klf.Domain.Entities;

namespace Klf.Domain.Tests.Entities;

public sealed class SiteSettingTests
{
    [Fact]
    public void Value_is_replaced_when_setting_is_updated()
    {
        var setting = new SiteSetting(SiteSettingKeys.Seo, "{\"title\":\"A\"}");

        setting.Update("{\"title\":\"B\"}");

        Assert.Equal("{\"title\":\"B\"}", setting.ValueJson);
        Assert.Equal(SiteSettingKeys.Seo, setting.Key);
    }

    [Fact]
    public void Keys_are_unique_when_listed()
    {
        Assert.Equal(SiteSettingKeys.All.Count, SiteSettingKeys.All.Distinct().Count());
    }
}
