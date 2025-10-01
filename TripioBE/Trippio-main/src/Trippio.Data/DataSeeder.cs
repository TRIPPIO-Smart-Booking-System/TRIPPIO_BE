using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Domain.Entities;

namespace Trippio.Data
{
    public class DataSeeder
    {
        public async Task SeedAsync(TrippioDbContext context)
        {
            await SeedRolesAndUsers(context);
            await SeedHotelsAndRooms(context);
            await SeedTransportAndTrips(context);
            await SeedShows(context);
            await SeedBookingsWithDetails(context);
        }

        private async Task SeedRolesAndUsers(TrippioDbContext context)
        {
            var passwordHasher = new PasswordHasher<AppUser>();

            // Seed roles
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");
            if (adminRole == null)
            {
                adminRole = new AppRole
                {
                    Id = Guid.Parse("39D2FA36-117C-4552-AC04-7A90993075FF"),
                    Name = "admin",
                    NormalizedName = "ADMIN",
                    DisplayName = "Quản trị viên",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                await context.Roles.AddAsync(adminRole);
            }

            var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "CUSTOMER");
            if (customerRole == null)
            {
                customerRole = new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = "customer",
                    NormalizedName = "CUSTOMER",
                    DisplayName = "Khách hàng",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                await context.Roles.AddAsync(customerRole);
            }

            await context.SaveChangesAsync();

            // Helper method to create user
            async Task<AppUser> CreateUserIfNotExists(string userName, string email, string phone, string firstName, string lastName, Guid roleId, bool isAdmin = false)
            {
                var normalizedUserName = userName.ToUpper();
                var user = await context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
                if (user != null) return user;

                var userId = Guid.NewGuid();
                user = new AppUser
                {
                    Id = userId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    UserName = userName,
                    NormalizedUserName = normalizedUserName,
                    PhoneNumber = phone,
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    DateCreated = DateTime.UtcNow,
                    Dob = new DateTime(1990, 1, 1),
                    IsEmailVerified = true,
                    IsFirstLogin = false,
                    Balance = isAdmin ? 10000 : 50000,
                    LoyaltyAmountPerPost = 1000
                };
                user.PasswordHash = passwordHasher.HashPassword(user, isAdmin ? "Admin@123$" : "Customer@123$");
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                // Assign role
                var userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                if (userRole == null)
                {
                    await context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                    {
                        UserId = userId,
                        RoleId = roleId
                    });
                    await context.SaveChangesAsync();
                }

                return user;
            }

            // Create admin users
            await CreateUserIfNotExists("VietAdmin", "vietbmt19@gmail.com", "0977452762", "Việt", "Admin", adminRole.Id, true);
            await CreateUserIfNotExists("LinhLonton", "linhbinhtinh12344@gmail.com", "0382574698", "Linh", "Admin", adminRole.Id, true);
            await CreateUserIfNotExists("DuyAnhBulon", "tranhoduyanh03@gmail.com", "091234567", "Duy Anh", "Admin", adminRole.Id, true);

            // Create customer users
            await CreateUserIfNotExists("customer1", "customer1@gmail.com", "0901234567", "Nguyễn", "Văn A", customerRole.Id);
            await CreateUserIfNotExists("customer2", "customer2@gmail.com", "0902345678", "Trần", "Thị B", customerRole.Id);
            await CreateUserIfNotExists("customer3", "customer3@gmail.com", "0903456789", "Lê", "Văn C", customerRole.Id);
        }

        private async Task SeedHotelsAndRooms(TrippioDbContext context)
        {
            // Seed Hotels
            var hotel1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var hotel2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var hotel3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

            if (!await context.Hotels.AnyAsync())
            {
                var hotels = new List<Hotel>
                {
                    new Hotel
                    {
                        Id = hotel1Id,
                        Name = "Saigon Prince Hotel",
                        Address = "63 Nguyen Hue Boulevard, District 1",
                        City = "Ho Chi Minh City",
                        Country = "Vietnam",
                        Description = "Khách sạn 4 sao sang trọng tại trung tâm Sài Gòn với view sông đẹp",
                        Stars = 4,
                        DateCreated = DateTime.UtcNow
                    },
                    new Hotel
                    {
                        Id = hotel2Id,
                        Name = "Park Hyatt Saigon",
                        Address = "2 Lam Son Square, District 1",
                        City = "Ho Chi Minh City",
                        Country = "Vietnam",
                        Description = "Khách sạn 5 sao đẳng cấp quốc tế với dịch vụ hoàn hảo",
                        Stars = 5,
                        DateCreated = DateTime.UtcNow
                    },
                    new Hotel
                    {
                        Id = hotel3Id,
                        Name = "Liberty Central Saigon Citypoint",
                        Address = "59-61 Pasteur Street, District 1",
                        City = "Ho Chi Minh City",
                        Country = "Vietnam",
                        Description = "Khách sạn boutique hiện đại với thiết kế độc đáo",
                        Stars = 4,
                        DateCreated = DateTime.UtcNow
                    }
                };

                await context.Hotels.AddRangeAsync(hotels);
                await context.SaveChangesAsync();
            }

            // Seed Rooms
            if (!await context.Rooms.AnyAsync())
            {
                var rooms = new List<Room>
                {
                    // Rooms for Saigon Prince Hotel
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel1Id,
                        RoomType = "Standard Room",
                        Capacity = 2,
                        PricePerNight = 80,
                        AvailableRooms = 10,
                        DateCreated = DateTime.UtcNow
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel1Id,
                        RoomType = "Deluxe Room",
                        Capacity = 2,
                        PricePerNight = 120,
                        AvailableRooms = 8,
                        DateCreated = DateTime.UtcNow
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel1Id,
                        RoomType = "Suite",
                        Capacity = 4,
                        PricePerNight = 200,
                        AvailableRooms = 3,
                        DateCreated = DateTime.UtcNow
                    },

                    // Rooms for Park Hyatt Saigon
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel2Id,
                        RoomType = "Park Room",
                        Capacity = 2,
                        PricePerNight = 300,
                        AvailableRooms = 15,
                        DateCreated = DateTime.UtcNow
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel2Id,
                        RoomType = "Opera Suite",
                        Capacity = 3,
                        PricePerNight = 500,
                        AvailableRooms = 5,
                        DateCreated = DateTime.UtcNow
                    },

                    // Rooms for Liberty Central
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel3Id,
                        RoomType = "Superior Room",
                        Capacity = 2,
                        PricePerNight = 90,
                        AvailableRooms = 12,
                        DateCreated = DateTime.UtcNow
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotel3Id,
                        RoomType = "Executive Room",
                        Capacity = 2,
                        PricePerNight = 150,
                        AvailableRooms = 8,
                        DateCreated = DateTime.UtcNow
                    }
                };

                await context.Rooms.AddRangeAsync(rooms);
                await context.SaveChangesAsync();
            }
        }

        private async Task SeedTransportAndTrips(TrippioDbContext context)
        {
            // Seed Transport
            var transport1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var transport2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var transport3Id = Guid.Parse("66666666-6666-6666-6666-666666666666");

            if (!await context.Transports.AnyAsync())
            {
                var transports = new List<Transport>
                {
                    new Transport
                    {
                        Id = transport1Id,
                        TransportType = "Airline",
                        Name = "Vietnam Airlines",
                        DateCreated = DateTime.UtcNow
                    },
                    new Transport
                    {
                        Id = transport2Id,
                        TransportType = "Bus",
                        Name = "Phuong Trang Bus",
                        DateCreated = DateTime.UtcNow
                    },
                    new Transport
                    {
                        Id = transport3Id,
                        TransportType = "Train",
                        Name = "Saigon Express Train",
                        DateCreated = DateTime.UtcNow
                    }
                };

                await context.Transports.AddRangeAsync(transports);
                await context.SaveChangesAsync();
            }

            // Seed Transport Trips
            if (!await context.TransportTrips.AnyAsync())
            {
                var trips = new List<TransportTrip>
                {
                    // Vietnam Airlines trips
                    new TransportTrip
                    {
                        Id = Guid.NewGuid(),
                        TransportId = transport1Id,
                        Departure = "Ho Chi Minh City",
                        Destination = "Hanoi",
                        DepartureTime = new DateTime(2024, 12, 15, 6, 0, 0),
                        ArrivalTime = new DateTime(2024, 12, 15, 8, 15, 0),
                        Price = 150,
                        AvailableSeats = 180,
                        DateCreated = DateTime.UtcNow
                    },
                    new TransportTrip
                    {
                        Id = Guid.NewGuid(),
                        TransportId = transport1Id,
                        Departure = "Ho Chi Minh City",
                        Destination = "Da Nang",
                        DepartureTime = new DateTime(2024, 12, 16, 14, 30, 0),
                        ArrivalTime = new DateTime(2024, 12, 16, 15, 45, 0),
                        Price = 120,
                        AvailableSeats = 180,
                        DateCreated = DateTime.UtcNow
                    },

                    // Phuong Trang Bus trips
                    new TransportTrip
                    {
                        Id = Guid.NewGuid(),
                        TransportId = transport2Id,
                        Departure = "Ho Chi Minh City",
                        Destination = "Vung Tau",
                        DepartureTime = new DateTime(2024, 12, 15, 7, 0, 0),
                        ArrivalTime = new DateTime(2024, 12, 15, 9, 30, 0),
                        Price = 15,
                        AvailableSeats = 45,
                        DateCreated = DateTime.UtcNow
                    },
                    new TransportTrip
                    {
                        Id = Guid.NewGuid(),
                        TransportId = transport2Id,
                        Departure = "Ho Chi Minh City",
                        Destination = "Da Lat",
                        DepartureTime = new DateTime(2024, 12, 17, 22, 0, 0),
                        ArrivalTime = new DateTime(2024, 12, 18, 4, 0, 0),
                        Price = 25,
                        AvailableSeats = 40,
                        DateCreated = DateTime.UtcNow
                    },

                    // Train trips
                    new TransportTrip
                    {
                        Id = Guid.NewGuid(),
                        TransportId = transport3Id,
                        Departure = "Ho Chi Minh City",
                        Destination = "Nha Trang",
                        DepartureTime = new DateTime(2024, 12, 16, 19, 20, 0),
                        ArrivalTime = new DateTime(2024, 12, 17, 6, 45, 0),
                        Price = 45,
                        AvailableSeats = 200,
                        DateCreated = DateTime.UtcNow
                    }
                };

                await context.TransportTrips.AddRangeAsync(trips);
                await context.SaveChangesAsync();
            }
        }

        private async Task SeedShows(TrippioDbContext context)
        {
            if (!await context.Shows.AnyAsync())
            {
                var shows = new List<Show>
                {
                    new Show
                    {
                        Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                        Name = "AO Show",
                        Location = "Saigon Opera House, 7 Lam Son Square",
                        City = "Ho Chi Minh City",
                        StartDate = new DateTime(2024, 12, 15, 19, 30, 0),
                        EndDate = new DateTime(2024, 12, 15, 21, 0, 0),
                        Price = 50,
                        AvailableTickets = 200,
                        DateCreated = DateTime.UtcNow
                    },
                    new Show
                    {
                        Id = Guid.NewGuid(),
                        Name = "Teh Dar Show",
                        Location = "Diamond Plaza, 34 Le Duan Street",
                        City = "Ho Chi Minh City",
                        StartDate = new DateTime(2024, 12, 16, 20, 0, 0),
                        EndDate = new DateTime(2024, 12, 16, 21, 30, 0),
                        Price = 65,
                        AvailableTickets = 150,
                        DateCreated = DateTime.UtcNow
                    },
                    new Show
                    {
                        Id = Guid.NewGuid(),
                        Name = "Water Puppet Show",
                        Location = "Golden Dragon Theatre, 55B Nguyen Thi Minh Khai",
                        City = "Ho Chi Minh City",
                        StartDate = new DateTime(2024, 12, 17, 18, 0, 0),
                        EndDate = new DateTime(2024, 12, 17, 19, 0, 0),
                        Price = 25,
                        AvailableTickets = 80,
                        DateCreated = DateTime.UtcNow
                    }
                };

                await context.Shows.AddRangeAsync(shows);
                await context.SaveChangesAsync();
            }
        }

        private async Task SeedBookingsWithDetails(TrippioDbContext context)
        {
            // Get sample users
            var customers = await context.Users
                .Where(u => context.UserRoles.Any(ur => ur.UserId == u.Id && 
                    context.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == "CUSTOMER")))
                .Take(3)
                .ToListAsync();

            if (!customers.Any()) return;

            // Get sample data
            var hotels = await context.Hotels.Take(2).ToListAsync();
            var rooms = await context.Rooms.Take(3).ToListAsync();
            var trips = await context.TransportTrips.Take(2).ToListAsync();
            var shows = await context.Shows.Take(2).ToListAsync();

            if (!await context.Bookings.AnyAsync())
            {
                var bookings = new List<Booking>();
                var accommodationDetails = new List<AccommodationBookingDetail>();
                var transportDetails = new List<TransportBookingDetail>();
                var entertainmentDetails = new List<EntertainmentBookingDetail>();
                var extraServices = new List<ExtraService>();
                var feedbacks = new List<Feedback>();
                var comments = new List<Comment>();

                // Create bookings
                for (int i = 0; i < 6; i++)
                {
                    var customer = customers[i % customers.Count];
                    var bookingId = Guid.NewGuid();
                    var startDate = DateTime.UtcNow.AddDays(i * 7 + 10); // Future bookings
                    var endDate = startDate.AddDays(3);

                    var booking = new Booking
                    {
                        Id = bookingId,
                        UserId = customer.Id,
                        BookingType = "Travel Package",
                        BookingDate = DateTime.UtcNow.AddDays(-i),
                        TotalAmount = 500 + (i * 100),
                        Status = i < 4 ? "Confirmed" : "Pending",
                        DateCreated = DateTime.UtcNow.AddDays(-i),
                        ModifiedDate = DateTime.UtcNow.AddDays(-i)
                    };
                    bookings.Add(booking);

                    // Add accommodation details
                    if (i % 2 == 0 && rooms.Any())
                    {
                        var room = rooms[i % rooms.Count];
                        accommodationDetails.Add(new AccommodationBookingDetail
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            RoomId = room.Id,
                            CheckInDate = startDate,
                            CheckOutDate = endDate,
                            GuestCount = 2,
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    // Add transport details
                    if (i % 3 == 1 && trips.Any())
                    {
                        var trip = trips[i % trips.Count];
                        transportDetails.Add(new TransportBookingDetail
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            TripId = trip.Id,
                            SeatNumber = $"A{(i * 2 + 1):D2}",
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    // Add entertainment details
                    if (i % 3 == 2 && shows.Any())
                    {
                        var show = shows[i % shows.Count];
                        entertainmentDetails.Add(new EntertainmentBookingDetail
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            ShowId = show.Id,
                            SeatNumber = $"VIP-{(i + 1):D2}",
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    // Add extra services
                    if (i % 2 == 0)
                    {
                        extraServices.Add(new ExtraService
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            Name = "Airport Transfer",
                            Price = 30,
                            Quantity = 1,
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    if (i % 3 == 1)
                    {
                        extraServices.Add(new ExtraService
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            Name = "City Tour",
                            Price = 50,
                            Quantity = 2,
                            DateCreated = DateTime.UtcNow
                        });
                    }

                    // Add feedback for completed bookings
                    if (i < 3)
                    {
                        var feedbackId = Guid.NewGuid();
                        feedbacks.Add(new Feedback
                        {
                            Id = feedbackId,
                            BookingId = bookingId,
                            Rating = 4 + (i % 2),
                            Comment = $"Great experience! Service was excellent. Booking {i + 1}",
                            CreatedAt = DateTime.UtcNow.AddDays(-i + 5)
                        });

                        // Add comments to feedback
                        comments.Add(new Comment
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            Content = $"Additional comment: The staff was very helpful throughout our stay.",
                            CreatedAt = DateTime.UtcNow.AddDays(-i + 6)
                        });
                    }
                }

                // Save all data
                await context.Bookings.AddRangeAsync(bookings);
                await context.SaveChangesAsync();

                await context.AccommodationBookingDetails.AddRangeAsync(accommodationDetails);
                await context.TransportBookingDetails.AddRangeAsync(transportDetails);
                await context.EntertainmentBookingDetails.AddRangeAsync(entertainmentDetails);
                await context.ExtraServices.AddRangeAsync(extraServices);
                await context.SaveChangesAsync();

                await context.Feedbacks.AddRangeAsync(feedbacks);
                await context.SaveChangesAsync();

                await context.Comments.AddRangeAsync(comments);
                await context.SaveChangesAsync();
            }
        }
    }
}
