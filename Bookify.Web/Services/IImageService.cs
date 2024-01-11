using System.Net;

namespace Bookify.Web.Services
{
	public interface IImageService
	{
	   Task<(bool isUploaded,string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbNail);
	   void DeleteImage(string imagePath, string? imageThumbNailPath = null);
	}
}
