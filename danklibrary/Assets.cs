namespace danklibrary
{
    public class Asset
    {
        required public string Id { get; set; }

        required public string Name { get; set; }

        required public string Description { get; set; }

        required public string Location { get; set; }

        public string? Picture { get; set; }

        public string? Keywords { get; set; }
    }
}
