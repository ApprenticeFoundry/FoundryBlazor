using FoundryBlazor.Shape;

namespace FoundryBlazor.Persistence;

public class ModelPersist
{
    public VersionInfo? Version { get; set; }
    public PageSetPersist<PagePersist>? Pages { get; set; }

    public ModelPersist()
    {
    }
    public ModelPersist(VersionInfo version)
    {
        Version = version;
    }

    public PagePersist PersistPage(FoPage2D page) 
    {
        Pages ??= new PageSetPersist<PagePersist>();
        var Page = new PagePersist();
        Pages.Add(Page.SavePage(page));
        return Page;
    }

    public void RestorePages(IPageManagement manager)
    {
        var page = manager.CurrentPage();
        Pages?.ForEach(Page =>
        {
            Page.RestorePage(page);
        });


    }
}
