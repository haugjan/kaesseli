using CsvHelper.Configuration.Attributes;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Kaesseli.Infrastructure.Integration;

public class PostFinanceCsvSchema
{
    // ReSharper disable StringLiteralTypo
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IdentifierTypo
    [Name(name: "Datum")]
    public DateTime Datum { get; set; }

    [Name(name: "Bewegungstyp")]
    public string Bewegungstyp { get; set; }

    [Name(name: "Avisierungstext")]
    public string Avisierungstext { get; set; }

    [Name(name: "Gutschrift in CHF")]
    public decimal? GutschriftInChf { get; set; }

    [Name(name: "Lastschrift in CHF")]
    public decimal? LastschriftInChf { get; set; }

    [Name(name: "Label")]
    public string Label { get; set; }

    [Name(name: "Kategorie")]
    public string Kategorie { get; set; }
    // ReSharper restore StringLiteralTypo
    // ReSharper restore UnusedMember.Global
    // ReSharper restore IdentifierTypo
}