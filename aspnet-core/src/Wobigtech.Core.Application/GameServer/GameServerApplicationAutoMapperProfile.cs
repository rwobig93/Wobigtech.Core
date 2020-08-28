using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Core.GameServer
{
    public class GameServerApplicationAutoMapperProfile : Profile
    {
        public GameServerApplicationAutoMapperProfile()
        {
            CreateMap<Game, GameDto>();
            CreateMap<CreateUpdateGameDto, Game>();
            CreateMap<Server, ServerDto>();
            CreateMap<AddServerDto, Server>();
            CreateMap<GameServer, GameServerDto>();
            CreateMap<CreateUpdateGameServerDto, GameServer>();
        }
    }
}
