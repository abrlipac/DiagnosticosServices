﻿using Clientes.Persistence.Database;
using Clientes.Service.Queries.DTOs;
using Common.Result;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clientes.Service.Queries
{
    public interface IPacienteQueryService
    {
        Task<DataCollection<PacienteDto>> GetAllAsync(
            string dni, 
            string usuarioId, 
            int page, 
            int take, 
            IEnumerable<int> ids = null);
        Task<PacienteDto> GetAsync(int id);
    }

    public class PacienteQueryService : IPacienteQueryService
    {
        private readonly ApplicationDbContext _context;

        public PacienteQueryService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataCollection<PacienteDto>> GetAllAsync(string dni, string usuarioId, int page, int take, IEnumerable<int> ids = null) 
        {
            var collection = await _context.Pacientes
                .Where(x => dni == null || x.Dni == dni)
                .Where(x => usuarioId == null || x.Usuario_Id == usuarioId)
                .Where(x => ids == null || ids.Contains(x.Id))
                .OrderByDescending(x => x.Id)
                .GetPagedAsync(page, take);

            return collection.Adapt<DataCollection<PacienteDto>>();
        }

        public async Task<PacienteDto> GetAsync(int id)
        {
            try
            {
                return (await _context.Pacientes.SingleAsync(x => x.Id == id)).Adapt<PacienteDto>();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
