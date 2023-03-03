using Amazoom.Data;
using Amazoom.Data.Static;
using Amazoom.Data.ViewModels;
using Amazoom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amazoom.Controllers
{ 
    public class AccountController : Controller
    {//Can only be changed when initializing this part
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        //if the user is validated, return the view of the user's interface
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        //intial view is the login interface
        public IActionResult Login() => View(new LoginVM());

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            //if login failed
            if (!ModelState.IsValid) return View(loginVM);
            //async method that read the account info to check whether do they match
            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            //if the user name exist in the database
            if(user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                //if the password matches
                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Items");
                    }
                }
                //else
                TempData["Error"] = "Wrong credentials. Please, try again!";
                return View(loginVM);
            }
            //if the user name doesn't exist
            TempData["Error"] = "Wrong credentials. Please, try again!";
            return View(loginVM);
        }

        //register action
        public IActionResult Register() => View(new RegisterVM());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {//not registered
            if (!ModelState.IsValid) return View(registerVM);
         //check whether the user name is already existed
            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if(user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerVM);
            }
         //create new user
            var newUser = new ApplicationUser()
            {
                FullName = registerVM.FullName,
                Email = registerVM.EmailAddress,
                UserName = registerVM.EmailAddress
            };
            //Add the new user to the database
            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);

            return View("RegisterCompleted");
        }
        //signout action
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Items");
        }

        public IActionResult AccessDenied(string ReturnUrl)
        {
            return View();
        }
    }
}
