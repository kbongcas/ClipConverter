using ClipConverter.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipConverter.Utils;
public static class ClipHtmlGenerator
{
    public static string GenerateHtml(GenerateHtmlRequestDto generateHtmlRequestDto)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<!DOCTYPE html>");
        sb.Append("<html>");
        sb.Append("<head>");
        sb.Append($"<meta property='og:site_name' content='Clipdat' />");
        sb.Append($"<meta property='og:title' content='{generateHtmlRequestDto.Name}' />");
        sb.Append($"<meta property='og:url' content='{generateHtmlRequestDto.ConvertedFile}' />");
        sb.Append($"<meta property='og:image' content='{generateHtmlRequestDto.ConvertedFile}' />");
        sb.Append($"<meta property='og:description' content='{generateHtmlRequestDto.Description}' />");
        sb.Append("<meta name='theme-color' content='#FF0000'>");
        sb.Append("<meta name='twitter:card' content='summary_large_image''>");
        sb.Append("</head>");
        sb.Append("</body>");
        sb.Append("</html>");
        return sb.ToString();
    }
}
