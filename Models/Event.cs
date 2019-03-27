using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam.Models
{
    public class Event
    {
        [Key]
        public int EventId {get;set;}

        [Required]
        [MinLength(2, ErrorMessage="Title must be at least 2 characters!")]
        public string Title {get;set;}

        [Required]
        // validate that DateTime is in the future
        [DataType(DataType.Date)]
        public DateTime Date {get;set;}

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Time {get;set;}

        [NotMapped]
        public DateTime EndTime {
            get{
                DateTime end = new DateTime();
                if (DurationUnits == "D"){
                    end = Date.AddDays(Duration);
                } else if (DurationUnits == "H"){
                    end = Date.AddHours(Duration);
                } else if (DurationUnits == "M"){
                    end = Date.AddMinutes(Duration);
                }
                return end;
            }
        }


        [Required]
        [NotMapped]
        public int Duration {get;set;}

        [Required]
        [NotMapped]
        public string DurationUnits {get;set;}


        [Required]
        [MinLength(10, ErrorMessage="Description must be at least 10 characters!")]
        public string Description {get;set;}

        public int UserId {get;set;}
        // navigation property for related User object
        public User Coordinator {get;set;}

        // navigation property for related User objects
        public List<RSVP> Attendees {get;set;} = new List<RSVP>();
        public int AttendeeCount {
            get{
                int count = 0;
                foreach (RSVP attendee in Attendees){
                    count += 1;
                }
                return count;
            }
        }

        public DateTime Created_At {get;set;} = DateTime.Now;

        public DateTime Updated_At {get;set;} = DateTime.Now;
    }
}

