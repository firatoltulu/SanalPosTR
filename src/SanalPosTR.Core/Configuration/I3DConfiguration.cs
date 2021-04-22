namespace SanalPosTR.Configuration
{
    public interface I3DConfiguration: IProviderConfiguration
    {
        string SiteSuccessUrl { get; set; }

        string SiteFailUrl { get; set; }

    }
}