using Microsoft.AspNetCore.Http;
using System.Linq;

namespace EcommerceAPI.Validators;

public static class ImageValidator
{
    public static async Task<bool> IsValidImageFileAsync( IFormFile file,  string extension, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        // JPG / JPEG
        if (extension is ".jpg" or ".jpeg")
        {
            var buffer = new byte[3];

            var bytesRead = await stream.ReadAsync( buffer, cancellationToken);

            return bytesRead == 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 &&  buffer[2] == 0xFF;
        }

        // PNG
        if (extension == ".png")
        {
            var buffer = new byte[8];

            var bytesRead = await stream.ReadAsync( buffer, cancellationToken);

            byte[] pngSignature =
            {
                0x89, 0x50, 0x4E, 0x47,
                0x0D, 0x0A, 0x1A, 0x0A
            };

            return bytesRead == 8 && buffer.SequenceEqual(pngSignature);
        }

        // WEBP
        if (extension == ".webp")
        {
            var buffer = new byte[12];

            var bytesRead = await stream.ReadAsync(  buffer, cancellationToken);

            if (bytesRead != 12) return false;

            return
                buffer[0] == (byte)'R' &&
                buffer[1] == (byte)'I' &&
                buffer[2] == (byte)'F' &&
                buffer[3] == (byte)'F' &&
                buffer[8] == (byte)'W' &&
                buffer[9] == (byte)'E' &&
                buffer[10] == (byte)'B' &&
                buffer[11] == (byte)'P';
        }

        return false;
    }
}
