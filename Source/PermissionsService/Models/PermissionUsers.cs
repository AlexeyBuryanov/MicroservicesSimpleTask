using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace PermissionsService.Models
{
    public class PermissionUsers
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string PermissionId { get; set; }

        public List<string> UserIds { get; set; }
    }
}
