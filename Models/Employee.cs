using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementAPI.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        // Personal Details
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string GuardianName { get; set; }
        public bool PanAvailable { get; set; }
        public string? PanNumber { get; set; }
        public bool AadharAvailable { get; set; }
        public string? AadharNumber { get; set; }

        // Academic (One-to-Many)
        public List<AcademicQualification> Qualifications { get; set; }
        public string OtherQualification { get; set; }
        public Languages LanguagesKnown { get; set; }
        public string SkillsHobbies { get; set; }
        public string Experience { get; set; }

        // HR
        public string Designation { get; set; }
        public string EmployeeType { get; set; }
        public string Salary { get; set; }
        public string Grade { get; set; }
        public string EmployeeId { get; set; }
    }
}
