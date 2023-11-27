using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController:ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        public FilesController(
            FileExtensionContentTypeProvider fileExtensionContentTypeProvider
            )
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new System.ArgumentNullException(
                    nameof(fileExtensionContentTypeProvider)
                    );
        }
        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId) 
        {
            var pathToFile = "copyimage.png";
            if(!System.IO.File.Exists(pathToFile)) 
            {
                return NotFound();
            }
            if(_fileExtensionContentTypeProvider.TryGetContentType(
                pathToFile,out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }

    }
}
