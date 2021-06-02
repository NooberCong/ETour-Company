﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Company.Models;
using Core.Entities;
using Core.Interfaces;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Company.Controllers
{
    public class ItineraryController : Controller
    {
        private readonly ITripRepository _tripRepository;
        private readonly IItineraryRepository _itineraryRepository;
        private readonly IRemoteFileStorageHandler _remoteFileStorageHandler;
        private readonly HtmlDocument _doc;
        private readonly IUnitOfWork _unitOfWork;

        public ItineraryController(ITripRepository tripRepository, IItineraryRepository itineraryRepository, IRemoteFileStorageHandler remoteFileStorageHandler, HtmlDocument doc, IUnitOfWork unitOfWork)
        {
            _tripRepository = tripRepository;
            _itineraryRepository = itineraryRepository;
            _remoteFileStorageHandler = remoteFileStorageHandler;
            _doc = doc;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> New(int tripID)
        {
            var trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == tripID);

            if (trip == null || !trip.IsOpen)
            {
                return NotFound();
            }

            return View(new ItineraryFormModel
            {
                Trip = trip
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Itinerary itinerary, string returnUrl)
        {
            returnUrl ??= Url.Action("Detail", "Trip", new { id = itinerary.TripID });

            var trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == itinerary.TripID);

            if (trip == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new ItineraryFormModel
                {
                    Trip = trip,
                    Itinerary = itinerary
                });
            }

            await ProcessDetail(itinerary);

            await _itineraryRepository.AddAsync(itinerary);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itinerary created successfully";
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var itinerary = await _itineraryRepository.Queryable
                .Include(it => it.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == id);

            if (itinerary == null)
            {
                return NotFound();
            }

            return View(new ItineraryFormModel
            {
                Trip = itinerary.Trip,
                Itinerary = itinerary
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Itinerary itinerary, string returnUrl)
        {
            returnUrl ??= Url.Action("Detail", "Trip", new { id = itinerary.TripID });

            var existingItinerary = await _itineraryRepository.Queryable
                .Include(it => it.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == itinerary.ID);

            if (existingItinerary == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new ItineraryFormModel
                {
                    Trip = existingItinerary.Trip,
                    Itinerary = itinerary
                });
            }

            await ProcessDetail(itinerary);

            var imagesToDelete = existingItinerary.ImageUrls.Where(url => !itinerary.ImageUrls.Contains(url));

            foreach (var imageUrl in imagesToDelete)
            {
                await _remoteFileStorageHandler.DeleteAsync(imageUrl);
            }

            await _itineraryRepository.UpdateAsync(itinerary);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itinerary updated successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string returnUrl)
        {
            var itinerary = await _itineraryRepository.FindAsync(id);

            if (itinerary == null)
            {
                return NotFound();
            }

            returnUrl ??= Url.Action("Detail", "Trip", new { id = itinerary.TripID });

            await _itineraryRepository.DeleteAsync(itinerary);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itinerary deleted successfully";
            return LocalRedirect(returnUrl);
        }

        private async Task ProcessDetail(Itinerary itinerary)
        {
            _doc.LoadHtml(itinerary.Detail);

            foreach (var img in _doc.DocumentNode.SelectNodes("//img"))
            {

                var src = img.Attributes["src"].Value;
                // Upload image and set src to the image url
                if (src.StartsWith("data:image"))
                {
                    var stream = new MemoryStream(Convert.FromBase64String(src.Split(";base64,")[1]));
                    src = await _remoteFileStorageHandler.UploadAsync(stream, "jpg");
                    img.Attributes["src"].Value = src;
                }
                itinerary.ImageUrls.Add(src);
            }

            itinerary.Detail = _doc.DocumentNode.InnerHtml;
        }
    }
}