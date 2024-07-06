using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ShareNet.Data;
using System.IO;
using Microsoft.AspNetCore.Http;
using NuGet.Packaging;

namespace ShareNet.Areas.Identity.Pages.Account
{
    [Authorize]
    [RequestSizeLimit(512 * 1024 * 1024)] // pliki max 512MB
    public class myFilesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        public IdentityUser? user1;

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration Configuration;
        public string _baseLink = "";

        public List<Models.File> Files { get; set; } = new List<Models.File>();

        public myFilesModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            this._userManager = userManager;
            Configuration = configuration;
            
        }

        public void AddtoLog(string text)
        {
            _context.Logi.Add(new Models.ShareNetLog() { UserId = user1.Id, LogTime = DateTime.Now, Logtext = text });
            _context.SaveChanges();
        }

        public void OnGet()
        {
            var task = _userManager.GetUserAsync(User);
            task.Wait();

            user1 = task.Result;
            Files = _context.Files.Where(x => x.UserId == user1.Id).ToList();
            _baseLink = Request.Scheme + "://" + Request.Host.Value + "/SharedFiles?id=";

        }

        public string GetSize(long size)
        {
            if (size < 1000000) return (size / 1000).ToString() + " KB";
            else return (size / 1000000).ToString() + " MB";


        }

        public IActionResult OnPostUploadFile(IFormFile formFile)
        {
            if (formFile == null) return RedirectToPage(); // pusty




            var task = _userManager.GetUserAsync(User);
            task.Wait();

            user1 = task.Result;

            Models.File newFile = new Models.File();

            newFile.FileName = formFile.FileName;
            newFile.FilePath = Configuration["AppSettings:UsersFolderForFiles"] + "/" + user1.Id + "/" + formFile.FileName;

            if (System.IO.File.Exists(newFile.FilePath)) return RedirectToPage(); //taki plik istnieje

            newFile.UserId = user1.Id;
            newFile.Size = formFile.Length;
            _context.Files.Add(newFile);
            SaveFileAsync(formFile, newFile.FilePath);
            _context.SaveChanges();

            AddtoLog("Dodano nowy plik: " + newFile.FileName + " (" + GetSize(newFile.Size) + ")");

            return RedirectToPage();
        }

        public void SaveFileAsync(IFormFile file, string path)
        {

                using var fileStream = new FileStream(path, FileMode.Create);
                file.CopyTo(fileStream);
            
        }
        
        public IActionResult OnPostDownloadFun(IFormCollection param)
        {
            //je¿eli w wartoœciach z post jest idpliku sprawdziæ czy plik nale¿y do w³aœciciela
            if (param.TryGetValue("FileId", out var stringid))
            {
                var filelist = _context.Files.Where(x => x.Id == Int32.Parse(stringid)).ToList();

                if (!filelist.Any()) return NotFound();

                Models.File file = filelist[0];

                //czy plik jest w bazie danych
                if (file != null) {
                    var task = _userManager.GetUserAsync(User);
                    task.Wait();

                    user1 = task.Result;
                    //czy plik nale¿y do osoby wywo³uj¹cej
                    if(file.UserId == user1.Id)
                    {
                        if (System.IO.File.Exists(file.FilePath))
                        {
                            AddtoLog("Pobrano przez u¿ytkownika plik: " + file.FileName);
                            return File(System.IO.File.OpenRead(file.FilePath), "application/octet-stream", file.FileName);
                        }
                        return NotFound();
                    }
                    else
                        throw new Exception("OnPostDownloadFun Chcesz pobraæ plik który nie jest twój");
                }
                else
                    throw new Exception("OnPostDownloadFun nie znaleziono takiego pliku");
            }
            else
                throw new Exception("OnPostDownloadFun error przy pobraniu brak fileid lub nie jest to twój plik");
        }

        public IActionResult OnPostShareFunStop(IFormCollection param)
        {

            //je¿eli w wartoœciach z post jest idpliku sprawdziæ czy plik nale¿y do w³aœciciela
            if (param.TryGetValue("FileId", out var stringid))
            {
                var filelist = _context.Files.Where(x => x.Id == Int32.Parse(stringid)).ToList();

                if (!filelist.Any()) return NotFound();

                Models.File file = filelist[0];

                if (file != null)
                {
                    var task = _userManager.GetUserAsync(User);
                    task.Wait();

                    user1 = task.Result;

                    if (file.UserId == user1.Id)
                    {
                        if (System.IO.File.Exists(file.FilePath))
                        {
                            file.isShared = false;
                            _context.Files.Update(file);
                            AddtoLog("Zatrzymano Udostêpnianie pliku: " + file.FileName);
                            _context.SaveChanges();

                            return RedirectToPage();
                        }
                        return NotFound();
                    }
                    else
                        throw new Exception("OnPostDownloadFun Chcesz pobraæ plik który nie jest twój");
                }
                else
                    throw new Exception("OnPostDownloadFun nie znaleziono takiego pliku");
            }
            else
                throw new Exception("OnPostDownloadFun error przy pobraniu brak fileid lub nie jest to twój plik");


        }

        public IActionResult OnPostShareFun(IFormCollection param)
        {

            //je¿eli w wartoœciach z post jest idpliku sprawdziæ czy plik nale¿y do w³aœciciela
            if (param.TryGetValue("FileId", out var stringid))
            {
                var filelist = _context.Files.Where(x => x.Id == Int32.Parse(stringid)).ToList();

                if (!filelist.Any()) return NotFound();

                Models.File file = filelist[0];

                if (file != null)
                {
                    var task = _userManager.GetUserAsync(User);
                    task.Wait();

                    user1 = task.Result;

                    if (file.UserId == user1.Id)
                    {
                        if (System.IO.File.Exists(file.FilePath))
                        {
                            file.isShared = true;
                            _context.Files.Update(file);
                            AddtoLog("Udostêpniono przez u¿ytkownika plik: " + file.FileName);
                            _context.SaveChanges();

                            return RedirectToPage();
                        }
                        return NotFound();
                    }
                    else
                        throw new Exception("OnPostDownloadFun Chcesz pobraæ plik który nie jest twój");
                }
                else
                    throw new Exception("OnPostDownloadFun nie znaleziono takiego pliku");
            }
            else
                throw new Exception("OnPostDownloadFun error przy pobraniu brak fileid lub nie jest to twój plik");


        }
        public IActionResult OnPostDeleteFun(IFormCollection param)
        {

            //je¿eli w wartoœciach z post jest idpliku sprawdziæ czy plik nale¿y do w³aœciciela
            if (param.TryGetValue("FileId", out var stringid))
            {
                var filelist = _context.Files.Where(x => x.Id == Int32.Parse(stringid)).ToList();

                if (!filelist.Any()) return NotFound();

                Models.File file = filelist[0];

                if (file != null)
                {
                    var task = _userManager.GetUserAsync(User);
                    task.Wait();

                    user1 = task.Result;

                    if (file.UserId == user1.Id)
                    {
                        if (System.IO.File.Exists(file.FilePath))
                        {
                            _context.Files.Remove(file);
                            System.IO.File.Delete(file.FilePath);
                            AddtoLog("Usuniêto przez u¿ytkownika plik: " + file.FileName);
                            _context.SaveChanges();

                            return RedirectToPage();
                        }
                        return NotFound();
                    }
                    else
                        throw new Exception("OnPostDownloadFun Chcesz pobraæ plik który nie jest twój");
                }
                else
                    throw new Exception("OnPostDownloadFun nie znaleziono takiego pliku");
            }
            else
                throw new Exception("OnPostDownloadFun error przy pobraniu brak fileid lub nie jest to twój plik");

        }
    }
}
