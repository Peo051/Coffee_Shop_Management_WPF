using System.Globalization;
using System.IO;
using System.Text;

namespace CoffeeShop.Wpf.Services;

internal static class SimplePdfWriter
{
    public static async Task WriteLinesAsPdfAsync(
        string outputPath,
        IReadOnlyList<string> lines,
        CancellationToken cancellationToken = default)
    {
        var safeLines = lines
            .Select(SanitizeToPdfText)
            .Select(EscapePdfText)
            .ToList();

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 11 Tf");
        contentBuilder.AppendLine("40 800 Td");
        contentBuilder.AppendLine("14 TL");

        foreach (var line in safeLines)
        {
            contentBuilder.Append('(');
            contentBuilder.Append(line);
            contentBuilder.AppendLine(") Tj");
            contentBuilder.AppendLine("T*");
        }

        contentBuilder.AppendLine("ET");
        var content = contentBuilder.ToString();
        var contentBytes = Encoding.ASCII.GetBytes(content);

        var objects = new List<byte[]>
        {
            Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"),
            Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
            Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>"),
            Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"),
            BuildContentObject(contentBytes)
        };

        await using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: false);

        writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n"));

        var offsets = new List<int> { 0 };
        for (var i = 0; i < objects.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            offsets.Add((int)stream.Position);
            writer.Write(Encoding.ASCII.GetBytes($"{i + 1} 0 obj\n"));
            writer.Write(objects[i]);
            writer.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        var xrefPosition = (int)stream.Position;
        writer.Write(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n"));
        writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));

        for (var i = 1; i < offsets.Count; i++)
        {
            writer.Write(Encoding.ASCII.GetBytes($"{offsets[i]:D10} 00000 n \n"));
        }

        writer.Write(Encoding.ASCII.GetBytes($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF"));
        await stream.FlushAsync(cancellationToken);
    }

    private static byte[] BuildContentObject(byte[] contentBytes)
    {
        var header = Encoding.ASCII.GetBytes($"<< /Length {contentBytes.Length} >>\nstream\n");
        var footer = Encoding.ASCII.GetBytes("\nendstream");
        var result = new byte[header.Length + contentBytes.Length + footer.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(contentBytes, 0, result, header.Length, contentBytes.Length);
        Buffer.BlockCopy(footer, 0, result, header.Length + contentBytes.Length, footer.Length);
        return result;
    }

    private static string EscapePdfText(string input)
    {
        return input
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static string SanitizeToPdfText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Lưu ý: PDFWriter này chỉ là một công cụ demo cơ bản cho mục đích học tập. 
        // Trong môi trường WPF /.NET 8, việc vẽ Native PDF khá phức tạp (Font Unicode).
        // Cách này đang chuyển toàn bộ string có dấu / ký tự không xác định về dạng ASCII bằng cách loại bỏ dấu tiếng Việt
        // để đảm bảo không lỗi PDF header stream object (gây lỗi file).
        // Cách tối ưu nhất cho báo cáo đồ án: Hãy dùng iText7, PDFSharp hoặc ReportViewer.
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (c > 31 && c < 127)
            {
                sb.Append(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                sb.Append(' ');
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
