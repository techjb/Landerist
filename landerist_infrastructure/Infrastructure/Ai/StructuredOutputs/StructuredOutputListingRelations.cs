using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Ai.StructuredOutputs;

internal static class StructuredOutputListingRelations
{
    internal static void Attach(
        Listing listing,
        Anuncio announcement,
        Page page,
        WebsiteAccessServices websiteAccess,
        ListingMaterializationRules rules,
        StructuredOutputMaterializationOperations operations)
    {
        AddMedia(listing, announcement, page, websiteAccess, rules, operations);
        listing.sources.Add(new Source
        {
            sourceGuid = string.IsNullOrEmpty(announcement.ReferenciaDelAnuncio)
                ? null
                : operations.Clean(announcement.ReferenciaDelAnuncio),
            sourceUrl = page.Uri,
            sourceName = page.Website.Host
        });
    }

    private static void AddMedia(
        Listing listing,
        Anuncio announcement,
        Page page,
        WebsiteAccessServices websiteAccess,
        ListingMaterializationRules rules,
        StructuredOutputMaterializationOperations operations)
    {
        if (!rules.MediaEnabled || announcement.ImagenesDelAnuncio is not { Count: > 0 })
        {
            return;
        }

        List<(string url, string? title)> images = [];
        foreach (var image in announcement.ImagenesDelAnuncio
            .Take((int)StructuredOutputEsJson.MAX_URLS_DE_IMAGENES_DEL_ANUNCIO))
        {
            if (image is not null && !string.IsNullOrWhiteSpace(image.Url))
            {
                images.Add((image.Url, image.Titulo));
            }
        }
        operations.AddMediaImages(listing, page, websiteAccess, images);
    }
}
