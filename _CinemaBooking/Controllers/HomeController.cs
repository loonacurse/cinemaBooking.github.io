using _CinemaBooking.Models;
using _CinemaBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace _CinemaBooking.Controllers
{
    public class HomeController:Controller
    {
        private CinemaBookingEntities1 db = new CinemaBookingEntities1();

        public async Task<ActionResult> Index()
        {
            var sessions = await (from s in db.Sessions
                                  join f in db.Films on s.IdFilm equals f.IdFilm
                                  orderby s.Date_session, s.StartTime  
                                  select new
                                  {
                                      FilmId = f.IdFilm,
                                      FilmTitle = f.Title,
                                      FilmDescription = f.Description_film,
                                      FilmDuration = f.Duration,
                                      FilmGenre = f.Genre,
                                      FilmRating = f.Rating,
                                      FilmImageUrl = f.ImageUrl, 
                                      SessionId = s.IdSession,
                                      SessionStartTime = s.StartTime,
                                      SessionEndTime = s.EndTime,
                                      SessionDate = s.Date_session,
                                  }).ToListAsync();

            if (!sessions.Any())
            {
                ViewBag.Message = "Сеанси не знайдено.";
                return View("NoSchedule");
            }

            var groupedResult = sessions
                .GroupBy(s => new
                {
                    s.FilmId,
                    s.FilmTitle,
                    s.FilmDescription,
                    s.FilmDuration,
                    s.FilmGenre,
                    s.FilmRating,
                    s.FilmImageUrl 
                })
                .Select(group => new FilmScheduleViewModel
                {
                    FilmId = group.Key.FilmId,
                    Title = group.Key.FilmTitle,
                    Duration = group.Key.FilmDuration,
                    Genre = group.Key.FilmGenre,
                    Rating = group.Key.FilmRating,
                    ImageUrl = group.Key.FilmImageUrl, 
                    Sessions = group.Select(g => new SessionViewModel
                    {
                        SessionId = g.SessionId,
                        StartTime = g.SessionStartTime,
                        EndTime = g.SessionEndTime,
                        Date = g.SessionDate,
                    }).ToList()
                })
                .ToList();
            return View(groupedResult);
        }
        public async Task<ActionResult> Details(int id)
        {
            var film = await db.Films
                .Where(f => f.IdFilm == id)
                .Select(f => new FilmScheduleViewModel
                {
                    FilmId = f.IdFilm,
                    Title = f.Title,
                    Description = f.Description_film,
                    Duration = f.Duration,
                    Genre = f.Genre,
                    Rating = f.Rating,
                    ImageUrl = f.ImageUrl,
                    Sessions = db.Sessions
                        .Where(s => s.IdFilm == f.IdFilm)
                        .Select(s => new SessionViewModel
                        {
                            SessionId = s.IdSession,
                            StartTime = s.StartTime,
                            EndTime = s.EndTime,
                            Date = s.Date_session,
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (film == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(film); 
        }

        public ActionResult Bookings()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Cabinet");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                var bookings = db.Bookings
                       .Where(b => b.IdUser == userId)
                       .Select(b => new BookingViewModel
                       {
                           IdBooking = b.IdBooking,
                           FilmTitle = b.Session.Film.Title,
                           HallName = b.Session.Hall.Name,
                           StartTime = b.Session.StartTime,
                           DateSession = b.Session.Date_session,
                           Row = b.Seat.RowNumber,      
                           Seat = b.Seat.SeatNumber, 
                           Status = b.Status_book,
                           CreatedAt = b.CreatedAt
                       })
                       .ToList();

                return View(bookings);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Помилка під час отримання бронювань.";
                return View(new List<BookingViewModel>());
            }
        }
        public ActionResult SelectSeats(int sessionId)
        {
            try
            {
                var session = db.Sessions
                        .Include(s => s.Film)  
                        .Include(s => s.Hall)  
                        .FirstOrDefault(s => s.IdSession == sessionId);

                if (session == null)
                {
                    return HttpNotFound(); 
                }

                var allSeats = db.Seats
                    .Where(s => s.IdHall == session.IdHall)
                    .ToList();

                var bookedSeats = db.Bookings
                    .Where(b => b.IdSession == sessionId)
                    .Select(b => b.IdSeat)
                    .Distinct()
                    .ToList();

                var availableSeats = allSeats
                    .Select(s => new SeatViewModel
                    {
                        SeatId = s.IdSeat,
                        RowNumber = s.RowNumber,
                        SeatNumber = s.SeatNumber,
                        IsAvailable = !bookedSeats.Contains(s.IdSeat) 
                    })
                    .OrderBy(s => s.RowNumber)
                    .ThenBy(s => s.SeatNumber)
                    .ToList();

                var viewModel = new SelectSeatsViewModel
                {
                    SessionId = sessionId,
                    AvailableSeats = availableSeats,
                    FilmTitle = session.Film.Title,
                    HallName = session.Hall.Name,
                    DateSession = session.Date_session,
                    StartTime = session.StartTime
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Помилка під час отримання бронювань.";
                return View();
            }
        }
        [HttpPost]
        public ActionResult BookSeats(int sessionId, int[] selectedSeats)
        {

            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Cabinet");
            }
            try
            {
                if (selectedSeats == null || selectedSeats.Length == 0)
                {
                    TempData["ErrorMessage"] = "Не вибрано жодного місця для бронювання.";
                    return RedirectToAction("SelectSeats", new { sessionId = sessionId });
                }

                var session = db.Sessions.FirstOrDefault(s => s.IdSession == sessionId);
                if (session == null)
                {
                    TempData["ErrorMessage"] = "Сеанс не знайдено.";
                    return RedirectToAction("Index");
                }

                var bookedSeats = db.Bookings
                                    .Where(b => b.IdSession == sessionId && selectedSeats.Contains(b.IdSeat))
                                    .Select(b => b.IdSeat)
                                    .ToList();

                if (bookedSeats.Any())
                {
                    TempData["ErrorMessage"] = "Одне або більше місць вже заброньовані.";
                    return RedirectToAction("SelectSeats", new { sessionId = sessionId });
                }

                var user = GetCurrentUser(); 

                foreach (var seatId in selectedSeats)
                {
                    var seat = db.Seats.FirstOrDefault(s => s.IdSeat == seatId);
                    if (seat == null)
                    {
                        TempData["ErrorMessage"] = $"Місце з ID {seatId} не знайдено.";
                        return RedirectToAction("SelectSeats", new { sessionId = sessionId });
                    }

                    var isSeatBooked = db.Bookings.Any(b => b.IdSeat == seatId && b.IdSession == sessionId);
                    if (isSeatBooked)
                    {
                        TempData["ErrorMessage"] = $"Місце {seatId} вже заброньоване.";
                        return RedirectToAction("SelectSeats", new { sessionId = sessionId });
                    }

                    var booking = new Booking
                    {
                        IdSession = sessionId,
                        IdUser = user.IdUser,
                        IdSeat = seatId,
                        Status_book = "Заброньовано",
                        CreatedAt = DateTime.Now
                    };
                    db.Bookings.Add(booking);
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Місця успішно заброньовано.Бронь можете перевірити у вкладці \"Мій кабінет\"";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Виникла помилка бронювання. Спробуйте пізніше ще раз.";
            }
            return RedirectToAction("SelectSeats", new { sessionId = sessionId });
        }
        private User_cinema GetCurrentUser()
        {
            var userId = Session["UserId"] as int?; 
            if (userId.HasValue)
            {
                return db.User_cinema.FirstOrDefault(u => u.IdUser == userId.Value);
            }
            return null;
        }
    }
}

