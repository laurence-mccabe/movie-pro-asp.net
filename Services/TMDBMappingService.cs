﻿using Microsoft.Extensions.Options;
using MoviePro.Enums;
using MoviePro.Models.Settings;
using MoviePro.Models.Database;
using MoviePro.Models.TMDB;
using MoviePro.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MoviePro.Services
{
    public class TMDBMappingService : IDataMappingService
    {
        private readonly AppSettings _appsettings;
        private readonly IImageService _imageService;

        public TMDBMappingService(IOptions<AppSettings> appsettings,
                                  IImageService imageService)
        {
            _appsettings = appsettings.Value;
            _imageService = imageService;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public ActorDetail MapActorDetail(ActorDetail actor)
        {
            actor.profile_path = BuildCastImage(actor.profile_path);

            if (string.IsNullOrEmpty(actor.biography))
                actor.biography = "Not Available";

            if (string.IsNullOrEmpty(actor.place_of_birth))
                actor.place_of_birth = "Not Available";

            if (string.IsNullOrEmpty(actor.birthday))
                actor.birthday = "Not Available";
            else
                actor.birthday = DateTime.Parse(actor.birthday).ToString("MMM dd, yyyy");
            return actor;
                
        }
       //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<Movie> MapMovieDetailAsync(MovieDetail movie)
        {
            Movie newMovie = null;
            try
            {
                newMovie = new Movie()
                {
                    MovieId = movie.id,
                    Title = movie.title,
                    TagLine = movie.tagline,
                    OverView = movie.overview,
                    RunTime = movie.runtime,
                    VoteAverage = movie.vote_average,
                    ReleaseDate = DateTime.Parse(movie.release_date),
                    TrailerUrl = BuildTrailerPath(movie.videos),
                    Backdrop = await EncodeBackdropImageAsync(movie.backdrop_path),
                    BackdropType = BuildImageType(movie.backdrop_path),
                    Poster = await EncodePosterImageAsync(movie.poster_path),
                    PosterType = BuildImageType(movie.poster_path),
                    Rating = GetRating(movie.release_dates)
                };

                var castMembers = movie.credits.cast.OrderByDescending(c => c.popularity)
                                                                       .GroupBy(c => c.cast_id)
                                                                       .Select(g => g.FirstOrDefault())
                                                                       .Take(20)
                                                                       .ToList();

                castMembers.ForEach(member =>
                {
                    newMovie.Cast.Add(new MovieCast()
                    {
                        CastId = member.id,
                        Department = member.known_for_department,
                        Name = member.name,
                        Character = member.character,
                        ImageUrl = BuildCastImage(member.profile_path)

                    });
                });

                var crewMembers = movie.credits.crew.OrderByDescending(c => c.popularity)
                                                                      .GroupBy(c => c.id)
                                                                      .Select(g => g.First())
                                                                      .Take(20)
                                                                      .ToList();

                crewMembers.ForEach(member =>
                {
                    newMovie.Crew.Add(new MovieCrew()
                    {
                        CrewId = member.id,
                        Department = member.department,
                        Name = member.name,
                        Job = member.job,
                        ImageUrl = BuildCastImage(member.profile_path)

                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MapMovieDetailAsync: {ex.Message}");
            }
            return newMovie;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        private MovieRating GetRating (Release_Dates dates)
        {
            var movieRating = MovieRating.NR;
            var certification = dates.results.FirstOrDefault(r => r.iso_3166_1 == "US");
            if(certification is not null)
            {
                var apiRating = certification.release_dates.FirstOrDefault(c => c.certification != "")?.certification.Replace("-", "");
                if (!string.IsNullOrEmpty(apiRating))
                {
                    movieRating = (MovieRating)Enum.Parse(typeof(MovieRating), apiRating,true);
                }
            }
            return movieRating;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string BuildTrailerPath(Videos videos)
        {
            var videoKey = videos.results.FirstOrDefault(r => r.type.ToLower().Trim() == "trailer" && r.key != "")?.key;
            return string.IsNullOrEmpty(videoKey) ? videoKey : $"{ _appsettings.TMDBSettings.BaseYouTubePath}{videoKey}";
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private async Task<byte[]> EncodeBackdropImageAsync(string path)
        {
            var backdropPath = $"{_appsettings.TMDBSettings.BaseImagePath}/{_appsettings.MovieProSettings.DefaultBackdropSize}/{path}";
            return await _imageService.EncodeImageURLAsync(backdropPath);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string BuildImageType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return $"image/{Path.GetExtension(path).TrimStart('.')}";
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private async Task<byte[]> EncodePosterImageAsync(string path)
        {
            var posterPath = $"{_appsettings.TMDBSettings.BaseImagePath}/{_appsettings.MovieProSettings.DefaultPosterSize}/{path}";
            return await _imageService.EncodeImageURLAsync(posterPath); 
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private string BuildCastImage(string profilePath)
        {
            if (string.IsNullOrEmpty(profilePath))
                return _appsettings.MovieProSettings.DefaultCastImage;

            return $"{_appsettings.TMDBSettings.BaseImagePath}/{_appsettings.MovieProSettings.DefaultPosterSize}/{profilePath}";
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    }

}
