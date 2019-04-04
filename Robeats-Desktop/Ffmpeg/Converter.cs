using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.NET;

namespace Robeats_Desktop.Ffmpeg
{
    public class Converter
    {
        public Engine Engine { get; set; }

        public Converter(Engine engine)
        {
            Engine = engine;
        }

        /// <summary>
        /// Convert <see cref="MediaFile"/> to an MP3 file asynchronous
        /// </summary>
        /// <param name="inputFile">The input file of type <see cref="MediaFile"/></param>
        /// <param name="outputDir">The output directory where the MP3 will be stored</param>
        /// <returns><see cref="Task{TResult}"/></returns>
        public Task<MediaFile> ConvertAsync(MediaFile inputFile, string outputDir)
        {
            return ConvertAsync(inputFile, outputDir, inputFile.FileInfo.Name);
        }

        /// <summary>
        /// Convert <see cref="MediaFile"/> to an MP3 file asynchronous
        /// </summary>
        /// <param name="inputFile">The input file of type <see cref="MediaFile"/></param>
        /// <param name="outputDir">The output directory where the MP3 will be stored</param>
        /// <param name="name">The name of the file</param>
        /// <returns><see cref="Task{TResult}"/></returns>
        public Task<MediaFile> ConvertAsync(MediaFile inputFile, string outputDir, string name)
        {
            var outputFile = new MediaFile(Path.Combine(outputDir, $"{name}.mp3"));
            return Engine.ConvertAsync(inputFile, outputFile);
        }
    }
}
