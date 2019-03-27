using Microsoft.EntityFrameworkCore;
using Event = BeltExam.Models.Event;

namespace BeltExam.Models
{
    public class BeltExamContext : DbContext
    {
        public BeltExamContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get;set;}
        public DbSet<Event> Events {get;set;}
        public DbSet<RSVP> RSVPs {get;set;}

    }
}

