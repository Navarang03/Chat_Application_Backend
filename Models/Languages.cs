using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class Languages
    {
        [Key]
        public int Id { get; set; }
        public string Speak { get; set; }
        public string Read { get; set; }
        public string Write { get; set; }

        // FK to Employee
        public int EmployeeId { get; set; }
    }
}
