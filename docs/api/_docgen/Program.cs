using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

var apiDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var mdPath = args.Length > 0 ? args[0] : Path.Combine(apiDir, "Endpoints-Booking-Atracciones.md");
var outPath = args.Length > 1 ? args[1] : Path.Combine(apiDir, "Endpoints-Booking-Atracciones.docx");

if (!File.Exists(mdPath))
    throw new FileNotFoundException($"No se encontró: {mdPath}");

var lines = await File.ReadAllLinesAsync(mdPath);

using var doc = WordprocessingDocument.Create(outPath, WordprocessingDocumentType.Document);
var mainPart = doc.AddMainDocumentPart();
mainPart.Document = new Document(new Body());
var body = mainPart.Document.Body!;

foreach (var raw in lines)
{
    var line = raw.TrimEnd();
    if (line.StartsWith("# "))
        body.AppendChild(MakeParagraph(line[2..], "Heading1"));
    else if (line.StartsWith("## "))
        body.AppendChild(MakeParagraph(line[3..], "Heading2"));
    else if (line.StartsWith("### "))
        body.AppendChild(MakeParagraph(line[4..], "Heading3"));
    else if (line == "---")
        body.AppendChild(MakeParagraph("", "Normal"));
    else
        body.AppendChild(MakeParagraph(line, "Normal"));
}

mainPart.Document.Save();
Console.WriteLine($"Generado: {outPath}");

static Paragraph MakeParagraph(string text, string style)
{
    var p = new Paragraph();
  if (!string.IsNullOrEmpty(style))
    p.AppendChild(new ParagraphProperties(new ParagraphStyleId { Val = style }));
    var run = new Run();
    if (!string.IsNullOrEmpty(text))
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    p.AppendChild(run);
    return p;
}
