﻿using Jobs.Common.Models;
using Jobs.Service.Common.Helpers;
using Jobs.Service.Common.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Jobs.Service.Common.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public abstract class BaseController<TEntity> : ControllerBase where TEntity : IEntity
    {
        protected readonly IEntityRepository<TEntity> _pepository;

        public BaseController(IEntityRepository<TEntity> pepository)
        {
            _pepository = pepository;
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation(Summary = "To get all items")]
        [SwaggerResponse(200, "Return list of found items if it's finished successfully", typeof(RequestModel))]
        public virtual async Task<RequestModel> Get()
        {
            var entities = await _pepository.GetEntities();
            return await RequestModel.SuccessAsync(entities);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "To get an item by Id")]
        [SwaggerResponse(200, "Return the found item if it's finished successfully", typeof(RequestModel))]
        [SwaggerResponse(404, "An item with the specified ID was not found", typeof(RequestModel))]
        public virtual async Task<RequestModel> Get(Guid id)
        {
            var entity = await _pepository.GetEntityByID(id);
            if (entity == null)
                return await RequestModel.NotFoundAsync();

            return await RequestModel.SuccessAsync(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "To add a new item. For this you must be authorized")]
        [SwaggerResponse(200, "Return OK if it's added successfully", typeof(RequestModel))]
        public virtual async Task<RequestModel> Post([FromBody] TEntity entity)
        {
            var createdEntity = await _pepository.InsertEntity(entity);
            return await RequestModel.SuccessAsync(createdEntity);
        }

        [HttpPut]
        [SwaggerOperation(Summary = "To update exists an item. For this you must be authorized")]
        [SwaggerResponse(200, "Return OK if it's updated successfully", typeof(RequestModel))]
        public virtual async Task<RequestModel> Put([FromBody] TEntity entity)
        {
            if (entity != null)
            {
                if (await _pepository.GetEntityByID(entity.Id) == null)
                    return await RequestModel.NotFoundAsync();

                await _pepository.UpdateEntity(entity);
                return await RequestModel.SuccessAsync();
            }
            return await RequestModel.ErrorRequestAsync("An item can not be null");
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "To delete an item. For this you must be authorized")]
        [SwaggerResponse(200, "Return OK if it's deleted successfully", typeof(RequestModel))]
        public virtual async Task<RequestModel> Delete(Guid id)
        {
            if (await _pepository.GetEntityByID(id) == null)
                return await RequestModel.NotFoundAsync();

            await _pepository.DeleteEntity(id);
            return await RequestModel.SuccessAsync();
        }
    }
}
