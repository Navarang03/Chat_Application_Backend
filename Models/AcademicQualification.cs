using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class AcademicQualification
    {
        [Key]
        public int Id { get; set; }
        public string CourseName { get; set; }
        public int YearOfPassing { get; set; }
        public string InstitutionName { get; set; }
        public float MarksPercentage { get; set; }

        // FK to Employee
        public int EmployeeId { get; set; }
    }
}
