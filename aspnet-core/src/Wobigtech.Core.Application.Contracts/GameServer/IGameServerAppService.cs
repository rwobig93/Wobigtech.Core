using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wobigtech.Core.GameServer
{
    public interface IGameServerAppService : ICrudAppService<GameServerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateGameServerDto, CreateUpdateGameServerDto>
    {

    }
}
