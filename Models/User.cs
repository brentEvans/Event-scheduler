using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam.Models
{
    public class User
    {
        [Key]
        public int UserId {get;set;}

        [Required]
        [MinLength(2, ErrorMessage="First Name must be at least 2 characters!")]
        [Display(Name="First Name")]
        public string FirstName {get;set;}

        [Required]
        [MinLength(2, ErrorMessage="Last Name must be at least 2 characters!")]
        [Display(Name="Last Name")]
        public string LastName {get;set;}

        [Required]
        [EmailAddress]
        [Display(Name="Email Address")]
        public string Email {get;set;}

        [Required]
        [MinLength(8, ErrorMessage="Password must be at least 8 characters!")]
        [RegularExpression("^(?=.*?[A-Za-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")]
        [DataType(DataType.Password)]
        public string Password {get;set;}

        [NotMapped]
        [Compare("Password")]
        [Display(Name="Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPW {get;set;}

        // navigation property for related Event objects (many-many)
        public List<RSVP> RSVPs {get;set;} = new List<RSVP>();

        // navigation property for related Event objects (one-many)
        public List<Event> CoordinatedEvents {get;set;} = new List<Event>();

        public DateTime Created_At {get;set;} = DateTime.Now;

        public DateTime Updated_At {get;set;} = DateTime.Now;
    }
}



