using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Employee employee)
        {
            employee.EmployeeId = $"EMP{DateTime.Now.Ticks}";
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees
                .Include(e => e.Qualifications)
                .Include(e => e.LanguagesKnown)
                .ToListAsync();

            return Ok(employees);
        }

        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(string employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Qualifications)
                .Include(e => e.LanguagesKnown)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return NotFound($"Employee with ID {employeeId} not found");

            return Ok(employee);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Employee updatedEmployee)
        {
            var employee = await _context.Employees
                .Include(e => e.Qualifications)
                .Include(e => e.LanguagesKnown)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound($"Employee with ID {id} not found");

            // ?? Update scalar fields manually
            employee.Name = updatedEmployee.Name;
            employee.Email = updatedEmployee.Email;
            employee.Contact = updatedEmployee.Contact;
            employee.DateOfBirth = updatedEmployee.DateOfBirth;
            employee.Gender = updatedEmployee.Gender;
            employee.MaritalStatus = updatedEmployee.MaritalStatus;
            employee.GuardianName = updatedEmployee.GuardianName;
            employee.PanAvailable = updatedEmployee.PanAvailable;
            employee.PanNumber = updatedEmployee.PanNumber;
            employee.AadharAvailable = updatedEmployee.AadharAvailable;
            employee.AadharNumber = updatedEmployee.AadharNumber;
            employee.OtherQualification = updatedEmployee.OtherQualification;
            employee.SkillsHobbies = updatedEmployee.SkillsHobbies;
            employee.Experience = updatedEmployee.Experience;
            employee.Designation = updatedEmployee.Designation;
            employee.EmployeeType = updatedEmployee.EmployeeType;
            employee.Salary = updatedEmployee.Salary;
            employee.Grade = updatedEmployee.Grade;

            // ?? Update Qualifications (One-to-Many)
            if (employee.Qualifications != null)
                _context.AcademicQualifications.RemoveRange(employee.Qualifications);

            if (updatedEmployee.Qualifications != null && updatedEmployee.Qualifications.Any())
            {
                // Clear IDs to avoid conflict
                foreach (var q in updatedEmployee.Qualifications)
                {
                    q.Id = 0;
                    q.EmployeeId = employee.Id; // Link to current employee
                }

                employee.Qualifications = updatedEmployee.Qualifications;
            }

            // ?? Update LanguagesKnown (One-to-One)
            if (employee.LanguagesKnown != null)
                _context.Languages.Remove(employee.LanguagesKnown);

            if (updatedEmployee.LanguagesKnown != null)
            {
                updatedEmployee.LanguagesKnown.Id = 0; // Avoid ID conflict
                updatedEmployee.LanguagesKnown.EmployeeId = employee.Id; // Link to current employee

                employee.LanguagesKnown = updatedEmployee.LanguagesKnown;
            }

            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpDelete("{employeeId}")]
        public async Task<IActionResult> Delete(string employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Qualifications)
                .Include(e => e.LanguagesKnown)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return NotFound($"Employee with ID {employeeId} not found");

            // Remove qualifications if any
            if (employee.Qualifications != null && employee.Qualifications.Any())
            {
                _context.AcademicQualifications.RemoveRange(employee.Qualifications);
            }

            // Remove languages known if any
            if (employee.LanguagesKnown != null)
            {
                _context.Languages.Remove(employee.LanguagesKnown);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Employee with ID {employeeId} deleted successfully." });

        }





    }
}
