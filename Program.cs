using ScrapeBootstrapVue;

var groups = await ComponentGroup.Scrape();

List<Component> components = new List<Component>();
foreach (var group in groups)
{
    Console.WriteLine($"{group.Name} - {group.Link}");
    components.AddRange(await Component.Scrape(group.Link));
}

File.WriteAllLines(@"c:\temp\components.csv", components.Select(c => c.ToString().Trim()));

