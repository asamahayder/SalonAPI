using SalonAPI.Models.DTOs;

namespace SalonAPI.Models
{
    public static class Mapper
    {
        public static ServiceDTO MapToDTO(Service service)
        {
            var listOfIds = new List<int>();

            foreach (var employee in service.Employees)
            {
                listOfIds.Add(employee.Id);
            }

            var serviceDTO = new ServiceDTO()
            {
                Id = service.Id,
                SalonId = service.SalonId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                DurationInMinutes = service.DurationInMinutes,
                PauseStartInMinutes = service.PauseStartInMinutes,
                PauseEndInMinutes = service.PauseEndInMinutes,
                EmployeesIds = listOfIds
            };
            
            return serviceDTO;
        }

        public static SalonDTO MapToDTO(Salon salon)
        {
            SalonDTO salonDTO = new SalonDTO()
            {
                Id = salon.Id,
                City = salon.City,
                PostCode = salon.PostCode,
                StreetName = salon.StreetName,
                StreetNumber = salon.StreetNumber,
                Suit = salon.Suit,
                Door = salon.Door,
                Phone = salon.Phone,
                Email = salon.Email,
            };

            return salonDTO;
        }

        public static BookingDTO MapToDTO(Booking booking)
        {
            BookingDTO bookingDTO = new BookingDTO()
            {
                Id = booking.Id,
                BookedById = booking.BookedById,
                EmployeeId = booking.EmployeeId,
                ServiceId = booking.ServiceId,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime
            };

            return bookingDTO;
        }
    }
}
