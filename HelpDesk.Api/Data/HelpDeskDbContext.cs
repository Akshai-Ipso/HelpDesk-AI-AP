using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data
{
    public class HelpDeskDbContext : DbContext
    {
        public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets => Set<Ticket>();

        public DbSet<TicketAntwort> TicketAntworten =>
            Set<TicketAntwort>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets");
                entity.HasKey(ticket => ticket.Id);
            });

            modelBuilder.Entity<TicketAntwort>(entity =>
            {
                entity.ToTable("TicketAntworten");
                entity.HasKey(antwort => antwort.Id);

                entity.HasOne(antwort => antwort.Ticket)
                    .WithMany(ticket => ticket.Antworten)
                    .HasForeignKey(antwort => antwort.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}