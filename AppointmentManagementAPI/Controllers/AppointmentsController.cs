using Microsoft.AspNetCore.Mvc;
using AppointmentManagementAPI.Services;
using AppointmentManagementAPI.DTOs;
using System;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace AppointmentManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;
        private readonly string PacificTimeZone = "Pacific Standard Time";

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var appointments = await _service.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentByIdAsync(id);
                if (appointment == null)
                    return NotFound(new { message = $"Appointment with ID {id} not found." });

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
                return BadRequest(new { message = "Invalid appointment data." });

            if (!IsValidPSTTime(appointmentDto.ScheduledDate))
                return BadRequest(new { message = "Appointment must be scheduled between 9 AM - 7 PM PST." });

            try
            {
                var newAppointment = await _service.CreateAppointmentAsync(appointmentDto);
                Console.WriteLine(newAppointment);
                if (newAppointment == null || newAppointment.Id <= 0)
                    return StatusCode(500, new { message = "Failed to create the appointment." });

                return CreatedAtAction(nameof(Get), new { id = newAppointment.Id }, newAppointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null || id != appointmentDto.Id)
                return BadRequest(new { message = "Invalid data or ID mismatch." });

            Console.WriteLine($"Updating appointment ID: {id}");

            var existingAppointment = await _service.GetAppointmentByIdAsync(id);
            if (existingAppointment == null)
                return NotFound(new { message = $"Appointment with ID {id} not found." });

            if (existingAppointment.Status == "Completed" || existingAppointment.Status == "Cancelled")
                return BadRequest(new { message = "Cannot modify a completed or cancelled appointment." });

            if (!IsValidPSTTime(appointmentDto.ScheduledDate))
                return BadRequest(new { message = "Appointment must be scheduled between 9 AM - 7 PM PST." });

            var updated = await _service.UpdateAppointmentAsync(appointmentDto);

            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAppointmentAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Appointment with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// Validates if the given UTC time falls within 9 AM - 7 PM PST
        private bool IsValidPSTTime(DateTime utcDateTime)
        {
            try
            {
                var pstZone = TZConvert.GetTimeZoneInfo(PacificTimeZone);
                var pstDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pstZone);

                return pstDateTime.TimeOfDay >= TimeSpan.FromHours(9) &&
                       pstDateTime.TimeOfDay <= TimeSpan.FromHours(19);
            }
            catch
            {
                return false; // If conversion fails, assume invalid time.
            }
        }
    }
}
