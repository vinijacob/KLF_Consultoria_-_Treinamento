using Klf.Application.DTOs.Common;
using Klf.Application.Validators.Common;

namespace Klf.Application.Tests.Validators;

public sealed class PagedRequestValidatorTests
{
    private readonly PagedRequestValidator _validator = new();

    [Fact]
    public void Request_is_valid_when_using_defaults()
    {
        var result = _validator.Validate(new PagedRequest());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, PagedRequest.MaxPageSize + 1)]
    public void Request_is_invalid_when_page_or_size_is_out_of_range(int page, int pageSize)
    {
        var result = _validator.Validate(new PagedRequest { Page = page, PageSize = pageSize });

        Assert.False(result.IsValid);
    }
}
