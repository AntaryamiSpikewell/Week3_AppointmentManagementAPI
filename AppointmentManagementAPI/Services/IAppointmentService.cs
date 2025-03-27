using AppointmentManagementAPI.DTOs;

namespace AppointmentManagementAPI.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<AppointmentDto?> CreateAppointmentAsync(AppointmentDto appointmentDto);
        Task<bool> UpdateAppointmentAsync(AppointmentDto appointmentDto);
        Task<bool> DeleteAppointmentAsync(int id);
    }
}
