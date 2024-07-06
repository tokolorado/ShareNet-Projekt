// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using System.Runtime.Intrinsics.X86;

namespace ShareNet.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly IConfiguration Configuration;

        public DownloadPersonalDataModel(
            UserManager<IdentityUser> userManager,
            ILogger<DownloadPersonalDataModel> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult OnGet()
        {
            return NotFound();
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(IdentityUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

            string userFolderPath = Configuration["AppSettings:UsersFolderForFiles"] + "/" + user.Id;

            string jsonString = JsonSerializer.Serialize(personalData);
            SaveJsonFile(jsonString, userFolderPath + "/Dane.json");
            string zipFilePath = Configuration["AppSettings:UsersFolderForFiles"] + "/" + user.Id + ".zip";

            

            bool zipExists = System.IO.File.Exists(zipFilePath); if (zipExists) System.IO.File.Delete(zipFilePath);

            ZipFile.CreateFromDirectory(userFolderPath, zipFilePath);


            return File(System.IO.File.OpenRead(zipFilePath), "application/octet-stream", "TwojeDaneShareNet.zip");


        }

        public void SaveJsonFile(string json, string path)
        {
            System.IO.File.WriteAllText(path, json);
        }
    }
}
