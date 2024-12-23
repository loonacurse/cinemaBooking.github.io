using _CinemaBooking.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace _CinemaBooking.ViewModels
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password_user { get; set; }

        [System.Web.Mvc.Compare("Password_user", ErrorMessage = "Паролі не співпадають.")]
        public string ConfirmPassword { get; set; }
    }

    public class User_cinemas
    {
        public int IdUser { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Пароль повинен бути не більше 10 символів")]
        public string Password_user { get; set; }

        [NotMapped]
        [Compare("Password_user", ErrorMessage = "Паролі не співпадають.")]
        [Required]
        public string ConfirmPassword { get; set; }
    }
    public class FilmScheduleViewModel
    {
        public int FilmId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string Genre { get; set; }
        public decimal? Rating { get; set; }
        public string ImageUrl { get; set; } 
        public List<SessionViewModel> Sessions { get; set; }
        public List<Hall> HallList { get; set; }
    }

    public class SessionViewModel
    {
        public int SessionId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime Date { get; set; }

        [Display(Name = "Назва залу")]
        public string HallName { get; set; }

        public int HallId { get; set; }
        public int FilmId { get; set; }

        [Display(Name = "Назва фільму")]
        public string FilmTitle { get; set; }

        public List<Session> Sessions { get; set; } 
        public SelectList Halls { get; set; }
    }
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class AdminViewModel
    {
        public int IdAdmin { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password_admin { get; set; }
        public string Role_admin { get; set; }
    }

    public class BookingViewModel
    {
        public int IdBooking { get; set; }
        public string FilmTitle { get; set; }
        public string HallName { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime DateSession { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Row { get; set; } // Ряд
        public int Seat { get; set; } // Місце
    }
    public class SeatViewModel
    {
        public int SeatId { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsBooked { get; set; }
    }

    public class SelectSeatsViewModel
    {
        public int SessionId { get; set; }
        public string FilmTitle { get; set; }
        public string HallName { get; set; }
        public DateTime DateSession { get; set; }
        public TimeSpan StartTime { get; set; }
        public List<SeatViewModel> AvailableSeats { get; set; }
        public List<int> BookedSeats { get; set; }

    }

    public class Films
    {
        public int IdFilm { get; set; }
        public string Title { get; set; }
        public string Description_film { get; set; }
        public System.TimeSpan Duration { get; set; }
        public string Genre { get; set; }
        [Range(0, 10, ErrorMessage = "Рейтинг має бути від 0 до 10.")]
        public Nullable<decimal> Rating { get; set; }
        public int IdAdmin { get; set; }
        public string ImageUrl { get; set; }
    }
    public class AddFilmViewModel
    {
        public string Title { get; set; }
        public string Description_film { get; set; }
        public string Duration { get; set; }
        public string Genre { get; set; }

        public string Rating { get; set; }

        public string ImageUrl { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DateSession { get; set; }
        public List<Hall> AvailableHalls { get; set; } = new List<Hall>();
        public List<Hall> HallList { get; set; }
        public List<SessionViewModel> Sessions { get; set; } = new List<SessionViewModel>();
    }
    public class Halls
    {
        public int IdHall { get; set; }
        public string Name { get; set; }
    }

    public class Admin
    {
        public int IdAdmin { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password_admin { get; set; }
        public string Role_admin { get; set; }
    }

    public class UserCinema
    {
        public int IdUser { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password_user { get; set; }
    }
    public class Sessions
    {
        public int IdSession { get; set; }
        public DateTime DateSession { get; set; }
        public TimeSpan StartTime { get; set; }

        public int IdFilm { get; set; }

        [ForeignKey("IdFilm")]
        public virtual Film Films { get; set; }

        public int IdHall { get; set; }

        [ForeignKey("IdHall")]
        public virtual Hall Hall { get; set; }
    }
}