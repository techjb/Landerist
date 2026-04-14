using landerist_library.Websites;

namespace landerist_tests
{
    internal static class PagesTests
    {
        public static void Run()
        {
            //Pages.DeleteNumPagesExceded();
            //Pages.DeleteCurentMachineLogs(PageType.ForbiddenLastSegment);
            //Pages.DeleteDuplicateUriQuery();
            //Pages.DeleteListingsHttpStatusCodeError();
            //Pages.DeleteListingsResponseBodyRepeated();
            //Pages.UpdateInvalidCadastastralReferences();
            //Pages.UpdateNextUpdate();
            //Pages.RemoveResponseBodyTextHashToAll();
            //Pages.RemoveResponseBodyTextHash(PageType.NotListingByLastSegment);
            //Pages.DeleteUnpublishedListings();
            //Pages.DeleteUrisLikePrint();
            //Pages.DeleteProhibitedUris();
            //new Page("https://areagestio.com/propiedades/25974555/").Insert();
            //var page = new Page("https://inmobiliariascalifal.com/inmueble/venta/piso/cordoba/cordoba/cal04135/");
            //page.Insert();

            Pages.UpdateNextUpdate();
        }
    }
}
