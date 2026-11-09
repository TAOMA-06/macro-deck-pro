using System.Drawing;
using System.IO;
using SuchByte.MacroDeck.Utils;

namespace SuchByte.MacroDeck.Icons;

public class Icon
{
    private string _filePath;

    private string _iconBase64 = "";

    public string FilePath
    {
        get => _filePath;
        set => _filePath = value;
    }

    public string IconId { get; set; } = Guid.NewGuid().ToString();

    public Image IconImage
    {
        get
        {
            if (IconCache.TryGet(_filePath, out var cachedImage) && cachedImage != null)
            {
                return cachedImage;
            }

            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;
            var image = Image.FromStream(ms);

            IconCache.Set(_filePath, image);
            return image;
        }
    }

    public string IconBase64
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_iconBase64))
            {
                _iconBase64 = Base64.GetBase64FromImage(IconImage);
            }
            return _iconBase64;
        }
    }

    public string IconHex128_64Base64 => Base64.GetBase64ByteArray((Bitmap)IconImage, new Size(128,64));
}