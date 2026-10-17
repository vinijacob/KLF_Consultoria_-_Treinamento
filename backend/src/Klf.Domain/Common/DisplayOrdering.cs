using Klf.Domain.Exceptions;

namespace Klf.Domain.Common;

/// <summary>
/// Keeps the positions of a hand-ordered list unique and sequential (0, 1, 2…). Choosing a position another item
/// holds swaps the two; an item without a chosen position keeps its place, or goes to the end when it is new to the list.
/// </summary>
public static class DisplayOrdering
{
    /// <summary>Places <paramref name="item"/> in its list and renumbers the whole list from 0.</summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="list">
    /// Items of the list in their current order. It may contain <paramref name="item"/>; when it does not (a new item,
    /// or one coming from another list), the item starts at the end.
    /// </param>
    /// <param name="item">Item being created or edited.</param>
    /// <param name="position">Chosen position, or <see langword="null"/> to keep the current one (the end, for a new item).</param>
    /// <exception cref="ValidationException">The position is outside the list.</exception>
    public static void Place<T>(IEnumerable<T> list, T item, int? position)
        where T : IOrderedEntity
    {
        var items = list.ToList();
        var current = items.FindIndex(other => other.Id == item.Id);

        if (current < 0)
        {
            items.Add(item);
            current = items.Count - 1;
        }
        else
        {
            items[current] = item;
        }

        var target = position ?? current;

        if (target < 0 || target >= items.Count)
        {
            throw new ValidationException(
                nameof(IOrderedEntity.DisplayOrder),
                items.Count == 1 ? "Este é o único item da lista: a posição é 0." : $"Escolha uma posição de 0 a {items.Count - 1}.");
        }

        (items[current], items[target]) = (items[target], items[current]);
        Renumber(items);
    }

    /// <summary>Takes <paramref name="item"/> out of its list and closes the gap it leaves.</summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="list">Items of the list in their current order, with or without <paramref name="item"/>.</param>
    /// <param name="item">Item leaving the list (deleted, or moved to another list).</param>
    public static void Remove<T>(IEnumerable<T> list, T item)
        where T : IOrderedEntity =>
        Renumber([.. list.Where(other => other.Id != item.Id)]);

    private static void Renumber<T>(List<T> items)
        where T : IOrderedEntity
    {
        for (var position = 0; position < items.Count; position++)
        {
            if (items[position].DisplayOrder != position)
            {
                items[position].MoveTo(position);
            }
        }
    }
}
