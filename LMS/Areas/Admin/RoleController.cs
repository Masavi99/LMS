using LMS.Data;
using LMS.Models.DBModels;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        UserManager<ApplicationUser> _userManager;
        public RoleController(RoleManager<IdentityRole> roleManager, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _db = db;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = roles;
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            IdentityRole identityRole = new IdentityRole();
            identityRole.Name = name;
            var isExist = await _roleManager.RoleExistsAsync(identityRole.Name);
            if (isExist)
            {
                ViewBag.message = "This Role is Already Exist";
                ViewBag.name = name;
                return View();
            }
            var result = await _roleManager.CreateAsync(identityRole);
            if (result.Succeeded)
            {
                TempData["save"] = "Role Saved Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            ViewBag.id = role.Id;
            ViewBag.name = role.Name;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string name, string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            role.Name = name;
            var isExist = await _roleManager.RoleExistsAsync(role.Name);
            if (isExist)
            {
                ViewBag.message = "This Role is Already Exist";
                ViewBag.name = name;
                return View();
            }
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData["update"] = "Role Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            ViewBag.id = role.Id;
            ViewBag.name = role.Name;
            return View();
        }
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["delete"] = "Role Deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public IActionResult Assign()
        {
            ViewBag.UserId = _userManager.Users.Where(c => c.LockoutEnd < DateTime.Now || c.LockoutEnd == null).Select(u => new { u.Id, u.UserName }).ToList();
            ViewBag.RoleId = _roleManager.Roles.Select(u => new { u.Id, u.Name }).ToList();
            return View();
        }
   
        [HttpPost]
        public async Task<IActionResult> Assign(RoleUserVm roleUser)
        {
            // Get user
            var user = _userManager.Users.FirstOrDefault(c => c.Id == roleUser.UserId);
            if (user == null)
            {
                ViewBag.message = "User not found";
                goto Repopulate; // jump to repopulate ViewBags
            }

            string roleName = roleUser.RoleId; // RoleId now contains role name from dropdown

            // Check if user already has this role
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains(roleName))
            {
                ViewBag.message = "This user already has this role";
                goto Repopulate;
            }

            // Optional: remove all existing roles if you want only one role per user
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                TempData["save"] = "User Role Assigned Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.message = string.Join(", ", result.Errors.Select(e => e.Description));
            }

        Repopulate:
            // Re-populate ViewBags before returning
            ViewBag.UserId = _userManager.Users
                .Where(c => c.LockoutEnd < DateTime.Now || c.LockoutEnd == null)
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            ViewBag.RoleId = _roleManager.Roles
                .Select(u => new { u.Id, u.Name })
                .ToList();

            return View();
        }


        //public async Task<IActionResult> Assign(RoleUserVm roleUser)
        //{
        //    var user = _userManager.Users.FirstOrDefault(c => c.Id == roleUser.UserId);
        //    var isCheckRoleAssign = await _userManager.IsInRoleAsync(user, roleUser.RoleId);
        //    if (isCheckRoleAssign)
        //    {
        //        ViewBag.message = "This user already assign this role";
        //        ViewBag.UserId = _userManager.Users.Where(c => c.LockoutEnd < DateTime.Now || c.LockoutEnd == null).Select(u => new { u.Id, u.UserName }).ToList();
        //        ViewBag.RoleId = _roleManager.Roles.Select(u => new { u.Id, u.Name }).ToList();
        //        return View();
        //    }

        //    var role = await _userManager.AddToRoleAsync(user, roleUser.RoleId);
        //    if (role.Succeeded)
        //    {
        //        TempData["save"] = "User Role Assigned";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View();
        //}

        ////Show Assigned User Role
        [HttpGet]
        public ActionResult AssignUserRole()
        {
            var result = from ur in _db.UserRoles
                         join r in _db.Roles on ur.RoleId equals r.Id
                         join a in _db.Users on ur.UserId equals a.Id
                         select new UserRoleMaping()
                         {
                             UserId = ur.UserId,
                             RoleId = ur.RoleId,
                             UserName = a.UserName,
                             RoleName = r.Name
                         };
            ViewBag.UserRoles = result;
            return View();
        }
    }
}
