using System.Collections.Generic;
using Infrastructure.VisualObjects;

namespace Infrastructure.Interfaces
{
    public interface IFileReader
    {
        string Name { get; }
        IEnumerable<string> SupportedFormats { get; }

        VisualModel Load(string fileName);
    }
}
