using EasyMicroservices.Cores.Interfaces;

namespace EasyMicroservices.IdentityMicroservice.Contracts.Responses
{
    public class UserResponseContract : IUniqueIdentitySchema
    {
        public string Token { get; set; }
        public string UniqueIdentity { get; set; }
    }
}
