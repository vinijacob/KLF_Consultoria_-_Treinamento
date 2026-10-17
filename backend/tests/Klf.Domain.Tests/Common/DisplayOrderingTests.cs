using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Common;

public sealed class DisplayOrderingTests
{
    private static List<Client> List(params int[] positions) =>
        [.. positions.Select((position, index) => new Client($"Loja {index}", null, null, position, true))];

    private static int[] Positions(IEnumerable<Client> clients) => [.. clients.Select(client => client.DisplayOrder)];

    [Fact]
    public void New_item_goes_to_the_end_when_no_position_is_chosen()
    {
        var list = List(0, 1, 2);
        var added = new Client("Nova", null, null, 0, true);

        DisplayOrdering.Place(list, added, position: null);

        Assert.Equal(3, added.DisplayOrder);
        Assert.Equal([0, 1, 2], Positions(list));
    }

    [Fact]
    public void Choosing_a_taken_position_swaps_the_two_items()
    {
        var list = List(0, 1, 2, 3);

        DisplayOrdering.Place(list, list[3], position: 1);

        Assert.Equal([0, 3, 2, 1], Positions(list));
    }

    [Fact]
    public void New_item_at_a_taken_position_sends_the_other_item_to_the_end()
    {
        var list = List(0, 1, 2);
        var added = new Client("Nova", null, null, 0, true);

        DisplayOrdering.Place(list, added, position: 0);

        Assert.Equal(0, added.DisplayOrder);
        Assert.Equal([3, 1, 2], Positions(list));
    }

    [Fact]
    public void Repeated_numbers_and_gaps_become_a_sequence_when_an_item_is_placed()
    {
        var list = List(0, 0, 4, 10);

        DisplayOrdering.Place(list, list[2], position: null);

        Assert.Equal([0, 1, 2, 3], Positions(list));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Position_outside_the_list_is_rejected_and_nothing_moves(int position)
    {
        var list = List(0, 1, 2);

        var error = Assert.Throws<ValidationException>(() => DisplayOrdering.Place(list, list[0], position));

        Assert.Equal(["Escolha uma posição de 0 a 2."], error.Errors["DisplayOrder"]);
        Assert.Equal([0, 1, 2], Positions(list));
    }

    [Fact]
    public void Removing_an_item_closes_the_gap()
    {
        var list = List(0, 1, 2, 3);

        DisplayOrdering.Remove(list, list[1]);

        Assert.Equal([0, 1, 1, 2], Positions(list));
    }
}
