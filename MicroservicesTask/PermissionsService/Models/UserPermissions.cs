using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace PermissionsService.Models
{
    public class UserPermissions
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string UserId { get; set; }

        public List<string> PermissionIds { get; set; }
    }
}
