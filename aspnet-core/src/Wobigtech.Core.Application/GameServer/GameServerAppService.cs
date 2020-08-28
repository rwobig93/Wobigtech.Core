using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Wobigtech.Core.GameServer
{
    public class GameServerAppService : CrudAppService<GameServer, GameServerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateGameServerDto, CreateUpdateGameServerDto>, IGameServerAppService
    {
        public GameServerAppService(IRepository<GameServer, Guid> repository) : base(repository)
        {
        }
    }
}
