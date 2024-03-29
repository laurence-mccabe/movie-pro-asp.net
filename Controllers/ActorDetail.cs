﻿using Microsoft.AspNetCore.Mvc;
using MoviePro.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviePro.Controllers
{
    public class ActorDetail : Controller
    {
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _mappingService;

        public ActorDetail(IRemoteMovieService tmdbMovieService, 
                           IDataMappingService mappingService)
        {
            _tmdbMovieService = tmdbMovieService;
            _mappingService = mappingService;
        }

        public async Task<IActionResult> Detail(int id)
        {
            var actor = await _tmdbMovieService.ActorDetailAsync(id);
            actor = _mappingService.MapActorDetail(actor);
            return View(actor);
        }
    }
}
