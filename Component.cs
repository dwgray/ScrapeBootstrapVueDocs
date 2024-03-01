using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.Text;

internal class Component
{
    public string Name { get; set; }
    public List<ComponentItem> Properties { get; set; }
    public List<ComponentItem> Slots { get; set; }
    public List<ComponentItem> Events { get; set; }

    public override string ToString()
    {
        var result = new StringBuilder();
        result.AppendLine($"Component,Unknown,Unknown,Unknown,{Name},,,,{Name}");
        foreach (var p in Properties)
        {
            result.AppendLine($"Property,Unknown,Unknown,Unknown,{Name},,,,{p.Name},{p.Type},{p.Default}");
        }
        foreach (var e in Events)
        {
            result.AppendLine($"Event,Unknown,Unknown,Unknown,{Name},,,,{e.Name}");
        }
        foreach (var s in Slots)
        {
            result.AppendLine($"Slot,Unknown,Unknown,Unknown,{Name},,,,{s.Name}");
        }

        return result.ToString();
    }

    private Component(string name, List<ComponentItem> properties, List<ComponentItem> slots, List<ComponentItem> events)
    {
        Name = name;
        Properties = properties;
        Slots = slots;
        Events = events;
    }

    public static async Task<IEnumerable<Component>> Scrape(string url)
    {
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var reference = htmlDocument.DocumentNode.SelectSingleNode("//section[contains(@class, 'bd-component-reference')]");
        var componentNodes = reference.SelectNodes("//h3[starts-with(@id, 'comp-ref')]");

        var components = new List<Component>();
        foreach (var node in componentNodes)
        {
            components.Add(ScrapeComponent(node));
        }

        return components;
    }

    private static Component ScrapeComponent(HtmlNode node)
    {
        var title = node.Id.Replace("comp-ref-", "");

        var props = GetElements(node, "props", AddProp);
        var slots = GetElements(node, "slots", AddItem);
        var events = GetElements(node, "events", AddItem);
        var modelRow = GetElementRows(node, "v-model").FirstOrDefault();
        if (modelRow != null)
        {
            var cells = modelRow.SelectNodes(".//td")!;
            DecorateModel(cells[0], props);
            DecorateModel(cells[1], events);
        }

        return new Component(KebabToPascal(title), props, slots, events);
    }

    private static void DecorateModel(HtmlNode nameCell, List<ComponentItem> items)
    {
        var name = GetCode(nameCell);
        var item = items.FirstOrDefault(i => i.Name == name);
        if (item != null)
        {
            item.TagModel();
        }
    }

    private static List<ComponentItem> GetElements(HtmlNode title, string name, Func<HtmlNodeCollection, ComponentItem> add)
    {
        var properties = new List<ComponentItem>();
        foreach (var row in GetElementRows(title, name))
        {
            var cells = row.SelectNodes(".//td")!;

            properties.Add(add(cells));
        }
        return properties;
    }

    private static IEnumerable<HtmlNode> GetElementRows(HtmlNode title, string name)
    {
        var table = FindElementTable(title, name);
        return table == null ? new List<HtmlNode>() : table.SelectNodes(".//tr")!.Skip(1);
    }

    private static ComponentItem AddProp(HtmlNodeCollection nodes)
    {
        var name  = GetCode(nodes[0]);
        var type = GetCode(nodes[1]);
        var defaultValue = GetCode(nodes[2]);
        return new ComponentItem(
            name, type, string.IsNullOrWhiteSpace(defaultValue) ? "undefined" : defaultValue );
    }

    private static ComponentItem AddItem(HtmlNodeCollection nodes)
    {
        return new ComponentItem(GetCode(nodes[0]));
    }

    private static string GetCode(HtmlNode node)
    {
        var code = node.SelectSingleNode(".//code");
        var decoded = WebUtility.HtmlDecode(code == null ? node.InnerText : code.InnerText).Trim().Replace("\n","");
        return decoded.Contains(",") ? $"\"{decoded}\"" : decoded;
    }

    private static HtmlNode? FindElementTable(HtmlNode title, string name)
    {
        var section = title.Ancestors("section").FirstOrDefault();
        if (section == null)
        {
            Console.WriteLine($"No section found for {title.InnerText}:{name}");
            return null;
        }
        var head = section?.SelectSingleNode($".//h4[@id='{title.Id}-{name}']");
        if (head == null)
        {
            Console.WriteLine($"No head found for {title.InnerText}:{name}");
            return null;
        }
        var article = head.ParentNode;
        return article?.SelectSingleNode(".//table")!;
    }

    private static string KebabToPascal(string str)
    {
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        string titleCase = textInfo.ToTitleCase(str.Replace("-", " "));
        return titleCase.Replace(" ", "");
    }
}


public class ComponentItem
{
    public string Name { get; private set; }
    public string? Type { get; }
    public string? Default { get; }

    public ComponentItem(string name, string? type = null, string? @default = null)
    {
        Name = name;
        Type = type;
        Default = @default;
    }

    public void TagModel()
    {
        Name = $"{Name}:MODEL";
    }
}