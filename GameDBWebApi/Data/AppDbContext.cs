using Microsoft.EntityFrameworkCore;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<UserGame> UserGames { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<UserListGame> UserListGames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. USER CONFIGURATION
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                // Username must be unique and required
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(50);
                entity.HasIndex(u => u.UserName).IsUnique();

                // Email must be unique and required
                entity.Property(u => u.UserEmail).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.UserEmail).IsUnique();
            });

            // 2. GAME CONFIGURATION
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");

                entity.HasKey(g => g.Id);

                entity.HasIndex(g => g.RawgId).IsUnique();

                entity.Property(g => g.Title).IsRequired().HasMaxLength(250);
            });

            // 3. USER-GAME CONFIGURATION (Core Status)
            modelBuilder.Entity<UserGame>(entity =>
            {
                entity.ToTable("UserGames");

                // COMPOSITE PRIMARY KEY: A user can only have one core status per game
                entity.HasKey(ug => new { ug.UserId, ug.GameId });

                // Relationship configuration (1-to-Many from User to UserGame)
                entity.HasOne(ug => ug.User)
                      .WithMany(u => u.UserGames)
                      .HasForeignKey(ug => ug.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // If User is deleted, delete their game statuses

                // Relationship configuration (1-to-Many from Game to UserGame)
                entity.HasOne(ug => ug.Game)
                      .WithMany(g => g.UserGames)
                      .HasForeignKey(ug => ug.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. USER-LIST CONFIGURATION (Custom Playlists)
            modelBuilder.Entity<UserList>(entity =>
            {
                entity.ToTable("UserLists");

                entity.HasKey(ul => ul.Id);

                entity.Property(ul => ul.Name).IsRequired().HasMaxLength(100);

                // Relationship configuration (A user owns many lists)
                entity.HasOne(ul => ul.User)
                      .WithMany(u => u.UserLists)
                      .HasForeignKey(ul => ul.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete all their lists
            });

            // 5. USER-LIST-GAME CONFIGURATION (Games inside Playlists)
            modelBuilder.Entity<UserListGame>(entity =>
            {
                entity.ToTable("UserListGames");

                // COMPOSITE PRIMARY KEY: A game can only be added ONCE to a specific list
                entity.HasKey(ulg => new { ulg.UserListId, ulg.GameId });

                // Relationship configuration (1-to-Many from UserList to UserListGame)
                entity.HasOne(ulg => ulg.UserList)
                      .WithMany(ul => ul.ListedGames)
                      .HasForeignKey(ulg => ulg.UserListId)
                      .OnDelete(DeleteBehavior.Cascade);

                // UNIDIRECTIONAL RELATIONSHIP
                entity.HasOne(ulg => ulg.Game)
                      .WithMany()
                      .HasForeignKey(ulg => ulg.GameId)
                      .OnDelete(DeleteBehavior.Cascade); // If game is removed from global DB, remove it from all lists
            });
        }
    }
}
