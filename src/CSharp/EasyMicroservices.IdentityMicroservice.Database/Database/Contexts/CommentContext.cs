//using EasyMicroservices.IdentityMicroservice.Database.Entities;
using EasyMicroservices.Cores.Relational.EntityFrameworkCore;
using EasyMicroservices.Cores.Relational.EntityFrameworkCore.Intrerfaces;
using Microsoft.EntityFrameworkCore;

namespace EasyMicroservices.IdentityMicroservice.Database.Contexts
{
    public class IdentityContext : RelationalCoreContext
    {
        IEntityFrameworkCoreDatabaseBuilder _builder;
        public IdentityContext(IEntityFrameworkCoreDatabaseBuilder builder) : base(builder)
        {
        }

        //public DbSet<IdentityEntity> Identity { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_builder != null)
                _builder.OnConfiguring(optionsBuilder);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



        }
    }
}