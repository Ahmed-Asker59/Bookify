﻿using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace Bookify.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public UsersController(UserManager<ApplicationUser> userManager,
            IMapper mapper,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _mapper = mapper;
            _WebHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
        }
        public async Task<IActionResult> Index()
        {
            
           // await _emailSender.SendEmailAsync("askarahmad189@gmail.com", "Test",body);
            var user = User;
            var users = await _userManager.Users.ToListAsync();
            var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var viewModel = new UserFormViewModel()
            {
                Roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToListAsync(),
            };
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
         {
           if(!ModelState.IsValid)
                return BadRequest();

            ApplicationUser user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
               
                //handle confirmation mail
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id,  code },
                    protocol: Request.Scheme);

                var placeholders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/askerhub/image/upload/v1704818810/icon-positive-vote-1_jwmgvw.png" },
                    {"header", $"hey {user.FullName}," + $" thanks for joining us!" },
                    {"body", "Please confirm your email" },
                    {"url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}" } ,
                    {"linkTitle", "Activate Account" }

                };
                var body = _emailBodyBuilder.GenerateEmailBody(EmailTemplates.Email, placeholders);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);


                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var viewModel = _mapper.Map<UserFormViewModel>(user);
            viewModel.SelectedRoles = await _userManager.GetRolesAsync(user);
            viewModel.Roles = await _roleManager.Roles.Select(
                r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }
                ).ToListAsync();
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
                return NotFound();

           user = _mapper.Map(model,user);
           user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
           user.LastUpdatedOn = DateTime.Now;
          var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesUpated = !currentRoles.SequenceEqual(model.SelectedRoles);
                if (rolesUpated)
                {
                    await _userManager.RemoveFromRolesAsync(user,currentRoles);
                    await _userManager.AddToRolesAsync(user,model.SelectedRoles);
                   
                }
				await _userManager.UpdateSecurityStampAsync(user);
				var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(",",result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var viewModel = new ResetPasswordFormViewModel{ Id = user.Id};
            return PartialView("_ResetPasswordForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid) 
                return BadRequest();
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
                return NotFound();
            //kep the old password hash, as deletion has not succedded
            //we want to save the old password
            var oldPasswordHash = user.PasswordHash;
            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.Password);
            if (result.Succeeded)
            {
                user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOn = DateTime.Now;
                await _userManager.UpdateAsync(user);
                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            //if operation did not succecd
            user.PasswordHash = oldPasswordHash;
            await _userManager.UpdateAsync(user);
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            user.IsDeleted=!user.IsDeleted;
            user.LastUpdatedById= User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;
            await _userManager.UpdateAsync(user);

            if (user.IsDeleted)
            {
                await _userManager.UpdateSecurityStampAsync(user); ;
            }
            return Ok(user.LastUpdatedOn.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
                await _userManager.SetLockoutEndDateAsync(user,null);

            return Ok();
        }

        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            //Username is alwoed if does not exist or we are editing
            var isAllowed = user is null || user.Id.Equals(model.Id);
            return Json(isAllowed);

        }

        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            //Username is alwoed if does not exist or we are editing
            var isAllowed = user is null || user.Id.Equals(model.Id);
            return Json(isAllowed);

        }
    }
}
