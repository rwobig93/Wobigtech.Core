using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Wobigtech.Core.GameServer;
using System.Linq;

namespace Wobigtech.Core.Background
{
    public class GameServerUpdate015Worker : AsyncPeriodicBackgroundWorkerBase
    {
        private readonly IRepository<GameServer.GameServer, Guid> _gameServerRepository;
        private readonly IRepository<Server, Guid> _serverRepository;
        public GameServerUpdate015Worker(AbpTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            timer.Period = 900000; // 15 min interval
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            Logger.LogDebug("Running 15min Game Server Update Worker");

            List<GameServer.GameServer> gameServerListed = new List<GameServer.GameServer>();

            foreach (var gameServer in await _gameServerRepository.GetListAsync())
            {
                gameServerListed.Add(gameServer);
            }

            foreach (var server in await _serverRepository.GetListAsync())
            {
                var localList = gameServerListed.FindAll(x => x.ServerID == server.Id);

            }
        }
    }
}
