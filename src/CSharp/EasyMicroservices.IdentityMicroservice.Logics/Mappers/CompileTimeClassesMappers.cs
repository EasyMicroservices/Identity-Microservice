//using System.Threading.Tasks;
//using EasyMicroservices.Mapper.CompileTimeMapper.Interfaces;
//using EasyMicroservices.Mapper.Interfaces;
//using System.Linq;

//namespace CompileTimeMapper
//{
//    public class IdentityEntity_IdentityContract_Mapper : IMapper
//    {
//        readonly IMapperProvider _mapper;
//        public IdentityEntity_IdentityContract_Mapper(IMapperProvider mapper)
//        {
//            _mapper = mapper;
//        }
//        public global::EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity Map(global::EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract fromObject, string uniqueRecordId, string language, object[] parameters)
//        {
//            if (fromObject == default)
//                return default;
//            var mapped = new global::EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity()
//            {
//                Id = fromObject.Id,
//                ParentId = fromObject.ParentId,
//                Name = fromObject.Name,
//                Text = fromObject.Text,
//                Email = fromObject.Email,
//                Website = fromObject.Website,
//                CreationDateTime = fromObject.CreationDateTime,
//                ModificationDateTime = fromObject.ModificationDateTime,
//                IsDeleted = fromObject.IsDeleted,
//                DeletedDateTime = fromObject.DeletedDateTime,
//                UniqueIdentity = fromObject.UniqueIdentity
//            };
//            return mapped;
//        }
//        public global::EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract Map(global::EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity fromObject, string uniqueRecordId, string language, object[] parameters)
//        {
//            if (fromObject == default)
//                return default;
//            var mapped = new global::EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract()
//            {
//                Id = fromObject.Id,
//                ParentId = fromObject.ParentId,
//                Name = fromObject.Name,
//                Text = fromObject.Text,
//                Email = fromObject.Email,
//                Website = fromObject.Website,
//                CreationDateTime = fromObject.CreationDateTime,
//                ModificationDateTime = fromObject.ModificationDateTime,
//                IsDeleted = fromObject.IsDeleted,
//                DeletedDateTime = fromObject.DeletedDateTime,
//                UniqueIdentity = fromObject.UniqueIdentity
//            };
//            return mapped;
//        }
//        public async Task<global::EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity> MapAsync(global::EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract fromObject, string uniqueRecordId, string language, object[] parameters)
//        {
//            if (fromObject == default)
//                return default;
//            var mapped = new global::EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity()
//            {
//                Id = fromObject.Id,
//                ParentId = fromObject.ParentId,
//                Name = fromObject.Name,
//                Text = fromObject.Text,
//                Email = fromObject.Email,
//                Website = fromObject.Website,
//                CreationDateTime = fromObject.CreationDateTime,
//                ModificationDateTime = fromObject.ModificationDateTime,
//                IsDeleted = fromObject.IsDeleted,
//                DeletedDateTime = fromObject.DeletedDateTime,
//                UniqueIdentity = fromObject.UniqueIdentity
//            };
//            return mapped;
//        }
//        public async Task<global::EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract> MapAsync(global::EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity fromObject, string uniqueRecordId, string language, object[] parameters)
//        {
//            if (fromObject == default)
//                return default;
//            var mapped = new global::EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract()
//            {
//                Id = fromObject.Id,
//                ParentId = fromObject.ParentId,
//                Name = fromObject.Name,
//                Text = fromObject.Text,
//                Email = fromObject.Email,
//                Website = fromObject.Website,
//                CreationDateTime = fromObject.CreationDateTime,
//                ModificationDateTime = fromObject.ModificationDateTime,
//                IsDeleted = fromObject.IsDeleted,
//                DeletedDateTime = fromObject.DeletedDateTime,
//                UniqueIdentity = fromObject.UniqueIdentity

//            };
//            return mapped;
//        }
//        public object MapObject(object fromObject, string uniqueRecordId, string language, object[] parameters)
//        {
//            if (fromObject == default)
//                return default;
//            if (fromObject.GetType() == typeof(EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity))
//                return Map((EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity)fromObject, uniqueRecordId, language, parameters);
//            return Map((EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract)fromObject, uniqueRecordId, language, parameters);
//        }
//        public async Task<object> MapObjectAsync(object fromObject, string uniqueRecordId, string language, object[] parameters)
//        {
//            if (fromObject == default)
//                return default;
//            if (fromObject.GetType() == typeof(EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity))
//                return await MapAsync((EasyMicroservices.IdentityMicroservice.Database.Entities.IdentityEntity)fromObject, uniqueRecordId, language, parameters);
//            return await MapAsync((EasyMicroservices.IdentityMicroservice.Contracts.Common.IdentityContract)fromObject, uniqueRecordId, language, parameters);
//        }
//    }
//}