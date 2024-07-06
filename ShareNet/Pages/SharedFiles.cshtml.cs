using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShareNet.Data;
using System.Runtime.Intrinsics.X86;

namespace ShareNet.Pages
{
    public class SharedFilesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Models.File> Files { get; set; } = new List<Models.File>();

        [BindProperty(Name = "id", SupportsGet = true)]
        public string? id { get; set; }
        public Models.File SharedFile { get; set; }

        public SharedFilesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            if(id is null) return NotFound();

            var filelist = _context.Files.Where(x => x.Id == Int32.Parse(id)).ToList();

            if (!filelist.Any()) return NotFound();

            Models.File file = filelist[0];

            if (file is null ) return NotFound();

            //je¿eli nie jest udostêpniony to czego tu szukasz
            if (!file.isShared) return NotFound();

            SharedFile = file;
            return null;
        }
        public string GetSize(long size)
        {
            if (size < 1000000) return (size / 1000).ToString() + " KB";
            else return (size / 1000000).ToString() + " MB";


        }

        public void AddtoLog(string text, string fileOwnerId)
        {
            _context.Logi.Add(new Models.ShareNetLog() { UserId = fileOwnerId, LogTime = DateTime.Now, Logtext = text });
            _context.SaveChanges();
        }

        public IActionResult OnPostDownloadsharedFile(IFormCollection param)
        {
            //je¿eli w wartoœciach z post jest idpliku sprawdziæ czy plik nale¿y do w³aœciciela
            if (param.TryGetValue("FileId", out var stringid))
            {
                var filelist = _context.Files.Where(x => x.Id == Int32.Parse(stringid)).ToList();

                if (!filelist.Any()) return NotFound();

                Models.File file = filelist[0];

                //je¿eli nie jest udostêpniony to czego tu szukasz
                if (!file.isShared)
                    return NotFound();

                //czy plik jest w bazie danych
                if (file != null)
                {
                    //plik istnieje
                    if (System.IO.File.Exists(file.FilePath))
                    {
                        AddtoLog("Pobrano przez udostêpniony link plik: " + file.FileName, file.UserId);
                        return File(System.IO.File.OpenRead(file.FilePath), "application/octet-stream", file.FileName);
                    }
                    return NotFound();

                }
                else
                    throw new Exception("OnPostDownloadsharedFile nie znaleziono takiego pliku");
            }
            else
                throw new Exception("OnPostDownloadsharedFile error przy pobraniu brak fileid");
        }
    }
}
