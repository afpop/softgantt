using Della.Domain.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;

namespace Della.Domain.Context
{
    public class DellaContext : IdentityDbContext
    {

        public DellaContext()
            : base("DellaConnection")
        {
            Database.SetInitializer<DellaContext>(null);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }


        #region DbSets

        public DbSet<User> User { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<State> State { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Place> Place { get; set; }
        public DbSet<Check> Check { get; set; }
        #endregion

        #region Methods

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Postgre
            //modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("User");
            modelBuilder.Entity<IdentityRole>().ToTable("Role");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogin");

            /********************************************
             *            Configurações Globais
             ********************************************/

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Properties()
                .Where(p => p.Name == "ID")
                .Configure(p => p.IsKey().HasColumnOrder(0));

            modelBuilder.Properties<string>()
                .Configure(p => p.HasColumnType("varchar"));

            modelBuilder.Properties<string>()
                .Configure(p => p.HasMaxLength(100));

            modelBuilder.Properties()
                .Where(p => p.Name == "ModificationDate")
                .Configure(p => p.IsOptional());

            modelBuilder.Properties()
                .Where(p => p.Name == "CreationDate")
                .Configure(p => p.IsRequired());

            modelBuilder.Properties()
                .Where(p => p.Name == "Active")
                .Configure(p => p.IsRequired());

            /********************************************
             *         Fim Configurações Globais
             ********************************************/
        }

        public override int SaveChanges()
        {

            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("CreationDate") != null))
            {

                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreationDate").CurrentValue = DateTime.Now;
                    if (entry.Entity.GetType().GetProperty("Active") != null) entry.Property("Active").CurrentValue = true;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property("CreationDate").IsModified = false;
                    if (entry.Entity.GetType().GetProperty("ModificationDate") != null) entry.Property("ModificationDate").CurrentValue = DateTime.Now;
                }

            }


            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync()
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("CreationDate") != null))
            {

                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreationDate").CurrentValue = DateTime.Now;
                    if (entry.Entity.GetType().GetProperty("Active") != null) entry.Property("Active").CurrentValue = true;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property("CreationDate").IsModified = false;
                    if (entry.Entity.GetType().GetProperty("ModificationDate") != null) entry.Property("ModificationDate").CurrentValue = DateTime.Now;
                }

            }

            return await base.SaveChangesAsync();
        }

        #endregion

    }
}
