using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LWS_Gateway.Management.Model
{
    public class NodeInformation
    {
        // Default Node Information
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string NodeNickName { get; set; }
        public string NodeUrl { get; set; }
        public int NodeMaximumCpu { get; set; }
        public int NodeMaximumRam { get; set; }
        public string NodeKey { get; set; }

        // Allocated Resource Information(Which is NOT identical to real system usage.)
        public int NodeAllocatedCpu { get; set; }
        public int NodeAllocatedRam { get; set; }
        
        // Usage calculated.(TODO: Auto-Calculate Usage)
        public double NodeCpuUsage { get; set; }
        public double NodeRamUsage { get; set; }
    }
}