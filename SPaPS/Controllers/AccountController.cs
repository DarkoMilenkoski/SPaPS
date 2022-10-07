using SPaPS.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using SPaPS.Models;
using Microsoft.AspNetCore.Identity;
using SPaPS.Data;
using DataAccess.Services;
using SPaPS.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SPaPS.Models.CustomModels;

namespace SPaPS.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SPaPSContext _context;
        private readonly IEmailSenderEnhance _emailService;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, SPaPSContext context, IEmailSenderEnhance emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {

            if (TempData["Success"] != null)
                ModelState.AddModelError("Success", Convert.ToString(TempData["Success"]));


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Сите полиња треба да се пополнети!");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(userName: model.Email, password: model.Password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded || result.IsLockedOut || result.IsNotAllowed)
            {
                ModelState.AddModelError("Error", "Погрешно корисничко име или лозинка!");
                return View(model);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.ReferenceTypesClient = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 2).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCity = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 3).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCountry = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 4).ToList(), "ReferenceId", "Description");

            ViewBag.Roles = new SelectList(_context.AspNetRoles.ToList(), "Name", "Name");
            ViewBag.Activities = new SelectList(_context.Activities.ToList(), "ActivityId", "Name");

            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            ViewBag.ReferenceTypesClient = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 2).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCity = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 3).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCountry = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 4).ToList(), "ReferenceId", "Description");

            ViewBag.Roles = new SelectList(_context.AspNetRoles.ToList(), "Name", "Name");
            ViewBag.Activities = new SelectList(_context.Activities.ToList(), "ActivityId", "Name");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Сите полиња треба да се пополнети!");
                return View(model);
            }
            var userExists = await _userManager.FindByEmailAsync(model.Email);

            if (userExists != null)
            {
                ModelState.AddModelError("Error", "Корисникот веќе постои!");
                return View(model);
            }

            IdentityUser user = new IdentityUser()
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var newPassword = Shared.GeneratePassword(8);

            var createUser = await _userManager.CreateAsync(user, newPassword);

            if (!createUser.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            Client client = new Client()
            {
                UserId = user.Id,
                Name = model.Name,
                Address = model.Address,
                IdNo = model.IdNo,
                ClientTypeId = model.ClientTypeId,
                CityId = model.CityId,
                CountryId = model.CountryId
            };

            if (model.Role == "Изведувач")
            {
                client.DateEstablished = model.DateOfEstablishment;
                client.NoOfEmployees = model.NoOfEmployees;
            }

            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            if (model.Role == "Изведувач")
            {
                List<ClientActivity> clientActivities = model.Activities.Select(x => new ClientActivity()
                {
                    ClientId = client.ClientId,
                    ActivityId = x
                }).ToList();

                await _context.ClientActivities.AddRangeAsync(clientActivities);
                await _context.SaveChangesAsync();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callback = Url.Action(action: "ResetPassword", controller: "Account", values: new { token, email = user.Email }, HttpContext.Request.Scheme);
            

            /* https://localhost:5001/Account/ResetPassword?token=123asdrew123&email=nikola.stankovski@foxit.mk */

            EmailSetUp emailSetUp = new EmailSetUp()
            {
                To = user.Email,
                Template = "Register",
                Username = user.Email,
                Callback = callback,
                Token = token,
                RequestPath = _emailService.PostalRequest(Request),
            };

            await _emailService.SendEmailAsync(emailSetUp);

            TempData["Success"] = "Успешно креиран корисник!";

            return RedirectToAction(nameof(Login));
        }



        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]

        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Внесете мејл адреса!");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callback = Url.Action(action: "ResetPassword", controller: "Account", values: new { token, email = user.Email }, HttpContext.Request.Scheme);

            EmailSetUp emailSetUp = new EmailSetUp()
            {
                To = user.Email,
                Template = "ResetPassword",
                Username = user.Email,
                Callback = callback,
                Token = token,
                RequestPath = _emailService.PostalRequest(Request),
            };

            await _emailService.SendEmailAsync(emailSetUp);

            TempData["Success"] = "Ве молиме проверете го вашето влезно сандаче!";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {

            ResetPasswordModel model = new ResetPasswordModel()
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Сите полиња треба да се пополнети!");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            var resetPassword = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!resetPassword.Succeeded)
            {
                ModelState.AddModelError("Error", "Се случи грешка. Обидете се повторно!");
                return View();
            }

            TempData["Success"] = "Успешно промената лозинка!";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Лозинките не се совпаѓаат!");
                return View();
            }

            var loggedInUserEmail = User.Identity.Name;

            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);

            var changePassword = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePassword.Succeeded)
            {
                ModelState.AddModelError("Error", "Се случи грешка. Обидете се повторно!");
                return View();
            }

            ModelState.AddModelError("Success", "Успешно променета лозинка!");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeInfo()
        {
            ViewBag.ReferenceTypesClient = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 2).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCity = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 3).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCountry = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 4).ToList(), "ReferenceId", "Description");
            ViewBag.Roles = new SelectList(_context.AspNetRoles.ToList(), "Name", "Name");
            var loggedInUserEmail = User.Identity.Name;

            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);

            var userRole = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = new SelectList(_context.AspNetRoles.ToList(), "Name", "Name", userRole.FirstOrDefault());

            Client? client = await _context.Clients.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
            ViewBag.Activities = new SelectList(_context.Activities.ToList(), "ActivityId", "Name", _context.ClientActivities.Where(x => x.ClientId == client.ClientId).Select(x => x.ActivityId).ToList());
            ChangeInfoModel model = new ChangeInfoModel()
            {
                PhoneNumber = user.PhoneNumber,
                ClientTypeId = client.ClientTypeId,
                Name = client.Name,
                Address = client.Address,
                IdNo = client.IdNo,
                CityId = client.CityId,
                CountryId = client.CountryId,
                NoOfEmployees = client.NoOfEmployees,
                DateOfEstablishment = client.DateEstablished,
                Activities = _context.ClientActivities.Where(x => x.ClientId == client.ClientId).Select(x => x.ActivityId).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeInfo(ChangeInfoModel model)
        {
            ViewBag.ReferenceTypesClient = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 2).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCity = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 3).ToList(), "ReferenceId", "Description");
            ViewBag.ReferenceTypesCountry = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 4).ToList(), "ReferenceId", "Description");

            ViewBag.Roles = new SelectList(_context.AspNetRoles.ToList(), "Name", "Name");
            ViewBag.Activities = new SelectList(_context.Activities.ToList(), "ActivityId", "Name");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Сите полиња треба да се пополнети!");
                return View(model);
            }

            var loggedInUserEmail = User.Identity.Name;

            var user = await _userManager.FindByEmailAsync(loggedInUserEmail);

            Client? client = await _context.Clients.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();

            user.PhoneNumber = model.PhoneNumber;
            client.ClientTypeId = model.ClientTypeId;
            client.Name = model.Name;
            client.Address = model.Address;
            client.IdNo = model.IdNo;
            client.CityId = model.CityId;
            client.CountryId = model.CountryId;
            client.NoOfEmployees = model.NoOfEmployees;
            client.DateEstablished = model.DateOfEstablishment;

            client.UpdatedOn = DateTime.Now;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            List<ClientActivity> clientActivities = model.Activities.Select(x => new ClientActivity()
            {
                ClientId = client.ClientId,
                ActivityId = x
            }).ToList();

            var oldClientActivities = _context.ClientActivities.Where(x => x.ClientId == client.ClientId).ToList();
            _context.ClientActivities.RemoveRange(oldClientActivities);
            _context.ClientActivities.UpdateRange(clientActivities);
            await _context.SaveChangesAsync();

            ModelState.AddModelError("Success", "Успешно променети информации!");

            return View();
        }
    }
}
