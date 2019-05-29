using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Infrastructure.Interfaces;
using Microsoft.Win32;
using StlReader;

namespace OpenTkViewer.Models
{
    public class FileManager
    {
        private readonly Dictionary<string, IFileReader> formatReaderPair;
        private readonly IEnumerable<IFileReader> fileReaders;
        private string ofdFilter;

        private readonly ViewerScene viewerScene;


        public FileManager(ViewerScene viewerScene)
        {
            this.viewerScene = viewerScene;
            formatReaderPair = new Dictionary<string, IFileReader>();
            fileReaders = new[] {new Reader() };
            Initialize();
        }

        private void Initialize()
        {
            var formats = new StringBuilder();
            foreach (var fileReader in fileReaders)
            {
                foreach (var supportedFormat in fileReader.SupportedFormats)
                {
                    formatReaderPair.Add(supportedFormat, fileReader);
                    formats.Append($"{fileReader.Name} (*{supportedFormat})|*{supportedFormat}|");
                }
            }

            ofdFilter = formats.ToString(0, formats.Length - 1);
        }

        public void OpenFile()
        {
            var openFileDialog = new OpenFileDialog { Filter = ofdFilter };
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    var fileFormat = Path.GetExtension(fileName);
                    if(string.IsNullOrWhiteSpace(fileFormat))
                        continue;

                    fileFormat = fileFormat.ToLower(CultureInfo.InvariantCulture);
                    if(!formatReaderPair.ContainsKey(fileFormat))
                        continue;

                    var model = formatReaderPair[fileFormat].Load(fileName);
                    if(model != null)
                        viewerScene.Add(model);
                }
            }
        }
    }
}
