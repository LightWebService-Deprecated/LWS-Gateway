using System.Threading.Tasks;
using LWS_Gateway.Management.Model;
using LWS_Gateway.Repository;
using MongoDB.Driver;

namespace LWS_Gateway.Management.Repository
{
    public interface INodeRepository
    {
        Task AddNodeInfoAsync(NodeInformation nodeInformation);
    }

    public class NodeRepository : INodeRepository
    {
        private readonly IMongoCollection<NodeInformation> _nodeCollection;

        public NodeRepository(MongoContext mongoContext)
        {
            _nodeCollection = mongoContext.MongoDatabase.GetCollection<NodeInformation>(nameof(NodeInformation));
        }

        public async Task AddNodeInfoAsync(NodeInformation nodeInformation)
        {
            await _nodeCollection.InsertOneAsync(nodeInformation);
        }
    }
}