using Klf.Infrastructure.Services;

namespace Klf.Infrastructure.Tests.Services;

public sealed class HtmlContentSanitizerTests
{
    private readonly HtmlContentSanitizer _sanitizer = new();

    [Theory]
    [InlineData("<p><u>sublinhado</u> <s>tachado</s> <mark>marca-texto</mark> m<sup>2</sup> H<sub>2</sub>O</p>")]
    [InlineData("<h4>Tópico</h4>")]
    [InlineData("<ol type=\"a\" class=\"klf-list-lower-alpha\"><li><p>item</p></li></ol>")]
    [InlineData("<ol type=\"I\" start=\"3\" class=\"klf-list-upper-roman\"><li><p>item</p></li></ol>")]
    [InlineData("<ul class=\"klf-list-dash\"><li><p>item</p></li></ul>")]
    [InlineData("<p><span class=\"klf-color-brand\">azul</span> <span class=\"klf-color-accent\">tijolo</span></p>")]
    [InlineData("<p style=\"text-align: center\">centro</p>")]
    [InlineData("<img src=\"/midia/01a136cf-8a56-7170-b090-f4916404e890\" alt=\"Turma no treinamento\">")]
    public void Keeps_what_the_editor_produces(string html)
    {
        Assert.Equal(html, _sanitizer.Sanitize(html));
    }

    [Fact]
    public void Removes_classes_that_are_not_from_the_editor()
    {
        var clean = _sanitizer.Sanitize("<p class=\"fixed inset-0 klf-color-brand\">x</p>");

        Assert.Equal("<p class=\"klf-color-brand\">x</p>", clean);
    }

    [Fact]
    public void Removes_every_css_property_except_alignment()
    {
        var clean = _sanitizer.Sanitize("<p style=\"text-align: right; position: fixed; color: red\">x</p>");

        Assert.Equal("<p style=\"text-align: right\">x</p>", clean);
    }

    [Theory]
    [InlineData("<p>oi<script>alert(1)</script></p>", "<p>oi</p>")]
    [InlineData("<a href=\"javascript:alert(1)\">x</a>", "<a>x</a>")]
    [InlineData("<img src=\"http://inseguro.test/a.png\" onerror=\"alert(1)\">", "<img>")]
    public void Removes_scripts_and_unsafe_links(string html, string expected)
    {
        Assert.Equal(expected, _sanitizer.Sanitize(html));
    }
}
