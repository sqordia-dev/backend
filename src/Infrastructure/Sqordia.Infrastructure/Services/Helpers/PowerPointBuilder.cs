using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Drawing = DocumentFormat.OpenXml.Drawing;
using Sqordia.Application.Models.Export;

namespace Sqordia.Infrastructure.Services.Helpers;

/// <summary>
/// Fluent builder for creating themed PowerPoint presentations using OpenXml.
/// Produces 16:9 widescreen slides (12192000 x 6858000 EMUs).
/// </summary>
public class PowerPointBuilder : IDisposable
{
    private readonly MemoryStream _stream;
    private PresentationDocument? _doc;
    private readonly PresentationPart _presentationPart;
    private readonly ExportTheme _theme;
    private readonly SlideLayoutPart _slideLayoutPart;
    private uint _nextSlideId = 256;
    private uint _nextShapeId = 2;
    private bool _built;

    // 16:9 dimensions in EMUs
    private const long SlideWidth = 12192000;
    private const long SlideHeight = 6858000;

    // Standard notes size (7.5" x 10" in EMUs)
    private const int NotesSizeWidth = 6858000;
    private const int NotesSizeHeight = 9144000;

    public PowerPointBuilder(ExportTheme theme)
    {
        _theme = theme;
        _stream = new MemoryStream();
        _doc = PresentationDocument.Create(_stream, PresentationDocumentType.Presentation);
        _presentationPart = _doc.AddPresentationPart();

        // OOXML schema requires strict child order in <p:presentation>:
        // sldMasterIdLst → sldIdLst → sldSz → notesSz
        _presentationPart.Presentation = new Presentation();

        // 1. Theme, SlideMaster, SlideLayout — adds SlideMasterIdList first
        _slideLayoutPart = InitPresentationStructure();

        // 2. SlideIdList (populated by AppendSlide)
        _presentationPart.Presentation.AppendChild(new SlideIdList());

        // 3. SlideSize
        _presentationPart.Presentation.AppendChild(
            new SlideSize { Cx = (int)SlideWidth, Cy = (int)SlideHeight, Type = SlideSizeValues.Custom });

        // 4. NotesSize
        _presentationPart.Presentation.AppendChild(
            new NotesSize { Cx = NotesSizeWidth, Cy = NotesSizeHeight });
    }

    // ═══════════════════════════════════════════════════════════
    // Title Slide
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddTitleSlide(string companyName, string title, string? subtitle, string? date)
    {
        var slide = CreateSlide();

        // Full background in primary color
        AddShapeWithFill(slide, 0, 0, SlideWidth, SlideHeight, _theme.PrimaryColor);

        // Company name
        AddTextBox(slide, 500000, 1200000, SlideWidth - 1000000, 1200000,
            companyName, 3600, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        // Title
        AddTextBox(slide, 500000, 2600000, SlideWidth - 1000000, 1000000,
            title, 2400, "FFFFFF", bold: false, align: Drawing.TextAlignmentTypeValues.Center);

        // Subtitle
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            AddTextBox(slide, 500000, 3700000, SlideWidth - 1000000, 600000,
                subtitle, 1600, "FFFFFF", bold: false, align: Drawing.TextAlignmentTypeValues.Center);
        }

        // Date
        if (!string.IsNullOrWhiteSpace(date))
        {
            AddTextBox(slide, 500000, 5800000, SlideWidth - 1000000, 400000,
                date, 1200, "FFFFFF", bold: false, align: Drawing.TextAlignmentTypeValues.Center);
        }

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Table of Contents
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddTableOfContentsSlide(string tocTitle, List<string> sectionTitles)
    {
        var slide = CreateSlide();

        // Title bar
        AddShapeWithFill(slide, 0, 0, SlideWidth, 1200000, _theme.PrimaryColor);
        AddTextBox(slide, 500000, 300000, SlideWidth - 1000000, 600000,
            tocTitle, 2800, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Left);

        // Section list
        var yOffset = 1500000L;
        for (int i = 0; i < sectionTitles.Count && yOffset < SlideHeight - 400000; i++)
        {
            var text = $"{i + 1}. {sectionTitles[i]}";
            AddTextBox(slide, 800000, yOffset, SlideWidth - 1600000, 400000,
                text, 1600, HexOnly(_theme.TextColor), bold: false, align: Drawing.TextAlignmentTypeValues.Left);
            yOffset += 450000;
        }

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Section Divider Slide
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddSectionDividerSlide(string sectionTitle, string? sectionNumber = null)
    {
        var slide = CreateSlide();

        // Full background in secondary color
        AddShapeWithFill(slide, 0, 0, SlideWidth, SlideHeight, _theme.SecondaryColor);

        // Section number (large, semi-transparent)
        if (!string.IsNullOrWhiteSpace(sectionNumber))
        {
            AddTextBox(slide, 500000, 1400000, SlideWidth - 1000000, 1200000,
                sectionNumber, 7200, HexOnly(_theme.AccentColor), bold: true,
                align: Drawing.TextAlignmentTypeValues.Center);
        }

        // Section title
        var titleY = string.IsNullOrWhiteSpace(sectionNumber) ? 2600000L : 2800000L;
        AddTextBox(slide, 500000, titleY, SlideWidth - 1000000, 1200000,
            sectionTitle, 3200, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Content Slide (single)
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddContentSlide(string title, List<string> bulletPoints)
    {
        var slide = CreateSlide();

        // Title bar
        AddShapeWithFill(slide, 0, 0, SlideWidth, 1200000, _theme.PrimaryColor);
        AddTextBox(slide, 500000, 300000, SlideWidth - 1000000, 600000,
            title, 2400, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Left);

        // Accent separator line
        AddShapeWithFill(slide, 500000, 1200000, SlideWidth - 1000000, 40000, _theme.AccentColor);

        // Bullet points
        var yOffset = 1500000L;
        foreach (var point in bulletPoints.Take(6))
        {
            var bulletText = $"\u2022  {point}";
            AddTextBox(slide, 700000, yOffset, SlideWidth - 1400000, 500000,
                bulletText, 1400, HexOnly(_theme.TextColor), bold: false, align: Drawing.TextAlignmentTypeValues.Left);
            yOffset += 550000;
        }

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Content Slides (multi-slide for long sections)
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddContentSlides(string title, List<string> bulletPoints,
        int maxPerSlide = 5, string language = "en")
    {
        if (bulletPoints.Count <= maxPerSlide)
        {
            return AddContentSlide(title, bulletPoints);
        }

        var contSuffix = language == "fr" ? " (suite)" : " (cont.)";
        var chunks = bulletPoints
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / maxPerSlide)
            .Select(g => g.Select(x => x.item).ToList())
            .ToList();

        for (int i = 0; i < chunks.Count; i++)
        {
            var slideTitle = i == 0 ? title : title + contSuffix;
            AddContentSlide(slideTitle, chunks[i]);
        }

        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Two-Column Slide
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddTwoColumnSlide(string title,
        string leftTitle, List<string> leftBullets,
        string rightTitle, List<string> rightBullets)
    {
        var slide = CreateSlide();

        // Title bar
        AddShapeWithFill(slide, 0, 0, SlideWidth, 1200000, _theme.PrimaryColor);
        AddTextBox(slide, 500000, 300000, SlideWidth - 1000000, 600000,
            title, 2400, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        // Column dimensions
        var colWidth = (SlideWidth - 1200000) / 2; // ~5.5M each
        var leftX = 400000L;
        var rightX = leftX + colWidth + 400000;
        var topY = 1400000L;

        // Left column header
        AddTextBox(slide, leftX, topY, colWidth, 400000,
            leftTitle, 1800, HexOnly(_theme.HeadingColor), bold: true, align: Drawing.TextAlignmentTypeValues.Left);

        // Left bullets
        var yOffset = topY + 500000;
        foreach (var point in leftBullets.Take(5))
        {
            AddTextBox(slide, leftX + 100000, yOffset, colWidth - 200000, 450000,
                $"\u2022  {point}", 1200, HexOnly(_theme.TextColor), bold: false, align: Drawing.TextAlignmentTypeValues.Left);
            yOffset += 480000;
        }

        // Right column header
        AddTextBox(slide, rightX, topY, colWidth, 400000,
            rightTitle, 1800, HexOnly(_theme.HeadingColor), bold: true, align: Drawing.TextAlignmentTypeValues.Left);

        // Right bullets
        yOffset = topY + 500000;
        foreach (var point in rightBullets.Take(5))
        {
            AddTextBox(slide, rightX + 100000, yOffset, colWidth - 200000, 450000,
                $"\u2022  {point}", 1200, HexOnly(_theme.TextColor), bold: false, align: Drawing.TextAlignmentTypeValues.Left);
            yOffset += 480000;
        }

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Table Slide
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddTableSlide(string title,
        List<string> headers, List<List<string>> rows, int? highlightRow = null)
    {
        var slide = CreateSlide();

        // Title bar
        AddShapeWithFill(slide, 0, 0, SlideWidth, 1000000, _theme.PrimaryColor);
        AddTextBox(slide, 500000, 250000, SlideWidth - 1000000, 500000,
            title, 2400, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Left);

        // Build OpenXML table
        var colCount = headers.Count;
        if (colCount == 0) { AppendSlide(slide); return this; }

        var tableWidth = SlideWidth - 800000;
        var colWidth = tableWidth / colCount;
        var maxRows = Math.Min(rows.Count, 8);
        var rowHeight = 350000L;
        var tableHeight = (maxRows + 1) * rowHeight; // +1 for header
        var tableX = 400000L;
        var tableY = 1200000L;

        // Create table graphic frame
        var graphicFrame = new GraphicFrame();
        var tableShapeId = _nextShapeId++;
        var nvGfPr = new NonVisualGraphicFrameProperties(
            new NonVisualDrawingProperties { Id = tableShapeId, Name = $"Table{tableShapeId}" },
            new NonVisualGraphicFrameDrawingProperties(),
            new ApplicationNonVisualDrawingProperties());
        graphicFrame.Append(nvGfPr);

        graphicFrame.Append(new Transform(
            new Drawing.Offset { X = tableX, Y = tableY },
            new Drawing.Extents { Cx = tableWidth, Cy = tableHeight }));

        var table = new Drawing.Table();

        // Table properties
        var tblPr = new Drawing.TableProperties { FirstRow = true, BandRow = true };
        tblPr.Append(new Drawing.NoFill());
        table.Append(tblPr);

        // Grid columns
        var tblGrid = new Drawing.TableGrid();
        for (int c = 0; c < colCount; c++)
            tblGrid.Append(new Drawing.GridColumn { Width = colWidth });
        table.Append(tblGrid);

        // Header row
        table.Append(CreateTableRow(headers, rowHeight, isHeader: true));

        // Data rows
        for (int r = 0; r < maxRows; r++)
        {
            var row = rows[r];
            var isHighlight = highlightRow.HasValue && r == highlightRow.Value;
            var isAlternate = r % 2 == 1;
            table.Append(CreateTableRow(row, rowHeight, isHeader: false,
                isAlternate: isAlternate, isHighlight: isHighlight));
        }

        // Overflow indicator
        if (rows.Count > 8)
        {
            var overflowText = $"... +{rows.Count - 8} more rows";
            AddTextBox(slide, tableX, tableY + tableHeight + 50000, tableWidth, 300000,
                overflowText, 1000, HexOnly(_theme.MutedTextColor), bold: false,
                align: Drawing.TextAlignmentTypeValues.Right);
        }

        var graphic = new Drawing.Graphic(
            new Drawing.GraphicData(table) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/table" });
        graphicFrame.Append(graphic);

        slide.ShapeTree!.Append(graphicFrame);
        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // SWOT Slide (2x2 grid)
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddSwotSlide(string title,
        List<string> strengths, List<string> weaknesses,
        List<string> opportunities, List<string> threats)
    {
        var slide = CreateSlide();

        // Title bar
        AddShapeWithFill(slide, 0, 0, SlideWidth, 1000000, _theme.PrimaryColor);
        AddTextBox(slide, 500000, 250000, SlideWidth - 1000000, 500000,
            title, 2400, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        // 2x2 grid
        var halfW = (SlideWidth - 1200000) / 2;
        var halfH = (SlideHeight - 1400000) / 2;
        var left = 400000L;
        var top = 1200000L;

        // Quadrant colors from chart palette
        var colors = _theme.ChartColorPalette;

        AddQuadrant(slide, left, top, halfW, halfH, "S", strengths, colors.Count > 0 ? colors[0] : "#2563EB");
        AddQuadrant(slide, left + halfW + 200000, top, halfW, halfH, "W", weaknesses, colors.Count > 1 ? colors[1] : "#EF4444");
        AddQuadrant(slide, left, top + halfH + 200000, halfW, halfH, "O", opportunities, colors.Count > 2 ? colors[2] : "#10B981");
        AddQuadrant(slide, left + halfW + 200000, top + halfH + 200000, halfW, halfH, "T", threats, colors.Count > 3 ? colors[3] : "#F59E0B");

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Metrics Slide
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddMetricsSlide(string title, List<(string Label, string Value)> metrics)
    {
        var slide = CreateSlide();

        // Title bar
        AddShapeWithFill(slide, 0, 0, SlideWidth, 1200000, _theme.PrimaryColor);
        AddTextBox(slide, 500000, 300000, SlideWidth - 1000000, 600000,
            title, 2400, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        // Metric cards (up to 4)
        var cardCount = Math.Min(metrics.Count, 4);
        if (cardCount == 0) { AppendSlide(slide); return this; }

        var cardWidth = (SlideWidth - 1000000 - (cardCount - 1) * 200000) / cardCount;
        var xOffset = 500000L;

        foreach (var (label, value) in metrics.Take(4))
        {
            // Card background
            AddShapeWithFill(slide, xOffset, 1800000, cardWidth, 2400000, _theme.MetricCardBg);

            // Value
            AddTextBox(slide, xOffset + 100000, 2200000, cardWidth - 200000, 1000000,
                value, 2800, HexOnly(_theme.MetricValueColor), bold: true, align: Drawing.TextAlignmentTypeValues.Center);

            // Label
            AddTextBox(slide, xOffset + 100000, 3300000, cardWidth - 200000, 600000,
                label, 1200, HexOnly(_theme.MetricLabelColor), bold: false, align: Drawing.TextAlignmentTypeValues.Center);

            xOffset += cardWidth + 200000;
        }

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Thank You Slide (with gradient)
    // ═══════════════════════════════════════════════════════════

    public PowerPointBuilder AddThankYouSlide(string title, string? contactInfo)
    {
        var slide = CreateSlide();

        // Full background
        AddShapeWithFill(slide, 0, 0, SlideWidth, SlideHeight, _theme.PrimaryColor);

        // Thank you text
        AddTextBox(slide, 500000, 2000000, SlideWidth - 1000000, 1200000,
            title, 3600, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        if (!string.IsNullOrWhiteSpace(contactInfo))
        {
            AddTextBox(slide, 500000, 4000000, SlideWidth - 1000000, 800000,
                contactInfo, 1400, "FFFFFF", bold: false, align: Drawing.TextAlignmentTypeValues.Center);
        }

        AppendSlide(slide);
        return this;
    }

    // ═══════════════════════════════════════════════════════════
    // Build
    // ═══════════════════════════════════════════════════════════

    public byte[] Build()
    {
        if (_built) throw new InvalidOperationException("Build() has already been called.");
        _built = true;

        _presentationPart.Presentation.Save();
        _doc!.Save();
        _doc.Dispose();
        _doc = null; // Prevent double-dispose from Dispose()
        return _stream.ToArray();
    }

    public void Dispose()
    {
        _doc?.Dispose();
        _doc = null;
        _stream?.Dispose();
    }

    // ───────────────────────────────────────────────────────────
    // Private helpers
    // ───────────────────────────────────────────────────────────

    private CommonSlideData CreateSlide()
    {
        var shapeTree = new ShapeTree();
        shapeTree.Append(new NonVisualGroupShapeProperties(
            new NonVisualDrawingProperties { Id = 1U, Name = "" },
            new NonVisualGroupShapeDrawingProperties(),
            new ApplicationNonVisualDrawingProperties()));
        shapeTree.Append(new GroupShapeProperties(CreateGroupTransform()));

        return new CommonSlideData { ShapeTree = shapeTree };
    }

    private void AppendSlide(CommonSlideData csd)
    {
        var slidePart = _presentationPart.AddNewPart<SlidePart>();
        slidePart.AddPart(_slideLayoutPart); // Link slide → layout → master chain
        slidePart.Slide = new Slide(csd, new ColorMapOverride(new Drawing.MasterColorMapping()));
        slidePart.Slide.Save();

        var slideIdList = _presentationPart.Presentation.SlideIdList!;
        slideIdList.AppendChild(new SlideId
        {
            Id = _nextSlideId++,
            RelationshipId = _presentationPart.GetIdOfPart(slidePart)
        });
    }

    private void AddTextBox(CommonSlideData csd, long x, long y, long width, long height,
        string text, int fontSizeHundredths, string fontColorHex, bool bold,
        Drawing.TextAlignmentTypeValues align)
    {
        var shape = new Shape();

        var nvSpPr = new NonVisualShapeProperties(
            new NonVisualDrawingProperties { Id = _nextShapeId++, Name = $"TextBox{_nextShapeId}" },
            new NonVisualShapeDrawingProperties(),
            new ApplicationNonVisualDrawingProperties());
        shape.Append(nvSpPr);

        var spPr = new ShapeProperties(
            new Drawing.Transform2D(
                new Drawing.Offset { X = x, Y = y },
                new Drawing.Extents { Cx = width, Cy = height }),
            new Drawing.PresetGeometry(new Drawing.AdjustValueList()) { Preset = Drawing.ShapeTypeValues.Rectangle });
        shape.Append(spPr);

        var txBody = new TextBody(
            new Drawing.BodyProperties { Wrap = Drawing.TextWrappingValues.Square },
            new Drawing.ListStyle());

        var para = new Drawing.Paragraph();
        var pPr = new Drawing.ParagraphProperties { Alignment = align };
        para.Append(pPr);

        var run = new Drawing.Run();
        var rPr = new Drawing.RunProperties { Language = "en-US", FontSize = fontSizeHundredths, Bold = bold };
        rPr.Append(new Drawing.SolidFill(new Drawing.RgbColorModelHex { Val = fontColorHex }));
        run.Append(rPr);
        run.Append(new Drawing.Text(text));
        para.Append(run);

        txBody.Append(para);
        shape.Append(txBody);

        csd.ShapeTree!.Append(shape);
    }

    private void AddShapeWithFill(CommonSlideData csd, long x, long y, long width, long height, string fillColorHex)
    {
        var shape = new Shape();

        var nvSpPr = new NonVisualShapeProperties(
            new NonVisualDrawingProperties { Id = _nextShapeId++, Name = $"Shape{_nextShapeId}" },
            new NonVisualShapeDrawingProperties(),
            new ApplicationNonVisualDrawingProperties());
        shape.Append(nvSpPr);

        var spPr = new ShapeProperties(
            new Drawing.Transform2D(
                new Drawing.Offset { X = x, Y = y },
                new Drawing.Extents { Cx = width, Cy = height }),
            new Drawing.PresetGeometry(new Drawing.AdjustValueList()) { Preset = Drawing.ShapeTypeValues.Rectangle },
            new Drawing.SolidFill(new Drawing.RgbColorModelHex { Val = HexOnly(fillColorHex) }),
            new Drawing.Outline(new Drawing.NoFill()));
        shape.Append(spPr);

        csd.ShapeTree!.Append(shape);
    }

    private void AddQuadrant(CommonSlideData csd, long x, long y, long w, long h,
        string label, List<string> items, string bgColor)
    {
        AddShapeWithFill(csd, x, y, w, h, bgColor);

        // Label (letter)
        AddTextBox(csd, x, y, w, 400000,
            label, 2000, "FFFFFF", bold: true, align: Drawing.TextAlignmentTypeValues.Center);

        // Items
        var itemText = string.Join("\n", items.Take(4).Select(i => $"\u2022 {i}"));
        if (!string.IsNullOrEmpty(itemText))
        {
            AddTextBox(csd, x + 100000, y + 450000, w - 200000, h - 500000,
                itemText, 1100, "FFFFFF", bold: false, align: Drawing.TextAlignmentTypeValues.Left);
        }
    }

    private Drawing.TableRow CreateTableRow(List<string> cells, long rowHeight,
        bool isHeader = false, bool isAlternate = false, bool isHighlight = false)
    {
        var row = new Drawing.TableRow { Height = rowHeight };

        foreach (var cellText in cells)
        {
            var cell = new Drawing.TableCell();

            // Cell text
            var txBody = new Drawing.TextBody(
                new Drawing.BodyProperties(),
                new Drawing.ListStyle());

            var para = new Drawing.Paragraph();
            var run = new Drawing.Run();
            var rPr = new Drawing.RunProperties
            {
                Language = "en-US",
                FontSize = isHeader ? 1200 : 1100,
                Bold = isHeader
            };

            var fontColor = isHeader
                ? HexOnly(_theme.TableHeaderFg)
                : HexOnly(_theme.TextColor);
            rPr.Append(new Drawing.SolidFill(new Drawing.RgbColorModelHex { Val = fontColor }));
            run.Append(rPr);
            run.Append(new Drawing.Text(cellText ?? ""));
            para.Append(run);
            txBody.Append(para);
            cell.Append(txBody);

            // Cell properties with fill
            var tcPr = new Drawing.TableCellProperties();
            tcPr.Append(new Drawing.LeftBorderLineProperties(new Drawing.SolidFill(
                new Drawing.RgbColorModelHex { Val = HexOnly(_theme.TableBorderColor) })) { Width = 12700 });
            tcPr.Append(new Drawing.RightBorderLineProperties(new Drawing.SolidFill(
                new Drawing.RgbColorModelHex { Val = HexOnly(_theme.TableBorderColor) })) { Width = 12700 });
            tcPr.Append(new Drawing.TopBorderLineProperties(new Drawing.SolidFill(
                new Drawing.RgbColorModelHex { Val = HexOnly(_theme.TableBorderColor) })) { Width = 12700 });
            tcPr.Append(new Drawing.BottomBorderLineProperties(new Drawing.SolidFill(
                new Drawing.RgbColorModelHex { Val = HexOnly(_theme.TableBorderColor) })) { Width = 12700 });

            string bgHex;
            if (isHeader) bgHex = HexOnly(_theme.TableHeaderBg);
            else if (isHighlight) bgHex = HexOnly(_theme.TableHighlightBg);
            else if (isAlternate) bgHex = HexOnly(_theme.TableAlternateRowBg);
            else bgHex = HexOnly(_theme.PageBackgroundColor);

            tcPr.Append(new Drawing.SolidFill(new Drawing.RgbColorModelHex { Val = bgHex }));
            cell.Append(tcPr);

            row.Append(cell);
        }

        return row;
    }

    private SlideLayoutPart InitPresentationStructure()
    {
        // 1. Theme part
        var themePart = _presentationPart.AddNewPart<ThemePart>();
        themePart.Theme = BuildMinimalTheme();
        themePart.Theme.Save();

        // 2. Slide master part (references theme)
        var slideMasterPart = _presentationPart.AddNewPart<SlideMasterPart>();
        slideMasterPart.AddPart(themePart);

        // 3. Slide layout part (child of master)
        var layoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
        layoutPart.SlideLayout = new SlideLayout(
            new CommonSlideData(new ShapeTree(
                new NonVisualGroupShapeProperties(
                    new NonVisualDrawingProperties { Id = 1U, Name = "" },
                    new NonVisualGroupShapeDrawingProperties(),
                    new ApplicationNonVisualDrawingProperties()),
                new GroupShapeProperties(CreateGroupTransform()))),
            new ColorMapOverride(new Drawing.MasterColorMapping()))
        { Type = SlideLayoutValues.Blank };
        layoutPart.SlideLayout.Save();

        // 4. Slide master element
        slideMasterPart.SlideMaster = new SlideMaster(
            new CommonSlideData(new ShapeTree(
                new NonVisualGroupShapeProperties(
                    new NonVisualDrawingProperties { Id = 1U, Name = "" },
                    new NonVisualGroupShapeDrawingProperties(),
                    new ApplicationNonVisualDrawingProperties()),
                new GroupShapeProperties(CreateGroupTransform()))),
            new ColorMap
            {
                Background1 = Drawing.ColorSchemeIndexValues.Light1,
                Text1 = Drawing.ColorSchemeIndexValues.Dark1,
                Background2 = Drawing.ColorSchemeIndexValues.Light2,
                Text2 = Drawing.ColorSchemeIndexValues.Dark2,
                Accent1 = Drawing.ColorSchemeIndexValues.Accent1,
                Accent2 = Drawing.ColorSchemeIndexValues.Accent2,
                Accent3 = Drawing.ColorSchemeIndexValues.Accent3,
                Accent4 = Drawing.ColorSchemeIndexValues.Accent4,
                Accent5 = Drawing.ColorSchemeIndexValues.Accent5,
                Accent6 = Drawing.ColorSchemeIndexValues.Accent6,
                Hyperlink = Drawing.ColorSchemeIndexValues.Hyperlink,
                FollowedHyperlink = Drawing.ColorSchemeIndexValues.FollowedHyperlink
            },
            new SlideLayoutIdList(
                new SlideLayoutId
                {
                    Id = 2147483649U,
                    RelationshipId = slideMasterPart.GetIdOfPart(layoutPart)
                }));
        slideMasterPart.SlideMaster.Save();

        // 5. Register master in the presentation (appended first — SlideIdList added later in constructor)
        _presentationPart.Presentation.AppendChild(
            new SlideMasterIdList(
                new SlideMasterId
                {
                    Id = 2147483648U,
                    RelationshipId = _presentationPart.GetIdOfPart(slideMasterPart)
                }));

        return layoutPart;
    }

    private Drawing.Theme BuildMinimalTheme()
    {
        var colorScheme = new Drawing.ColorScheme(
            new Drawing.Dark1Color(new Drawing.SystemColor { Val = Drawing.SystemColorValues.WindowText, LastColor = "000000" }),
            new Drawing.Light1Color(new Drawing.SystemColor { Val = Drawing.SystemColorValues.Window, LastColor = "FFFFFF" }),
            new Drawing.Dark2Color(new Drawing.RgbColorModelHex { Val = HexOnly(_theme.PrimaryColor) }),
            new Drawing.Light2Color(new Drawing.RgbColorModelHex { Val = HexOnly(_theme.PageBackgroundColor) }),
            new Drawing.Accent1Color(new Drawing.RgbColorModelHex { Val = HexOnly(_theme.AccentColor) }),
            new Drawing.Accent2Color(new Drawing.RgbColorModelHex { Val = HexOnly(_theme.SecondaryColor) }),
            new Drawing.Accent3Color(new Drawing.RgbColorModelHex { Val = "9BBB59" }),
            new Drawing.Accent4Color(new Drawing.RgbColorModelHex { Val = "8064A2" }),
            new Drawing.Accent5Color(new Drawing.RgbColorModelHex { Val = "4BACC6" }),
            new Drawing.Accent6Color(new Drawing.RgbColorModelHex { Val = "F79646" }),
            new Drawing.Hyperlink(new Drawing.RgbColorModelHex { Val = "0563C1" }),
            new Drawing.FollowedHyperlinkColor(new Drawing.RgbColorModelHex { Val = "954F72" }))
        { Name = "Sqordia" };

        var fontScheme = new Drawing.FontScheme(
            new Drawing.MajorFont(
                new Drawing.LatinFont { Typeface = "Calibri" },
                new Drawing.EastAsianFont { Typeface = "" },
                new Drawing.ComplexScriptFont { Typeface = "" }),
            new Drawing.MinorFont(
                new Drawing.LatinFont { Typeface = "Calibri" },
                new Drawing.EastAsianFont { Typeface = "" },
                new Drawing.ComplexScriptFont { Typeface = "" }))
        { Name = "Office" };

        return new Drawing.Theme(
            new Drawing.ThemeElements(colorScheme, fontScheme, BuildMinimalFormatScheme()),
            new Drawing.ObjectDefaults(),
            new Drawing.ExtraColorSchemeList())
        { Name = "Sqordia" };
    }

    private static Drawing.FormatScheme BuildMinimalFormatScheme()
    {
        var fs = new Drawing.FormatScheme { Name = "Office" };

        // Fill styles (3 required)
        var fills = new Drawing.FillStyleList();
        for (int i = 0; i < 3; i++)
            fills.Append(new Drawing.SolidFill(new Drawing.SchemeColor { Val = Drawing.SchemeColorValues.PhColor }));
        fs.Append(fills);

        // Line styles (3 required)
        var lines = new Drawing.LineStyleList();
        for (int i = 0; i < 3; i++)
        {
            var outline = new Drawing.Outline { Width = 12700 };
            outline.Append(new Drawing.SolidFill(new Drawing.SchemeColor { Val = Drawing.SchemeColorValues.PhColor }));
            lines.Append(outline);
        }
        fs.Append(lines);

        // Effect styles (3 required)
        var effects = new Drawing.EffectStyleList();
        for (int i = 0; i < 3; i++)
            effects.Append(new Drawing.EffectStyle(new Drawing.EffectList()));
        fs.Append(effects);

        // Background fill styles (3 required)
        var bgFills = new Drawing.BackgroundFillStyleList();
        for (int i = 0; i < 3; i++)
            bgFills.Append(new Drawing.SolidFill(new Drawing.SchemeColor { Val = Drawing.SchemeColorValues.PhColor }));
        fs.Append(bgFills);

        return fs;
    }

    private static Drawing.TransformGroup CreateGroupTransform() => new(
        new Drawing.Offset { X = 0L, Y = 0L },
        new Drawing.Extents { Cx = 0L, Cy = 0L },
        new Drawing.ChildOffset { X = 0L, Y = 0L },
        new Drawing.ChildExtents { Cx = 0L, Cy = 0L });

    private static string HexOnly(string hex)
    {
        var clean = hex?.TrimStart('#') ?? "000000";
        return clean.Length is 6 or 8 && clean.All(IsHexChar) ? clean : "000000";
    }

    private static bool IsHexChar(char c) =>
        c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
}
