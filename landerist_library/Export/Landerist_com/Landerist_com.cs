using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Export.Landerist_com
{
    public enum ExportType
    {
        Listings,
        Updates
    }

    public class Landerist_com
    {

        protected static string GetFilePath(string subdirectory, string fileName)
        {
            return Config.EXPORT_DIRECTORY + subdirectory + "\\" + fileName;
        }

        protected static string GetLocalSubdirectory(CountryCode countryCode, ExportType exportType)
        {
            return countryCode.ToString() + "\\" + exportType.ToString();
        }

        protected static string GetFileName(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            return GetFileName(countryCode, exportType) + "." + fileExtension;
        }

        protected static string GetFileName(CountryCode countryCode, ExportType exportType)
        {
            return countryCode.ToString() + "_" + exportType.ToString();
        }

        protected static string GetObjectKey(CountryCode countryCode, ExportType exportType, string fileExtension)
        {
            string fileName = GetFileName(countryCode, exportType, fileExtension);            
            return countryCode.ToString() + "/" + exportType.ToString() + "/" + fileName;
        }

        protected static string GetFileNameWidhDate(DateTime dateTime, string prefix, string extension)
        {
            string datePart = dateTime.ToString("yyyyMMdd");
            return prefix + "_" + datePart + "." + extension;
        }
    }
}
