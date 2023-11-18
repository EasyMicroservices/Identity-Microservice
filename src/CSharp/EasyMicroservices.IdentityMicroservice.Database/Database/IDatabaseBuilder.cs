using Microsoft.EntityFrameworkCore;

namespace EasyMicroservices.IdentityMicroservice.Database
{
    public interface IDatabaseBuilder
    {
        void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
    }
}
