namespace landerist_library.Pages;

public partial class Page
{
    public bool IsMainPage()
    {
        return Website is not null && Uri.Equals(Website.MainUri);
    }

    public bool RedirectToAnotherUrl()
    {
        return !string.IsNullOrEmpty(RedirectUrl);
    }
}