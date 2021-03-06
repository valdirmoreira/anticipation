﻿using api.Infrastructure;
using api.Models.EntityModel;
using api.Models.ResultModel;
using api.Models.ServiceModel;
using api.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/v2/anticipations")]
    [ApiController]
    public class AnticipationsController : ControllerBase
    {
        private ApiDbContext _dbContext;

        public AnticipationsController(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Solicitar antecipação de transações selecionadas
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(AnticipationJson))]
        [HttpPost, Route("")]
        public async Task<IActionResult> Add([FromBody] RequestAnticipationModel model)
        {
            var anticipation = await _dbContext.Anticipations
                .WhereInProgress()
                .SingleOrDefaultAsync();

            if (anticipation != null)
            {
                return new AnticipationAlreadyInProgress(anticipation);
            }

            var anticipationProcessing = new AnticipationProcessing(_dbContext);

            if (!await anticipationProcessing.Process(model))
            {
                return BadRequest();
            }

            return new AnticipationJson(anticipationProcessing.Anticipation);
        }

        /// <summary>
        /// Consultar os detalhes da solicitação em andamento
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(AnticipationJson))]
        [HttpGet, Route("in-progress")]
        public async Task<IActionResult> FindInProgress()
        {
            var anticipation = await _dbContext.Anticipations
                 .WhereInProgress()
                 .SingleOrDefaultAsync();

            if(anticipation == null)
            {
                return NotFound();
            }

            return new AnticipationJson(anticipation);
        }

        /// <summary>
        /// Consultar histórico das solicitações realizadas em um determinado período
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(AnticipationListJson))]
        [HttpGet, Route("")]
        public async Task<IActionResult> List([FromQuery] AnticipationListModel model)
        {
            var anticipationQuery = _dbContext.Anticipations
                 .WhereDateFrom(model.StartDate)
                 .WhereDateUntil(model.EndDate);

            var anticipations = await anticipationQuery
                .OrderByDescendingCreatedAt()
                .Skip(model.Index)
                .Take(model.Length)
                .ToListAsync();

            var periodRequestedAmount = await anticipationQuery.SumAsync(anticipation => anticipation.RequestedAmount);
            var count = await anticipationQuery.LongCountAsync();

            return new AnticipationListJson(anticipations, count, periodRequestedAmount);
        }

        /// <summary>
        /// Iniciar a analise de uma antecipação
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(AnticipationJson))]
        [HttpPatch, Route("start-analysis")]
        public async Task<IActionResult> StartAnalysis([FromBody] AnticipationModel model)
        {
            var anticipationProcessing = new AnticipationProcessing(_dbContext);

            if (!await anticipationProcessing.StartAnalysis(model))
            {
                return BadRequest();
            }

            return new AnticipationJson(anticipationProcessing.Anticipation);
        }
    }
}