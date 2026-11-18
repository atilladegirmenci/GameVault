using System.Text.Json;
using FluentAssertions;
using GameDBWebApi.DTOs;
using GameDBWebApi.DTOs.Game;

namespace GameDBWebApi.Tests.Unit.DTOs;

public class PaginatedListTests
{
    [Fact]
    public void SerializesItemsAndPaginationMetadataAsAnObject()
    {
        var response = new PaginatedList<GameCardDto>(
            [new GameCardDto { CatalogId = 42, Title = "Game", ImageUrl = "image" }],
            21,
            2,
            10);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var root = json.RootElement;

        root.ValueKind.Should().Be(JsonValueKind.Object);
        root.GetProperty("items").GetArrayLength().Should().Be(1);
        root.GetProperty("currentPage").GetInt32().Should().Be(2);
        root.GetProperty("pageSize").GetInt32().Should().Be(10);
        root.GetProperty("totalCount").GetInt32().Should().Be(21);
        root.GetProperty("totalPages").GetInt32().Should().Be(3);
        root.GetProperty("hasPrevious").GetBoolean().Should().BeTrue();
        root.GetProperty("hasNext").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void EmptyPageKeepsTheSameObjectShapeAndMetadata()
    {
        var response = new PaginatedList<GameCardDto>([], 21, 4, 10);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var root = json.RootElement;

        root.ValueKind.Should().Be(JsonValueKind.Object);
        root.GetProperty("items").GetArrayLength().Should().Be(0);
        root.GetProperty("currentPage").GetInt32().Should().Be(4);
        root.GetProperty("totalCount").GetInt32().Should().Be(21);
        root.GetProperty("totalPages").GetInt32().Should().Be(3);
        root.GetProperty("hasPrevious").GetBoolean().Should().BeTrue();
        root.GetProperty("hasNext").GetBoolean().Should().BeFalse();
    }
}
