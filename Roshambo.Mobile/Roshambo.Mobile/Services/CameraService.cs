using System.IO;
using System.Threading.Tasks;

using Xamarin.Essentials;

using Roshambo.Mobile.Models;

namespace Roshambo.Mobile.Services
{
    public class CameraService
    {
        public static async Task<LocalPhoto> TakePhoto(bool capture)
        {
            var photo = capture
                ? await MediaPicker.CapturePhotoAsync()
                : await MediaPicker.PickPhotoAsync();

            var file = await SavePhoto(photo);

            return new LocalPhoto()
            {
                NewFile = file,
                PhotoFile = photo
            };
        }

        private static async Task<string> SavePhoto(FileResult file)
        {
            if (file == null)
                return string.Empty;

            var fileName = file.FileName;
            var folder = FileSystem.AppDataDirectory;
            var newFile = Path.Combine(folder, fileName);

            using (var stream = await file.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);

            return newFile;
        }
    }
}