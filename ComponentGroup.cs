using HtmlAgilityPack;
namespace ScrapeBootstrapVue;
internal class ComponentGroup
{
    public string Name { get; set; }
    public string Link { get; set; }

    private ComponentGroup(string name, string link)
    {
        Name = name;
        Link = link;
    }

    public static async Task<List<ComponentGroup>> Scrape()
    {
        var urlBase = "https://bootstrap-vue.org";
        var url = urlBase + "/docs/components";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var components = new List<ComponentGroup>();

        var componentNodes = htmlDocument.DocumentNode.SelectNodes("//nav[contains(@class, 'list-group')]//a[contains(@class, 'list-group-item')]");

        foreach (var node in componentNodes)
        {
            var nameNode = node.SelectSingleNode("strong");
            var name = nameNode.InnerText.Trim();
            var link = urlBase + node.GetAttributeValue("href", "");

            components.Add(new ComponentGroup(name,link));
        }

        return components;
    }
}
