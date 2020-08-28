using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wobigtech.Core.GameServer
{
    public interface IServerAppService : ICrudAppService<ServerDto, Guid, PagedAndSortedResultRequestDto, AddServerDto, AddServerDto>
    {

    }
}
