// UploadModel.cs
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AspNet8Mvc_Tallybook.Models
{
    public class UploadModel
    {
        public List<IFormFile> Files { get; set; }
    }
}
