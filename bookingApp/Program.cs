using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using bookingApp.Models;
using Newtonsoft.Json;


namespace bookingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 4 || args[0] != "--hotels" || args[2] != "--bookings")
            {
                Console.WriteLine("Usage: bookingApp --hotels hotels.json --bookings bookings.json");
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return;
            }

            string hotelsFilePath = args[1];
            string bookingsFilePath = args[3];

            if (!File.Exists(hotelsFilePath) || !File.Exists(bookingsFilePath))
            {
                Console.WriteLine("Error: One or both JSON files not found.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return;
            }

            var hotels = LoadHotels(hotelsFilePath);
            var bookings = LoadBookings(bookingsFilePath);

            Console.WriteLine("Enter commands like: Availability(H1, 20240404, SGL) or blank line to exit.");

            while (true)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    break;

                if (input.StartsWith("Availability"))
                {
                    try
                    {
                        var command = input.Substring(12).Trim('(', ')');
                        var parts = command.Split(", ");

                        string hotelId = parts[0];
                        string dateRange = parts[1];
                        string roomType = parts[2];

                        int availability = CheckAvailability(hotels, bookings, hotelId, dateRange, roomType);
                        Console.WriteLine(availability);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command. Use Availability(hotelId, dateRange, roomType).");
                }
            }
        }

        private static List<Hotel> LoadHotels(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Hotel>>(json);
        }

        private static List<Booking> LoadBookings(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Booking>>(json);
        }

        private static int CheckAvailability(List<Hotel> hotels, List<Booking> bookings, string hotelId, string dateRange, string roomType)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);
            if (hotel == null)
                throw new Exception("Hotel not found");

            int totalRooms = hotel.Rooms.Count(r => r.RoomType == roomType);
            if (totalRooms == 0)
                throw new Exception("Room type not found in hotel");

            string[] dates = dateRange.Split('-');
            DateTime startDate = DateTime.ParseExact(dates[0], "yyyyMMdd", null);
            DateTime endDate = startDate;
            if (dates.Length > 1)
            {
                endDate = DateTime.ParseExact(dates[1], "yyyyMMdd", null);
            }

            if (startDate > endDate) 
            {
                throw new Exception("Invalid date range");
            }

            int occupiedRooms = bookings.Count(b => b.HotelId == hotelId && b.RoomType == roomType &&
                !(DateTime.ParseExact(b.Departure, "yyyyMMdd", null) <= startDate ||
                  DateTime.ParseExact(b.Arrival, "yyyyMMdd", null) >= endDate));

            return totalRooms - occupiedRooms;
        }
    }

}