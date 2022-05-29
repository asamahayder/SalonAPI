using Microsoft.EntityFrameworkCore;
using SalonAPI.Models;

namespace SalonAPI.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>()
            //    .HasDiscriminator<string>("Role")
            //    .HasValue<Admin>("Admin")
            //    .HasValue<Owner>("Owner")
            //    .HasValue<Employee>("Employee")
            //    .HasValue<Customer>("Customer");


        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins{ get; set; }
        public DbSet<Owner> Owners{ get; set; }
        public DbSet<Employee> Employees{ get; set; }
        public DbSet<Customer> Customers{ get; set; }
        public DbSet<Salon> Salons { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Booking> Bookings { get; set; }


    }
}
