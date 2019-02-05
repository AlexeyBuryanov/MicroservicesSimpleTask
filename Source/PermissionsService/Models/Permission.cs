using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace PermissionsService.Models
{
    public class Permission
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string PermissionId { get; set; }

        public string Name { get; set; }
    }
}
