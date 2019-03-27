using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BeltExam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Event = BeltExam.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace BeltExam.Controllers
{
    public class HomeController : Controller
    {
        private BeltExamContext dbContext {get;set;}

        public HomeController(BeltExamContext context){
            dbContext = context;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/register/process")]
        public IActionResult RegisterProcess(User userSubmission){
            if (ModelState.IsValid){
                // check that no matching user email is in db
                User thisUser = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if (thisUser != null){
                    ModelState.AddModelError("Email", "This email is already in use!");
                    return View("Index");
                }

                if (dbContext == null){
                    System.Console.WriteLine("************");
                    System.Console.WriteLine("dbContext is null");
                }
                else if (dbContext.Users == null){
                    System.Console.WriteLine("************");
                    System.Console.WriteLine("dbContext.Users is null");
                }
                if (userSubmission == null){
                    System.Console.WriteLine("************");
                    System.Console.WriteLine("userSubmission is null");
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                userSubmission.Password = Hasher.HashPassword(userSubmission, userSubmission.Password);
                dbContext.Add(userSubmission);

                dbContext.SaveChanges();
                User LoggedInUser = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                HttpContext.Session.SetInt32("LoggedInUserId", LoggedInUser.UserId);
                return RedirectToAction("Home");
            }
            return View("Index");
        }

        [HttpPost("/login/process")]
        public IActionResult LoginProcess(LoginUser loginUserSubmission){
            if (ModelState.IsValid){
                User thisUser = dbContext.Users.FirstOrDefault(u => u.Email == loginUserSubmission.Email);
                if(thisUser == null){
                    ModelState.AddModelError("Email", "No user exists with the provided email address! Please register!");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(loginUserSubmission, thisUser.Password, loginUserSubmission.Password);
                if (result==0){
                    ModelState.AddModelError("Password", "Invalid email/password combination!");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("LoggedInUserId", thisUser.UserId);
                return RedirectToAction("Home");
            }
            return View("Index");
        }







        [HttpGet("/home")]
        public IActionResult Home(){
            // only allow access if logged in
            int? LoggedInUserId = HttpContext.Session.GetInt32("LoggedInUserId");
            if (LoggedInUserId == null){
                return RedirectToAction("Index");
            }
            User LoggedInUser = dbContext.Users
                .Include(u => u.RSVPs)
                .ThenInclude(r => r.Event)
                .FirstOrDefault(u => u.UserId == LoggedInUserId);
            ViewBag.User = LoggedInUser;
            // pass in List of all Events 
            List<Event> allEvents = dbContext.Events
                .Include(e => e.Coordinator)
                .Include(e => e.Attendees)
                .ToList();
            ViewBag.allEvents = allEvents;

            // pass in List of all Event LoggedInUser is attending

            List<RSVP> allUserRSVPs = dbContext.RSVPs
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.UserId == LoggedInUserId)
                .ToList();

            ViewBag.allUserRSVPs = allUserRSVPs;

            List<Event> allUserEvents = new List<Event>();
            foreach (RSVP rsvp in allUserRSVPs){
                allUserEvents.Add(rsvp.Event);
            }
            ViewBag.allUserEvents = allUserEvents;

            return View();
        }




        [HttpGet("/new")]
        public IActionResult NewEvent(){
            int? LoggedInUserId = HttpContext.Session.GetInt32("LoggedInUserId");
            System.Console.WriteLine(LoggedInUserId);
            return View();
        }



        [HttpPost("/new/create")]
        public IActionResult CreateEvent(Event eventSubmission){
            if (ModelState.IsValid){
                // check that start time and date are in the future
                if (DateTime.Now.CompareTo(eventSubmission.Date.Add(eventSubmission.Time)) > 0) {
                    ModelState.AddModelError("Date", "Date must be in the future");
                    ModelState.AddModelError("Time", "Time must be in the future");
                    return View("NewEvent");
                }
            
                User thisUser = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("LoggedInUserId"));
                eventSubmission.Coordinator = thisUser;
                eventSubmission.UserId = thisUser.UserId;
                dbContext.Events.Add(eventSubmission);
                RSVP thisRSVP = new RSVP{
                    EventId = eventSubmission.EventId,
                    UserId = thisUser.UserId,
                };
                ViewBag.User = thisUser;
                dbContext.RSVPs.Add(thisRSVP);
                dbContext.SaveChanges();
                return Redirect($"/Event/{eventSubmission.EventId}");
                // make sure that works
            } else {
                return View("NewEvent");
            }
        }





        [HttpGet("/Event/{eventId}")]
        public IActionResult Event(int eventId){
            // only allow access if logged in
            int? LoggedInUserId = HttpContext.Session.GetInt32("LoggedInUserId");
            if (LoggedInUserId == null){
                return RedirectToAction("Index");
            }
            var thisUser = dbContext.Users.FirstOrDefault(u => u.UserId == (int)LoggedInUserId);
            ViewBag.thisUser = thisUser;
            Event thisEvent = dbContext.Events
                .Include(e => e.Coordinator)
                .Include(e => e.Attendees)
                .ThenInclude(r => r.User)
                .FirstOrDefault(e => e.EventId == eventId);

            ViewBag.Coordinator = thisEvent.Coordinator;

            return View("Event", thisEvent);
        }



        [HttpGet("/event/{eventId}/join")]
        public IActionResult Join (int eventId){
            // do not allow if time conflict with thisUser.RSVPs

            // get list of thisUser RSVPs
            Event thisEvent = dbContext.Events.SingleOrDefault(e => e.EventId == eventId);
            User thisUser = dbContext.Users.Where(u => u.UserId == HttpContext.Session.GetInt32("LoggedInUserId")).FirstOrDefault();
            List<RSVP> thisUserRSVPs = thisUser.RSVPs;

            DateTime thisEventEnd = thisEvent.EndTime;
            
            foreach (var rsvp in thisUser.RSVPs) {
                DateTime end = thisEventEnd;
                if (end.CompareTo(thisEventEnd) > 0 && rsvp.Event.Date.CompareTo(thisEvent.Date.Add(thisEvent.Time)) < 0){
                    return RedirectToAction("Home");
                }
            }
            RSVP thisRSVP = new RSVP{
                EventId = thisEvent.EventId,
                UserId = thisUser.UserId,
            };
            thisUser.RSVPs.Add(thisRSVP);
            dbContext.SaveChanges();

            
            return RedirectToAction("Home");
        }

        [HttpGet("/event/{eventId}/delete")]
        public IActionResult Delete(int eventId) {
            if (HttpContext.Session.GetInt32("LoggedInUserId") == null) {
                return RedirectToAction("Index");
            }
            Event thisEvent = dbContext.Events
                .Include(e => e.Coordinator)
                .Include(e => e.Attendees)
                .ThenInclude(r => r.User)
                .FirstOrDefault(a => a.EventId == eventId);
            dbContext.Events.Remove(thisEvent);
            dbContext.SaveChanges();
            return RedirectToAction("Home");
        }

        [HttpGet("/event/{eventId}/leave")]
        public IActionResult Leave(int eventId){
            // add leave logic
            Event thisEvent = dbContext.Events.FirstOrDefault(e => e.EventId == eventId);
            User thisUser = dbContext.Users.Where(u => u.UserId == HttpContext.Session.GetInt32("LoggedInUserId")).FirstOrDefault();
            RSVP thisRSVP = dbContext.RSVPs
                .Where(r => r.EventId == eventId && r.UserId == (int)HttpContext.Session.GetInt32("LoggedInUserId"))
                .FirstOrDefault();

            dbContext.RSVPs.Remove(thisRSVP);
            dbContext.SaveChanges();

            
            return RedirectToAction("Home");
        }










        [HttpGet("/logout")]
        public IActionResult Logout(){
            HttpContext.Session.Clear();
            return View("Index");
        }
    }
}

