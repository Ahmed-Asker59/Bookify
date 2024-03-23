using Bookify.Web.Core.Models;

namespace Bookify.Web.Services
{
	public class ImageService : IImageService
	{
		private IWebHostEnvironment _webHostEnvironment;
		private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
		private int _maxedAllowedSize = 2097152;

		public ImageService(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbNai)
		{
			var extension = Path.GetExtension(image.FileName);
			if (!_allowedExtensions.Contains(extension))
				return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);

			if (image.Length > _maxedAllowedSize)
				return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);
			//generate the path of the image
			var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);
			

			//create a stream in the image path to save the image in it
			using var stream = File.Create(path);
			await image.CopyToAsync(stream);
			//dispose the stream to start another
			stream.Dispose();


			if (hasThumbNai)
			{
				var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);
				//open the image sent by end user
				using var loadedImage = Image.Load(image.OpenReadStream());
				var ratio = (float)loadedImage.Width / 200;
				var height = loadedImage.Height / ratio;
				//reduce the size to create a thumbnail
				loadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
				//save the thumbnail
				loadedImage.Save(thumbPath);
			}

			return (isUploaded: true, errorMessage:null);
		}

		public void DeleteImage(string imagePath, string? imageThumbNailPath = null)
		{
			//get the old image path in the application to delete it
			var oldImagePath = $"{_webHostEnvironment.WebRootPath}{imagePath}";
			var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{imageThumbNailPath}";
			//delete the old image
			if (File.Exists(oldImagePath))
				File.Delete(oldImagePath);
			//delete the thumbnail
			if (File.Exists(oldThumbPath))
				File.Delete(oldThumbPath);

		}

	}
}
