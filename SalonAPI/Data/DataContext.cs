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
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Role")
                .HasValue<Admin>("Admin")
                .HasValue<Owner>("Owner")
                .HasValue<Employee>("Employee")
                .HasValue<Customer>("Customer");

            modelBuilder.Entity<Customer>().Property(e => e.SalonId).HasColumnName("SalonId");
            modelBuilder.Entity<Employee>().Property(e => e.SalonId).HasColumnName("SalonId");

            modelBuilder.Entity<Employee>().Property(e => e.PasswordSalt).HasColumnName("PasswordSalt");
            modelBuilder.Entity<Admin>().Property(e => e.PasswordSalt).HasColumnName("PasswordSalt");
            modelBuilder.Entity<Owner>().Property(e => e.PasswordSalt).HasColumnName("PasswordSalt");

            modelBuilder.Entity<Employee>().Property(e => e.PasswordHash).HasColumnName("PasswordHash");
            modelBuilder.Entity<Admin>().Property(e => e.PasswordHash).HasColumnName("PasswordHash");
            modelBuilder.Entity<Owner>().Property(e => e.PasswordHash).HasColumnName("PasswordHash");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins{ get; set; }
        public DbSet<Owner> Owners{ get; set; }
        public DbSet<Employee> Employees{ get; set; }
        public DbSet<Customer> Customers{ get; set; }
        public DbSet<Salon> Salons { get; set; }


    }
}
