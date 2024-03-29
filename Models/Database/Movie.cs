﻿using Microsoft.AspNetCore.Http;
using MoviePro.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviePro.Models.Database
{
    public class Movie
    {
        public int Id { get; set; }
        public int MovieId { get; set; }

        public string Title { get; set; }
        public string TagLine { get; set; }
        public string OverView { get; set; }
        public int RunTime { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "ReleaseDate")]
        public DateTime ReleaseDate { get; set; }

        public MovieRating Rating { get; set; }
        public float VoteAverage { get; set; }

        public byte[] Poster { get; set; }
        public string PosterType { get; set; }

        public byte[] Backdrop { get; set; }
        public string BackdropType { get; set; }

        public string TrailerUrl { get; set; }


        //postgres does not know what an IformFile is so it's set to notMapped
        [NotMapped]
        [Display(Name = "Poster Image")]
        public IFormFile PosterFile { get; set; }

        [NotMapped]
        [Display(Name = "Backdrop Image")]
        public IFormFile BackdropFile { get; set; }

        public ICollection<MovieCollection> Collections { get; set; } = new HashSet<MovieCollection>();
        public ICollection<MovieCast> Cast { get; set; } = new HashSet<MovieCast>();
        public ICollection<MovieCrew> Crew { get; set; } = new HashSet<MovieCrew>();


    }
} 
