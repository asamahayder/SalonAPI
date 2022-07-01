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
                Name = salon.Name,
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
                EndTime = booking.EndTime,
                Note = booking.Note
            };

            return bookingDTO;
        }

        public static RequestDTO MapToDTO(Request request)
        {
            RequestDTO requestDTO = new RequestDTO()
            {
                Id = request.Id,
                EmployeeId = request.EmployeeId,
                SalonId = request.SalonId,
                Date = request.Date,
                RequestStatus = request.RequestStatus
            };

            return requestDTO;
        }

        public static OpeningHoursDTO MapToDTO(OpeningHours openingHours)
        {
            OpeningHoursDTO openingHoursDTO = new OpeningHoursDTO()
            {
                EmployeeId = openingHours.EmployeeId,
                MondayOpen = openingHours.MondayOpen,
                MondayStart = openingHours.MondayStart,
                MondayEnd = openingHours.MondayEnd,
                TuesdayOpen = openingHours.TuesdayOpen,
                TuesdayStart = openingHours.TuesdayStart,
                TuesdayEnd = openingHours.TuesdayEnd,
                WednessdayOpen = openingHours.WednessdayOpen,
                WednessdayStart = openingHours.WednessdayStart,
                WednessdayEnd = openingHours.WednessdayEnd,
                ThursdayOpen = openingHours.ThursdayOpen,
                ThursdayStart = openingHours.ThursdayStart,
                ThursdayEnd = openingHours.ThursdayEnd,
                FridayOpen = openingHours.FridayOpen,
                FridayStart = openingHours.FridayStart,
                FridayEnd = openingHours.FridayEnd,
                SaturdayOpen = openingHours.SaturdayOpen,
                SaturdayStart = openingHours.SaturdayStart,
                SaturdayEnd = openingHours.SaturdayEnd,
                SundayOpen = openingHours.SundayOpen,
                SundayStart = openingHours.SundayStart,
                SundayEnd = openingHours.SundayEnd
            };

            return openingHoursDTO;
        }

        public static SpecialOpeningHoursDTO MapToDTO(SpecialOpeningHours specialOpeningHours)
        {
            SpecialOpeningHoursDTO specialOpeningHoursDTO = new SpecialOpeningHoursDTO()
            {
                EmployeeId = specialOpeningHours.EmployeeId,
                Week = specialOpeningHours.Week,
                MondayOpen = specialOpeningHours.MondayOpen,
                MondayStart = specialOpeningHours.MondayStart,
                MondayEnd = specialOpeningHours.MondayEnd,
                TuesdayOpen = specialOpeningHours.TuesdayOpen,
                TuesdayStart = specialOpeningHours.TuesdayStart,
                TuesdayEnd = specialOpeningHours.TuesdayEnd,
                WednessdayOpen = specialOpeningHours.WednessdayOpen,
                WednessdayStart = specialOpeningHours.WednessdayStart,
                WednessdayEnd = specialOpeningHours.WednessdayEnd,
                ThursdayOpen = specialOpeningHours.ThursdayOpen,
                ThursdayStart = specialOpeningHours.ThursdayStart,
                ThursdayEnd = specialOpeningHours.ThursdayEnd,
                FridayOpen = specialOpeningHours.FridayOpen,
                FridayStart = specialOpeningHours.FridayStart,
                FridayEnd = specialOpeningHours.FridayEnd,
                SaturdayOpen = specialOpeningHours.SaturdayOpen,
                SaturdayStart = specialOpeningHours.SaturdayStart,
                SaturdayEnd = specialOpeningHours.SaturdayEnd,
                SundayOpen = specialOpeningHours.SundayOpen,
                SundayStart = specialOpeningHours.SundayStart,
                SundayEnd = specialOpeningHours.SundayEnd
            };

            return specialOpeningHoursDTO;
        }

        public static OpeningHoursDTO MapToOpeningHoursDTO(SpecialOpeningHours specialOpeningHours)
        {
            OpeningHoursDTO openingHoursDTO = new OpeningHoursDTO()
            {
                EmployeeId = specialOpeningHours.EmployeeId,
                MondayOpen = specialOpeningHours.MondayOpen,
                MondayStart = specialOpeningHours.MondayStart,
                MondayEnd = specialOpeningHours.MondayEnd,
                TuesdayOpen = specialOpeningHours.TuesdayOpen,
                TuesdayStart = specialOpeningHours.TuesdayStart,
                TuesdayEnd = specialOpeningHours.TuesdayEnd,
                WednessdayOpen = specialOpeningHours.WednessdayOpen,
                WednessdayStart = specialOpeningHours.WednessdayStart,
                WednessdayEnd = specialOpeningHours.WednessdayEnd,
                ThursdayOpen = specialOpeningHours.ThursdayOpen,
                ThursdayStart = specialOpeningHours.ThursdayStart,
                ThursdayEnd = specialOpeningHours.ThursdayEnd,
                FridayOpen = specialOpeningHours.FridayOpen,
                FridayStart = specialOpeningHours.FridayStart,
                FridayEnd = specialOpeningHours.FridayEnd,
                SaturdayOpen = specialOpeningHours.SaturdayOpen,
                SaturdayStart = specialOpeningHours.SaturdayStart,
                SaturdayEnd = specialOpeningHours.SaturdayEnd,
                SundayOpen = specialOpeningHours.SundayOpen,
                SundayStart = specialOpeningHours.SundayStart,
                SundayEnd = specialOpeningHours.SundayEnd
            };

            return openingHoursDTO;
        }

        public static UserDTO MapToDTO(User user)
        {
            var userDTO = new UserDTO()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Role = user.Role.GetString()
            };

            return userDTO;
        }

        public static TextDTO MapToDTO(Text text)
        {
            var textDTO = new TextDTO()
            {
                Id = text.Id,
                Key = text.Key,
                DanishValue = text.DanishValue,
                EnglishValue = text.EnglishValue
            };

            return textDTO;
        }
    }
}
