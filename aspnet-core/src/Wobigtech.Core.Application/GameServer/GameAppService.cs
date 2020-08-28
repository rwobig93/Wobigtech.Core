using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Wobigtech.Core.GameServer
{
    public class GameAppService : CrudAppService<Game, GameDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateGameDto, CreateUpdateGameDto>, IGameAppService
    {
        public GameAppService(IRepository<Game, Guid> repository) : base(repository)
        {

        }
    }
}
