using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using SPaPS.Data;
using SPaPS.Models;

namespace SPaPS.Controllers
{
    public class RequestsController : Controller
    {
        private readonly SPaPSContext _context;
        private readonly IEmailSenderEnhance _emailService;
        private readonly UserManager<IdentityUser> _userManager;

        public RequestsController(SPaPSContext context, IEmailSenderEnhance emailService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            var sPaPSContext = _context.Requests.Include(r => r.Service);
            return View(await sPaPSContext.ToListAsync());
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.Service)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: Requests/Create
        [Authorize(Roles = "Корисник")]
        public IActionResult Create()
        {
            ViewData["Services"] = new SelectList(_context.Services.ToList(), "ServiceId", "Description");
            ViewData["BuildingTypes"] = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 6).ToList(), "ReferenceId", "Description");

            Request model = new Request()
            {
                RequestDate = DateTime.Now.Date
            };

            return View(model);
        }

        // POST: Requests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Корисник")]
        public async Task<IActionResult> Create(Request request)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Services"] = new SelectList(_context.Services.ToList(), "ServiceId", "Description");
                ViewData["BuildingTypes"] = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 6).ToList(), "ReferenceId", "Description");

                return View(request);
            }

            request.RequestDate = DateTime.Now;
            request.CreatedOn = DateTime.Now;
            request.CreatedBy = 1;
            request.IsActive = true;

            _context.Add(request);
            await _context.SaveChangesAsync();

            var serviceActivitiesIds = _context.ServiceActivities.Where(x => x.ServiceId == request.ServiceId).Select(x => x.ActivityId).ToList();

            var clientIds = _context.ClientActivities
                                    .Where(x => serviceActivitiesIds.Contains(x.ActivityId))
                                    .Select(x => x.ClientId)
                                    .Distinct()
                                    .ToList();


            foreach (var item in clientIds)
            {
                var client = _context.Clients.Find(item);
                var user = await _userManager.FindByIdAsync(client.UserId);

                /*EmailSetUp emailSetUp = new EmailSetUp()
                {
                    To = user.Email,
                    Template = "NewRequest",
                    RequestPath = _emailService.PostalRequest(Request),
                };

                var userRole = await _userManager.GetRolesAsync(user);
                if (userRole.FirstOrDefault() == "Изведувач")
                {
                    await _emailService.SendEmailAsync(emailSetUp);
                }*/

                /*--------------------*/

                var userRole = await _userManager.GetRolesAsync(user);
                if (userRole.FirstOrDefault() == "Изведувач")
                {
                    var callback = Url.Action(action: "Edit", controller: "Requests", values: new { id = request.RequestId, email = user.Email }, HttpContext.Request.Scheme);

                https://localhost:5001/Requests/Edit?token=123asdrew123&id=1

                    EmailSetUp emailSetUp = new EmailSetUp()
                    {
                        To = user.Email,
                        Template = "NewRequest",
                        Username = user.Email,
                        Callback = callback,
                        RequestPath = _emailService.PostalRequest(Request),
                    };

                    await _emailService.SendEmailAsync(emailSetUp);
                }

                /*var token = await _userManager.GeneratePasswordResetTokenAsync(user); ???

                var callback = Url.Action(action: "Edit", controller: "Requests", values: new { token, id = client.ClientId }, HttpContext.Request.Scheme);

                https://localhost:5001/Requests/Edit?token=123asdrew123&id=1 ???

                EmailSetUp emailSetUp = new EmailSetUp()
                {
                    To = user.Email,
                    Template = "NewRequest",
                    Username = user.Email,
                    Callback = callback,
                    Token = token,
                    RequestPath = _emailService.PostalRequest(Request),
                };

                await _emailService.SendEmailAsync(emailSetUp);*/
            }

            TempData["NotifySuccess"] = "Успешно известени изведувачите!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(long? id, string email)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            Request request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            ViewData["Services"] = new SelectList(_context.Services.ToList(), "ServiceId", "Description");
            ViewData["BuildingTypes"] = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 6).ToList(), "ReferenceId", "Description");

            var user = await _userManager.FindByEmailAsync(email);
            Client? client = await _context.Clients.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
            request.ContractorId = client.ClientId; /* ??? */
            return View(request);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Request request) /* ??? */
        {
            request.UpdatedOn = DateTime.Now;
            request.UpdatedBy = 1;

            _context.Update(request);
            await _context.SaveChangesAsync();

            ViewData["Services"] = new SelectList(_context.Services.ToList(), "ServiceId", "Description");
            ViewData["BuildingTypes"] = new SelectList(_context.References.Where(x => x.ReferenceTypeId == 6).ToList(), "ReferenceId", "Description");

            ModelState.AddModelError("Success", "Успешно прифатено барање!");

            return View();
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Requests == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.Service)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Requests == null)
            {
                return Problem("Entity set 'SPaPSContext.Requests'  is null.");
            }
            var request = await _context.Requests.FindAsync(id);
            if (request != null)
            {
                _context.Requests.Remove(request);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequestExists(long id)
        {
            return _context.Requests.Any(e => e.RequestId == id);
        }
    }
}
