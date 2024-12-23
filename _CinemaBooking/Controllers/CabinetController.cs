using _CinemaBooking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _CinemaBooking.Controllers
{
    public class CabinetController:Controller
    {
        private CinemaBookingEntities1 db = new CinemaBookingEntities1();
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            var user = db.User_cinema
                         .FirstOrDefault(u => u.Email == email && u.Password_user == password);

            if (user != null)
            {
                Session["UserId"] = user.IdUser;
                Session["UserEmail"] = user.Email;
                Session["UserName"] = user.Name;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Невірний логін або пароль!";
                return View();
            }
        }

        [HttpGet]
        public ActionResult LoginAsAdmin()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginAsAdmin(string email, string password)
        {
            var admin = db.Admins
                         .FirstOrDefault(u => u.Email == email && u.Password_admin == password && u.Role_admin == "Admin");

            if (admin != null)
            {
                Session["AdminId"] = admin.IdAdmin;
                Session["AdminEmail"] = admin.Email;
                Session["AdminName"] = admin.Name;
                Session["Role"] = "Admin";

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Невірний логін або пароль для адміністратора!";
                return View();
            }
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User_cinema model)
        {
            try
            {
                var existingUser = db.User_cinema.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "Користувач з таким email вже існує.";
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var state = ModelState[modelStateKey];
                        if (state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                Console.WriteLine($"Field: {modelStateKey}, Error: {error.ErrorMessage}");
                            }
                        }
                    }
                    return View(model);
                }

                var newId = db.User_cinema.Max(u => u.IdUser) + 1;
                var newUser = new User_cinema()
                {
                    IdUser = newId,
                    Name = model.Name,
                    Email = model.Email,
                    Password_user = model.Password_user
                };

                db.User_cinema.Add(newUser);
                db.SaveChanges();
                Session["UserId"] = newUser.IdUser;
                Session["UserEmail"] = newUser.Email;
                Session["UserName"] = newUser.Name;
                Session["Role"] = "User";

                return RedirectToAction("Index", "Home");
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
                ViewBag.ErrorMessage = "Помилка валідації при збереженні користувача.";
                return View(model);
            }
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }


    }
}
