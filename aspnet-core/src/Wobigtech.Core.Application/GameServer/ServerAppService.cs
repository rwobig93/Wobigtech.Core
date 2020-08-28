using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Wobigtech.Core.GameServer
{
    public class ServerAppService : CrudAppService<Server, ServerDto, Guid, PagedAndSortedResultRequestDto, AddServerDto, AddServerDto>, IServerAppService
    {
        public ServerAppService(IRepository<Server, Guid> repository) : base(repository)
        {
        }
    }
}
