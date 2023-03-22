using Blogifier.Core.Providers;
using Blogifier.Shared;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;


namespace Blogifier.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StorageController : ControllerBase
	{
		private readonly IStorageProvider _storageProvider;
		private readonly IAuthorProvider _authorProvider;
		private readonly IBlogProvider _blogProvider;
		private readonly IPostProvider _postProvider;
		private static readonly FormOptions _defaultFormOptions = new FormOptions(){
			
		};
		private readonly string[] _permittedExtensions = { ".svg", ".png", ".gif", ".jpeg", ".jpg", ".zip", ".7z", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".mp3", ".mp4", ".avi" };
		private readonly long _fileSizeLimit = 1024L * 1024L * 1024L * 2L;
		private readonly string _targetFilePath;
		private readonly string _slash = Path.DirectorySeparatorChar.ToString();
        public StorageController(IStorageProvider storageProvider, IAuthorProvider authorProvider, IBlogProvider blogProvider, IPostProvider postProvider)
		{
			_storageProvider = storageProvider;
			_authorProvider = authorProvider;
			_blogProvider = blogProvider;
			_postProvider = postProvider;
			_targetFilePath = $"{ContentRoot}{_slash}wwwroot{_slash}data{_slash}";
		}

		private string ContentRoot
		{
			get
			{
				string path = Directory.GetCurrentDirectory();
				string testsDirectory = $"tests{_slash}Blogifier.Tests";
				string appDirectory = $"src{_slash}Blogifier";

				Serilog.Log.Information($"Current directory path: {path}");

				// development unit test run
				if (path.LastIndexOf(testsDirectory) > 0)
				{
					path = path.Substring(0, path.LastIndexOf(testsDirectory));
					Serilog.Log.Information($"Unit test path: {path}src{_slash}Blogifier");
					return $"{path}src{_slash}Blogifier";
				}

				// this needed to make sure we have correct data directory
				// when running in debug mode in Visual Studio
				// so instead of debug (src/Blogifier/bin/Debug..)
				// will be used src/Blogifier/wwwroot/data
				// !! this can mess up installs that have "src/Blogifier" in the path !!
				if (path.LastIndexOf(appDirectory) > 0)
				{
					path = path.Substring(0, path.LastIndexOf(appDirectory));
					Serilog.Log.Information($"Development debug path: {path}src{_slash}Blogifier");
					return $"{path}src{_slash}Blogifier";
				}
				Serilog.Log.Information($"Final path: {path}");
				return path;
			}
		}


		[Authorize]
		[HttpGet("themes")]
		public async Task<IList<string>> GetThemes()
		{
			return await _storageProvider.GetThemes();
		}

		[Authorize]
		[HttpPut("exists")]
		public async Task<IActionResult> FileExists([FromBody] string path)
		{
			return (await Task.FromResult(_storageProvider.FileExists(path))) ? Ok() : BadRequest();
		}

		[Authorize]
		[HttpPost("upload/{uploadType}")]
		public async Task<ActionResult> Upload(IFormFile file, UploadType uploadType, int postId = 0)
		{
			var author = await _authorProvider.FindByEmail(User.Identity.Name);
			var post = postId == 0 ? new Post() : await _postProvider.GetPostById(postId);

            var path = $"{author.Id}/{DateTime.Now.Year}/{DateTime.Now.Month}";
			var fileName = $"data/{path}/{file.FileName}";
			string webRoot = Url.Content("~/");
            var origin = $"{Request.Scheme}s://{Request.Host}{webRoot}";

            if (uploadType == UploadType.PostImage)
                fileName = origin + fileName;

			if (await _storageProvider.UploadFormFile(file, path))
			{
				var blog = await _blogProvider.GetBlog();

				switch (uploadType)
				{
					case UploadType.Avatar:
						author.Avatar = fileName;
						return (await _authorProvider.Update(author)) ? new JsonResult(fileName) : BadRequest();
					case UploadType.AppLogo:
						blog.Logo = fileName;
						return (await _blogProvider.Update(blog)) ? new JsonResult(fileName) : BadRequest();
					case UploadType.AppCover:
						blog.Cover = fileName;
						return (await _blogProvider.Update(blog)) ? new JsonResult(fileName) : BadRequest();
					case UploadType.PostCover:
						post.Cover = fileName;
                        return new JsonResult(fileName);
                    case UploadType.PostImage:
						return new JsonResult(fileName);
				}
				return Ok();
			}
			else
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("uploadmultipart")]
		[DisableFormValueModelBinding]
		[GenerateAntiforgeryTokenCookie]
		[RequestSizeLimit(1024L * 1024L * 1024L * 2L)]
		//[RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 1024L)]
		public async Task<IActionResult> UploadPhysical()
		{
			Serilog.Log.Information($"UploadPhysical");
			var author = await _authorProvider.FindByEmail(User.Identity.Name);

            var path = $"{author.Id}/{DateTime.Now.Year}/{DateTime.Now.Month}";

			if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
			{
				Serilog.Log.Information($"The request couldn't be processed (Error 1)");
				ModelState.AddModelError("File", 
					$"The request couldn't be processed (Error 1).");
				// Log error

				return BadRequest(ModelState);
			}

			var boundary = MultipartRequestHelper.GetBoundary(
				MediaTypeHeaderValue.Parse(Request.ContentType),
				_defaultFormOptions.MultipartBoundaryLengthLimit);
			var reader = new MultipartReader(boundary, HttpContext.Request.Body);
			var section = await reader.ReadNextSectionAsync();
			var trustedFileNameForFileStorage = Path.GetRandomFileName();
			var fullPath = Path.Combine(_targetFilePath, Path.Combine(path, trustedFileNameForFileStorage));
			var trustedFileNameForDisplay = string.Empty;
			Serilog.Log.Information($"Starting While");
			while (section != null)
			{
				Serilog.Log.Information($"Inside While");
				var hasContentDispositionHeader = 
					ContentDispositionHeaderValue.TryParse(
						section.ContentDisposition, out var contentDisposition);

				if (hasContentDispositionHeader)
				{
					// This check assumes that there's a file
					// present without form data. If form data
					// is present, this method immediately fails
					// and returns the model error.
					if (!MultipartRequestHelper
						.HasFileContentDisposition(contentDisposition))
					{
						using (var targetStream = System.IO.File.Create(fullPath))
						{
							await section.Body.CopyToAsync(targetStream);
						}
					}
					else
					{
						// Don't trust the file name sent by the client. To display
						// the file name, HTML-encode the value.
						trustedFileNameForDisplay = WebUtility.HtmlEncode(
								contentDisposition.FileName.Value);

						fullPath = Path.Combine(_targetFilePath, Path.Combine(path, trustedFileNameForDisplay));

						// **WARNING!**
						// In the following example, the file is saved without
						// scanning the file's contents. In most production
						// scenarios, an anti-virus/anti-malware scanner API
						// is used on the file before making the file available
						// for download or for use by other systems. 
						// For more information, see the topic that accompanies 
						// this sample.
						
						// Also, I commented out this code because it causes a OOM Error
						/*var streamedFileContent = await FileHelpers.ProcessStreamedFile(
							section, contentDisposition, ModelState,
							_permittedExtensions, _fileSizeLimit);
						*/
						if (!ModelState.IsValid)
						{
							Serilog.Log.Information($"!ModelState.IsValid");
							
							return BadRequest(ModelState);
						}
						Directory.CreateDirectory(Path.Combine(_targetFilePath, path));
						using (var targetStream = System.IO.File.Create(fullPath))
						{
							await section.Body.CopyToAsync(targetStream);
							//await targetStream.WriteAsync(streamedFileContent);

							Serilog.Log.Information($"Uploaded file '{trustedFileNameForDisplay}' saved to '{fullPath}'");
						}
					}
				}

				// Drain any remaining section body that hasn't been consumed and
				// read the headers for the next section.
				Serilog.Log.Information($"ReadNextSectionAsync");
				section = await reader.ReadNextSectionAsync();
			}
			Serilog.Log.Information($"Returning");
			var fileName = $"data/{path}/{trustedFileNameForDisplay}";
			string webRoot = Url.Content("~/");
            var origin = $"{Request.Scheme}s://{Request.Host}{webRoot}";
            fileName = origin + fileName;
			return new JsonResult(fileName);
		}

		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
		public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
		{
			public void OnResourceExecuting(ResourceExecutingContext context)
			{
				var factories = context.ValueProviderFactories;
				factories.RemoveType<FormValueProviderFactory>();
				factories.RemoveType<FormFileValueProviderFactory>();
				factories.RemoveType<JQueryFormValueProviderFactory>();
			}

			public void OnResourceExecuted(ResourceExecutedContext context)
			{
			}
		}
		public class GenerateAntiforgeryTokenCookieAttribute : ResultFilterAttribute
		{
			public override void OnResultExecuting(ResultExecutingContext context)
			{
				var antiforgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();

				// Send the request token as a JavaScript-readable cookie
				var tokens = antiforgery.GetAndStoreTokens(context.HttpContext);

				context.HttpContext.Response.Cookies.Append(
					"RequestVerificationToken",
					tokens.RequestToken,
					new CookieOptions() { HttpOnly = false });
			}

			public override void OnResultExecuted(ResultExecutedContext context)
			{
			}
		}
		public static class MultipartRequestHelper
		{
			// Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
			// The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
			public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
			{
				var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

				if (string.IsNullOrWhiteSpace(boundary))
				{
					throw new InvalidDataException("Missing content-type boundary.");
				}

				if (boundary.Length > lengthLimit)
				{
					throw new InvalidDataException(
						$"Multipart boundary length limit {lengthLimit} exceeded.");
				}

				return boundary;
			}

			public static bool IsMultipartContentType(string contentType)
			{
				return !string.IsNullOrEmpty(contentType)
					&& contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
			}

			public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
			{
				// Content-Disposition: form-data; name="key";
				return contentDisposition != null
					&& contentDisposition.DispositionType.Equals("form-data")
					&& string.IsNullOrEmpty(contentDisposition.FileName.Value)
					&& string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
			}

			public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
			{
				// Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
				return contentDisposition != null
					&& contentDisposition.DispositionType.Equals("form-data")
					&& (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
						|| !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
			}
		}
	}
	public static class FileHelpers
    {
        // If you require a check on specific characters in the IsValidFileExtensionAndSignature
        // method, supply the characters in the _allowedChars field.
        private static readonly byte[] _allowedChars = { };
        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]> 
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };

        // **WARNING!**
        // In the following file processing methods, the file's content isn't scanned.
        // In most production scenarios, an anti-virus/anti-malware scanner API is
        // used on the file before making the file available to users or other
        // systems. For more information, see the topic that accompanies this sample
        // app.

        public static async Task<byte[]> ProcessFormFile<T>(IFormFile formFile, 
            ModelStateDictionary modelState, string[] permittedExtensions, 
            long sizeLimit)
        {
            var fieldDisplayName = string.Empty;

            // Use reflection to obtain the display name for the model
            // property associated with this IFormFile. If a display
            // name isn't found, error messages simply won't show
            // a display name.
            MemberInfo property =
                typeof(T).GetProperty(
                    formFile.Name.Substring(formFile.Name.IndexOf(".",
                    StringComparison.Ordinal) + 1));

            if (property != null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute)) is
                    DisplayAttribute displayAttribute)
                {
                    fieldDisplayName = $"{displayAttribute.Name} ";
                }
            }

            // Don't trust the file name sent by the client. To display
            // the file name, HTML-encode the value.
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                formFile.FileName);

            // Check the file length. This check doesn't catch files that only have 
            // a BOM as their content.
            if (formFile.Length == 0)
            {
                modelState.AddModelError(formFile.Name, 
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");

                return Array.Empty<byte>();
            }
            
            if (formFile.Length > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / 1048576;
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) exceeds {megabyteSizeLimit} MB.");

                return Array.Empty<byte>();
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);

                    // Check the content length in case the file's only
                    // content was a BOM and the content is actually
                    // empty after removing the BOM.
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");
                    }

                    if (!IsValidFileExtensionAndSignature(
                        formFile.FileName, memoryStream, permittedExtensions))
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldDisplayName}({trustedFileNameForDisplay}) file type isn't permitted or the file's signature doesn't match the file's extension.");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError(formFile.Name,
                    $"Error Uploading {ex.ToString()}");
				Serilog.Log.Information($"Error Uploading {ex.ToString()}");
                // Log the exception
            }

            return Array.Empty<byte>();
        }

		// This causes OOM Errors. Do not use
        public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition, 
            ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

					Serilog.Log.Information($"memoryStream array size: {memoryStream.Length}");

                    // Check if the file is empty or exceeds the size limit.
                    if (memoryStream.Length == 0)
                    {
						Serilog.Log.Information($"The file is empty.");
                        modelState.AddModelError("File", "The file is empty.");
                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabyteSizeLimit = sizeLimit / 1048576;
                        modelState.AddModelError("File",
                        $"The file exceeds {megabyteSizeLimit} MB.");
						Serilog.Log.Information($"The file exceeds {megabyteSizeLimit} MB.");
                    }
                    else if (!IsValidFileExtensionAndSignature(
                        contentDisposition.FileName.Value, memoryStream, 
                        permittedExtensions))
                    {
                        modelState.AddModelError("File",
                            "The file type isn't permitted or the file's " +
                            "signature doesn't match the file's extension.");
						Serilog.Log.Information($"The file type isn't permitted {contentDisposition.FileName.Value}");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("File",
                    "The upload failed. Please contact the Help Desk " +
                    $" for support. Error: {ex.ToString()}");
				Serilog.Log.Information($"The upload failed {ex.ToString()}");
                // Log the exception
            }

            return Array.Empty<byte>();
        }
		

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
				Serilog.Log.Information($"The fileName or data is null");
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
            {
				Serilog.Log.Information($"The file extension is null");
                return false;
            }
			if(!permittedExtensions.Contains(ext))
			{
				Serilog.Log.Information($"The file type isn't permitted {ext}");
				return false;
			}

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                if (ext.Equals(".txt") || ext.Equals(".csv") || ext.Equals(".prn"))
                {
                    if (_allowedChars.Length == 0)
                    {
                        // Limits characters to ASCII encoding.
                        for (var i = 0; i < data.Length; i++)
                        {
                            if (reader.ReadByte() > sbyte.MaxValue)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // Limits characters to ASCII encoding and
                        // values of the _allowedChars array.
                        for (var i = 0; i < data.Length; i++)
                        {
                            var b = reader.ReadByte();
                            if (b > sbyte.MaxValue ||
                                !_allowedChars.Contains(b))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                // Uncomment the following code block if you must permit
                // files whose signature isn't provided in the _fileSignature
                // dictionary. We recommend that you add file signatures
                // for files (when possible) for all file types you intend
                // to allow on the system and perform the file signature
                // check.
                
                if (!_fileSignature.ContainsKey(ext))
                {
                    return true;
                }
                

                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature
                // dictionary, the following code tests the input content's
                // file signature.
                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature => 
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
