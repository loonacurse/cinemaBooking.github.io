using _CinemaBooking.Models;
using _CinemaBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _CinemaBooking.Controllers
{
    public class AdminHomeController:Controller
    {
        private CinemaBookingEntities1 db = new CinemaBookingEntities1();
        public ActionResult GetFilms()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            var films = db.Films
                                .Include(f => f.Sessions) 
                                .ToList();

            return View(films); 
        }
        public ActionResult AdminCab()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Cabinet"); 
            }

            return View();
        }
        public ActionResult AddFilm()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Cabinet");
            }
            return View();  
        }

        [HttpPost]
        public ActionResult AddFilm(Films model)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var AdminId = (int)Session["AdminId"];
                    if (model.Rating != null && (model.Rating < 0 || model.Rating > 10 || Math.Round((decimal)model.Rating, 1) != model.Rating))
                    {
                        TempData["Error"]= "Рейтинг має бути числом у діапазоні від 0 до 10 з одним знаком після коми.";
                        return RedirectToAction("AddFilm", "AdminHome");
                    }
                    Film film = new _CinemaBooking.Models.Film
                    {
                        Title = model.Title,
                        Description_film = model.Description_film,
                        Duration = model.Duration,
                        Genre = model.Genre,
                        Rating = model.Rating,
                        ImageUrl = model.ImageUrl,
                        IdAdmin = AdminId
                    };

                    db.Films.Add(film);
                    db.SaveChanges();

                    TempData["Success"] = "Фільм успішно додано!";
                    return RedirectToAction("GetFilms", "AdminHome");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Сталася помилка при додаванні даних. Фільм: " + ex.Message;
                    return RedirectToAction("GetFilms", "AdminHome");
                }
            }

            TempData["Error"] = "Сталася помилка при додаванні даних.";
            return RedirectToAction("GetFilms", "AdminHome");
        }
        public ActionResult ConfirmDelete(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            var film = db.Films.FirstOrDefault(f => f.IdFilm == id);

            if (film == null)
            {
                return HttpNotFound();
            }

            return View(film);
        }
        [HttpPost]
        public ActionResult DeleteFilm(int filmId)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            var film = db.Films.FirstOrDefault(f => f.IdFilm == filmId);

            if (film == null)
            {
                return HttpNotFound();
            }

            try
            {
                db.Films.Remove(film);
                db.SaveChanges();

                TempData["Success"] = "Фільм успішно видалено!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Сталася помилка при видаленні фільму!";
            }

            return RedirectToAction("GetFilms", "AdminHome");
        }
        public ActionResult UpdateFilm(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            var film = db.Films.FirstOrDefault(f => f.IdFilm == id);
            if (film == null)
            {
                return HttpNotFound();
            }
            return View(film); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateFilm(int id, Film model)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            if (ModelState.IsValid)
            {
                var film = db.Films.FirstOrDefault(f => f.IdFilm == id);
                if (film == null)
                {
                    return HttpNotFound();
                }
                if (model.Rating != null && (model.Rating < 0 || model.Rating > 10 || Math.Round((decimal)model.Rating, 1) != model.Rating))
                {
                    TempData["Error"] = "Рейтинг має бути числом у діапазоні від 0 до 10 з одним знаком після коми.";
                    return RedirectToAction("UpdateFilm", "AdminHome");
                }
                film.Title = model.Title;
                film.Description_film = model.Description_film;
                film.Duration = model.Duration;
                film.Genre = model.Genre;
                film.Rating = model.Rating;
                film.ImageUrl = model.ImageUrl;

                db.SaveChanges();

                return RedirectToAction("GetFilms"); 
            }
            return View(model); 
        }


        public ActionResult AddSession(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            var film = db.Films.FirstOrDefault(f => f.IdFilm == id);
            if (film == null)
            {
                TempData["Error"] = "Фільм не знайдено.";
                return RedirectToAction("GetFilms");
            }

            var halls = db.Halls.ToList();  
            var model = new SessionViewModel
            {
                FilmId = film.IdFilm,
                Halls = new SelectList(halls, "IdHall", "Name")  
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AddSession(SessionViewModel model)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "You are not authorized to add films.";
                return RedirectToAction("Login", "Cabinet");
            }
            if (!ModelState.IsValid)
            {
                var halls = db.Halls.ToList();
                model.Halls = new SelectList(halls, "IdHall", "Name");
                return View(model);
            }

            try
            {
                var film = db.Films.FirstOrDefault(f => f.IdFilm == model.FilmId);
                if (film == null)
                {
                    TempData["Error"] = "Фільм не знайдений.";
                    return RedirectToAction("AddSession", "AdminHome");
                }
                TimeSpan sessionDuration = model.EndTime - model.StartTime;
                if (sessionDuration < film.Duration)
                {
                    TempData["Error"] = "Сеанс не може бути коротший за тривалість фільму.";
                    return RedirectToAction("AddSession", "AdminHome");
                }

                var session = new Session
                {
                    IdFilm = model.FilmId,
                    Date_session = model.Date,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    IdHall = model.HallId,
                    IdAdmin = (int)Session["AdminId"]
                };

                db.Sessions.Add(session);
                db.SaveChanges();

                TempData["Success"] = "Сеанс успішно додано!";
                return RedirectToAction("GetFilms");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Сталася помилка: {ex.Message}";
                var halls = db.Halls.ToList();
                model.Halls = new SelectList(halls, "IdHall", "Name");
                return View(model);
            }
        }
        public ActionResult GetUsers()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Cabinet");
            }
            var users = db.User_cinema
                          .Select(u => new
                          {
                              u.Name,
                              u.Email,
                              u.Password_user
                          })
                          .ToList();

            var userViewModels = users.Select(u => new User_cinemas
            {
                Name = u.Name,
                Email = u.Email,
                Password_user = u.Password_user
            }).ToList();

            return View(userViewModels);
        }

        public ActionResult GetAdmins()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Cabinet");
            }
            var admins = db.Admins.Select(a => new AdminViewModel
            {
                IdAdmin = a.IdAdmin,
                Name = a.Name,
                Email = a.Email,
                Role_admin = a.Role_admin
            }).ToList();

            return View(admins); 
        }
    }
}