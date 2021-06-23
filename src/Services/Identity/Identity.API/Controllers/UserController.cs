﻿using IdentityService.Models;
using Microsoft.AspNetCore.Mvc;
using Jobs.Service.Common.Controllers;
using Jobs.Service.Common.Helpers;
using Jobs.Service.Common.Repository;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using EventBus.RabbitMQ;
using IdentityService.RabbitMQEvents.Events;
using AutoMapper;

namespace IdentityService.Controllers
{
    [Route("api/[controller]")]
    public class UserController : BaseController<User, UserDTO>
    {
        private readonly IEventBusRabbitMQ _eventBus;

        public UserController(IEntityRepository<User> pepository, IMapper mapper, IEventBusRabbitMQ eventBus) : base(pepository, mapper)
        {
            _eventBus = eventBus;
        }

        [SwaggerResponse(200, "Return OK if it's added successfully", typeof(UserDTO))]
        [SwaggerResponse(400, "The Password field is required.", typeof(RequestModel))]
        public override async Task<RequestModel> Post([FromBody] UserDTO entity)
        {
            if (string.IsNullOrEmpty(entity.Password))
                return await RequestModel.ErrorRequestAsync("The Password field is required.", 400);

            entity.Password = Encryptor.SH1Hash(entity.Password);

            return await base.Post(entity);
        }

        [SwaggerOperation(Summary = "To update exists user info. For this you must be authorized")]
        [SwaggerResponse(200, "Return OK if it's updated successfully", typeof(RequestModel))]
        [SwaggerResponse(404, "User with the specified ID was not found", typeof(RequestModel))]
        public override async Task<RequestModel> Put([FromBody] UserDTO entity)
        {
            User oldUser = null;
            if (entity != null)
                oldUser = await _pepository.GetEntityByID(entity.Id);

            if (oldUser == null)
                return await RequestModel.NotFoundAsync();

            var oldPassword = oldUser.HashPassword;
            var oldName = oldUser.Name;
            _mapper.Map(entity, oldUser);

            if (string.IsNullOrEmpty(entity.Password) || Encryptor.SH1Hash(entity.Password) == oldPassword)
                oldUser.HashPassword = oldPassword;
            else
                oldUser.HashPassword = Encryptor.SH1Hash(entity.Password);

            await _pepository.UpdateEntity(oldUser);

            if (oldName != entity.Name)
            {
                var nameUpdated = new UserNameUpdatedEvent() { UserId = entity.Id, OldName = oldName, NewName = entity.Name };
                _eventBus.Publish(nameUpdated);
            }

            return await RequestModel.SuccessAsync();
        }
    }
}

