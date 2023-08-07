﻿using ClipConverter.Models;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Diagnostics;

namespace ClipConverter.Services;
public class ClipConverterService : IClipConverterService
{
    private IConfiguration _config;

    public ClipConverterService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> ConvertClipToGif(string clipName)
    {
        var nextExt = Path.ChangeExtension(clipName, ".gif");
        nextExt = Path.GetFileName(nextExt);
        var convertedClipFileName = Path.Combine(_config.GetValue<string>("ConvertedOutputDir"), nextExt);

        var conversionParams = "-vf \"fps=20,scale=600:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=64:reserve_transparent=0[p];[s1][p]paletteuse\"";


        using(Process comp = new Process())
        {
            comp.StartInfo.FileName = "ffmpeg";
            comp.StartInfo.Arguments = $"-i {clipName} {conversionParams} -y {convertedClipFileName}";
            comp.StartInfo.UseShellExecute = false;
            comp.Start();
            await comp.WaitForExitAsync();
        }
        return convertedClipFileName;
    }
}
