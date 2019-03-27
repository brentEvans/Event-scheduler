
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam.Models
{
    public class RSVP
    {
        [Key]
        public int RSVPId { get;set; }

        public int UserId { get;set; }

        public int EventId { get;set; }

        public User User { get;set; }

        public Event Event { get;set; }

    }
}
