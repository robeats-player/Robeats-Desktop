using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.NET;

namespace Robeats_Desktop.Ffmpeg
{
    class Converter
    {
        public Engine Engine { get; set; }

        public Converter(Engine engine)
        {
            Engine = engine;
        }

        public Task<MediaFile> Convert(MediaFile inputFile, string outputDir)
        {
            return Convert(inputFile, outputDir, inputFile.FileInfo.Name);
        }
        public Task<MediaFile> Convert(MediaFile inputFile, string outputDir, string name)
        {
            var outputFile = new MediaFile(Path.Combine(outputDir, $"{name}.mp3"));
            return Engine.ConvertAsync(inputFile, outputFile);
        }
    }
}
